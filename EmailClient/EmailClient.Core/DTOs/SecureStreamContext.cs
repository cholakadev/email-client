using System.Net.Security;

namespace EmailClient.Core.DTOs
{
    public class SecureStreamContext
    {
        public SslStream Stream { get; set; }

        public StreamWriter Writer { get; set; }

        public StreamReader Reader { get; set; }
    }
}
