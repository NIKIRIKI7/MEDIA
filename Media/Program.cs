using System;
using System.Collections.Generic;
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
            await DownloadVideo();
            await EditVideo();
            await EditAudio();
            await DownloadAudio();
            await DownloadThumbnail();
        }

        private static async Task DownloadVideo()
        {
            List<string> videos = new List<string>() { "https://www.youtube.com/shorts/ZPiYFU4pqiQ" };
            string downloadPath = "/home/mcniki/Видео/";

            IDownloadContentService downloadVideo = new DownloadVideoService(videos, downloadPath);
            await downloadVideo.DownloadContents();
        }

        private static async Task EditVideo()
        {
            List<string> videosPath = new List<string>() {"/home/mcniki/Видео/video.webm"};
            string timePartVideo = "00:00:30";

            IEditingContentService editingVideo = new EditingVideoService(videosPath, timePartVideo);
            await editingVideo.CutContents();
        }

        private static async Task EditAudio()
        {
            List<string> audiosPath = new List<string>() {"/home/mcniki/Видео/audio.mp3"};
            string timePartAudio = "00:00:15";

            IEditingContentService editingAudio = new EditingAudioService(audiosPath, timePartAudio);
            await editingAudio.CutContents();
        }

        private static async Task DownloadAudio()
        {
            List<string> audios = new List<string>() { "https://www.youtube.com/shorts/ZPiYFU4pqiQ" };
            string downloadPath = "/home/mcniki/Видео/";

            IDownloadContentService downloadAudio = new DownloadAudioService(audios, downloadPath);
            await downloadAudio.DownloadContents();
        }

        private static async Task DownloadThumbnail()
        {
            List<string> videoUrls = new List<string>() { "https://www.youtube.com/shorts/ZPiYFU4pqiQ" };
            string downloadPath = "/home/mcniki/Изображения/";

            IDownloadContentService downloadThumbnail = new DownloadThumbnailService(videoUrls, downloadPath);
            await downloadThumbnail.DownloadContents();
        }
    }
}