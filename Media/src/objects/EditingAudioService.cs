using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Media.src.objects
{
    public class EditingAudioService : IEditingAudioService
    {
        private readonly string _timePartAudio;
        private readonly List<string> _audiosPath;

        public EditingAudioService(List<string> audiosPath, string timePartAudio)
        {
            _audiosPath = audiosPath ?? throw new ArgumentException("Audios list cannot be null or empty.");
            _timePartAudio = !string.IsNullOrWhiteSpace(timePartAudio) ? timePartAudio : throw new ArgumentException("timePartAudio cannot be null or empty.");
        }

        public async Task CutAudios()
        {
            foreach (var audio in _audiosPath)
            {
                await CutAudio(audio);
            }
        }

        private async Task CutAudio(string audioPath)
        {
            var audioDuration = await GetAudioDuration(audioPath);
            var partDuration = TimeSpan.Parse(_timePartAudio);
            var parts = (int)Math.Ceiling(audioDuration.TotalSeconds / partDuration.TotalSeconds);

            for (int i = 0; i < parts; i++)
            {
                var start = TimeSpan.FromSeconds(i * partDuration.TotalSeconds);
                var end = start + partDuration;
                if (end > audioDuration)
                    end = audioDuration;

                var outputPath = Path.Combine(
                    Path.GetDirectoryName(audioPath) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(audioPath)}_part{i + 1}{Path.GetExtension(audioPath)}"
                );

                await RunFFmpegCommand(audioPath, outputPath, start, end - start);
            }
        }

        private async Task<TimeSpan> GetAudioDuration(string audioPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{audioPath}\"",
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

            throw new Exception($"Unable to determine duration of audio: {audioPath}\nOutput: {output}\nError: {error}");
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