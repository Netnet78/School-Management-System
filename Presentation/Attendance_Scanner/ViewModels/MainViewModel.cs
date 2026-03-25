using AForge.Video;
using AForge.Video.DirectShow;
using Attendance_Scanner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Models;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;
using School_Management.Presentation.Shared.Helpers;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace Attendance_Scanner.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IMessageService _messageService;
        private readonly IAttendanceQRService _qrService;
        private readonly SoundPlayer successPlayer = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources/Audio/sfx/scan_success.wav"));
        private readonly SoundPlayer errorPlayer = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources/Audio/sfx/scan_error.wav"));

        // Students Data
        [ObservableProperty]
        private string currentPhotoPath;
        [ObservableProperty]
        private Student currentStudent;
        [ObservableProperty]
        private StudentClass studentClass;

        // Video Cameras Selection
        [ObservableProperty]
        private VideoCaptureDevice? videoCaptureDevice;
        [ObservableProperty]
        private FilterInfoCollection filterInfoCollection;
        [ObservableProperty]
        private List<string> availableDevices;
        [ObservableProperty]
        private int selectedDevice;
        [ObservableProperty]
        private BitmapSource? bitmapFrame;
        [ObservableProperty]
        private string scannedCode;

        // Last scan time for Scan Cooldown
        private DateTime _lastScanTime = DateTime.MinValue;

        [ObservableProperty]
        private bool isReady;
        [ObservableProperty]
        private bool isLive;

        private readonly int cooldown = 5;

        public MainViewModel(IAttendanceQRService attendanceQRService, IMessageService messageService)
        {
            _qrService = attendanceQRService;
            _messageService = messageService;

            CurrentPhotoPath = "";
            CurrentStudent = new();
            StudentClass = new();

            FilterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice = new();
            AvailableDevices = [];
            SelectedDevice = 0;
            BitmapFrame = null;
            foreach (FilterInfo device in FilterInfoCollection)
            {
                AvailableDevices.Add(device.Name);
            }

            IsReady = true;
            IsLive = false;

            ScannedCode = "";

            errorPlayer.Load();
            successPlayer.Load();

            Debug.WriteLine(successPlayer.SoundLocation);
            Debug.WriteLine(File.Exists(successPlayer.SoundLocation));
        }

        [RelayCommand]
        private async Task StartScanAsync()
        {
            IsLive = true;
        }

        [RelayCommand]
        private async Task StopScanAsync()
        {
            IsLive = false;
        }

        [RelayCommand]
        private void RefreshValue()
        {
            ScannedCode = string.Empty;
        }

        [RelayCommand]
        private void OnAppClosing()
        {
            if (VideoCaptureDevice != null && VideoCaptureDevice.IsRunning)
            {
                VideoCaptureDevice.NewFrame -= VideoCaptureDeviceNewFrame;
                VideoCaptureDevice.SignalToStop();   // Signals the thread to stop
                VideoCaptureDevice.WaitForStop();     // Waits for thread to finish
                VideoCaptureDevice = null;
            }
        }

        private void VideoCaptureDeviceNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!IsReady) return;

            BitmapSource bitmapSource = ((Bitmap)eventArgs.Frame.Clone()).ConvertToBitmapsource();

            App.Current.Dispatcher.Invoke(() =>
            {
                BitmapFrame = bitmapSource;
            });

            BarcodeReader<BitmapSource> reader = new(
                b => new BitmapSourceLuminanceSource(b)
            )
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                }
            };
            //QRCodeReader reader = new();
            //Result? result = reader.Decode(bitmapSource);
            Result? result = reader.Decode(bitmapSource);
            if (result != null && result.Text != ScannedCode)
            {
                if ((DateTime.Now - _lastScanTime).TotalSeconds < cooldown) //Cooldown
                {
                    return;
                }
                ScannedCode = result.Text;
            }
        }

        private void ProcessScanAsync(string value)
        {
            Task<Student?> task = _qrService.GetStudentByQRCode(value);
            Student? student = null;

            Task.Run(async() =>
            {
                student = await task;
            });

            if (student == null)
            {
                errorPlayer.Play();
                App.Current.Dispatcher.InvokeAsync(async() =>
                {
                    _messageService.Show("គ្មាន​ទិន្នន័យសិស្សនៃ QR Code មួយនេះទេ! សូមព្យាយាមម្ដងទៀតនៅពេលក្រោយ!\nប្រសិនបើមានបញ្ហាបច្ចេកទេស សូមទាក់ទងជាមួយលោកគ្រូអ្នកគ្រូដើម្បីដោះស្រាយបញ្ហាមួយនេះ",
                                        "ទិន្នន័យមិនស្គាល់", icon: MessageBoxIcon.Error, autoHide: 4);
                });
            }
            else
            {
                successPlayer.Play();
            }

            CurrentStudent = student ?? new();
        }

        partial void OnScannedCodeChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            IsReady = false;
            try
            {
                ProcessScanAsync(value);
            }
            finally
            {
                IsReady = true;
            }
            _lastScanTime = DateTime.Now;
        }
        partial void OnIsLiveChanged(bool value)
        {
            if (value == false)
            {
                if (VideoCaptureDevice != null && VideoCaptureDevice.IsRunning)
                {
                    VideoCaptureDevice.NewFrame -= VideoCaptureDeviceNewFrame;
                    VideoCaptureDevice.SignalToStop();   // Signals the thread to stop
                    VideoCaptureDevice.WaitForStop();     // Waits for thread to finish
                    VideoCaptureDevice = null;
                    BitmapFrame = null;
                }
                return;
            }
            VideoCaptureDevice = new(FilterInfoCollection[SelectedDevice].MonikerString);
            
            VideoCaptureDevice.NewFrame += VideoCaptureDeviceNewFrame;
            VideoCaptureDevice.Start();
        }
    }
}
