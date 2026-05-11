using AttendanceScanner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Presentation.Contracts;
using SchoolManagement.Core.Shared.Models;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace AttendanceScanner.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable, IViewModel
{
    // Internal services
    private readonly IMessageService _messageService;
    private readonly IAttendanceQRService _qrService;
    private readonly IPhotoFetchService _photoFetchService;
    private readonly IUserSessionService _userSessionService;
    private readonly ISoundService _soundService;
    private readonly ICameraService _cameraService;
    private readonly IFrameProcessingService _frameService;
    private readonly IQRScannerService _qRScannerService;
    private readonly IDispatcherService _dispatcherService;
    public ILoadingService LoadingService { get; }

    // Sound effects
    private readonly SoundObject _successSound = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources/Audio/sfx/scan_success.wav"));
    private readonly SoundObject _errorSound = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources/Audio/sfx/scan_error.wav"));

    // Events
    public event Action<ReturnResponse>? OnScanStatusChanged;

    // Boolean
    private bool _debugMode = true;

    // Students Data
    [ObservableProperty]
    private string _currentPhotoPath;
    [ObservableProperty]
    private Student _currentStudent;
    [ObservableProperty]
    private StudentClass _studentClass;

    // Video Cameras Selection
    [ObservableProperty]
    private CameraItem[] _availableDevices;
    [ObservableProperty]
    private CameraItem _selectedDevice;
    [ObservableProperty]
    private BitmapSource? _frameSource;
    [ObservableProperty]
    private string _scannedCode;
    private string _lastScannedCode = string.Empty;

    // Last scan time for Scan Cooldown
    private DateTime _lastScanTime = DateTime.MinValue;
    private int _isDecoding = 0;
    private bool _isProcessingScan = false;

    [ObservableProperty]
    private bool _isReady;
    [ObservableProperty]
    private bool _isLive;
    [ObservableProperty]
    private bool _isCameraInitialized;

    private readonly int _cooldown = 3;

    public MainViewModel(
        IAttendanceQRService attendanceQRService,
        IMessageService messageService,
        IPhotoFetchService photoFetchService,
        IUserSessionService userSessionService,
        ISoundService soundService,
        ICameraService cameraService,
        IFrameProcessingService frameProcessingService,
        IQRScannerService qRScannerService,
        IDispatcherService dispatcherService,
        ILoadingService loadingService)
    {
        _qrService = attendanceQRService;
        _messageService = messageService;
        _photoFetchService = photoFetchService;
        _userSessionService = userSessionService;
        _soundService = soundService;
        _cameraService = cameraService;
        _frameService = frameProcessingService;
        _qRScannerService = qRScannerService;
        _dispatcherService = dispatcherService;
        LoadingService = loadingService;

        CurrentPhotoPath = string.Empty;
        CurrentStudent = new();
        StudentClass = new();

        AvailableDevices = _cameraService.GetCameras();
        SelectedDevice = AvailableDevices.First();

        IsReady = true;
        IsLive = false;
        IsCameraInitialized = false;

        ScannedCode = string.Empty;

        _soundService.Load(_errorSound);
        _soundService.Load(_successSound);

        // Camera service updates
        _cameraService.FrameReady += frame =>
        {
            DecodeFrame(frame);

            _dispatcherService.Invoke(() =>
            {
                FrameSource = (WriteableBitmap)_frameService.ProcessFrame(frame);
            });
        };

    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (_userSessionService.CurrentUser == null && !_debugMode) return;
        IsLive = true;
    }

    [RelayCommand]
    private async Task StopScanAsync()
    {
        if (_userSessionService.CurrentUser == null && !_debugMode) return;
        IsLive = false;
    }

    [RelayCommand]
    private void RefreshValue()
    {
        if (_userSessionService.CurrentUser == null && !_debugMode) return;
        ScannedCode = string.Empty;
        _lastScannedCode = string.Empty;
    }

    [RelayCommand]
    private async Task RefreshCameraList()
    {
        AvailableDevices = _cameraService.GetCameras();

        if (AvailableDevices.Length == 0)
        {
            AvailableDevices = [new() { Name = "No cameras found" }];
        }

        SelectedDevice = AvailableDevices.FirstOrDefault(d => d.Name == SelectedDevice.Name) ?? AvailableDevices.FirstOrDefault()!;
    }

    [RelayCommand]
    private void OnAppClosing()
    {
        Dispose();
    }

    private async Task ProcessQRAsync(string value)
    {
        IsReady = false;
        try
        {
            StudentQRResponse qrResponse = await _qrService.GetStudentByQRCode(value);
            
            if (qrResponse.Status == Status.Failed || qrResponse.Student == null)
            {
                _soundService.Play(_errorSound);
                CurrentStudent = new();
                CurrentPhotoPath = string.Empty;

                await _dispatcherService.InvokeAsync(() =>
                {
                    _messageService.Show(qrResponse.Message, "Scan Failed", icon: MessageIcon.Error, autoHide: 4);
                });
                return;
            }

            _soundService.Play(_successSound);
            CurrentStudent = qrResponse.Student;
            OnScanStatusChanged?.Invoke(new()
            {
                Status = Status.Success
            });

            try
            {
                FileObject? photoPath = (await _photoFetchService.GetStudentPhoto(CurrentStudent.PhotoKey)).Value;
                await _dispatcherService.InvokeAsync(() =>
                {
                    CurrentPhotoPath = photoPath?.FilePath ?? string.Empty;
                    if (_debugMode)
                    {
                        Debug.WriteLine($"Photo path set to: {CurrentPhotoPath}");
                        Debug.WriteLine($"Photo exists: {File.Exists(CurrentPhotoPath)}");
                    }
                });
            }
            catch (Exception photoEx)
            {
                if (_debugMode)
                {
                    Debug.WriteLine($"Error fetching photo: {photoEx.GetType().Name}");
                    Debug.WriteLine($"Error message: {photoEx.Message}");
                    Debug.WriteLine($"Stack trace: {photoEx.StackTrace}");
                    Debug.WriteLine($"Inner exception: {photoEx.InnerException?.Message}");
                }
                await _dispatcherService.InvokeAsync(() =>
                {
                    CurrentPhotoPath = string.Empty;
                    _messageService.Show($"Unable to load student photo.\n{photoEx.Message}",
                                        "Photo Load Error", icon: MessageIcon.Exclamation, autoHide: 3);
                });
            }
        }
        catch (Exception qrEx)
        {
            if (_debugMode)
            {
                Debug.WriteLine($"Error in QR scan: {qrEx.GetType().Name}");
                Debug.WriteLine($"Error message: {qrEx.Message}");
                Debug.WriteLine($"Stack trace: {qrEx.StackTrace}");
            }
            _soundService.Play(_errorSound);
            await _dispatcherService.InvokeAsync(() =>
            {
                _messageService.Show($"An error occurred while processing the QR code.\n{qrEx.Message}",
                                    "Scan Error", icon: MessageIcon.Error, autoHide: 4);
                CurrentStudent = new();
                CurrentPhotoPath = string.Empty;
            });
            OnScanStatusChanged?.Invoke(new() { Status = Status.Failed });
        }
        finally
        {
            IsReady = true;
        }
    }

    private async void DecodeFrame(CameraFrame frame)
    {
        if (Interlocked.CompareExchange(ref _isDecoding, 1, 0) != 0) return;

        try
        {
            string? value = _qRScannerService.Decode(frame);

            if (string.IsNullOrWhiteSpace(value)) return;

            await Task.Run(() => HandleScan(value));
            ScannedCode = value;
        }
        finally
        {
            Interlocked.Exchange(ref _isDecoding, 0);
        }
    }

    private async void HandleScan(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        if (_isProcessingScan) return;

        if ((DateTime.Now - _lastScanTime).TotalSeconds <= _cooldown)
            return;

        if (value == _lastScannedCode)
            return;

        _isProcessingScan = true;
        _lastScannedCode = value;

        try
        {
            LoadingService.ShowLoading("សូមប្អូនមេត្តារងចាំ...");

            if (_debugMode)
            {
                await Task.Delay(1000);
            }
            else
            {
                
            }
            await ProcessQRAsync(value);

            LoadingService.ShowSuccess("វត្តមានរបស់ប្អូនត្រូវបានកត់ត្រាទុកជាការស្រេច! ​សូមប្អូនបន្តទៅរកថ្នាក់របស់ប្អូន");
            await Task.Delay(3500);
            LoadingService.Hide();

            _lastScanTime = DateTime.Now;
        }
        finally
        {
            _isProcessingScan = false;
        }
    }

    async partial void OnIsLiveChanged(bool value)
    {
        IsCameraInitialized = false;
        try
        {
            await Task.Run(async () =>
            {
                if (value == false)
                {
                    _cameraService.Stop();
                    return;
                }

                _cameraService.Start(SelectedDevice);
                Debug.WriteLine($"Selected Device name: {SelectedDevice.Name}");
                Debug.WriteLine($"Selected Device moniker string: {SelectedDevice.MonikerString}");
            });
        }
        finally
        {
            IsCameraInitialized = true;
        }
    }

    public void Dispose()
    {
        _cameraService.Stop();
    }
}
