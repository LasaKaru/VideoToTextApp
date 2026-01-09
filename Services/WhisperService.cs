using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace VideoToTextApp.Services
{
    public class WhisperService
    {
        // We look for the model in a "models" folder next to the .exe
        private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "ggml-small.en.bin");

        public async Task<string> TranscribeAsync(string audioPath, IProgress<string> progress)
        {
            if (!File.Exists(_modelPath))
            {
                throw new FileNotFoundException($"Model not found at: {_modelPath}. Please download ggml-small.en.bin.");
            }

            using var factory = WhisperFactory.FromPath(_modelPath);

            // Configure the processor
            using var processor = factory.CreateBuilder()
                .WithLanguage("en") // Force English
                .Build();

            using var fileStream = File.OpenRead(audioPath);
            var sb = new StringBuilder();

            // Process the audio file
            await foreach (var segment in processor.ProcessAsync(fileStream))
            {
                // Format: [00:00:00 -> 00:00:05] Text
                var time = segment.Start;
                var line = $"[{time:hh\\:mm\\:ss}] {segment.Text}";

                sb.AppendLine(line);

                // Send update to UI
                progress.Report(line);
            }

            return sb.ToString();
        }
    }
}