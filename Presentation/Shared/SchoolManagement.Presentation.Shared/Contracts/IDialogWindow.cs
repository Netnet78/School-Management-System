namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface IDialogWindow
    {
        event Action<bool?>? OnDialogClosed;
        event Action? OnDialogOpened;
        bool? OpenDialog(IViewModel? viewModel = null);
    }
}
