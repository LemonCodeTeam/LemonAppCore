using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LemonAppCore
{
    public class LyricView : UserControl
    {
        public delegate void NextData(string text);
        public event NextData NextLyric;
        #region 
        public class LrcModel
        {
            public TextBlock c_LrcTb { get; set; }
            public string LrcText { get; set; }
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

        public void LoadLrc(string lrcstr)
        {
            Lrcs.Clear();
            c_lrc_items.Children.Clear();
            foucslrc = null;
            foreach (string str in lrcstr.Split("\r\n".ToCharArray()))
            {
                if (str.Length > 0 && str.IndexOf(":") != -1 && !str.StartsWith("[ti:") && !str.StartsWith("[ar:") && !str.StartsWith("[al:") && !str.StartsWith("[by:") && !str.StartsWith("[offset:"))
                {
                    TimeSpan time = GetTime(str);
                    string lrc = str.Split(']')[1];
                    TextBlock c_lrcbk = new TextBlock();
                    c_lrcbk.FontSize = 20;
                    c_lrcbk.Width = mw.Width - 100;
                    c_lrcbk.Foreground = NoramlLrcColor;
                    c_lrcbk.TextWrapping = TextWrapping.Wrap;
                    c_lrcbk.TextAlignment = TextAlignment;
                    c_lrcbk.Text = lrc.Replace("^", "\n").Replace("//", "").Replace("null", "");
                    if (c_lrc_items.Children.Count > 0)
                        c_lrcbk.Margin = new Thickness(0, 15, 0, 15);
                    if (!Lrcs.ContainsKey(time.TotalMilliseconds))
                        Lrcs.Add(time.TotalMilliseconds, new LrcModel()
                        {
                            c_LrcTb = c_lrcbk,
                            LrcText = lrc,
                            Time = time.TotalMilliseconds

                        });
                    c_lrc_items.Children.Add(c_lrcbk);
                }
            }
        }
        public TimeSpan GetTime(string str)
        {
            Regex reg = new Regex(@"\[(?<time>.*)\]", RegexOptions.IgnoreCase);
            string timestr = reg.Match(str).Groups["time"].Value;
            int m = Convert.ToInt32(timestr.Split(':')[0]);
            int s = 0, f = 0;
            if (timestr.Split(':')[1].IndexOf(".") != -1)
            {
                s = Convert.ToInt32(timestr.Split(':')[1].Split('.')[0]);
                f = Convert.ToInt32(timestr.Split(':')[1].Split('.')[1]);
            }
            else
                s = Convert.ToInt32(timestr.Split(':')[1]);
            return new TimeSpan(0, 0, m, s, f);
        }
        public void LrcRoll(double nowtime, bool needScrol)
        {
            if (foucslrc == null)
            {
                foucslrc = Lrcs.Values.First();
                foucslrc.c_LrcTb.Foreground = FoucsLrcColor;
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
                        foucslrc.c_LrcTb.Foreground = FoucsLrcColor;
                        ResetLrcviewScroll();
                    }
                    string tx = foucslrc.LrcText.Replace("//", "");
                    if (tx.Substring(tx.Length - 1, 1) == "^")
                        tx = tx.Substring(0, tx.Length - 1);
                    tx = tx.Replace("^", "\r\n");
                    NextLyric?.Invoke(tx);
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
