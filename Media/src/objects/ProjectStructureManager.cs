using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Media.src.objects;
using MEDIA.src.objects;

namespace MEDIA.src
{
    public class ProjectStructureManager
    {
        private readonly string _baseDirectory;
        private readonly List<string> _videoUrls;
        private readonly string _cutDuration;

        public ProjectStructureManager(string baseDirectory, List<string> videoUrls, string cutDuration)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            _videoUrls = videoUrls ?? throw new ArgumentNullException(nameof(videoUrls));
            _cutDuration = cutDuration ?? throw new ArgumentNullException(nameof(cutDuration));
        }

        public async Task CreateProjectStructure()
        {
            // Создаем основную структуру директорий
            string videoDir = Path.Combine(_baseDirectory, "video");
            string cutVideosDir = Path.Combine(videoDir, "cut_videos");
            string previewVideosDir = Path.Combine(videoDir, "preview_videos");
            string infoVideosDir = Path.Combine(videoDir, "info_videos");

            Directory.CreateDirectory(videoDir);
            Directory.CreateDirectory(cutVideosDir);
            Directory.CreateDirectory(previewVideosDir);
            Directory.CreateDirectory(infoVideosDir);

            // Скачиваем видео
            var downloadVideoService = new DownloadVideoService(_videoUrls, videoDir);
            await downloadVideoService.DownloadContents();

            // Получаем список скачанных видео
            var videoFiles = Directory.GetFiles(videoDir, "*.*", SearchOption.TopDirectoryOnly);
            var videoFilesList = new List<string>(videoFiles);

            // Обрезаем видео
            var editingVideoService = new EditingVideoService(videoFilesList, _cutDuration);
            await editingVideoService.CutContents();

            // Перемещаем обрезанные видео в папку cut_videos
            MoveFiles(videoDir, cutVideosDir, "*_part*");

            // Скачиваем превью
            var downloadThumbnailService = new DownloadThumbnailService(_videoUrls, previewVideosDir);
            await downloadThumbnailService.DownloadContents();

            // Скачиваем информацию о видео
            var downloadVideoInfoService = new DownloadVideoInfoService(_videoUrls, infoVideosDir);
            await downloadVideoInfoService.DownloadContents();

            Console.WriteLine("Project structure created successfully.");
        }

        // Вспомогательный метод для перемещения файлов
        private void MoveFiles(string sourceDir, string targetDir, string searchPattern)
        {
            var files = Directory.GetFiles(sourceDir, searchPattern);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(targetDir, fileName);
                File.Move(file, destFile, true);
            }
        }
    }
}