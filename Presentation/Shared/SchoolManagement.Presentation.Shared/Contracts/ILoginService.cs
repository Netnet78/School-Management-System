namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface ILoginService
    {
        Task<bool?> ShowLoginWindow<TViewModel>()
            where TViewModel : IViewModel;
    }
}
