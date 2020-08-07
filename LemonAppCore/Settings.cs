using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using LemonAppCore.Helpers;

namespace LemonAppCore
{
    public class Settings
    {
        /* Load:
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
            Load();
         */
        public static StData USettings = new StData();
        public static string CachePath => AppDomain.CurrentDomain.BaseDirectory + "\\Settings\\";
        public static string MusicCachePath => AppDomain.CurrentDomain.BaseDirectory + "\\MusicCache\\";
        public static string DownloadPath => AppDomain.CurrentDomain.BaseDirectory + "\\Download\\";
        public static void Load() {
            string fileName = CachePath+"Settings.st";
            if (!File.Exists(fileName))
            {
                //No Login
                Save();
            }
            else
            {
                //Login    read settings.
                string json = File.ReadAllText(fileName, Encoding.UTF8);
                if (json != string.Empty)
                {
                    JObject obj = JObject.Parse(json);
                    USettings.qq = obj["qq"].ToString();
                    USettings.cookies = obj["cookies"].ToString();
                    USettings.g_tk = obj["g_tk"].ToString();
                    USettings.name = obj["name"].ToString();
                    if (json.Contains("LastPlay"))
                        USettings.LastPlay = JsonConvert.DeserializeObject<Music>(obj["LastPlay"].ToString());
                    if (json.Contains("MusicGDataPlayList"))
                    {
                        USettings.MusicGDataPlayList = JsonConvert.DeserializeObject<List<Music>>(obj["MusicGDataPlayList"].ToString());
                    }
                    if (json.Contains("PlayingIndex"))
                        USettings.PlayingIndex = int.Parse(obj["PlayingIndex"].ToString());
                    if (json.Contains("XHMode"))
                        USettings.XHMode = int.Parse(obj["XHMode"].ToString());
                }
            }
        }
        public static async void Save() {
            string fileName = CachePath + "Settings.st";
            await File.WriteAllTextAsync(fileName, JSON.ToJSON(USettings));
        }
    }

    public class StData {
        public string qq="";
        public string cookies="";
        public string g_tk="";
        public string name = "";

        public Music LastPlay=new Music();
        public int PlayingIndex = -1;
        public List<Music> MusicGDataPlayList = new List<Music>();
        /// <summary>
        /// 循环模式: 0:列表循环 1:单曲循环
        /// </summary>
        public int XHMode = 0;
    }

    public class JSON
    {
        public static object JsonToObject(string jsonString, object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return serializer.ReadObject(mStream);
        }
        public static string ToJSON(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
