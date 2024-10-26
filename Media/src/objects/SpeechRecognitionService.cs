using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whisper;

namespace Media.src.objects
{
    public class SpeechRecognitionService : ISpeechRecognitionService
    {
        private readonly string _videosPath;
        private readonly string _modelPath;

        public SpeechRecognitionService(string videosPath, string modelPath)
        {
            _videosPath = videosPath ?? throw new ArgumentException("Videos path cannot be null or empty.");
            _modelPath = modelPath ?? throw new ArgumentException("Model path cannot be null or empty.");
        }

        public async Task RecognizeVideos()
        {
            var videoFiles = Directory.GetFiles(_videosPath, "*.*", SearchOption.AllDirectories)
                .Where(file => new[] { ".mp4", ".avi", ".mov", ".webm" }.Contains(Path.GetExtension(file).ToLower()))
                .ToList();

            if (videoFiles.Count == 0)
            {
                Console.WriteLine($"No video files found in {_videosPath}");
                return;
            }

            foreach (var video in videoFiles)
            {
                await RecognizeSpeechFromVideo(video);
            }
        }

        private async Task RecognizeSpeechFromVideo(string videoPath)
        {
            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"Video file not found: {videoPath}");
                return;
            }

            var outputDir = Path.Combine(Path.GetDirectoryName(videoPath), "recognized");
            Directory.CreateDirectory(outputDir);
            var outputTextPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(videoPath) + "_recognized.txt");

            string audioFilePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(videoPath) + ".wav");

            // Extract audio from video
            await ExtractAudioFromVideo(videoPath, audioFilePath);

            // Perform speech recognition using Whisper
            await PerformSpeechRecognition(audioFilePath, outputTextPath);
        }

        private async Task ExtractAudioFromVideo(string videoPath, string audioFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{videoPath}\" -ab 160k -ac 2 -ar 44100 -vn \"{audioFilePath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"ffmpeg command failed: {error}");
            }
        }

        private async Task PerformSpeechRecognition(string audioFilePath, string outputTextPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-m whisper \"{audioFilePath}\" --model \"{_modelPath}\" --output \"{outputTextPath}\"",
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

            Console.WriteLine($"whisper output: {output}");
            Console.WriteLine($"whisper error: {error}");

            if (process.ExitCode != 0)
            {
                throw new Exception($"Whisper command failed: {error}");
            }

            if (!File.Exists(outputTextPath))
            {
                throw new Exception($"Recognition completed, but output file not found: {outputTextPath}");
            }
        }
    }
}
