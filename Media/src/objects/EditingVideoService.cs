using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Media.src.objects
{
    public class EditingVideoService : IEditingVideoService
    {
        private readonly string _timePartVideo;
        private readonly List<string> _videosPath;

        public EditingVideoService(List<string> videosPath, string timePartVideo)
        {
            _videosPath = videosPath ?? throw new ArgumentException("Videos list cannot be null or empty.");
            _timePartVideo = !string.IsNullOrWhiteSpace(timePartVideo) ? timePartVideo : throw new ArgumentException("timePartVideo cannot be null or empty.");
        }

        public async Task CutVideos()
        {
            foreach (var video in _videosPath)
            {
                await CutVideo(video);
            }
        }

        private async Task CutVideo(string videoPath)
        {
            var videoDuration = await GetVideoDuration(videoPath);
            var partDuration = TimeSpan.Parse(_timePartVideo);
            var parts = (int)Math.Ceiling(videoDuration.TotalSeconds / partDuration.TotalSeconds);

            for (int i = 0; i < parts; i++)
            {
                var start = TimeSpan.FromSeconds(i * partDuration.TotalSeconds);
                var end = start + partDuration;
                if (end > videoDuration)
                    end = videoDuration;

                var outputPath = Path.Combine(
                    Path.GetDirectoryName(videoPath) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(videoPath)}_part{i + 1}{Path.GetExtension(videoPath)}"
                );

                await RunFFmpegCommand(videoPath, outputPath, start, end - start);
            }
        }

        private async Task<TimeSpan> GetVideoDuration(string videoPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            output = output.Trim();

            if (double.TryParse(output, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double seconds))
            {
                return TimeSpan.FromSeconds(seconds);
            }

            throw new Exception($"Unable to determine duration of video: {videoPath}\nOutput: {output}\nError: {error}");
        }

        private async Task RunFFmpegCommand(string inputPath, string outputPath, TimeSpan start, TimeSpan duration)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-ss {start} -i \"{inputPath}\" -t {duration} -c copy \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg command failed: {error}\nOutput: {output}");
            }
        }
    }
}