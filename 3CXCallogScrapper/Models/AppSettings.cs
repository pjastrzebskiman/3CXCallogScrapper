using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3CXCallogScrapper.Models
{
    class AppSettings
    {
        public ConnectionStrings? ConnectionStrings { get; set; }
        public ThreeCXApiSettings? ThreeCXApiSettings { get; set; }
    }
    public class ConnectionStrings
    {
        public string? PostgreSQL { get; set; }
    }

    public class ThreeCXApiSettings
    {
        public string? BaseUrl { get; set; }
        public string? AuthEndpoint { get; set; }
        public string? CallLogEndpoint { get; set; }
        public string? CallLogOldEndpoint { get; set; } // dodane pole
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SecurityCode { get; set; }
        public int QueryIntervalMinutes { get; set; } = 15;
        public int LookbackMinutes { get; set; } = 30;
        public bool GetOldCalls { get; set; } = false;
        public DateTime StartTimeGetOldCalls { get; set; } = DateTime.UtcNow.AddDays(-1);
    }
}
