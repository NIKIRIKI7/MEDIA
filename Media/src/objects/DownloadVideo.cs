using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MEDIA.src.objects
{
    public class DownloadVideosObjects : IDownloadVideo
    {
        private List<string> _videos;
        private string _downloadPath;

        private const int MaxRetries = 3;
        private const int DelayOnRetry = 1000; // milliseconds

        public DownloadVideosObjects(List<string> videos, string downloadPath)
        {
            _videos = videos ?? throw new ArgumentException("Videos list cannot be null or empty.");
            _downloadPath = !string.IsNullOrWhiteSpace(downloadPath) ? downloadPath : throw new ArgumentException("Download path cannot be null or empty.");
        }

        public List<string> GetVideos() => _videos;
        public string GetDownloadPath() => _downloadPath;

        public void SetVideos(List<string> videos)
        {
            if (videos == null || !videos.Any()) throw new ArgumentException("Videos list cannot be null or empty.");
            _videos = videos;
        }

        public void SetDownloadPath(string downloadPath)
        {
            if (string.IsNullOrWhiteSpace(downloadPath)) throw new ArgumentException("Download path cannot be null or empty.");
            _downloadPath = downloadPath;
        }

        public async Task DownloadVideosAsync()
        {
            foreach (var video in _videos)
            {
                await DownloadVideo(video);
            }
        }

        public async Task DownloadVideosAsync(List<string> videos, string downloadPath)
        {
            SetVideos(videos);
            SetDownloadPath(downloadPath);
            await DownloadVideosAsync();
        }

        private async Task DownloadVideo(string videoUrl)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var videoFileName = Path.Combine(_downloadPath, $"{Guid.NewGuid()}.%(ext)s"); // Use a unique filename
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp",
                        Arguments = $"-o \"{videoFileName}\" \"{videoUrl}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();

                        // Optionally read output and error streams
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();

                        await process.WaitForExitAsync();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"Downloaded: {videoUrl}");
                            return; // Exit if successful
                        }
                        else
                        {
                            Console.WriteLine($"Error downloading {videoUrl}: {error}");
                            throw new Exception($"yt-dlp exited with code {process.ExitCode}");
                        }
                    }
                }
                catch (Exception e) when (attempt < MaxRetries - 1)
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed: {e.Message}");
                    await Task.Delay(DelayOnRetry); // Wait before retrying
                }
            }

            throw new Exception("Failed to download video after multiple attempts.");
        }
    }
}