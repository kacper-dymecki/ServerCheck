using System;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server_status
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private UdpClient _udpClient;

        private double UpdateInterval { get; set; }

        public async Task<bool> StatusAsync(string ip, int port)
        {
            
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try
            {
                System.Net.IPEndPoint ipEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port);
                byte[] datagram = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 }; // ÿÿÿÿTSource Engine Query
                await _udpClient.SendAsync(datagram, datagram.Length, ipEndPoint);
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                string[] parsedData = Encoding.ASCII.GetString(result.Buffer).Remove(0, 6).Split('\0');
                if(parsedData != null)
                {
                    _udpClient.Client.Shutdown(SocketShutdown.Both);
                    _udpClient = null;
                    return true;
                }
                else
                {
                    _udpClient.Client.Shutdown(SocketShutdown.Both);
                    _udpClient = null;
                    return false;
                }
            }
            catch(Exception exc)
            {
                // nie szpila
                return false;
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItem server in ServersToCheckList.Items)
            {
                string[] serverinfo = server.Content.ToString().Split(':');
                if (await StatusAsync(serverinfo[0], Convert.ToInt32(serverinfo[1])))
                {
                    server.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(130,0,255,0));
                }
                else
                {
                    server.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(130, 255, 0, 0));
                }
                i++;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = IpBox.Text + ":" + PortBox.Text;
            ServersToCheckList.Items.Add(item);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateInterval = UpdateSlider.Value;
        }
    }
}
