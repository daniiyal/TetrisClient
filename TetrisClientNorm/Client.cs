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

        public IPEndPoint ServerEndPoint { get; set; }

        public Client()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
        }

        public void ConnectServer()
        {
            udpClient.Close();
            client.Connect(ServerEndPoint);
        }

        public async void SendMessageAsync(String message)
        {
            try
            {
                var messageBytes = Encoding.UTF8.GetBytes(message + '\n');
                await client.SendAsync(messageBytes, SocketFlags.None);
            }
            catch (Exception)
            {
                // ignored
            }
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
                catch (Exception)
                {
                    throw new SocketException();
                }

            }

            return Encoding.UTF8.GetString(response.ToArray());
        }

        public void DisconnectServer()
        {
            try
            {
                SendMessageAsync("Shutdown");
                client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {

            }
           
        }

        public async Task<List<string>> FindServer()
        {
            Task.Run(SendBroadcastMessageAsync);
            var address = await Task.Run(ReceiveMessage);
            return address;
        }

        private async Task SendBroadcastMessageAsync()
        {
            while (true)
            {
                try
                {
                    var data = Encoding.ASCII.GetBytes("Want to play");
                    udpClient.Send(data, data.Length, IPAddress.Broadcast.ToString(), 333);
                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
               
            }
        }

        private List<string> ReceiveMessage()
        {

            var from = new IPEndPoint(0, 0);

            while (true)
            {
                try
                {
                    var receiveBuffer = udpClient.Receive(ref from);

                    if (Encoding.ASCII.GetString(receiveBuffer) == "Come and play")
                    {
                        return new List<string>()
                            {
                                from.Address.ToString(),
                                from.Port.ToString()
                            };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }

        }

    }
}
