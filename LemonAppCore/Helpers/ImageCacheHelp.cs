using Avalonia.Media.Imaging;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LemonAppCore.Helpers
{
    //三级缓存        By:Starlight
    public class ImageCacheHelp
    {
        /// <summary>
        /// 图像的三级缓存 从URL中获取图像
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="DecodePixel">高Height,宽Width</param>
        /// <returns></returns>
        public static async Task<Bitmap> GetImageByUrl(string url, int[] DecodePixel = null)
        {
            try
            {
                url += DecodePixel != null ? "#" + string.Join(",", DecodePixel) : "";
                Bitmap bi = GetImageFormMemory(url, DecodePixel);
                if (bi != null) { return bi; }
                bi = GetImageFromFile(url);
                if (bi != null)
                {
                    AddImageToMemory(url, bi);
                    return bi;
                }
                return await GetImageFromInternet(url, DecodePixel);
            }
            catch
            {
                return null;
            }
        }
        private static ConditionalWeakTable<string, Bitmap> MemoryData = new ConditionalWeakTable<string, Bitmap>();
        private static Bitmap GetImageFormMemory(string url, int[] Decode)
        {
            string key = TextHelper.MD5.EncryptToMD5string(url);
            Bitmap bi;
            return MemoryData.TryGetValue(key, out bi) ? bi : null;
        }
        private static void AddImageToMemory(string url, Bitmap data)
        {
            string key = TextHelper.MD5.EncryptToMD5string(url);
            MemoryData.AddOrUpdate(key, data);
        }

        private static Bitmap GetImageFromFile(string url)
        {
            string file = Settings.MusicCachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
            if (File.Exists(file))
                return GetBitMapImageFromFile(file);
            else return null;
        }
        private static Bitmap GetBitMapImageFromFile(string file)
        {
            Bitmap bi = new Bitmap(file);
            return bi;
        }
        private static async Task<Bitmap> GetImageFromInternet(string url, int[] DecodePixel)
        {
            string file = Settings.MusicCachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
            if (DecodePixel != null)
            {
                HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
                using HttpWebResponse response = await hwr.GetResponseAsync() as HttpWebResponse;
                using Stream responseStream = response.GetResponseStream();
                System.Drawing.Image img = System.Drawing.Image.FromStream(responseStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(DecodePixel[1], DecodePixel[0]);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, bitmap.Width, bitmap.Height);
                g.Dispose();
                bitmap.Save(file, ImageFormat.Jpeg);
                bitmap.Dispose();
                img.Dispose();
            }
            else
            {
                await HttpHelper.HttpDownloadFileAsync(url, file);
            }
            Bitmap bi = GetBitMapImageFromFile(file);
            AddImageToMemory(url, bi);
            return bi;
        }
    }
}
