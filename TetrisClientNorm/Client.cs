using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TetrisClientNorm
{

    public class Client
    {
        private Socket client;

        private UdpClient udpClient;

        public IPEndPoint serverEndPoint { get; set; }

        public Client()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        }

        public void ConnectServer()
        {
            udpClient.Close();
            client.Connect(serverEndPoint);
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
                try
                {
                    var count = client.Receive(bytesRead);
                    if (count == 0 || bytesRead[0] == '\n') break;

                    response.Add(bytesRead[0]);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }

            return Encoding.UTF8.GetString(response.ToArray());
        }

        public void DisconnectServer()
        {
            client.Shutdown(SocketShutdown.Both);
        }

        public async Task<List<string>> FindServer()
        {
            //List<string> address;
            Task.Run(SendMessageAsync);
            var address = await ReceiveMessage();
            return address;
        }

        private async Task SendMessageAsync()
        {
            while (true)
            {
                var data = Encoding.ASCII.GetBytes("Want to play");
                udpClient.Send(data, data.Length, IPAddress.Broadcast.ToString(), 333);
                await Task.Delay(1000);
            }
        }

        private async Task<List<string>> ReceiveMessage()
        {
            var from = new IPEndPoint(0, 0);

          
            while (true)
            {
                var receiveBUffer = udpClient.Receive(ref from);
                
                Console.WriteLine(Encoding.ASCII.GetString(receiveBUffer));

                if (receiveBUffer != null)
                {
                    return new List<string>()
                    {
                        from.Address.ToString(),
                        from.Port.ToString()
                    };
                }
           
            }
        }

    }
}
