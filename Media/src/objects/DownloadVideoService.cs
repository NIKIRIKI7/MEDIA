using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MEDIA.src;

namespace MEDIA.src.objects
{
    public class DownloadVideoService : IDownloadContentService
    {
        private readonly List<string> _videos;
        private readonly string _downloadPath;

        private const int MaxRetries = 3;
        private const int DelayOnRetry = 1000; // milliseconds

        public DownloadVideoService(List<string> videos, string downloadPath)
        {
            _videos = videos ?? throw new ArgumentException("Videos list cannot be null or empty.");
            _downloadPath = !string.IsNullOrWhiteSpace(downloadPath) ? downloadPath : throw new ArgumentException("Download path cannot be null or empty.");
        }

        public async Task DownloadContents()
        {
            foreach (var video in _videos)
            {
                await DownloadVideo(video);
            }
        }

        private async Task DownloadVideo(string videoUrl)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var videoFileName = Path.Combine(_downloadPath, $"{Guid.NewGuid()}.mp4");
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "yt-dlp",
                        Arguments = $"-o \"{videoFileName}\" \"{videoUrl}\" --newline",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = processStartInfo })
                    {
                        process.Start();

                        var progressRegex = new Regex(@"\[download\]\s+(\d+\.\d+)%");
                        var progress = new Progress<double>(UpdateProgressBar);

                        while (!process.StandardOutput.EndOfStream)
                        {
                            var line = await process.StandardOutput.ReadLineAsync();
                            var match = progressRegex.Match(line);
                            if (match.Success && double.TryParse(match.Groups[1].Value, out double percentage))
                            {
                                ((IProgress<double>)progress).Report(percentage);
                            }
                        }

                        await process.WaitForExitAsync();

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"\nDownloaded: {videoUrl}");
                            return; // Exit if successful
                        }
                        else
                        {
                            Console.WriteLine($"\nError downloading {videoUrl}: {await process.StandardError.ReadToEndAsync()}");
                            throw new Exception($"yt-dlp exited with code {process.ExitCode}");
                        }
                    }
                }
                catch (Exception e) when (attempt < MaxRetries - 1)
                {
                    Console.WriteLine($"\nAttempt {attempt + 1} failed: {e.Message}");
                    await Task.Delay(DelayOnRetry); // Wait before retrying
                }
            }

            throw new Exception("Failed to download video after multiple attempts.");
        }

        private void UpdateProgressBar(double percentage)
        {
            Console.CursorLeft = 0;
            Console.Write("[");
            int width = 50;
            int position = (int)(percentage / 100 * width);
            Console.Write(new string('#', position));
            Console.Write(new string(' ', width - position));
            Console.Write($"] {percentage:F1}%");
        }
    }
}