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
        public List<User> userList;

        public Connection(string iPAddress, int port, User info)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(iPAddress), port);
            Client = new TcpClient();
            Buffer = new byte[BufferSize];
            userList = new List<User>();
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
                    Users = null
                };
                var msgs = JsonConvert.SerializeObject(msg);
                Stream.Write(Encoding.UTF8.GetBytes(msgs), 0, msgs.Length);
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
                    var msg = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(Buffer, 0, size));
                    switch (msg.UsersFunction)
                    {
                        case 0:
                            break;
                        case 1:
                            userList = msg.Users;
                            break;
                        case 2:
                            foreach (var user in msg.Users)
                            {
                                userList.Add(user);
                            }
                            break;
                        case 3:
                            foreach (var user in msg.Users)
                            {
                                userList.Remove(user);
                            }
                            break;
                    }
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
