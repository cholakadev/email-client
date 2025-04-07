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
using EmailClient.Core.Constants;

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

            await reader.ReadLineAsync();

            // Login
            var loginTag = GetNextTag();
            var loginResponse = await SendCommandAsync(
                writer,
                reader,
                loginTag,
                ImapCommands.Login(_settings.CurrentValue.Username, _settings.CurrentValue.Password));

            if (!loginResponse.Contains("OK"))
                throw new Exception($"IMAP LOGIN failed: {loginResponse}.");

            // Select INBOX
            var inboxTag = GetNextTag();
            var inboxResponse = await SendCommandAsync(writer, reader, inboxTag, ImapCommands.SelectInbox);
            if (!inboxResponse.Contains("OK"))
                throw new Exception($"Failed to select inbox: {inboxResponse}.");

            // Search for all messages
            var searchTag = GetNextTag();
            var searchResponse = await SendCommandAsync(writer, reader, searchTag, ImapCommands.SearchAll);

            // Splits the multiline result and get the first line that contains the ids after * SEARCH
            // Skips 2 positions since they are '*' and 'SEARCH' and parses the others into array that contains the ids to be
            // used later in the FETCH command
            var emailIds = searchResponse
                .Split('\n')
                .FirstOrDefault(x => x.StartsWith("* SEARCH"))?
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(2)
                .ToArray();

            if (emailIds == null || !emailIds.Any())
                return emails;

            var paginatedEmailIds = emailIds
                .Skip(request.Skip)
                .Take(request.Take)
                .ToArray();

            var fetchTag = GetNextTag();
            var fetchResponse = await SendCommandAsync(
                writer,
                reader,
                fetchTag,
                ImapCommands.FetchDataPaginated(paginatedEmailIds));

            var lines = fetchResponse.Split('\n');
            EmailDto currentEmail = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("*") && line.Contains("FETCH"))
                {
                    if (currentEmail != null)
                        emails.Add(currentEmail);

                    currentEmail = new EmailDto();

                    continue;
                }
                else if (line.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
                {
                    currentEmail!.Subject = DecodeText(line, "Subject:");
                }
                else if (line.StartsWith("From:", StringComparison.OrdinalIgnoreCase))
                {
                    var decodedFrom = DecodeText(line, "From:");
                    currentEmail!.From = ExtractEmailAddress(decodedFrom);
                }
                else if (line.StartsWith("To:", StringComparison.OrdinalIgnoreCase))
                {
                    var decodedTo = DecodeText(line, "To:");
                    currentEmail!.To = ExtractEmailAddress(decodedTo);
                }
                else if (line.StartsWith("Date:", StringComparison.OrdinalIgnoreCase))
                {
                    currentEmail!.Date = DateTime.TryParse(line.Substring("Date:".Length).Trim(), out var date)
                        ? date
                        : DateTime.MinValue;
                }
            }

            if (currentEmail != null)
                emails.Add(currentEmail);

            return emails;
        }

        // Sending command and reads the server response
        private async Task<string> SendCommandAsync(StreamWriter writer, StreamReader reader, string tag, string command)
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

        private string DecodeText(string line, string prefix)
        {
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var raw = line.Substring(prefix.Length).Trim();
            var bytes = Encoding.ASCII.GetBytes(raw);

            var decodedText = Rfc2047.DecodeText(ParserOptions.Default, bytes);

            return decodedText;
        }
    }
}
