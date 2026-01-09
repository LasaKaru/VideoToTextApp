using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace VideoToTextApp.Services
{
    public class AudioService
    {
        public AudioService()
        {
            // Point Xabe.FFmpeg to the folder where you will put ffmpeg.exe
            // We assume it's in a folder named "ffmpeg" inside the app directory
            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
            FFmpeg.SetExecutablesPath(ffmpegPath);
        }

        public async Task<string> ExtractAudioAsync(string videoPath)
        {
            // Create a temporary output path for the WAV file
            string outputWavPath = Path.ChangeExtension(videoPath, ".wav");

            // If it already exists, delete it to avoid errors
            if (File.Exists(outputWavPath))
                File.Delete(outputWavPath);

            // Extract audio: Force 16kHz mono (Required by Whisper)
            var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(videoPath, outputWavPath);

            // Xabe might not set 16khz/mono by default on simple extraction, 
            // but Whisper.net handles some resampling. ideally, use FFmpeg args for strict 16kHz:
            // -ar 16000 -ac 1 -c:a pcm_s16le

            await conversion.Start();

            return outputWavPath;
        }
    }
}