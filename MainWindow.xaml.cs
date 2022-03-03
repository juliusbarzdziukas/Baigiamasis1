using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Baigiamasis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Settings SettingsWindow { get; set; }
        private Connection? ConnectionObj { get; set; }
        private Server? ServerObj { get; set; }
        private User UserInfo;

        public MainWindow()
        {
            InitializeComponent();
            SettingsWindow = new Settings();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (!(ConnectionObj is null))
            {
                ConnectionObj.TX(EditBox.Text);
                EditBox.Text = "";
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Show();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConnectionObj is null)
                {
                    UserInfo.Nickname = SettingsWindow.Name.Text;
                    ConnectionObj = new Connection(SettingsWindow.IP.Text, Convert.ToInt32(SettingsWindow.Port.Text), UserInfo);
                    if (SettingsWindow.Server.IsChecked == true)
                    {
                        ServerObj = new Server(Convert.ToInt32(SettingsWindow.Port.Text), SettingsWindow.IP.Text);
                        ServerObj.Start();
                    }
                    ConnectionObj.Connect(msg =>
                    {
                        switch (msg.UsersFunction)
                        {
                            case 0:
                                MainChat.AppendText($"{msg.UserId}:{msg.Msg}\r\n");
                                break;
                            case 1:
                                Users.Clear();
                                foreach (var user in ConnectionObj.userList)
                                {
                                    Users.AppendText($"{user.Nickname}\r\n");
                                }
                                break;
                            case 2:
                                foreach (var user in msg.Users)
                                {
                                    Users.AppendText($"{user.Nickname}\r\n");
                                }
                                break;
                            case 3:
                                Users.Clear();
                                foreach (var user in ConnectionObj.userList)
                                {
                                    Users.AppendText($"{user.Nickname}\r\n");
                                }
                                break;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EditBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(ConnectionObj is null))
            {
                if (e.Key == Key.Enter)
                {
                    ConnectionObj.TX(EditBox.Text);
                    EditBox.Text = "";
                }
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (!(ConnectionObj is null)) 
            {
                ConnectionObj.Disconnect();
                ConnectionObj = null;
            }
            if(!(ServerObj is null))
            {
                ServerObj.Stop();
                ServerObj = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SettingsWindow.Close();
        }
    }
}
