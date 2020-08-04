using System;
using System.Collections.Generic;
using System.Text;

namespace LemonAppCore.Helpers
{
    public class SSBox
    {
        public string desc { get; set; }
        public string content { get; set; }
    }
    public class MusicGData
    {
        public List<Music> Data { get; set; } = new List<Music>();
        public string name { get; set; }
        public string pic { get; set; }
        public string subtitle { get; set; }

        public string desc;
        public MusicSinger Creater;
        public string id { get; set; }
        public bool IsOwn = false;
        public int listenCount { get; set; }
        public List<string> ids = new List<string>();
    }
    public class MusicSinger
    {
        public string Name { set; get; }
        public string Photo { set; get; }
        public string Mid { set; get; }
    }
    public class MusicGD
    {
        public string Name { set; get; }
        public string Photo { set; get; }
        public string ID { set; get; }
        public int ListenCount { set; get; }
    }

    public class Music
    {
        public string MusicName { set; get; } = "";
        public string MusicName_Lyric { get; set; } = "";
        public List<MusicSinger> Singer { set; get; } = new List<MusicSinger>();
        public string SingerText { get; set; } = "";
        public string MusicID { set; get; } = "";
        public string ImageUrl { set; get; } = "";
        public MusicGD Album { set; get; } = new MusicGD();
        public string Mvmid { set; get; } = "";
        public string Pz { set; get; } = "";
        public string Littleid;
    }
    public class LoginData
    {
        public string qq;
        public string cookie;
        public string g_tk;
    }
}
