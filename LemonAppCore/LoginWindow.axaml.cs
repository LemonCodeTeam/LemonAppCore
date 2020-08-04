using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LemonAppCore.Helpers;

namespace LemonAppCore
{
    public class LoginWindow : Window
    {
        public MainWindow mw { get; set; }
        public LoginWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.Get<Button>("LoginBtn").Click += OnLoginBtnClick;
        }

        private void OnLoginBtnClick(object sender, RoutedEventArgs e) {
            string cookie = this.Get<TextBox>("cookies").Text;
            string qq = TextHelper.XtoYGetTo(cookie, "p_luin=o", ";", 0);
            var logindata = new LoginData();
            if (cookie.Contains("p_skey="))
            {
                string p_skey = TextHelper.XtoYGetTo(cookie + ";", "p_skey=", ";", 0);
                long hash = 5381;
                for (int i = 0; i < p_skey.Length; i++)
                {
                    hash += (hash << 5) + p_skey[i];
                }
                long g_tk = hash & 0x7fffffff;
                logindata.qq = qq;
                logindata.cookie = cookie;
                logindata.g_tk = g_tk.ToString();
            }
            //CallBack
            mw.Login(logindata);
        }
    }
}
