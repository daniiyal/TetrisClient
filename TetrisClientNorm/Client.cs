using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Accessibility;

namespace TetrisClientNorm
{

    public class Client
    {
        Socket client;
        
        public Client()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void ConnectServer(String ip, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client.Connect(ipEndPoint);
        }

        public async void SendMessage(String message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message + '\n');
            await client.SendAsync(messageBytes, SocketFlags.None);
        }

        public string ReceiveResponse()
        {
            var response = new List<byte>();
            var bytesRead = new byte[1];

            while (true)
            {
                var count = client.Receive(bytesRead);
                if (count == 0 || bytesRead[0] == '\n') break;

                response.Add(bytesRead[0]);
            }

            return Encoding.UTF8.GetString(response.ToArray());
        }

        public void DisconnectServer()
        {
            client.Shutdown(SocketShutdown.Both);
        }

    }
}
