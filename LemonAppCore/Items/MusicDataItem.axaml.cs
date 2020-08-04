using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using LemonAppCore.Helpers;

namespace LemonAppCore.Items
{
    public class MusicDataItem : UserControl
    {
        public Music m { get; set; }

        //Controls
        private TextBlock MusicName;
        private TextBlock SingerName;
        private TextBlock Album;
        public Border color;
        public MusicDataItem()
        {
            this.InitializeComponent();
        }
        public MusicDataItem(Music ms)
        {
            this.InitializeComponent();
            m = ms;
            this.Initialized += MusicDataItem_Initialized;
            this.PointerEnter += delegate {
                Background = new SolidColorBrush(Color.FromArgb(50,0,0,0));
            };
            this.PointerLeave += delegate {
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            };
            this.DoubleTapped += delegate {
                this.Check(true);
                MainWindow.PlayCallBack(this);
            };
            MusicName = this.Get<TextBlock>("MusicName");
            SingerName = this.Get<TextBlock>("SingerName");
            Album = this.Get<TextBlock>("Album");
            color = this.Get<Border>("color");
        }
        /// <summary>
        /// type 0:list 1:Playlist
        /// </summary>
        public int type = 0;
        /// <summary>
        /// 是否选中
        /// </summary>
        /// <param name="a"></param>
        public void Check(bool a) {
            if (a)
            {
                if (type == 0)
                {
                    if (He.LastItem != null)
                        He.LastItem.Check(false);
                    He.LastItem = this;
                }
                else
                {
                    if (He.LastItemType1 != null)
                        He.LastItemType1.Check(false);
                    He.LastItemType1 = this;
                }
                color.IsVisible = true;
                MusicName.Foreground = new SolidColorBrush(Color.Parse("#6699FF"));
                SingerName.Foreground = MusicName.Foreground;
                Album.Foreground = MusicName.Foreground;
            }
            else {
                color.IsVisible = false;
                MusicName.Foreground = new SolidColorBrush(Colors.Black);
                SingerName.Foreground = MusicName.Foreground;
                Album.Foreground = MusicName.Foreground;
            }
        }
        private void MusicDataItem_Initialized(object sender, System.EventArgs e)
        {
            MusicName.Text = m.MusicName;
            SingerName.Text = m.SingerText+(m.MusicName_Lyric==string.Empty?"":(" - "+ m.MusicName_Lyric));
            Album.Text = m.Album.Name;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class He {
        public static MusicDataItem LastItem;
        public static MusicDataItem LastItemType1;
    }
}
