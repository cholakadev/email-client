﻿using EmailClient.Core.Constants;
using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace EmailClient.Infrastructure
{
    public class SmtpClient : ISmtpClient
    {
        private readonly IOptionsMonitor<SmtpSettings> _settings;

        public SmtpClient(IOptionsMonitor<SmtpSettings> settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Connects to an SMTP server and sends an email message using the SMTP protocol with STARTTLS upgrade for secure transmission.
        /// </summary>
        /// <param name="message">The email message to be sent, including sender, recipient, subject, and body.</param>
        /// <remarks>
        /// The method performs the following steps:
        /// 1. Establishes a TCP connection to the SMTP server.
        /// 2. Reads the server's initial greeting and checks for a 220 Service Ready response.
        /// 3. Sends an EHLO command to retrieve the server's capabilities.
        /// 4. Initiates a STARTTLS command to upgrade to a secure TLS/SSL connection.
        /// 5. Authenticates the user using the AUTH LOGIN SMTP command with base64-encoded credentials (username and password).
        /// 6. Sends the email data (sender, recipient) and the message body (headers and content).
        /// 7. Terminates the SMTP session gracefully with the QUIT command.
        /// The connection and all communication after STARTTLS command are secured using SSL.
        /// </remarks>
        /// <exception cref="Exception">Thrown if the server does not respond with expected SMTP response codes during all of the phases.</exception>
        /// <exception cref="AuthenticationException">Thrown if authentication with the SMTP server fails.</exception>
        public async Task SendEmailAsync(EmailMessageDto message)
        {
            if (!_settings.CurrentValue.RequiresStartTls)
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(_settings.CurrentValue.Host, int.Parse(_settings.CurrentValue.Port));

                var sslStream = new SslStream(tcpClient.GetStream(), leaveInnerStreamOpen: false);
                try
                {
                    await sslStream.AuthenticateAsClientAsync(_settings.CurrentValue.Host);
                }
                catch (Exception ex)
                {
                    throw new AuthenticationException($"TLS handshake failed with host '{_settings.CurrentValue.Host}'.", ex);
                }

                var reader = new StreamReader(sslStream, Encoding.ASCII);
                var writer = new StreamWriter(sslStream, Encoding.ASCII) { AutoFlush = true };

                var greeting = await ReadResponseAsync(reader);
                if (!greeting.StartsWith(SmtpResponseCodes.ServiceReady))
                    throw new Exception($"SMTP server did not respond with 220. Response: {greeting}");

                await SendCommandAsync(writer, SmtpCommands.Ehlo);
                await ReadMultilineResponseAsync(reader);

                var context = new SecureStreamContextDto
                {
                    Stream = sslStream,
                    Reader = reader,
                    Writer = writer
                };

                await AuthenticateAsync(context, _settings.CurrentValue.Username, _settings.CurrentValue.Password);
                await SendEmailDataAsync(context, message);
            }
            else
            {
                // Establishes TCP connection with the SMTP server
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(_settings.CurrentValue.Host, int.Parse(_settings.CurrentValue.Port));

                using var networkStream = tcpClient.GetStream();
                using var plainReader = new StreamReader(networkStream, Encoding.ASCII);
                using var plainWriter = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };

                var greetingResponse = await ReadResponseAsync(plainReader);
                if (!greetingResponse.StartsWith(SmtpResponseCodes.ServiceReady))
                    throw new Exception($"SMTP server did not respond with 220 Service Ready. Response: {greetingResponse}");

                await SendCommandAsync(plainWriter, SmtpCommands.Ehlo);
                await ReadMultilineResponseAsync(plainReader);

                // Upgrade to secure connection using TLS/SSL
                await UpgradeToTlsConnectionAsync(plainWriter, plainReader);
                var secureStreamContext = await EstablishSecureConnectionAsync(networkStream, _settings.CurrentValue.Host);

                // Authenticates with Base64 credentials and transmits the email message with the sender and recipient data
                await AuthenticateAsync(secureStreamContext, _settings.CurrentValue.Username, _settings.CurrentValue.Password);
                await SendEmailDataAsync(secureStreamContext, message);
            }
        }

        /// <summary>
        /// Upgrades a plain network stream to a secure TLS/SSL stream using SslStream and performs a new EHLO handshake with the SMTP server.
        /// </summary>
        /// <param name="stream">The original plaintext NetworkStream connected to the SMTP server.</param>
        /// <param name="host">The SMTP server host, used during TLS authentication.</param>
        /// <returns>A SecureStreamContext that contains the SSL stream and the associated to it reader and writer.</returns>
        /// <exception cref="AuthenticationException">Thrown if the TLS handshake fails within AuthenticateAsClientAsync invocation.</exception>
        private async Task<SecureStreamContextDto> EstablishSecureConnectionAsync(NetworkStream stream, string host)
        {
            var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);

            try
            {
                await sslStream.AuthenticateAsClientAsync(host);
            }
            catch (Exception ex)
            {
                throw new AuthenticationException($"TLS handshake failed with host '{host}'.", ex);
            }

            var reader = new StreamReader(sslStream, Encoding.ASCII);
            var writer = new StreamWriter(sslStream, Encoding.ASCII) { AutoFlush = true };

            await SendCommandAsync(writer, SmtpCommands.Ehlo);
            await ReadMultilineResponseAsync(reader);

            return new SecureStreamContextDto
            {
                Stream = sslStream,
                Reader = reader,
                Writer = writer,
            };
        }

        private async Task UpgradeToTlsConnectionAsync(StreamWriter writer, StreamReader reader)
        {
            await SendCommandAsync(writer, SmtpCommands.StartTls);
            var startTlsResponse = await ReadResponseAsync(reader);

            if (!startTlsResponse.StartsWith(SmtpResponseCodes.ServiceReady))
                throw new Exception($"{nameof(SmtpCommands.StartTls)} failed");
        }

        private async Task SendEmailDataAsync(SecureStreamContextDto context, EmailMessageDto message)
        {
            await SendCommandAsync(context.Writer, SmtpCommands.MailFrom.Replace("%%email%%", message.From));
            var mailFromResponse = await ReadResponseAsync(context.Reader);

            if (!mailFromResponse.StartsWith(SmtpResponseCodes.Ok))
                throw new ArgumentException($"MAIL FROM rejected: {mailFromResponse}");

            await SendCommandAsync(context.Writer, SmtpCommands.RcptTo.Replace("%%email%%", message.To));
            var rcptToResponse = await ReadResponseAsync(context.Reader);

            if (!rcptToResponse.StartsWith(SmtpResponseCodes.Ok))
                throw new Exception($"RCPT TO rejected: {rcptToResponse}");

            await SendCommandAsync(context.Writer, SmtpCommands.Data);
            var dataResponse = await ReadResponseAsync(context.Reader);

            // 354 indicates that the client /localhost/ can start sending the data)
            if (!dataResponse.StartsWith(SmtpResponseCodes.StartMailInput))
                throw new Exception($"DATA command rejected: {dataResponse}");

            await SendEmailContentAsync(context, message);
            await TerminateConnectionAsync(context);
        }

        /// <summary>
        /// Authenticates the client using the SMTP AUTH LOGIN command with base64-encoded username and password.
        /// </summary>
        /// <param name="reader">The stream reader for reading SMTP server responses.</param>
        /// <param name="writer">The stream writer for sending authentication commands and credentials.</param>
        /// <param name="username">The SMTP username.</param>
        /// <param name="password">The SMTP password.</param>
        /// <exception cref="AuthenticationException">
        /// Thrown if the server does not prompt for username/password properly, or authentication fails.
        /// </exception>
        private async Task AuthenticateAsync(SecureStreamContextDto context, string username, string password)
        {
            await SendCommandAsync(context.Writer, SmtpCommands.AuthLogin);
            var usernamePrompt = await ReadResponseAsync(context.Reader);

            if (!usernamePrompt.StartsWith(SmtpResponseCodes.CredentialsPrompt))
                throw new AuthenticationException($"Expected {SmtpResponseCodes.CredentialsPrompt} Username prompt, got: {usernamePrompt}");

            await SendCommandAsync(context.Writer, Convert.ToBase64String(Encoding.UTF8.GetBytes(username)));
            var passwordPrompt = await ReadResponseAsync(context.Reader);

            if (!passwordPrompt.StartsWith(SmtpResponseCodes.CredentialsPrompt))
                throw new AuthenticationException($"Expected {SmtpResponseCodes.CredentialsPrompt} Password prompt, got: {passwordPrompt}");

            await SendCommandAsync(context.Writer, Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
            var authResponse = await ReadResponseAsync(context.Reader);

            if (!authResponse.StartsWith(SmtpResponseCodes.AuthSuccess))
                throw new AuthenticationException($"Authentication failed: {authResponse}.");
        }

        /// <summary>
        /// Sends the email headers and body to the SMTP server after receiving a 354 response to the DATA command (server is ready to accept the email data).
        /// </summary>
        /// <param name="writer">The stream writer used to write to the SMTP server.</param>
        /// <param name="message">The email message to be sent.</param>
        private async Task SendEmailContentAsync(SecureStreamContextDto context, EmailMessageDto message)
        {
            await context.Writer.WriteLineAsync($"Subject: {message.Subject}");
            await context.Writer.WriteLineAsync($"To: {message.To}");
            await context.Writer.WriteLineAsync(SmtpCommands.ContentTypePlainText);
            await context.Writer.WriteLineAsync(""); // Empty line to separate headers from body
            await context.Writer.WriteLineAsync(message.Body);
            await context.Writer.WriteLineAsync(SmtpCommands.EndOfLineIndicator); // Indicates end of message
            await context.Writer.FlushAsync();

            var contentAcceptedResponse = await ReadResponseAsync(context.Reader);

            if (!contentAcceptedResponse.StartsWith(SmtpResponseCodes.Ok))
                throw new Exception($"Email content was not accepted by the server: {contentAcceptedResponse}");
        }

        /// <summary>
        /// Terminates the connection with the SMTP server by using QUIT command.
        /// </summary>
        /// <param name="writer">The stream writer used to write to the SMTP server.</param>
        /// <param name="writer">The stream reader used to write to the SMTP server.</param>
        private async Task TerminateConnectionAsync(SecureStreamContextDto context)
        {
            await SendCommandAsync(context.Writer, SmtpCommands.Quit);
            var quitResponse = await ReadResponseAsync(context.Reader);

            if (!quitResponse.StartsWith(SmtpResponseCodes.Bye))
                throw new Exception($"Unexpected response after QUIT: {quitResponse}");
        }

        private async Task SendCommandAsync(StreamWriter writer, string command)
            => await writer.WriteLineAsync(command);

        private async Task<string> ReadResponseAsync(StreamReader reader)
        {
            var line = await reader.ReadLineAsync();

            return line ?? string.Empty;
        }

        /// <summary>
        /// Reads a multiline response from the SMTP server until the final line is received.
        /// </summary>
        /// <remarks>
        /// SMTP servers may return multiple lines for certain commands like EHLO,
        /// where each line starts with the same status code followed by a dash ("250-").
        /// The final line of the response starts with the status code followed by a space ("250 ").
        /// This method continues reading lines until it reaches that terminating line.
        /// </remarks>
        /// <param name="reader">The StreamReader connected to the SMTP server.</param>
        private async Task ReadMultilineResponseAsync(StreamReader reader)
        {
            string line;
            do
            {
                line = await reader.ReadLineAsync();
            }
            while (line != null && (line.Length < 4 || line[3] == '-'));
        }
    }
}
