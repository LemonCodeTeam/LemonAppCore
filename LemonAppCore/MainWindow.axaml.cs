using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LemonAppCore.Helpers;
using LemonAppCore.Items;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LemonAppCore
{
    public class MainWindow : Window
    {
        #region Controls
        #region Me
        WrapPanel Me_MyGDCreated;
        WrapPanel Me_MyGDLoved;
        #endregion
        #region Play
        MusicPlayer mp = new MusicPlayer(IntPtr.Zero);
        MyTimer t = new MyTimer() { Interval = 1000 };
        MyTimer t_Cleaner = new MyTimer() { Interval = 5000 };
        Border MusicImage;
        TextBlock MusicTitle;
        TextBlock MSinger;
        TextBlock tTime_Now;
        TextBlock tTime_All;
        Border PlayBtn;
        Slider jd;
        Avalonia.Controls.Shapes.Path PlayPath;
        #endregion
        #region DataPanels
        StackPanel ResultListBox;
        StackPanel PlayListBox;
        #endregion
        #region Search
        ListBox SearchSmartSugBox;
        TextBox SearchBox;
        #endregion
        #region Tabs
        private TabItem MeTab;
        private TabItem FindTab;
        private TabItem SearchTab;
        private TabItem PlayListTab;
        private TabItem listTab;
        #endregion
        #endregion
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            #region Login&ReadSettings
            if (!Directory.Exists(Settings.CachePath))
                Directory.CreateDirectory(Settings.CachePath);
            if (!Directory.Exists(Settings.MusicCachePath))
                Directory.CreateDirectory(Settings.MusicCachePath);
            if (!Directory.Exists(Settings.MusicCachePath+"\\Image\\"))
                Directory.CreateDirectory(Settings.MusicCachePath+"\\Image\\");
            Settings.Load();
            if (Settings.USettings.qq != string.Empty)
            {
                this.Get<Border>("UserImg").Background = new ImageBrush(new Bitmap(Settings.CachePath + Settings.USettings.qq + ".jpg"));
                this.Get<TextBlock>("UserName").Text = Settings.USettings.name;

                SyncBtn_OnClick(null, null);
            }
            this.Get<TextBlock>("UserName").Tapped += LoginBtn_Tapped;
            #endregion
            #region Tabs
            MeTab = this.Get<TabItem>("MeTab");
            FindTab = this.Get<TabItem>("FindTab");
            SearchTab = this.Get<TabItem>("SearchTab");
            PlayListTab = this.Get<TabItem>("PlayListTab");
            listTab = this.Get<TabItem>("listTab");
            #endregion
            #region DataPanels
            ResultListBox = this.Get<StackPanel>("ResultListBox");
            PlayListBox = this.Get<StackPanel>("PlayListBox");
            #endregion
            #region Search
            this.Get<Button>("SearchBtn").Click += SearchBtn_Click;
            SearchSmartSugBox = this.Get<ListBox>("SearchSmartSugBox");
            SearchSmartSugBox.SelectionChanged += SearchSmartSugBox_SelectionChanged;
            SearchBox = this.Get<TextBox>("SearchBox");
            SearchBox.KeyUp += SearchBox_KeyUp;
            #endregion
            #region MusicPlay
            PlayCallBack = new Action<MusicDataItem>((m) =>
            {
                PlayMusic(m);
                if (m.type == 0)
                {
                    PlayListBox.Children.Clear();
                    foreach (MusicDataItem dt in ResultListBox.Children)
                    {
                        MusicDataItem md = new MusicDataItem(dt.m);
                        md.type = 1;
                        if (dt.m.MusicID == Playing.m.MusicID)
                        {
                            Playing = md;
                            md.Check(true);
                        }
                        PlayListBox.Children.Add(md);
                    }
                }
            });
            t.Elapsed += T_Elapsed;
            t_Cleaner.Elapsed += delegate{
                GC.Collect();
            };
            t_Cleaner.Start();
            this.Get<Border>("LastBtn").Tapped += LastBtn_Tapped;
            this.Get<Border>("NextBtn").Tapped += NextBtn_Tapped;
            MusicImage = this.Get<Border>("MusicImage");
            MusicTitle = this.Get<TextBlock>("MusicTitle");
            MSinger = this.Get<TextBlock>("MSinger");
            tTime_Now = this.Get<TextBlock>("tTime_Now");
            tTime_All = this.Get<TextBlock>("tTime_All");
            PlayBtn = this.Get<Border>("PlayBtn");
            PlayPath = this.Get<Avalonia.Controls.Shapes.Path>("PlayPath");
            jd = this.Get<Slider>("jd");
            //--------------Event Handler---------------------
            PlayBtn.Tapped += PlayBtn_Tapped;
            jd.AddHandler(PointerPressedEvent, delegate {
                CanJd = false;
            },RoutingStrategies.Tunnel);
            jd.AddHandler(PointerReleasedEvent,delegate {
                mp.Position = TimeSpan.FromMilliseconds(jd.Value);
                CanJd = true;
            },RoutingStrategies.Tunnel);
            #endregion
            this.Get<Button>("ILikeBtn").Click += ILikeBtn_OnClick;
            this.Get<Button>("SyncBtn").Click += SyncBtn_OnClick;
            Me_MyGDCreated = this.Get<WrapPanel>("Me_MyGDCreated");
            Me_MyGDLoved = this.Get<WrapPanel>("Me_MyGDLoved");
        }
        #region Search
        private void SearchSmartSugBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchSmartSugBox.SelectedItem != null&& SearchSmartSugBox.SelectedItem is SSBox) {
                SearchBox.Text = (SearchSmartSugBox.SelectedItem as SSBox).content;
            }
        }
        private async void SearchBox_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                SearchBtn_Click(null, null);
            else {
                if (SearchBox.Text != null)
                    SearchSmartSugBox.Items = await MusicLib.Search_SmartBoxAsync(SearchBox.Text);
            }
        }
        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = SearchBox.Text;
            var data = await MusicLib.SearchMusicAsync(text);
            ResultListBox.Children.Clear();
            foreach (var dt in data)
            {
                MusicDataItem md = new MusicDataItem(dt);
                md.type = 0;
                ResultListBox.Children.Add(md);
            }
            listTab.IsSelected = true;
        }
        #endregion
        #region PlayMusic
        private void LastBtn_Tapped(object sender, RoutedEventArgs e) {
            MusicDataItem k;
            if (PlayListBox.Children.IndexOf(Playing) ==0)
                k = PlayListBox.Children[PlayListBox.Children.Count-1] as MusicDataItem;
            else k = PlayListBox.Children[PlayListBox.Children.IndexOf(Playing) - 1] as MusicDataItem;
            k.Check(true);
            foreach (MusicDataItem dt in ResultListBox.Children)
                if (dt.m.MusicID == k.m.MusicID)
                {
                    dt.Check(true);
                    break;
                }

            PlayMusic(k);
        }
        private void NextBtn_Tapped(object sender, RoutedEventArgs e) {
            MusicDataItem k;
            if (PlayListBox.Children.IndexOf(Playing) + 1 == PlayListBox.Children.Count)
                k = PlayListBox.Children[0] as MusicDataItem;
            else k = PlayListBox.Children[PlayListBox.Children.IndexOf(Playing) + 1] as MusicDataItem;
            k.Check(true);
            foreach (MusicDataItem dt in ResultListBox.Children)
                if (dt.m.MusicID == k.m.MusicID)
                {
                    dt.Check(true);
                    break;
                }
            PlayMusic(k);
        }
        private void PlayBtn_Tapped(object sender, RoutedEventArgs e)
        {
            if (t.Enabled)
            {
                t.Stop();
                mp.Pause();
                PlayPath.Data = Geometry.Parse(Properties.Resources.Play);
            }
            else {
                t.Start();
                mp.Play();
                PlayPath.Data = Geometry.Parse(Properties.Resources.Pause);
            }
        }

        bool CanJd = true;
        private void T_Elapsed(object sender, EventArgs e)
        {
            var now = mp.Position.TotalMilliseconds;
            if (CanJd)
            {
                jd.Value = now;
                tTime_Now.Text = TimeSpan.FromMilliseconds(now).ToString(@"mm\:ss");
            }
            var all = mp.GetLength.TotalMilliseconds;
            string alls = TimeSpan.FromMilliseconds(all).ToString(@"mm\:ss");
            tTime_All.Text = alls;
            jd.Maximum = all;

            if (now == all && now > 2000 && all != 0) {
                //PLAY Finished
                NextBtn_Tapped(null, null);
            }
        }
        private MusicDataItem Playing;
        private async void PlayMusic(MusicDataItem m) {
            var mData = m.m;
            Playing = m;
            t.Stop();
            mp.Pause();

            Bitmap im = await ImageCacheHelp.GetImageByUrl(mData.ImageUrl) ?? await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000");
            MusicImage.Background = new ImageBrush(im);
            MusicTitle.Text = mData.MusicName;
            MSinger.Text = mData.SingerText;

            string downloadpath = Settings.MusicCachePath + mData.MusicID + ".mp3";
            if (File.Exists(downloadpath))
            {
                mp.Load(downloadpath);
            }
            else
            {
                var musicurl = await MusicLib.GetUrlAsync(mData.MusicID);
                mp.LoadUrl(downloadpath, musicurl, null, null);
            }
            t.Start();
            mp.Play();
            PlayPath.Data = Geometry.Parse(Properties.Resources.Pause);
        }
        public static Action<MusicDataItem> PlayCallBack;
        #endregion
        #region Login

        private void LoginBtn_Tapped(object sender, RoutedEventArgs e)
        {
            var loginw = new LoginWindow() { mw = this };
            loginw.Show();
        }

        public async void Login(LoginData ld) {
            Settings.USettings.qq = ld.qq;
            Settings.USettings.cookies = ld.cookie;
            Settings.USettings.g_tk = ld.g_tk;

            var sl = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={ld.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={ld.qq}&reqfrom=1&reqtype=0", Encoding.UTF8);
            var sdc = JObject.Parse(sl)["data"]["creator"];
            var imgpath = Settings.CachePath + ld.qq + ".jpg";
            await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString().Replace("http://", "https://"), imgpath);
            string name = sdc["nick"].ToString();
            this.Get<Border>("UserImg").Background = new ImageBrush(new Bitmap(imgpath));
            this.Get<TextBlock>("UserName").Text = name;
            Settings.USettings.name = name;
            Settings.Save();
        }
        #endregion

        private async void LoadGDByID(string id) {
            ResultListBox.Children.Clear();
            var data = await MusicLib.GetGDAsync(id, null, new Action<Music, bool>((m, b) => {
                MusicDataItem md = new MusicDataItem(m);
                md.type = 0;
                ResultListBox.Children.Add(md);
            }));
            listTab.IsSelected = true;
        }
        private async void ILikeBtn_OnClick(object sender, RoutedEventArgs e) {
            var id = await MusicLib.GetMusicLikeGDid();
            LoadGDByID(id);
        }
        private async void SyncBtn_OnClick(object sender, RoutedEventArgs e)
        {
            SortedDictionary<string, MusicGData> data = await MusicLib.GetGdListAsync();
            Me_MyGDCreated.Children.Clear();
            foreach (var a in data) {
                NormalItem n = new NormalItem(a.Value.name,a.Value.pic,a.Value);
                n.Tapped += GDTap_Tapped;
                n.Margin = new Thickness(10, 0, 10, 0);
                Me_MyGDCreated.Children.Add(n);
            }

            SortedDictionary<string, MusicGData> data1 = await MusicLib.GetGdILikeListAsync();
            Me_MyGDLoved.Children.Clear();
            foreach (var a in data1)
            {
                NormalItem n = new NormalItem(a.Value.name, a.Value.pic, a.Value);
                n.Tapped += GDTap_Tapped;
                n.Margin = new Thickness(10, 0, 10, 0);
                Me_MyGDLoved.Children.Add(n);
            }
        }

        private void GDTap_Tapped(object sender, RoutedEventArgs e)
        {
            LoadGDByID(((sender as NormalItem).data as MusicGData).id);
        }
    }
}
