using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Presentation.Contracts;

namespace CandidateManagement.ViewModels
{
    public partial class MainFormViewModel : ObservableObject, IViewModel
    {
        private readonly IUserSessionService _userSessionService;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the candidate management app.";

        public MainFormViewModel(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
            _userSessionService.OnUserSessionChanged += OnUserSessionChanged;

            SetWelcomeMessage(_userSessionService.CurrentUser);
        }

        private void OnUserSessionChanged(User? user)
        {
            SetWelcomeMessage(user);
        }

        private void SetWelcomeMessage(User? user)
        {
            WelcomeMessage = user == null
                ? "សូមស្វាគមន៍មកកាន់កម្មវិធីគ្រប់គ្រងសិស្សានុសិស្សបេក្ខជនដុនបូស្កូប៉ោយប៉ែត!,"
                : $"{user.Username}. សូមចុចលើប៊ូតុងនៅខាងឆ្វេងណាមួយដើម្បីធ្វើការបើកមើលផ្ទាំងផ្សេងៗ។";
        }
    }
}
