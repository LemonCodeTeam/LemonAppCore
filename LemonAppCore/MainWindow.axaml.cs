using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LemonAppCore.Helpers;
using LemonAppCore.Items;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        Border GCBtn;
        Border XHBtn;
        #endregion
        #region Lyric
        TextBlock Lrc_Title;
        TextBlock Lrc_Singer;
        LyricView Lrc_LyricView;
        #endregion
        #region DataPanels
        VirtualizingStackPanel ResultListBox;
        VirtualizingStackPanel PlayListBox;
        VirtualizingStackPanel DownloadList;

        Grid MainPage;
        Grid LyricPage;
        #endregion
        #region Search
        ListBox SearchSmartSugBox;
        TextBox SearchBox;
        #endregion
        #region Download
        Button Dl_PauseBtn;
        Button Dl_CancelAllBtn;
        #endregion
        #region Tabs
        private TabItem MeTab;
        private TabItem SearchTab;
        private TabItem PlayListTab;
        private TabItem listTab;
        #endregion
        #endregion
        #region Loader
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
            MainWindow_Load();
        }

        private void MainWindow_Load()
        {
            this.Closing += MainWindow_Closing;
            PropertyChanged += MainWindow_PropertyChanged;

            #region Login&ReadSettings
            if (!Directory.Exists(Settings.CachePath))
                Directory.CreateDirectory(Settings.CachePath);
            if (!Directory.Exists(Settings.MusicCachePath))
                Directory.CreateDirectory(Settings.MusicCachePath);
            if (!Directory.Exists(Settings.MusicCachePath + "\\Image\\"))
                Directory.CreateDirectory(Settings.MusicCachePath + "\\Image\\");
            if (!Directory.Exists(Settings.DownloadPath))
                Directory.CreateDirectory(Settings.DownloadPath);
            if (!Directory.Exists(Settings.MusicCachePath + "\\Lyric\\"))
                Directory.CreateDirectory(Settings.MusicCachePath + "\\Lyric\\");
            Settings.Load();
            if (Settings.USettings.qq != string.Empty)
            {
                this.Get<Border>("UserImg").Background = new ImageBrush(new Bitmap(Settings.CachePath + Settings.USettings.qq + ".jpg"));
                this.Get<TextBlock>("UserName").Text = Settings.USettings.name;

                SyncBtn_OnClick(null, null);
            }
            this.Get<TextBlock>("UserName").Tapped += LoginBtn_Tapped;
            #endregion
            #region Lyric
            Lrc_Title = this.Get<TextBlock>("Lrc_Title");
            Lrc_Singer = this.Get<TextBlock>("Lrc_Singer");
            Lrc_LyricView = this.Get<LyricView>("Lrc_LyricView");
            Lrc_LyricView.mw = this;
            PropertyChanged += Lrc_LyricView.Mw_PropertyChanged;
            MusicImage = this.Get<Border>("MusicImage");
            MusicImage.Tapped += MusicImage_Tapped;
            #endregion 
            #region Tabs
            MeTab = this.Get<TabItem>("MeTab");
            SearchTab = this.Get<TabItem>("SearchTab");
            PlayListTab = this.Get<TabItem>("PlayListTab");
            listTab = this.Get<TabItem>("listTab");
            #endregion
            #region DataPanels
            ResultListBox = this.Get<VirtualizingStackPanel>("ResultListBox");
            PlayListBox = this.Get<VirtualizingStackPanel>("PlayListBox");
            DownloadList = this.Get<VirtualizingStackPanel>("DownloadList");

            MainPage = this.Get<Grid>("MainPage");
            LyricPage = this.Get<Grid>("LyricPage");
            #endregion
            #region Search
            this.Get<Button>("SearchBtn").Click += SearchBtn_Click;
            SearchSmartSugBox = this.Get<ListBox>("SearchSmartSugBox");
            SearchSmartSugBox.SelectionChanged += SearchSmartSugBox_SelectionChanged;
            SearchBox = this.Get<TextBox>("SearchBox");
            SearchBox.KeyUp += SearchBox_KeyUp;
            #endregion
            #region MusicPlay
            //---------------------Prepare to play---------
            PlayCallBack = new Action<MusicDataItem>((m) =>
            {
                PlayMusic(m);
                if (m.type == 0)
                {
                    PlayListBox.Children.Clear();
                    Settings.USettings.MusicGDataPlayList.Clear();
                    int index = 0;
                    foreach (MusicDataItem dt in ResultListBox.Children)
                    {
                        MusicDataItem md = new MusicDataItem(dt.m);
                        md.type = 1;
                        md.Width = Width - 20;
                        if (dt.m.MusicID == Playing.m.MusicID)
                        {
                            Playing = md;
                            md.Check(true);
                            Settings.USettings.PlayingIndex = index;
                        }
                        PlayListBox.Children.Add(md);
                        Settings.USettings.MusicGDataPlayList.Add(dt.m);
                        index++;
                    }
                }
                else
                {
                    Settings.USettings.PlayingIndex = PlayListBox.Children.IndexOf(Playing);
                }
            });
            t.Elapsed += T_Elapsed;
            t_Cleaner.Elapsed += delegate
            {
                GC.Collect();
            };
            t_Cleaner.Start();
            this.Get<Border>("LastBtn").Tapped += LastBtn_Tapped;
            this.Get<Border>("NextBtn").Tapped += NextBtn_Tapped;
            MusicTitle = this.Get<TextBlock>("MusicTitle");
            MSinger = this.Get<TextBlock>("MSinger");
            tTime_Now = this.Get<TextBlock>("tTime_Now");
            tTime_All = this.Get<TextBlock>("tTime_All");
            PlayBtn = this.Get<Border>("PlayBtn");
            PlayPath = this.Get<Avalonia.Controls.Shapes.Path>("PlayPath");
            jd = this.Get<Slider>("jd");

            XHBtn = this.Get<Border>("XHBtn");
            XHBtn.Tapped += XHBtn_Tapped;
            if (Settings.USettings.XHMode == 0)
            {
                (XHBtn.Child as Avalonia.Controls.Shapes.Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
            }
            else
            {
                (XHBtn.Child as Avalonia.Controls.Shapes.Path).Data = Geometry.Parse(Properties.Resources.Dqxh);
            }
            GCBtn = this.Get<Border>("GCBtn");
            //--------------Event Handler---------------------
            PlayBtn.Tapped += PlayBtn_Tapped;
            jd.AddHandler(PointerPressedEvent, delegate
            {
                CanJd = false;
            }, RoutingStrategies.Tunnel);
            jd.AddHandler(PointerReleasedEvent, delegate
            {
                mp.Position = TimeSpan.FromMilliseconds(jd.Value);
                CanJd = true;
            }, RoutingStrategies.Tunnel);
            //---------------------Load LastPlay--------------------
            PlayListBox.Children.Clear();
            int index = 0;
            foreach (Music dt in Settings.USettings.MusicGDataPlayList)
            {
                MusicDataItem md = new MusicDataItem(dt);
                md.type = 1;
                md.Width = Width - 20;
                if (index == Settings.USettings.PlayingIndex)
                {
                    Playing = md;
                    md.Check(true);
                    PlayMusic(md, false);
                }
                PlayListBox.Children.Add(md);
                index++;
            }
            #endregion
            #region Me
            this.Get<Button>("ILikeBtn").Click += ILikeBtn_OnClick;
            this.Get<Button>("SyncBtn").Click += SyncBtn_OnClick;
            Me_MyGDCreated = this.Get<WrapPanel>("Me_MyGDCreated");
            Me_MyGDLoved = this.Get<WrapPanel>("Me_MyGDLoved");
            #endregion
            #region Download
            DownloadCallBack = new Action<Music>(PushDownload);
            Dl_CancelAllBtn = this.Get<Button>("Dl_CancelAllBtn");
            Dl_PauseBtn = this.Get<Button>("Dl_PauseBtn");

            Dl_CancelAllBtn.Click += Dl_CancelAllBtn_Click;
            Dl_PauseBtn.Click += Dl_PauseBtn_Click;

            this.Get<Button>("Relb_DlAllBtn").Click += delegate {
                List<Music> m = new List<Music>();
                foreach (MusicDataItem md in ResultListBox.Children)
                    m.Add(md.m);
                PushDownload(m);
            };
            this.Get<Button>("Pllb_DlAllBtn").Click += delegate {
                List<Music> m = new List<Music>();
                foreach (MusicDataItem md in PlayListBox.Children)
                    m.Add(md.m);
                PushDownload(m);
            };
            #endregion
        }
        #endregion
        #region MainWindow
        private void MainWindow_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == WidthProperty)
            {
                WidthUI(Me_MyGDCreated);
                WidthUI(Me_MyGDLoved);
                WidthList(PlayListBox);
                WidthList(ResultListBox);
                WidthList(DownloadList);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Save();
            mp.Free();
        }
        public void WidthList(VirtualizingStackPanel e) {
            foreach (UserControl a in e.Children) {
                a.Width = Width - 20;
            }
        }
        public void WidthUI(Panel wp, double? ContentWidth = null)
        {
            if (wp.IsVisible && wp.Children.Count > 0)
            {
                int lineCount = int.Parse(wp.Tag.ToString());
                var uc = wp.Children[0] as UserControl;
                double max = uc.MaxWidth;
                double min = uc.MinWidth;
                ContentWidth = ContentWidth ?? Width-20;
                if (ContentWidth > (20 + max) * lineCount)
                    lineCount++;
                else if (ContentWidth < (20 + min) * lineCount)
                    lineCount--;
                WidTX(wp, lineCount, (double)ContentWidth);
            }
        }

        private void WidTX(Panel wp, int lineCount, double ContentWidth)
        {
            foreach (UserControl dx in wp.Children)
                dx.Width = (ContentWidth - 20 * lineCount) / lineCount;
        }
        #endregion
        #region Lyric
        bool IsLyricPage = false;
        private void MusicImage_Tapped(object sender, RoutedEventArgs e)
        {
            if (IsLyricPage)
            {
                IsLyricPage = false;
                MainPage.IsVisible = true;
                LyricPage.IsVisible = false;
            }
            else {
                IsLyricPage = true;
                MainPage.IsVisible = false;
                LyricPage.IsVisible = true;
            }
        }
        #endregion
        #region Search
        private void SearchSmartSugBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchSmartSugBox.SelectedItem != null && SearchSmartSugBox.SelectedItem is SSBox)
            {
                SearchBox.Text = (SearchSmartSugBox.SelectedItem as SSBox).content;
            }
        }
        private async void SearchBox_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                SearchBtn_Click(null, null);
            else
            {
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
                md.Width = Width - 20;
                md.type = 0;
                ResultListBox.Children.Add(md);
            }
            listTab.IsSelected = true;
        }
        #endregion
        #region PlayMusic & Control
        private void XHBtn_Tapped(object sender, RoutedEventArgs e)
        {
            if (Settings.USettings.XHMode == 0)
            {
                Settings.USettings.XHMode = 1;
                (XHBtn.Child as Avalonia.Controls.Shapes.Path).Data = Geometry.Parse(Properties.Resources.Dqxh);
            }
            else
            {
                Settings.USettings.XHMode = 0;
                (XHBtn.Child as Avalonia.Controls.Shapes.Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
            }
        }

        private void LastBtn_Tapped(object sender, RoutedEventArgs e)
        {
            MusicDataItem k;
            if (PlayListBox.Children.IndexOf(Playing) == 0)
                k = PlayListBox.Children[PlayListBox.Children.Count - 1] as MusicDataItem;
            else k = PlayListBox.Children[PlayListBox.Children.IndexOf(Playing) - 1] as MusicDataItem;
            k.Check(true);
            Settings.USettings.PlayingIndex = PlayListBox.Children.IndexOf(k);
            foreach (MusicDataItem dt in ResultListBox.Children)
                if (dt.m.MusicID == k.m.MusicID)
                {
                    dt.Check(true);
                    break;
                }

            PlayMusic(k);
        }
        private void NextBtn_Tapped(object sender, RoutedEventArgs e)
        {
            MusicDataItem k;
            if (PlayListBox.Children.IndexOf(Playing) + 1 == PlayListBox.Children.Count)
                k = PlayListBox.Children[0] as MusicDataItem;
            else k = PlayListBox.Children[PlayListBox.Children.IndexOf(Playing) + 1] as MusicDataItem;
            k.Check(true);
            Settings.USettings.PlayingIndex = PlayListBox.Children.IndexOf(k);
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
            else
            {
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
            Lrc_LyricView.LrcRoll(now, IsLyricPage);

            if (now == all && now > 2000 && all != 0)
            {
                //PLAY Finished
                if (Settings.USettings.XHMode == 0)
                    NextBtn_Tapped(null, null);
                else {
                    mp.Pause();
                    mp.Position = TimeSpan.FromSeconds(0);
                    mp.Play();
                }
            }
        }
        private MusicDataItem Playing;
        private async void PlayMusic(MusicDataItem m,bool onceplay=true)
        {
            var mData = m.m;
            Playing = m;
            t.Stop();
            mp.Pause();

            Bitmap im = await ImageCacheHelp.GetImageByUrl(mData.ImageUrl) ?? await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000");
            MusicImage.Background = new ImageBrush(im);
            MusicTitle.Text = Lrc_Title.Text = mData.MusicName;
            MSinger.Text = Lrc_Singer.Text = mData.SingerText;

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
            //----------LoadLyric------------
            string dt = await MusicLib.GetLyric(mData.MusicID);
            Lrc_LyricView.LoadLrc(dt);
            if (onceplay)
            {
                t.Start();
                mp.Play();
                PlayPath.Data = Geometry.Parse(Properties.Resources.Pause);
            }
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

            SyncBtn_OnClick(null, null);
        }
        #endregion
        #region Me & GDLoader
        private async void LoadGDByID(string id) {
            ResultListBox.Children.Clear();
            var data = await MusicLib.GetGDAsync(id, null, new Action<Music, bool>((m, b) => {
                MusicDataItem md = new MusicDataItem(m);
                md.type = 0;
                md.Width = Width - 20;
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
            WidthUI(Me_MyGDCreated);
            SortedDictionary<string, MusicGData> data1 = await MusicLib.GetGdILikeListAsync();
            Me_MyGDLoved.Children.Clear();
            foreach (var a in data1)
            {
                NormalItem n = new NormalItem(a.Value.name, a.Value.pic, a.Value);
                n.Tapped += GDTap_Tapped;
                n.Margin = new Thickness(10, 0, 10, 0);
                Me_MyGDLoved.Children.Add(n);
            }
            WidthUI(Me_MyGDLoved);
        }
        private void GDTap_Tapped(object sender, RoutedEventArgs e)
        {
            LoadGDByID(((sender as NormalItem).data as MusicGData).id);
        }
        #endregion
        #region Download

        private void Dl_CancelAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Dl_RunningCode == 2 || Dl_RunningCode == 1)
            {
                Dl_RunningCode = 0;
                Dl_Stop = true;
                DownloadList.Children.Clear();
            }
        }

        private void Dl_PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Dl_RunningCode == 2)
            {
                Dl_RunningCode = 1;
                Dl_PauseBtn.Content = "开始";
            }
            else if (Dl_RunningCode == 1)
            {
                Dl_RunningCode = 2;
                Dl_PauseBtn.Content = "暂停";
            }
        }

        public static Action<Music> DownloadCallBack;
        private void PushDownload(Music m) {
            DownloadItem di = new DownloadItem(m);
            di.Width = this.Width - 20;
            DownloadList.Children.Add(di);
            Dl_Stop = false;
            Dl_PauseBtn.Content = "暂停";
            if (Dl_RunningCode == 0)
            {
                //如果没有启动下载则启动
                Dl_RunningCode = 2;
                Dl_Start();
            }
            else if (Dl_RunningCode == 1)
                //如果是暂停则开始下载
                Dl_RunningCode = 2;
            //已经在下载就不用管了
        }
        private void PushDownload(List<Music> mx)
        {
            foreach (var m in mx)
            {
                DownloadItem di = new DownloadItem(m);
                di.Width = this.Width - 20;
                DownloadList.Children.Add(di);
            }
            Dl_Stop = false;
            Dl_PauseBtn.Content = "暂停";
            if (Dl_RunningCode == 0)
            {
                //如果没有启动下载则启动
                Dl_RunningCode = 2;
                Dl_Start();
            }
            else if (Dl_RunningCode == 1)
                //如果是暂停则开始下载
                Dl_RunningCode = 2;
            //已经在下载就不用管了
        }
        /// <summary>
        /// DownloadCode 0:Hasn't Start Yet
        /// 1:Pause  2:Running
        /// </summary>
        private int Dl_RunningCode = 0;
        private int Dl_DownloadIndex = 0;
        private bool Dl_Stop = false;
        private  void Dl_Start() {
            Dl_Download(Dl_DownloadIndex);
        }

        private async void Dl_Download(int index) {
            if (DownloadList.Children.Count == index) {
                //download all finished.
                Dl_RunningCode = 0;
                Dispatcher.UIThread.Post(() =>
                {
                    Dl_PauseBtn.Content = "开始";
                });
                return;
            }
            if ((DownloadList.Children[index] as DownloadItem).code != 3)
            {
                var m = DownloadList.Children[index] as DownloadItem;
                string nick= m.data.MusicName + " - " + m.data.SingerText;
                string Path = Settings.DownloadPath + nick.MakeValidFileName()+".mp3";
                await Task.Run(async () =>
                {
                    string Url = await MusicLib.GetUrlAsync(m.data.MusicID);
                    HttpWebRequest Myrq = (HttpWebRequest)WebRequest.Create(Url);
                    Myrq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                    Myrq.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                    Myrq.Headers.Add("Cache-Control", "max-age=0");
                    Myrq.KeepAlive = true;
                    Myrq.Headers.Add("Cookie", Settings.USettings.cookies);
                    Myrq.Host = "aqqmusic.tc.qq.com";
                    Myrq.Headers.Add("Upgrade-Insecure-Requests", "1");
                    Myrq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36 Edg/80.0.361.40";
                    var myrp = (HttpWebResponse)Myrq.GetResponse();
                    Console.WriteLine(myrp.StatusCode.ToString());
                    var totalBytes = myrp.ContentLength;
                    Dispatcher.UIThread.Post(() =>
                    {
                        //Size:
                        m.size.Text = Getsize(totalBytes);
                    });
                    Stream st = myrp.GetResponseStream();
                    Stream so = new FileStream(Path, FileMode.Create);
                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1048576];
                    int osize = await st.ReadAsync(by, 0, (int)by.Length);
                    while (osize > 0)
                    {
                        if (Dl_Stop) break;
                        if (Dl_RunningCode==2)
                        {
                            totalDownloadedByte = osize + totalDownloadedByte;
                            await so.WriteAsync(by, 0, osize);
                            osize = await st.ReadAsync(by, 0, (int)by.Length);
                            int Progress = (int)((float)totalDownloadedByte / (float)totalBytes * 100);
                            // Fresh Progress
                            Dispatcher.UIThread.Post(() => {
                                m.pro.Value = Progress;
                            });
                        }
                    }
                    st.Close();
                    so.Close();
                    myrp.Close();
                    //Finished&Stopped
                    if (!Dl_Stop)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            m.pro.Value = 0;
                            m.code = 3;
                            m.size.Text += "  Finished";
                        });
                        Dl_DownloadIndex++;
                        Dl_Download(Dl_DownloadIndex);
                    }
                });
            }
            else
            {
                Dl_DownloadIndex++;
                Dl_Download(Dl_DownloadIndex);
            }
        }
        private string Getsize(double size)
        {
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return size.ToString("0.00") + units[i];
        }
        #endregion 
    }
}
