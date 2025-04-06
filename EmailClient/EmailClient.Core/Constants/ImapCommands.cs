namespace EmailClient.Core.Constants
{
    public static class ImapCommands
    {
        public const string SelectInbox = "SELECT INBOX";
        public const string SearchAll = "SEARCH ALL";

        public static string Login(string username, string password) =>
            $"LOGIN \"{username}\" \"{password}\"";

        public static string FetchDataPaginated(string[] paginatedEmailIds) =>
            $"FETCH {string.Join(",", paginatedEmailIds)} (BODY.PEEK[HEADER] BODY.PEEK[TEXT])";
    }
}
