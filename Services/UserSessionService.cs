using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace New_Student_Management.Services
{
    public interface IUserSessionService : INotifyPropertyChanged
    {
        string Username { get; }
        string Role { get; }
        DateTime LoginTime { get; }
        void SetUserSession(string userName, string role);
    }
    public sealed class UserSessionService : IUserSessionService
    {
        private string _username = string.Empty;
        private string _role = string.Empty;
        private DateTime _loginTime = DateTime.MinValue;

        public string Username
        {
            get => _username;
            private set
            {
                if (value != _username)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Role
        {
            get => _role; 
            private set
            {
                if (value != _role)
                {
                    _role = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime LoginTime
        {
            get => _loginTime; 
            private set
            {
                if (value != _loginTime)
                {
                    _loginTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public void SetUserSession(string userName, string role)
        {
            Username = userName;
            Role = role;
            LoginTime = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
