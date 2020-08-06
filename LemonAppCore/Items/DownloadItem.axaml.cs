using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LemonAppCore.Helpers;

namespace LemonAppCore.Items
{
    public class DownloadItem : UserControl
    {
        public Music data;
        /// <summary>
        /// 下载状态 0:未开始下载 1:正在下载 2:暂停 3:已完成
        /// </summary>
        public int code = 0;

        public ProgressBar pro;
        public TextBlock size;
        public DownloadItem()
        {
            this.InitializeComponent();
        }

        public DownloadItem(Music m)
        {
            this.InitializeComponent();
            data = m;
            size = this.Get<TextBlock>("size");
            pro = this.Get<ProgressBar>("pro");
            this.Get<TextBlock>("title").Text = m.MusicName + " - " + m.SingerText;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
