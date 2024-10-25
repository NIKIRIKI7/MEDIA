using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Media.src;
using Media.src.objects;
using MEDIA.src;
using MEDIA.src.objects;

namespace MEDIA
{
    class Program
    {
        public static async Task Main()
        {
            // List<string> videos = new List<string>() { "https://www.youtube.com/shorts/ZPiYFU4pqiQ" };
            // string downloadPath = "/home/mcniki/Видео/";

            // IDownloadVideoService downloadVideo = new DownloadVideosService(videos, downloadPath);
            // await downloadVideo.DownloadVideos();

            List<string> videosPath = new List<string>() {"/home/mcniki/Видео/video.webm"};
            string timePartVideo = "00:00:30";

            IEditingVideoService editingVideo = new EditingVideoService(videosPath, timePartVideo);
            await editingVideo.CutVideos();
        }
    }
}