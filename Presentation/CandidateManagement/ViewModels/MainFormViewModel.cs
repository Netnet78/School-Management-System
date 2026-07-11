using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace CandidateManagement.ViewModels
{
    public partial class MainFormViewModel : ObservableObject, IAsyncLoadable, IViewModel
    {
        private readonly IUserSessionService _userSessionService;
        private readonly ICandidateService _candidateService;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the candidate management app.";

        [ObservableProperty]
        private string _timeMessage = "អារុណសួស្ដី! (Good Morning!)";

        [ObservableProperty]
        private ISeries[] _joinedRateSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _joinedRateXAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private ISeries[] _genderSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _skillSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ObservableCollection<Candidate> _insertedToday = new();

        [ObservableProperty]
        private bool _isJoinedRateDataEmpty = true;

        [ObservableProperty]
        private bool _isGenderDataEmpty = true;

        [ObservableProperty]
        private bool _isSkillDataEmpty = true;

        [ObservableProperty]
        private bool _isInsertedTodayDataEmpty = true;

        [ObservableProperty]
        private bool _hasJoinedRateData = false;

        [ObservableProperty]
        private bool _hasGenderData = false;

        [ObservableProperty]
        private bool _hasSkillData = false;

        [ObservableProperty]
        private bool _hasInsertedTodayData = false;

        [ObservableProperty]
        private int _daysFilter = 7;

        public MainFormViewModel(IUserSessionService userSessionService, ICandidateService candidateService)
        {
            _userSessionService = userSessionService;
            _candidateService = candidateService;
            _userSessionService.OnUserSessionChanged += OnUserSessionChanged;

            SetWelcomeMessage(_userSessionService.CurrentUser);
        }

        async partial void OnDaysFilterChanged(int value)
        {
            await LoadDashboardDataAsync();
        }

        private void OnUserSessionChanged(User? user)
        {
            SetWelcomeMessage(user);
        }

        private void SetWelcomeMessage(User? user)
        {
            WelcomeMessage = user == null
                ? "សូមស្វាគមន៍មកកាន់កម្មវិធីគ្រប់គ្រងសិស្សានុសិស្សបេក្ខជនដុនបូស្កូប៉ោយប៉ែត!"
                : $"{user.Username}. សូមចុចលើប៊ូតុងនៅខាងឆ្វេងណាមួយដើម្បីធ្វើការបើកមើលផ្ទាំងផ្សេងៗ។";

            if (user?.Employee != null)
            {
                WelcomeMessage = $"{user.Employee.FullName}, សូមចុចលើប៊ូតុងនៅខាងឆ្វេងណាមួយដើម្បីធ្វើការបើកមើលផ្ទាំងផ្សេងៗ។";
            }

            if (DateTime.Now.Hour < 12 && DateTime.Now.Hour > 0)
            {
                TimeMessage = "អារុណសួស្ដី! (Good Morning!)";
            }
            else if (DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17)
            {
                TimeMessage = "ទិវាសួស្ដី! (Good Afternoon!)";
            }
            else
            {
                TimeMessage = "សាយ័ន្តសួស្ដី (Good Evening!)";
            }
        }

        public async Task LoadAsync()
        {
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            var response = await _candidateService.GetDashboardMetricsAsync(DaysFilter);
            if (response.Status == Status.Success && response.Value != null)
            {
                var metrics = response.Value;

                // 1. Joined Rate (Line Chart)
                var sortedJoined = metrics.JoinedRate.OrderBy(x => x.Key).ToList();
                JoinedRateSeries = new ISeries[]
                {
                    new LineSeries<int>
                    {
                        Values = sortedJoined.Select(x => x.Value).ToArray(),
                        Name = "សិស្សចុះឈ្មោះ",
                        Fill = null,
                        GeometrySize = 10,
                        LineSmoothness = 0.5
                    }
                };

                JoinedRateXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = sortedJoined.Select(x => x.Key.ToString("dd MMM")).ToArray(),
                        Name = "កាលបរិច្ឆេទ"
                    }
                };

                // 2. Gender Pie Chart
                GenderSeries = metrics.GenderDistribution.Select(g => new PieSeries<int>
                {
                    Values = new[] { g.Value },
                    Name = g.Key == "Male" ? "ប្រុស" : "ស្រី",
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue} នាក់",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White)
                }).ToArray();

                // 3. Skills Pie Chart
                SkillSeries = metrics.SkillDistribution.Select(s => new PieSeries<int>
                {
                    Values = new[] { s.Value },
                    Name = s.Key,
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}",
                    DataLabelsPaint = new SolidColorPaint(SKColors.White)
                }).ToArray();

                // 4. Inserted Today
                InsertedToday = new ObservableCollection<Candidate>(metrics.InsertedToday);

                // 5. Update Empty States
                HasJoinedRateData = metrics.JoinedRate.Any();
                IsJoinedRateDataEmpty = !HasJoinedRateData;

                HasGenderData = metrics.GenderDistribution.Any();
                IsGenderDataEmpty = !HasGenderData;

                HasSkillData = metrics.SkillDistribution.Any();
                IsSkillDataEmpty = !HasSkillData;

                HasInsertedTodayData = metrics.InsertedToday.Any();
                IsInsertedTodayDataEmpty = !HasInsertedTodayData;
            }
        }
    }
}

