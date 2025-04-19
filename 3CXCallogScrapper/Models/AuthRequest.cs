using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3CXCallogScrapper.Models
{
    public class AuthRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? SecurityCode { get; set; }
    }
}
