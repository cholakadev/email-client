using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
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

        public async Task SendEmailAsync(EmailMessage message)
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(_settings.CurrentValue.Host, int.Parse(_settings.CurrentValue.Port));

            using var networkStream = tcpClient.GetStream();
            using var plainReader = new StreamReader(networkStream, Encoding.ASCII);
            using var plainWriter = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };

            await ReadResponseAsync(plainReader); // 220 greeting
            await SendCommandAsync(plainWriter, "EHLO localhost");
            await ReadMultilineResponseAsync(plainReader); // capability list

            // Upgrade to TLS
            await SendCommandAsync(plainWriter, "STARTTLS");
            var startTlsResponse = await ReadResponseAsync(plainReader);
            if (!startTlsResponse.StartsWith("220")) throw new Exception("STARTTLS failed");

            var secureStreamContext = await EstablishSecureConnectionAsync(networkStream, _settings.CurrentValue.Host);
            await AuthenticateAsync(secureStreamContext.Reader, secureStreamContext.Writer, _settings.CurrentValue.Username, _settings.CurrentValue.Password);

            await SendEmailDataAsync(secureStreamContext.Reader, secureStreamContext.Writer, message);
        }

        private async Task<SecureStreamContext> EstablishSecureConnectionAsync(NetworkStream stream, string host)
        {
            var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
            await sslStream.AuthenticateAsClientAsync(host);

            var reader = new StreamReader(sslStream, Encoding.ASCII);
            var writer = new StreamWriter(sslStream, Encoding.ASCII) { AutoFlush = true };

            await SendCommandAsync(writer, "EHLO localhost");
            await ReadMultilineResponseAsync(reader);

            return new SecureStreamContext
            {
                Stream = sslStream,
                Reader = reader,
                Writer = writer,
            };
        }

        private async Task SendEmailDataAsync(StreamReader reader, StreamWriter writer, EmailMessage message)
        {
            await SendCommandAsync(writer, $"MAIL FROM:<{message.From}>");
            await ReadResponseAsync(reader);

            await SendCommandAsync(writer, $"RCPT TO:<{message.To}>");
            await ReadResponseAsync(reader);

            await SendCommandAsync(writer, "DATA");
            await ReadResponseAsync(reader); // Expect 354

            // Email headers + body
            await writer.WriteLineAsync($"Subject: {message.Subject}");
            await writer.WriteLineAsync($"To: {message.To}");
            await writer.WriteLineAsync("Content-Type: text/plain; charset=utf-8");
            await writer.WriteLineAsync("");
            await writer.WriteLineAsync(message.Body);
            await writer.WriteLineAsync(".");
            await writer.FlushAsync();

            await ReadResponseAsync(reader); // Expect 250

            await SendCommandAsync(writer, "QUIT");
            await ReadResponseAsync(reader); // Expect 221
        }

        private async Task AuthenticateAsync(StreamReader reader, StreamWriter writer, string username, string password)
        {
            await SendCommandAsync(writer, "AUTH LOGIN");
            await ReadResponseAsync(reader); // Expect 334 Username:

            await SendCommandAsync(writer, Convert.ToBase64String(Encoding.UTF8.GetBytes(username)));
            await ReadResponseAsync(reader); // Expect 334 Password:

            await SendCommandAsync(writer, Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
            var authResponse = await ReadResponseAsync(reader);

            if (!authResponse.StartsWith("235"))
                throw new Exception("Authentication failed: " + authResponse);
        }

        private async Task SendCommandAsync(StreamWriter writer, string command)
        {
            await writer.WriteLineAsync(command);
        }

        private async Task<string> ReadResponseAsync(StreamReader reader)
        {
            var line = await reader.ReadLineAsync();
            return line ?? string.Empty;
        }

        private async Task ReadMultilineResponseAsync(StreamReader reader)
        {
            string line;
            do line = await reader.ReadLineAsync();
            while (line != null && (line.Length < 4 || line[3] == '-'));
        }
    }
}
