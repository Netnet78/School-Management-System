using AttendanceScanner.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Assets;
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
    private readonly ILoadingService _loadingService;

    private bool _debugMode = false;

    // Sound effects
    private readonly SoundObject _successSound = new(Path.Combine(ResourcePaths.Audio, "sfx", "success-sound.wav"));
    private readonly SoundObject _errorSound = new(Path.Combine(ResourcePaths.Audio, "sfx", "error-sound.wav"));

    // Events
    public event Action<ReturnResponse>? OnScanStatusChanged;

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
        _loadingService = loadingService;

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
        await _dispatcherService.InvokeAsync(() =>
        {
            IsReady = false;
        });

        await _loadingService.ShowLoading("សូមប្អូនមេត្តារងចាំ...");
        try
        {
            StudentQRResponse qrResponse = await _qrService.MarkStudent(value);
            
            if (qrResponse.Status != Status.Success)
            {
                _soundService.Play(_errorSound);
                await _dispatcherService.InvokeAsync(() =>
                {
                    CurrentStudent = new();
                    CurrentPhotoPath = string.Empty;
                });

                await _dispatcherService.InvokeAsync(() =>
                {
                    _messageService.Show(qrResponse.Message, "Scan Failed", icon: MessageIcon.Error, autoHide: 4);
                });
            }
            else
            {
                await _dispatcherService.InvokeAsync(() =>
                {
                    CurrentStudent = qrResponse.Student!;
                });
                OnScanStatusChanged?.Invoke(new()
                {
                    Status = Status.Success
                });

                try
                {
                    ReturnResponse<FileObject> photoPath = await _photoFetchService.GetStudentPhoto(CurrentStudent.PhotoKey);

                    if (photoPath.Status != Status.Success)
                    {
                        await _dispatcherService.InvokeAsync(() =>
                        {
                            _messageService.Show("មិនអាចទាញរកព័ត៌មានរូបភាពនៃសិស្សានុសិស្សនេះទេ!"
                                , "មិនអាចរកឃើញធនធានទេ!", MessageButton.OK, MessageIcon.Exclamation, 3);
                        });
                    }

                    await _dispatcherService.InvokeAsync(() =>
                    {
                        if (_debugMode)
                        {
                            Debug.WriteLine($"Photo path set to: {CurrentPhotoPath}");
                            Debug.WriteLine($"Photo exists: {File.Exists(CurrentPhotoPath)}");
                        }
                        CurrentPhotoPath = photoPath.Value?.FilePath ?? string.Empty;
                    });
                }
                catch (Exception photoEx)
                {
                    Debug.WriteLine($"Error fetching photo: {photoEx.GetType().Name}");
                    Debug.WriteLine($"Error message: {photoEx.Message}");
                    Debug.WriteLine($"Stack trace: {photoEx.StackTrace}");
                    Debug.WriteLine($"Inner exception: {photoEx.InnerException?.Message}");
                    await _dispatcherService.InvokeAsync(() =>
                    {
                        CurrentPhotoPath = string.Empty;
                        _messageService.Show($"Unable to load student photo.\n{photoEx.Message}",
                                            "Photo Load Error", icon: MessageIcon.Exclamation, autoHide: 3);
                    });
                }

                _soundService.Play(_successSound);
                await _loadingService.ShowSuccess("វត្តមានរបស់ប្អូនត្រូវបានកត់ត្រាទុកជាការស្រេច! ​សូមប្អូនបន្តទៅរកថ្នាក់របស់ប្អូន");
                await Task.Delay(3500);
                await _loadingService.Hide();
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
            await _dispatcherService.InvokeAsync(() =>
            {
                IsReady = true;
            });
            await _loadingService.Hide();
        }
    }

    private async void DecodeFrame(BitmapInfo frame)
    {
        if (Interlocked.CompareExchange(ref _isDecoding, 1, 0) != 0) return;

        try
        {
            string? value = await Task.Run(() => _qRScannerService.Decode(frame));

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
            if (_debugMode)
            {
                await _loadingService.ShowLoading("សូមប្អូនមេត្តារងចាំ...");
                await Task.Delay(1000);
                await _loadingService.ShowSuccess("វត្តមានរបស់ប្អូនត្រូវបានកត់ត្រាទុកជាការស្រេច! ​សូមប្អូនបន្តទៅរកថ្នាក់របស់ប្អូន");
                await Task.Delay(3500);
                await _loadingService.Hide();
                return;
            }

            await ProcessQRAsync(value);

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
