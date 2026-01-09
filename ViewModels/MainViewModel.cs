using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VideoToTextApp.Services;

namespace VideoToTextApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AudioService _audioService;
        private readonly WhisperService _whisperService;

        private string _transcript;
        private string _status;
        private double _progressValue; // 0 to 100
        private bool _isBusy;

        public MainViewModel()
        {
            _audioService = new AudioService();
            _whisperService = new WhisperService();

            SelectVideoCommand = new RelayCommand(async (o) => await ProcessVideo());
            SaveScriptCommand = new RelayCommand(SaveScript, (o) => !string.IsNullOrEmpty(Transcript));

            Status = "Ready to start.";
        }

        // --- Properties ---
        public string Transcript
        {
            get => _transcript;
            set { _transcript = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // --- Commands ---
        public ICommand SelectVideoCommand { get; }
        public ICommand SaveScriptCommand { get; }

        // --- Logic ---
        private async Task ProcessVideo()
        {
            var openDialog = new OpenFileDialog { Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov" };
            if (openDialog.ShowDialog() != true) return;

            try
            {
                IsBusy = true;
                Transcript = "";
                ProgressValue = 0;
                string videoPath = openDialog.FileName;

                // 1. Get Duration for Progress Calculation
                Status = "Analyzing video...";
                var duration = await _audioService.GetVideoDuration(videoPath);

                // 2. Extract Audio
                Status = "Extracting audio (please wait)...";
                string audioPath = await _audioService.ExtractAudioAsync(videoPath);

                // 3. Transcribe with Progress
                Status = "Transcribing...";

                // We create a simpler progress handler here
                var progressReporter = new Progress<string>(segmentText =>
                {
                    // Whisper returns segments incrementally. 
                    // We append to the text box immediately.
                    Transcript += segmentText + Environment.NewLine;

                    // Simple progress estimation:
                    // If you want precise percentage, you need to change WhisperService to report TimeStamp
                    // For now, let's just show indeterminate activity or update text.
                });

                await _whisperService.TranscribeAsync(audioPath, progressReporter);

                Status = "Done!";
                ProgressValue = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                Status = "Error occurred.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SaveScript(object obj)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Text File|*.txt|Subtitle File|*.srt",
                FileName = "Transcript"
            };

            if (saveDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveDialog.FileName, Transcript);
                Status = "File Saved.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}