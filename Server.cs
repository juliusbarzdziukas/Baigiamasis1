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
    internal struct ClientStruct
    {
        public TcpClient Client;
        public byte[] Buffer;
        public User User;
    }

    internal class Server
    {
        public TcpListener Listener { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public CancellationTokenSource Listening { get; set; }
        public List<ClientStruct> Clients { get; set; }
        public int BufferSize { get; set; } = 65536;
        public List<User> Users { get; set; }

        public Server(int port, string ip = "127.0.0.1")
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Listener = new TcpListener(EndPoint);
            Clients = new List<ClientStruct>();
            Users = new List<User>();
        }
        public async void Start()
        {
            try
            {
                Listener.Start();
                Listening = new CancellationTokenSource();
                while (!Listening.IsCancellationRequested)
                {
                    //Other ways to implement
                    //ThreadPool.QueueUserWorkItem(HandleClient, Listener.AcceptTcpClient());
                    //new Thread(() => HandleClient(Listener.AcceptTcpClient()));
                    var client = await Listener.AcceptTcpClientAsync(Listening.Token).ConfigureAwait(false);
                    HandleClient(client);
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
        public void Stop()
        {
            try
            {
                Listening?.Cancel();
                foreach(var client in Clients)
                {
                    client.Client?.Close();
                }
                Clients.Clear();
                Listener?.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async void HandleClient(TcpClient client)
        {
            var con = new ClientStruct() {
                Buffer = new byte[BufferSize],
                Client = client
            };
            Clients.Add(con);
            Message msg;
            try
            {
                int size;
                var stream = client.GetStream();
                if (client.Connected && !Listening.IsCancellationRequested)
                {
                    size = await stream.ReadAsync(con.Buffer, Listening.Token);
                    con.User = JsonConvert.DeserializeObject<User>(Encoding.UTF8.GetString(con.Buffer));
                    Users.Add(con.User);
                    msg = new Message()
                    {
                        Msg = "",
                        UserId = "",
                        UsersFunction = 1,
                        Users = Users
                    };
                    foreach (var client1 in Clients)
                    {
// For demonstration echo
                        await client1.Client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)), Listening.Token);
                    }
                }
                while (client.Connected && !Listening.IsCancellationRequested)
                {
                    size = await stream.ReadAsync(con.Buffer, Listening.Token);
                    foreach (var client1 in Clients)
                    {
// For demonstration echo
                        await client1.Client.GetStream().WriteAsync(con.Buffer.AsMemory(0, size), Listening.Token);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Clients.Remove(con);
            Users.Remove(con.User);
            msg = new Message()
            {
                Msg = "",
                UserId = "",
                UsersFunction = 1,
                Users = Users
            };
            foreach (var client1 in Clients)
            {
                await client1.Client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)), Listening.Token);
            }
        }
    }
}
