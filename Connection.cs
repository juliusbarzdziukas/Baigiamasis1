using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Baigiamasis
{
    internal class Connection
    {
        public IPEndPoint EndPoint { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public CancellationTokenSource Listening { get; set; }
        private byte[] Buffer { get; set; }
        public int BufferSize { get; set; } = 65536;

        public User userInfo;

        public Connection(string iPAddress, int port, User info)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(iPAddress), port);
            Client = new TcpClient();
            Buffer = new byte[BufferSize];
            userInfo = info;
        }

        public void Connect(Action<Message> T)
        {
            try
            {
                Client.Connect(EndPoint);
                Stream = Client.GetStream();
                Listening = new CancellationTokenSource();
                Stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userInfo)));
                RX(T);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                Listening?.Cancel();
                Client?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void TX(string s)
        {
            try
            {
                var msg = new Message()
                {
                    UserId = userInfo.Nickname,
                    Msg = s,
                    UsersFunction = 0,
                    Users = new List<User>()
                };
                Stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async void RX(Action<Message> T)
        {
            try
            {
                int size;
                while (Client.Connected && !Listening.IsCancellationRequested)
                {
                    size = await Stream.ReadAsync(Buffer, Listening.Token);
                    var msg = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(Buffer));
                    T(msg);
                }
            }
            catch (OperationCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
