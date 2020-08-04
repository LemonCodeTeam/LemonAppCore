using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LemonAppCore.Helpers
{
    public class HttpHelper
    {
        public static async Task HttpDownloadFileAsync(string url, string path)
        {
            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            hwr.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            hwr.Headers.Add("Cache-Control", "max-age=0");
            hwr.KeepAlive = true;
            hwr.Referer = url;
            hwr.Headers.Add("Upgrade-Insecure-Requests", "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            using HttpWebResponse response = await hwr.GetResponseAsync() as HttpWebResponse;
            using Stream responseStream = response.GetResponseStream();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] bArr = new byte[1024];
                int size = await responseStream.ReadAsync(bArr, 0, bArr.Length);
                while (size > 0)
                {
                    await stream.WriteAsync(bArr, 0, size);
                    size = await responseStream.ReadAsync(bArr, 0, bArr.Length);
                }
                stream.Close();
            }
            responseStream.Close();
        }
        public static async Task<string> GetWebDatacAsync(string url, Encoding c = null)
        {
            if (c == null) c = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.KeepAlive = true;
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
            hwr.Accept = "*/*";
            hwr.Referer = "https://y.qq.com/portal/player.html";
            hwr.Host = "c.y.qq.com";
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8");
            hwr.Headers.Add(HttpRequestHeader.Cookie, Settings.USettings.cookies);
            using WebResponse o = await hwr.GetResponseAsync();
            using StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
        public static async Task<string> GetWebAsync(string url, Encoding e = null)
        {
            if (e == null)
                e = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            using WebResponse res = await hwr.GetResponseAsync();
            using StreamReader sr = new StreamReader(res.GetResponseStream(), e);
            var st = await sr.ReadToEndAsync();
            return st;
        }
    }
}
