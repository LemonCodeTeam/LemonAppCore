using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace LemonAppCore.Helpers
{
    public class MusicLib
    {
        #region 歌词 获取|处理
        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="McMind"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<LyricData> GetLyric(string McMind, string file = "")
        {
            string split = "\n<LemonApp TransLyric/>\n";//分隔符
            if (file == "")
                file = Path.Combine(Settings.MusicCachePath, "Lyric", McMind + ".lmrc");
            if (!File.Exists(file))
            {
                WebClient c = new WebClient();
                c.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36");
                c.Headers.Add("Accept", "*/*");
                c.Headers.Add("Referer", "https://y.qq.com/portal/player.html");
                c.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                c.Headers.Add("Cookie", Settings.USettings.cookies);
                c.Headers.Add("Host", "c.y.qq.com");
                string url = $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?-=MusicJsonCallback_lrc&pcachetime=1563410858607&songmid={McMind}&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0";
                string td = c.DownloadString(url);
                JObject o = JObject.Parse(td);

                LyricData data = new LyricData();

                string lyric = WebUtility.HtmlDecode(Encoding.UTF8.GetString(Convert.FromBase64String(o["lyric"].ToString())));
                if (o["trans"].ToString() == "")
                {
                    //没有翻译  直接返回歌词
                    await Task.Run(() => { File.WriteAllText(file, lyric); });//保存歌词
                    data.lyric = lyric;
                    data.HasTrans = false;
                    return data;
                }
                else
                {
                    string trans = WebUtility.HtmlDecode(Encoding.UTF8.GetString(Convert.FromBase64String(o["trans"].ToString())));
                    await Task.Run(() => { File.WriteAllText(file, lyric + split + trans); });
                    data.lyric = lyric;
                    data.trans = trans;
                    data.HasTrans = true;
                    return data;
                }
            }
            else
            {
                string filedata = await File.ReadAllTextAsync(file);
                LyricData data = new LyricData();
                if (filedata.Contains(split))
                {
                    //有翻译歌词
                    var dta = filedata.Split(split);
                    data.lyric = dta[0];
                    data.trans = dta[1];
                    data.HasTrans = true;
                    return data;
                }
                else
                {
                    //没有翻译歌词
                    data.lyric = filedata;
                    data.HasTrans = false;
                    return data;
                }
            }
        }
        #endregion
        #region Search
        public static async Task<List<Music>> SearchMusicAsync(string Content, int osx = 1)
        {
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={osx}&n=20&w={HttpUtility.UrlDecode(Content)}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0"));
            List<Music> dt = new List<Music>();
            int i = 0;
            var dsl = o["data"]["song"]["list"];
            while (i < dsl.Count())
            {
                try
                {
                    var dsli = dsl[i];
                    Music m = new Music();
                    m.MusicName = dsli["title"].ToString();
                    m.MusicName_Lyric = dsli["lyric"].ToString();
                    string Singer = "";
                    List<MusicSinger> lm = new List<MusicSinger>();
                    for (int osxc = 0; osxc != dsli["singer"].Count(); osxc++)
                    {
                        Singer += dsli["singer"][osxc]["name"] + "&";
                        lm.Add(new MusicSinger() { Name = dsli["singer"][osxc]["name"].ToString(), Mid = dsli["singer"][osxc]["mid"].ToString() });
                    }
                    m.Singer = lm;
                    m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                    m.MusicID = dsli["mid"].ToString();
                    var amid = dsli["album"]["mid"].ToString();
                    if (amid == "001ZaCQY2OxVMg")
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{dsli["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                    if (amid != "")
                        m.Album = new MusicGD()
                        {
                            ID = amid,
                            Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                            Name = dsli["album"]["name"].ToString()
                        };
                    var file = dsli["file"];
                    if (file["size_320"].ToString() != "0")
                        m.Pz = "HQ";
                    if (file["size_flac"].ToString() != "0")
                        m.Pz = "SQ";
                    m.Mvmid = dsli["mv"]["vid"].ToString();
                    dt.Add(m);
                }
                catch { }
                i++;
            }
            return dt;
        }
        public static async Task<List<SSBox>> Search_SmartBoxAsync(string key)
        {
            try
            {
                var data = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg?key={HttpUtility.UrlDecode(key)}&utf8=1&is_xml=0&loginUin={Settings.USettings.qq}&qqmusic_ver=1592&searchid=3DA3E73D151F48308932D9680A3A5A1722872&pcachetime=1535710304"))["data"];
                List<SSBox> list = new List<SSBox>();
                var song = data["song"]["itemlist"];
                for (int i = 0; i < song.Count(); i++)
                {
                    var o = song[i];
                    var a = o["name"] + " - " + o["singer"];
                    list.Add(new SSBox() { desc = "歌曲:" + a, content = a });
                }
                var album = data["album"]["itemlist"];
                for (int i = 0; i < album.Count(); i++)
                {
                    var o = album[i];
                    var a = o["singer"] + " - 《" + o["name"] + "》";
                    list.Add(new SSBox() { desc = "专辑:" + a, content = a });
                }
                var singer = data["singer"]["itemlist"];
                for (int i = 0; i < singer.Count(); i++)
                {
                    var o = singer[i];
                    var a = o["singer"].ToString();
                    list.Add(new SSBox() { desc = "歌手:" + a, content = a });
                }
                return list;
            }
            catch {
                return null;
            }
        }
        #endregion
        #region MP3 URL Get
        public static async Task<string> GetUrlAsync(string Musicid)
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("https://i.y.qq.com/v8/playsong.html?songmid=000edOaL1WZOWq");
            hwr.Timeout = 20000;
            hwr.KeepAlive = true;
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3854.3 Mobile Safari/537.36";
            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            hwr.Referer = "https://i.y.qq.com/n2/m/share/details/album.html?albummid=003bBofB3UzHxS&ADTAG=myqq&from=myqq&channel=10007100";
            hwr.Host = "i.y.qq.com";
            hwr.Headers.Add("sec-fetch-mode", "navigate");
            hwr.Headers.Add("sec-fetch-site", "same - origin");
            hwr.Headers.Add("sec-fetch-user", "?1");
            hwr.Headers.Add("upgrade-insecure-requests", "1");
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
            hwr.Headers.Add(HttpRequestHeader.Cookie, Settings.USettings.cookies);
            var o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), Encoding.UTF8);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            string val = Regex.Match(st, "amobile.music.tc.qq.com/.*?.m4a.*?&fromtag=38").Value;
            string vk = TextHelper.XtoYGetTo(val, "m4a", "&fromtag=38", 0);
            if (string.IsNullOrEmpty(vk))
            {
                await Task.Delay(500);
                return await GetUrlAsync(Musicid);
            }
            var mid = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={Musicid}&platform=yqq&format=json"))["data"][0]["file"]["media_mid"].ToString();
            return $"http://musichy.tc.qq.com/amobile.music.tc.qq.com/M500{mid}.mp3" + vk + "&fromtag=98";
        }
        #endregion 
        #region 歌单
        /// <summary>
        /// 获取“我喜欢”的歌单ID
        /// </summary>
        public static async Task<string> GetMusicLikeGDid()
        {
            try
            {
                string dta = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.qq}&reqfrom=1&reqtype=0");
                JObject o = JObject.Parse(dta);
                string id = "";
                foreach (var a in o["data"]["mymusic"])
                {
                    if (a["title"].ToString() == "我喜欢")
                    {
                        id = a["id"].ToString();
                        break;
                    }
                }
                return id;
            }
            catch { return null; }
        }
        /// <summary>
        /// 获取 我创建的歌单 列表
        /// </summary>
        /// <returns></returns>
        public static async Task<SortedDictionary<string, MusicGData>> GetGdListAsync()
        {
            if (Settings.USettings.qq == "")
                return new SortedDictionary<string, MusicGData>();
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.qq}&reqfrom=1&reqtype=0");
            var o = JObject.Parse(dt);
            var data = new SortedDictionary<string, MusicGData>();
            var dx = o["data"]["mydiss"]["list"];
            foreach (var ex in dx)
            {
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.name = ex["title"].ToString();
                df.subtitle = ex["subtitle"].ToString();
                if (ex["picurl"].ToString() != "")
                    df.pic = ex["picurl"].ToString().Replace("http://", "https://");
                else df.pic = "https://y.gtimg.cn/mediastyle/global/img/cover_playlist.png?max_age=31536000";
                data.Add(df.id, df);
            }
            return data;
        }
        /// <summary>
        /// 获取 我收藏的歌单 列表
        /// </summary>
        /// <returns></returns>
        public static async Task<SortedDictionary<string, MusicGData>> GetGdILikeListAsync()
        {
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/fav/fcgi-bin/fcg_get_profile_order_asset.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&ct=20&cid=205360956&userid={Settings.USettings.qq}&reqtype=3&sin=0&ein=25");
            var o = JObject.Parse(dt);
            var data = new SortedDictionary<string, MusicGData>();
            var dx = o["data"]["cdlist"];
            foreach (var ex in dx)
            {
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.name = ex["dissname"].ToString();
                df.listenCount = int.Parse(ex["listennum"].ToString());
                if (ex["logo"].ToString() != "")
                    df.pic = ex["logo"].ToString().Replace("http://", "https://");
                else df.pic = "https://y.gtimg.cn/mediastyle/global/img/cover_playlist.png?max_age=31536000";
                data.Add(df.id, df);
            }
            return data;
        }

        public static async Task<MusicGData> GetGDAsync(string id = "2591355982", Action<MusicGData> GetInfo = null, Action<Music, bool> callback = null)
        {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var dt = new MusicGData();
            var c0 = o["cdlist"][0];
            dt.name = c0["dissname"].ToString();
            dt.pic = c0["logo"].ToString().Replace("http://", "https://");
            dt.id = id;
            dt.ids = c0["songids"].ToString().Split(',').ToList();
            dt.IsOwn = c0["login"].ToString() == c0["uin"].ToString();
            dt.desc = c0["desc"].ToString();
            dt.Creater = new MusicSinger()
            {
                Name = c0["nick"].ToString(),
                Photo = c0["headurl"].ToString()
            };
            GetInfo?.Invoke(dt);
            var c0s = c0["songlist"];
            foreach(var c0si in c0s)
            {
                string singer = "";
                string songtype = c0si["songtype"].ToString();
                if (songtype == "0")
                {
                    var c0sis = c0si["singer"];
                    List<MusicSinger> lm = new List<MusicSinger>();
                    foreach (var cc in c0sis)
                    {
                        singer += cc["name"].ToString() + "&";
                        lm.Add(new MusicSinger()
                        {
                            Name = cc["name"].ToString(),
                            Mid = cc["mid"].ToString()
                        });
                    }
                    Music m = new Music();
                    m.MusicName = c0si["songname"].ToString();
                    m.MusicName_Lyric = c0si["albumdesc"].ToString();
                    m.Singer = lm;
                    m.SingerText = singer.Substring(0, singer.Length - 1);
                    m.MusicID = c0si["songmid"].ToString();
                    var amid = c0si["albummid"].ToString();
                    if (amid == "001ZaCQY2OxVMg")
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{c0si["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                    if (amid != "")
                        m.Album = new MusicGD()
                        {
                            ID = amid,
                            Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                            Name = c0si["albumname"].ToString()
                        };
                    if (c0si["size320"].ToString() != "0")
                        m.Pz = "HQ";
                    if (c0si["sizeflac"].ToString() != "0")
                        m.Pz = "SQ";
                    m.Mvmid = c0si["vid"].ToString();
                  //  m.Littleid = dt.ids[index];
                    callback?.Invoke(m, dt.IsOwn);
                }
            }
            return dt;
        }
        #endregion
    }
}
