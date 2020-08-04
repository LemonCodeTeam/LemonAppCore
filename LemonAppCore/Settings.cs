using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;

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
        public static string CachePath { get
            {
                return AppDomain.CurrentDomain.BaseDirectory+"\\Settings\\";
            } }
        public static string MusicCachePath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + "\\MusicCache\\";
            }
        }
        public static void Load() {
            string fileName = CachePath+"Settings.st";
            if (!File.Exists(fileName))
            {
                //No Login
                File.CreateText(fileName);
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
