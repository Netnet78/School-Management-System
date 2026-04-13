using AForge.Video;
using AForge.Video.DirectShow;
using Attendance_Scanner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Core.Models;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ZXing;
using ZXing.Windows.Compatibility;

namespace Attendance_Scanner.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Internal services
    private readonly IMessageService _messageService;
    private readonly IAttendanceQRService _qrService;
    private readonly IPhotoFetchService _photoFetchService;
    private readonly IUserSessionService _userSessionService;
    private readonly ISoundService _soundService;

    // Sound effects
    private readonly FileObject _successSound = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources/Audio/sfx/scan_success.wav"));
    private readonly FileObject _errorSound = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources/Audio/sfx/scan_error.wav"));

    //Barcode reader 
    //private readonly BarcodeReader<BitmapSource> _reader = new(
    //    b => new BitmapSourceLuminanceSource(b)
    //)
    //{
    //    AutoRotate = true,
    //    Options = new ZXing.Common.DecodingOptions
    //    {
    //        TryHarder = true,
    //    }
    //};
    private readonly IBarcodeReader _barcodeReader;

    // Events
    public event Action? OnSuccessfulScan;

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
    private Bitmap? bitmapFrame;
    [ObservableProperty]
    private string scannedCode;

    // Last scan time for Scan Cooldown
    private DateTime _lastScanTime = DateTime.MinValue;

    [ObservableProperty]
    private bool isReady;
    [ObservableProperty]
    private bool isLive;

    private readonly int cooldown = 5;

    public MainViewModel(
        IAttendanceQRService attendanceQRService,
        IMessageService messageService,
        IPhotoFetchService photoFetchService,
        IUserSessionService userSessionService,
        ISoundService soundService,
        IBarcodeReader barcodeReader)
    {
        _qrService = attendanceQRService;
        _messageService = messageService;
        _photoFetchService = photoFetchService;
        _userSessionService = userSessionService;
        _soundService = soundService;
        _barcodeReader = barcodeReader;

        CurrentPhotoPath = string.Empty;
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

        ScannedCode = string.Empty;

        _soundService.Load(_errorSound);
        _soundService.Load(_successSound);
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (_userSessionService.CurrentUser == null) return;
        IsLive = true;
    }

    [RelayCommand]
    private async Task StopScanAsync()
    {
        if (_userSessionService.CurrentUser == null) return;
        IsLive = false;
    }

    [RelayCommand]
    private void RefreshValue()
    {
        if (_userSessionService.CurrentUser == null) return;
        ScannedCode = string.Empty;
    }

    [RelayCommand]
    private void OnAppClosing()
    {
        if (VideoCaptureDevice != null && VideoCaptureDevice.IsRunning)
        {
            VideoCaptureDevice.NewFrame -= VideoCaptureDeviceNewFrame;
            VideoCaptureDevice.SignalToStop();
            VideoCaptureDevice.WaitForStop();
            VideoCaptureDevice = null;
        }
    }

    private void VideoCaptureDeviceNewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

        App.Current.Dispatcher.Invoke(() =>
        {
            BitmapFrame = bitmap;
        });

        BitmapLuminanceSource source = new(bitmap);

        if (!IsReady) return;

        Result? result = _barcodeReader.Decode(source);
        if (result != null && result.Text != ScannedCode)
        {
            if ((DateTime.Now - _lastScanTime).TotalSeconds < cooldown) //Cooldown
            {
                return;
            }
            ScannedCode = result.Text;
        }
    }

    private async Task ProcessQRAsync(string value)
    {
        IsReady = false;
        try
        {
            StudentQRResponse qrResponse = await _qrService.GetStudentByQRCode(value);

            if (qrResponse.Status == ReturnStatus.Failed || qrResponse.Student == null)
            {
                _soundService.Play(_errorSound);
                CurrentStudent = new();
                CurrentPhotoPath = string.Empty;

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    _messageService.Show(qrResponse.Message, "Scan Failed", icon: MessageIcon.Error, autoHide: 4);
                });
                return;
            }

            _soundService.Play(_successSound);
            CurrentStudent = qrResponse.Student;
            OnSuccessfulScan?.Invoke();

            try
            {
                FileObject? photoPath = await _photoFetchService.GetStudentPhoto(CurrentStudent.Candidate.PhotoKey);
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    CurrentPhotoPath = photoPath?.FilePath ?? string.Empty;
                    Debug.WriteLine($"Photo path set to: {CurrentPhotoPath}");
                    Debug.WriteLine($"Photo exists: {File.Exists(CurrentPhotoPath)}");
                });
            }
            catch (Exception photoEx)
            {
                Debug.WriteLine($"Error fetching photo: {photoEx.GetType().Name}");
                Debug.WriteLine($"Error message: {photoEx.Message}");
                Debug.WriteLine($"Stack trace: {photoEx.StackTrace}");
                Debug.WriteLine($"Inner exception: {photoEx.InnerException?.Message}");
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    CurrentPhotoPath = string.Empty;
                    _messageService.Show($"Unable to load student photo.\n{photoEx.Message}",
                                        "Photo Load Error", icon: MessageIcon.Exclamation, autoHide: 3);
                });
            }
        }
        catch (Exception qrEx)
        {
            Debug.WriteLine($"Error in QR scan: {qrEx.GetType().Name}");
            Debug.WriteLine($"Error message: {qrEx.Message}");
            Debug.WriteLine($"Stack trace: {qrEx.StackTrace}");
            _soundService.Play(_errorSound);
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                _messageService.Show($"An error occurred while processing the QR code.\n{qrEx.Message}",
                                    "Scan Error", icon: MessageIcon.Error, autoHide: 4);
                CurrentStudent = new();
                CurrentPhotoPath = string.Empty;
            });
        }
        finally
        {
            IsReady = true;
        }
    }

    partial void OnScannedCodeChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        _ = ProcessQRAsync(value);
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
