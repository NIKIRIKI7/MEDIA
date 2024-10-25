using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Media.src;
using Newtonsoft.Json.Linq;

namespace MEDIA.src.objects
{
    public class DownloadVideoInfoService : IDownloadContentService
    {
        private readonly List<string> _videoUrls;
        private readonly string _downloadPath;

        private const int MaxRetries = 3;
        private const int DelayOnRetry = 1000; // milliseconds

        public DownloadVideoInfoService(List<string> videoUrls, string downloadPath)
        {
            _videoUrls = videoUrls ?? throw new ArgumentException("Video URLs list cannot be null or empty.");
            _downloadPath = !string.IsNullOrWhiteSpace(downloadPath) ? downloadPath : throw new ArgumentException("Download path cannot be null or empty.");
        }

        public async Task DownloadContents()
        {
            foreach (var videoUrl in _videoUrls)
            {
                await DownloadVideoInfo(videoUrl);
            }
        }

        private async Task DownloadVideoInfo(string videoUrl)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var infoFileName = Path.Combine(_downloadPath, $"{Guid.NewGuid()}.txt");
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp",
                        Arguments = $"--skip-download --dump-json \"{videoUrl}\"",
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
                            var jsonInfo = JObject.Parse(output);
                            var title = jsonInfo["title"]?.ToString() ?? "No title";
                            var description = jsonInfo["description"]?.ToString() ?? "No description";
                            var uploadDate = jsonInfo["upload_date"]?.ToString() ?? "Unknown date";
                            var duration = jsonInfo["duration"]?.ToString() ?? "Unknown duration";
                            var viewCount = jsonInfo["view_count"]?.ToString() ?? "Unknown views";

                            // Форматируем информацию для текстового файла
                            var infoContent = $"Title: {title}\n" +
                                              $"Upload Date: {uploadDate}\n" +
                                              $"Duration: {duration} seconds\n" +
                                              $"View Count: {viewCount}\n\n" +
                                              $"Description:\n{description}";

                            await File.WriteAllTextAsync(infoFileName, infoContent);

                            Console.WriteLine($"Downloaded info for: {videoUrl}");
                            Console.WriteLine($"Saved to: {infoFileName}");
                            return; // Exit if successful
                        }
                        else
                        {
                            Console.WriteLine($"Error downloading info for {videoUrl}: {error}");
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

            throw new Exception($"Failed to download info for {videoUrl} after multiple attempts.");
        }
    }
}