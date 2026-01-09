using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace VideoToTextApp.Services
{
    public class AudioService
    {
        public AudioService()
        {
            // Point Xabe.FFmpeg to the folder where you put ffmpeg.exe
            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
            FFmpeg.SetExecutablesPath(ffmpegPath);
        }

        public async Task<string> ExtractAudioAsync(string videoPath)
        {
            string outputWavPath = Path.ChangeExtension(videoPath, ".wav");

            if (File.Exists(outputWavPath))
                File.Delete(outputWavPath);

            // Get media info
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

            // FIX: Use .AddParameter to manually pass standard FFmpeg arguments
            // -vn : No Video
            // -ac 1 : Audio Channels 1 (Mono)
            // -ar 16000 : Audio Rate 16000Hz (Required by Whisper)
            // -c:a pcm_s16le : Codec PCM 16-bit Little Endian (Standard WAV)

            var conversion = FFmpeg.Conversions.New()
                .AddStream(mediaInfo.AudioStreams)
                .SetOutput(outputWavPath)
                .AddParameter("-vn -ac 1 -ar 16000 -c:a pcm_s16le");

            await conversion.Start();

            return outputWavPath;
        }

        public async Task<TimeSpan> GetVideoDuration(string videoPath)
        {
            var info = await FFmpeg.GetMediaInfo(videoPath);
            return info.Duration;
        }
    }
}