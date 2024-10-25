using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MEDIA.src;
using MEDIA.src.objects;

namespace MEDIA
{
    class Program
    {
        public static async Task Main()
        {
            List<string> videos = new List<string>() { "https://www.youtube.com/shorts/ZPiYFU4pqiQ" };
            string downloadPath = "/home/mcniki/Видео/";

            IDownloadVideoService downloadVideo = new DownloadVideosService(videos, downloadPath);
            await downloadVideo.DownloadVideos();
        }
    }
}