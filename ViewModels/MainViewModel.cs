using Microsoft.Win32;
using System.ComponentModel;
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
        private bool _isBusy;

        public MainViewModel()
        {
            _audioService = new AudioService();
            _whisperService = new WhisperService();
            SelectVideoCommand = new RelayCommand(async (o) => await ProcessVideo());
            Status = "Ready to start.";
        }

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

        // Disables button while processing
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); } // You can bind Button.IsEnabled to !IsBusy
        }

        public ICommand SelectVideoCommand { get; }

        private async Task ProcessVideo()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.avi|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsBusy = true;
                Transcript = ""; // Clear previous
                Status = "Extracting audio...";

                try
                {
                    string videoPath = openFileDialog.FileName;

                    // 1. Extract Audio
                    string audioPath = await _audioService.ExtractAudioAsync(videoPath);

                    Status = "Transcribing (This may take a while)...";

                    // 2. Transcribe
                    // Create a progress reporter to update UI in real-time
                    var progress = new Progress<string>(line =>
                    {
                        Transcript += line + "\n";
                    });

                    await _whisperService.TranscribeAsync(audioPath, progress);

                    Status = "Done! Transcript saved or ready to copy.";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    Status = "Failed.";
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}