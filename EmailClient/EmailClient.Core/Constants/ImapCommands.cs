namespace EmailClient.Core.Constants
{
    public static class ImapCommands
    {
        public const string SelectInbox = "SELECT INBOX";
        public const string SearchAll = "SEARCH ALL";

        public static string Login(string username, string password) =>
            $"LOGIN \"{username}\" \"{password}\"";

        public static string FetchDataPaginated(string[] ids, int max = 5) =>
            $"FETCH {string.Join(",", ids.Take(max))} (BODY.PEEK[HEADER] BODY.PEEK[TEXT])";
    }
}
