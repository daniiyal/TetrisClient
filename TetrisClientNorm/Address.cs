using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisClientNorm
{
    public class Address
    {
        public string IpAddress { get; set; }
        public string Port { get; set; }

        public Address(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
