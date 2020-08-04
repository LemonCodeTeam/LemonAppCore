using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LemonAppCore.Helpers;

namespace LemonAppCore.Items
{
    public class NormalItem : UserControl
    {
        public NormalItem()
        {
            this.InitializeComponent();
        }
        public object data;
        public NormalItem(string title,string imgurl,object dt)
        {
            this.InitializeComponent();
            this.Get<TextBlock>("title").Text = title;
            LoadImg(imgurl);
            data = dt;
        }
        private async void LoadImg(string url) {
            Bitmap im = await ImageCacheHelp.GetImageByUrl(url) ?? await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000");
            this.Get<Border>("img").Background = new ImageBrush(im);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
