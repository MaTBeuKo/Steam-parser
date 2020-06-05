using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SteamParse.Properties;

namespace SteamParse
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            apiKeyInput.Password = Settings.Default.ApiKey;

            if (apiKeyInput.Password == "")
                ApiLink.Visibility = Visibility.Visible;
        }
        private async void StartClick(object sender, RoutedEventArgs e)
        {
            if (steamidinput.Text.Contains("76561"))
            {
                ShowPlayerSumm(steamidinput.Text);
            }
            else
            {
                string buffer;
                using (WebClient wc = new WebClient())
                    buffer = await wc.DownloadStringTaskAsync($"https://steamcommunity.com/id/{steamidinput.Text}/?xml=1");
                Match match = Regex.Match(buffer, "<steamID64>(.*?)</steamID64>");
                ShowPlayerSumm(match.Groups[1].Value);
            }
        }

        private async void ShowPlayerSumm(string id)
        {
            string buffer;
            byte[] ar;
            byte[] str;

            try
            {
                using (WebClient wc = new WebClient())
                    str = await wc.DownloadDataTaskAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKeyInput.Password}&steamids={id})".Replace(" ", ""));
            }
            catch
            {
                str = null;
                MessageBox.Show("ApiKey doesn't exist or Steam servers are down");
            }

            if (str != null)
            {
                buffer = Encoding.UTF8.GetString(str, 0, str.Length - 1);
                Match[] match = new Match[3];
                match[0] = Regex.Match(buffer, @"steamid\u0022:\u0022(.*?)\u0022");
                match[1] = Regex.Match(buffer, @"personaname\u0022:\u0022(.*?)\u0022");
                match[2] = Regex.Match(buffer, @"full\u0022:\u0022(.*?)full");

                if (match[1].Groups[1].Value == "" && match[0].Groups[1].Value == "")
                {
                    MessageBox.Show("User not found");
                    return;
                }
                Steamname.Content = "Name: " + match[1].Groups[1].Value;
                steamid.Content = "ID: " + match[0].Groups[1].Value;
                using (WebClient wc = new WebClient())
                    ar = await wc.DownloadDataTaskAsync(match[2].Groups[1].Value + "full.jpg");
                BitmapImage biImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(ar);
                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.EndInit();

                ImageSource imgSrc = biImg as ImageSource;

                avatar.Source = imgSrc;
                Settings.Default.ApiKey = apiKeyInput.Password;
                Settings.Default.Save();
            }
        }

        private void ApiLink_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://steamcommunity.com/dev/apikey");
            ApiLink.Visibility = Visibility.Hidden;
        }

        private void steamidinput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (steamidinput.Text == "Your steamID")
                steamidinput.Text = "";
        }
    }
}
