using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace _3CXCallogScrapper.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("Token")]
        public TokenInfo? Token { get; set; }

        [JsonPropertyName("TwoFactorAuth")]
        public object? TwoFactorAuth { get; set; }
    }

    public class TokenInfo
    {
        [JsonPropertyName("token_type")]
        public string? Token_type { get; set; }

        [JsonPropertyName("expires_in")]
        public int Expires_in { get; set; }

        [JsonPropertyName("access_token")]
        public string? Access_token { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? Refresh_token { get; set; }
    }
}
