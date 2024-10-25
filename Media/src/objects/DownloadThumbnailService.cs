using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Media.src;

namespace MEDIA.src.objects
{
    public class DownloadThumbnailService : IDownloadContentService
    {
        private readonly List<string> _videoUrls;
        private readonly string _downloadPath;

        private const int MaxRetries = 3;
        private const int DelayOnRetry = 1000; // milliseconds

        public DownloadThumbnailService(List<string> videoUrls, string downloadPath)
        {
            _videoUrls = videoUrls ?? throw new ArgumentException("Video URLs list cannot be null or empty.");
            _downloadPath = !string.IsNullOrWhiteSpace(downloadPath) ? downloadPath : throw new ArgumentException("Download path cannot be null or empty.");
        }

        public async Task DownloadContents()
        {
            foreach (var videoUrl in _videoUrls)
            {
                await DownloadThumbnail(videoUrl);
            }
        }

        private async Task DownloadThumbnail(string videoUrl)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var thumbnailFileName = Path.Combine(_downloadPath, $"{Guid.NewGuid()}.jpg");
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp",
                        Arguments = $"--write-thumbnail --skip-download --convert-thumbnails jpg -o \"{thumbnailFileName}\" \"{videoUrl}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"Downloaded thumbnail for: {videoUrl}");
                            return; // Exit if successful
                        }
                        else
                        {
                            Console.WriteLine($"Error downloading thumbnail for {videoUrl}: {error}");
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

            throw new Exception($"Failed to download thumbnail for {videoUrl} after multiple attempts.");
        }
    }
}