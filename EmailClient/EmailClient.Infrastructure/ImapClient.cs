using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
using EmailClient.Core.Requests;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using MimeKit;
using MimeKit.Utils;
using System.Text.RegularExpressions;

namespace EmailClient.Infrastructure
{
    public class ImapClient : IImapClient
    {
        private int _tagCounter = 0;
        private readonly IOptionsMonitor<ImapSettings> _settings;

        public ImapClient(IOptionsMonitor<ImapSettings> settings)
        {
            _settings = settings;
        }

        // TODO: Extract the IMAP commands into separate static class with constants to avoid magic strings across the app.
        public async Task<List<EmailDto>> ReadEmailsAsync(GetEmailsRequest request)
        {
            var settings = _settings.CurrentValue;
            using var client = new TcpClient();
            await client.ConnectAsync(settings.Host, int.Parse(settings.Port));

            using var ssl = new SslStream(client.GetStream(), false);
            await ssl.AuthenticateAsClientAsync(settings.Host);

            using var reader = new StreamReader(ssl, Encoding.ASCII);
            using var writer = new StreamWriter(ssl, Encoding.ASCII) { AutoFlush = true };

            var emails = new List<EmailDto>();

            var tag = 0;
            string NextTag() => $"a{tag++:D3}";

            // Initial greeting
            await reader.ReadLineAsync();

            // Login
            var loginTag = NextTag();
            var loginResponse = await SendCommand(
                writer,
                reader,
                loginTag,
                $"LOGIN {_settings.CurrentValue.Username} {_settings.CurrentValue.Password}");

            if (!loginResponse.Contains("OK"))
                throw new Exception($"IMAP LOGIN failed: {loginResponse}.");

            // Select INBOX
            var inboxTag = NextTag();
            var inboxResponse = await SendCommand(writer, reader, inboxTag, "SELECT INBOX");
            if (!inboxResponse.Contains("OK"))
                throw new Exception("Failed to select inbox: " + inboxResponse);

            // Search for all messages
            var searchTag = NextTag();
            var searchResponse = await SendCommand(writer, reader, searchTag, "SEARCH ALL");

            // Splits the multiline result and get the first line that contains the ids after * SEARCH
            // Skips 2 positions since they are '*' and 'SEARCH' and parses the others into array that contains the ids to be
            // used later in the FETCH command
            var ids = searchResponse
                .Split('\n')
                .FirstOrDefault(x => x.StartsWith("* SEARCH"))?
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(2)
                .ToArray();

            if (ids == null || !ids.Any())
                return emails;

            var fetchTag = NextTag();
            var fetchResponse = await SendCommand(
                writer,
                reader,
                fetchTag,
                $"FETCH {string.Join(",", ids.Take(5))} (BODY[HEADER.FIELDS (SUBJECT FROM DATE)])");

            var lines = fetchResponse.Split('\n');
            var currentEmail = new EmailDto();

            foreach (var line in lines)
            {
                if (line.StartsWith("*") && line.Contains("FETCH"))
                {
                    if (currentEmail != null) emails.Add(currentEmail);
                    currentEmail = new EmailDto();
                }
                else if (line.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
                {
                    var raw = line.Substring("Subject:".Length).Trim();
                    var bytes = Encoding.ASCII.GetBytes(raw);
                    currentEmail.Subject = Rfc2047.DecodeText(ParserOptions.Default, bytes);
                }
                else if (line.StartsWith("From:", StringComparison.OrdinalIgnoreCase))
                {
                    var raw = line.Substring("From:".Length).Trim();
                    var bytes = Encoding.ASCII.GetBytes(raw);
                    var decodedFrom = Rfc2047.DecodeText(ParserOptions.Default, bytes);
                    currentEmail.From = ExtractEmailAddress(decodedFrom);
                }
                else if (line.StartsWith("Date:", StringComparison.OrdinalIgnoreCase))
                {
                    currentEmail.Date = DateTime.TryParse(line.Substring("Date:".Length).Trim(), out var date)
                        ? date
                        : DateTime.MinValue;
                }
            }

            if (currentEmail != null) emails.Add(currentEmail);

            return emails;
        }

        // Sending command and reads the server response
        private async Task<string> SendCommand(StreamWriter writer, StreamReader reader, string tag, string command)
        {
            await writer.WriteLineAsync($"{tag} {command}");
            var response = new StringBuilder();
            string line;
            do
            {
                line = await reader.ReadLineAsync();
                response.AppendLine(line);
            } while (line != null && !line.StartsWith(tag));

            return response.ToString();
        }

        // Generates a new tag sequentially with prefix 'a' and 3 digits after
        private string GetNextTag() => $"a{_tagCounter++:D3}";

        private string ExtractEmailAddress(string fromHeader)
        {
            var match = Regex.Match(fromHeader, @"<([^>]+)>");
            return match.Success ? match.Groups[1].Value : fromHeader;
        }
    }
}
