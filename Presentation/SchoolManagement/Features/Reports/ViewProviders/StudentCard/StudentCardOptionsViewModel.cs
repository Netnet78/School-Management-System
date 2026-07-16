using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Presentation.Features.Reports.Contracts;

namespace SchoolManagement.Presentation.Features.Reports.ViewProviders.StudentCard;

public partial class StudentCardOptionsViewModel : ObservableObject, IReportOptionsViewModel
{
    private readonly IFileDialogService _fileDialogService;

    public event Action? OptionsChanged;

    [ObservableProperty]
    private string _principalName = string.Empty;

    [ObservableProperty]
    private string _signaturePath = string.Empty;

    [ObservableProperty]
    private string _location = "វិ.ច.ប.ឯ.ដប.ប៉ប៉";

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    partial void OnPrincipalNameChanged(string value) => OptionsChanged?.Invoke();
    partial void OnLocationChanged(string value) => OptionsChanged?.Invoke();
    partial void OnSignaturePathChanged(string value) => OptionsChanged?.Invoke();
    partial void OnCreatedDateChanged(DateTime value) => OptionsChanged?.Invoke();

    public StudentCardOptionsViewModel(IFileDialogService fileDialogService)
    {
        _fileDialogService = fileDialogService;
    }

    [RelayCommand]
    private void BrowseSignature()
    {
        FileDialogObject fileDialog = _fileDialogService.ShowDialog(
            "Select principal/admin signature",
            false,
            "Image files",
            "png",
            "jpg",
            "jpeg",
            "bmp");

        if (fileDialog.File == null)
        {
            return;
        }

        SignaturePath = fileDialog.File.FilePath;
    }

    public object GetOptionsData()
    {
        return new StudentCardOptions
        {
            PrincipalName = PrincipalName,
            SignaturePath = SignaturePath,
            Location = Location,
            ReportDate = CreatedDate
        };
    }

    public void Reset()
    {
        PrincipalName = string.Empty;
        SignaturePath = string.Empty;
        Location = "វិ.ច.ប.ឯ.ដប.ប៉ប៉";
        CreatedDate = DateTime.Now;
    }
}
