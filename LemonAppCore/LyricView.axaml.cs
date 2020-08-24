using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using LemonAppCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LemonAppCore
{
    public class LyricView : UserControl
    {
        public delegate void NextData(string text,string lyric);
        public event NextData NextLyric;
        #region 
        public class LrcModel
        {
            public TextBlock c_LrcTb { get; set; }
            public string LrcText { get; set; }
            public string LrcTransText { get; set; }
            public double Time { get; set; }
        }
        #endregion
        #region 
        public Dictionary<double, LrcModel> Lrcs = new Dictionary<double, LrcModel>();
        public LrcModel foucslrc { get; set; }

        public SolidColorBrush NoramlLrcColor = new SolidColorBrush(Color.Parse("#404040"));
        public SolidColorBrush FoucsLrcColor = new SolidColorBrush(Color.Parse("#6699FF"));
        public TextAlignment TextAlignment = TextAlignment.Center;
        #endregion
        public LyricView()
        {
            this.InitializeComponent();
        }
        public MainWindow mw;

        public void Mw_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == WidthProperty) {
                foreach (TextBlock tb in c_lrc_items.Children)
                    tb.Width = mw.Width - 100;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            c_lrc_items = this.Get<VirtualizingStackPanel>("c_lrc_items");
            c_scrollviewer = this.Get<ScrollViewer>("c_scrollviewer");
        }

        ScrollViewer c_scrollviewer;
        VirtualizingStackPanel c_lrc_items;
        Brush SeletColor = new SolidColorBrush(Color.Parse("#6699FF"));
        public void LoadLrc(LyricData data)
        {
            Lrcs.Clear();
            c_lrc_items.Children.Clear();
            foucslrc = null;

            string[] lrcdata = data.lyric.Split('\n');
            string[] transdata = null;
            Dictionary<double, string> transDic = null;
            if (data.HasTrans)
            {
                transdata = data.trans.Split('\n');
                transDic = new Dictionary<double, string>();
                foreach (string str in transdata)
                {
                    if (CanSolve(str))
                    {
                        TimeSpan time = GetTime(str);
                        string tran = str.Split(']')[1];
                        transDic.Add(time.TotalMilliseconds, tran);
                    }
                }
            }
            foreach (string str in lrcdata)
            {
                if (CanSolve(str))
                {
                    //以歌词Lyric内的时间为准....
                    TimeSpan time = GetTime(str);

                    //歌词翻译的  解析和适配
                    //1.正常对应
                    //2.翻译与歌词之间有+-2ms的误差
                    string lrc = str.Split(']')[1];
                    string trans = null;
                    if (data.HasTrans)
                    {
                        IEnumerable<KeyValuePair<double, string>> s = transDic.Where(m => m.Key >= (time.TotalMilliseconds - 1));
                        string a = s.First().Value;
                        trans = a == "//" ? null : a;
                    }

                    TextBlock c_lrcbk = new TextBlock();
                    c_lrcbk.FontSize = 22;
                    c_lrcbk.Foreground = NoramlLrcColor;
                    c_lrcbk.TextWrapping = TextWrapping.Wrap;
                    c_lrcbk.TextAlignment = TextAlignment;
                    c_lrcbk.Text = lrc + (trans == null ? "" : ("\r\n" + trans));

                    if (c_lrc_items.Children.Count > 0)
                        c_lrcbk.Margin = new Thickness(0, 15, 0, 15);
                    if (!Lrcs.ContainsKey(time.TotalMilliseconds))
                        Lrcs.Add(time.TotalMilliseconds, new LrcModel()
                        {
                            c_LrcTb = c_lrcbk,
                            LrcText = lrc,
                            Time = time.TotalMilliseconds,
                            LrcTransText = trans
                        });
                    c_lrc_items.Children.Add(c_lrcbk);
                }
            }
        }
        public bool CanSolve(string str)
        {
            if (str.Length > 0)
            {
                //直接判断是否为数字...
                var key = TextHelper.XtoYGetTo(str, "[", ":", 0);
                return int.TryParse(key, out _);
            }
            else return false;
        }

        public TimeSpan GetTime(string str)
        {
            Regex reg = new Regex(@"\[(?<time>.*)\]", RegexOptions.IgnoreCase);
            string timestr = reg.Match(str).Groups["time"].Value;
            string[] sp = timestr.Split(':');
            int m = Convert.ToInt32(sp[0]);
            int s = 0, f = 0;
            if (sp[1].IndexOf(".") != -1)
            {
                s = Convert.ToInt32(sp[1].Split('.')[0]);
                f = Convert.ToInt32(sp[1].Split('.')[1]);
            }
            else
                s = Convert.ToInt32(sp[1]);
                return new TimeSpan(0, 0, m, s, f);
        }
        public void LrcRoll(double nowtime, bool needScrol)
        {
            if (foucslrc == null)
            {
                foucslrc = Lrcs.Values.First();
                foucslrc.c_LrcTb.Foreground = SeletColor;
            }
            else
            {
                IEnumerable<KeyValuePair<double, LrcModel>> s = Lrcs.Where(m => nowtime >= m.Key);
                if (s.Count() > 0)
                {
                    LrcModel lm = s.Last().Value;
                    if (needScrol)
                        foucslrc.c_LrcTb.Foreground = NoramlLrcColor;

                    foucslrc = lm;
                    if (needScrol)
                    {
                        foucslrc.c_LrcTb.Foreground = SeletColor;
                        ResetLrcviewScroll();
                    }
                    NextLyric?.Invoke(foucslrc.LrcText, foucslrc.LrcTransText);
                }
            }
        }
        public void ResetLrcviewScroll()
        {
            double loc = foucslrc.c_LrcTb.Bounds.Position.Y;
            double os = loc - (c_scrollviewer.Bounds.Height / 2) + 50;
            c_scrollviewer.Offset = new Vector(0, os);
        }
    }
}
