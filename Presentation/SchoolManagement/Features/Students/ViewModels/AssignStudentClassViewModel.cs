using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Presentation.Shared.Features.Classes.Observables;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public class AssignStudentClassParams : INavigationParams
    {
        public required Student Student { get; set; }
    }

    public partial class AssignStudentClassViewModel : ObservableObject, IViewModel, IAsyncLoadable, INavigationAware
    {
        private readonly IClassService _classService;
        private readonly IStudentClassService _studentClassService;
        private readonly IUserSessionService _userSessionService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMessageService _messageService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Student _student = null!;

        [ObservableProperty]
        private string _studentName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ClassCheckItem> _availableClasses = [];

        [ObservableProperty]
        private bool _showInactiveClasses;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _canAssignClass;

        public AssignStudentClassViewModel(
            IClassService classService,
            IStudentClassService studentClassService,
            IUserSessionService userSessionService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService)
        {
            _classService = classService;
            _studentClassService = studentClassService;
            _userSessionService = userSessionService;
            _authorizationService = authorizationService;
            _messageService = messageService;
            _navigationService = navigationService;
        }

        public async Task LoadAsync()
        {
            if (Student == null) return;

            await CheckPermissions();
            await LoadClassesAsync();
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is AssignStudentClassParams p)
            {
                Student = p.Student;
                StudentName = Student.Candidate?.FullName ?? $"Student #{Student.Id}";
            }
        }

        private async Task CheckPermissions()
        {
            ReturnResponse response = await _authorizationService.AuthorizeAsync(null, PermissionType.EditStudents);
            CanAssignClass = response.Status == Status.Success;

            if (!CanAssignClass)
            {
                _messageService.Show("You do not have permission to assign students to classes.", "No Permission", MessageButton.OK, MessageIcon.Exclamation);
            }
        }

        private List<FilterCondition<Class>> BuildClassFilter(User user)
        {
            List<FilterCondition<Class>> filters = [];

            if (user.IsAdmin())
            {
                if (!ShowInactiveClasses)
                {
                    filters.Add(new(c => c.Generation.Department.IsActive, FilterOperator.Equals, true));
                }
            }
            else if (user.IsHeadTeacher())
            {
                int departmentId = user.Employee?.DepartmentId ?? -1;
                filters.Add(new(c => c.Generation.DepartmentId, FilterOperator.Equals, departmentId));

                if (!ShowInactiveClasses)
                {
                    filters.Add(new(c => c.Generation.Department.IsActive, FilterOperator.Equals, true));
                }
            }
            else
            {
                int teacherId = user.Employee?.Id ?? -1;
                filters.Add(new(c => c.TeacherId, FilterOperator.Equals, teacherId));

                if (!ShowInactiveClasses)
                {
                    filters.Add(new(c => c.Generation.Department.IsActive, FilterOperator.Equals, true));
                }
            }

            return filters;
        }

        private async Task LoadClassesAsync()
        {
            if (!CanAssignClass) return;

            IsLoading = true;

            try
            {
                User? currentUser = _userSessionService.CurrentUser;
                if (currentUser == null)
                {
                    _messageService.Show("Current user session hasn't been set!");
                    return;
                }

                IEnumerable<FilterCondition<Class>> filter = BuildClassFilter(currentUser);

                ReturnResponse<IEnumerable<Class>> classResponse = await _classService.GetAllAsync(
                    filters: filter,
                    orderBy: [new SortCriteria<Class>("Grade.Name"), new SortCriteria<Class>("Generation.CohortNumber")]);

                if (classResponse.Status != Status.Success)
                {
                    _messageService.Show(classResponse.Message ?? "Failed to load classes");
                    return;
                }

                List<Class> allClasses = (classResponse.Value ?? []).ToList();

                ReturnResponse<IEnumerable<StudentClass>> enrollmentResponse = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.StudentId, FilterOperator.Equals, Student.Id)]);

                HashSet<int> enrolledClassIds = [];
                if (enrollmentResponse.Status == Status.Success && enrollmentResponse.Value != null)
                {
                    foreach (StudentClass sc in enrollmentResponse.Value)
                    {
                        if (sc.ClassId > 0)
                            enrolledClassIds.Add(sc.ClassId);
                    }
                }

                AvailableClasses.Clear();
                foreach (Class cls in allClasses)
                {
                    AvailableClasses.Add(new ClassCheckItem
                    {
                        Class = cls,
                        IsChecked = enrolledClassIds.Contains(cls.Id)
                    });
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while loading classes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async partial void OnShowInactiveClassesChanged(bool value)
        {
            await LoadClassesAsync();
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!CanAssignClass) return;

            IsLoading = true;

            try
            {
                List<ClassCheckItem> checkedItems = AvailableClasses.Where(c => c.IsChecked).ToList();
                HashSet<int> checkedIds = checkedItems.Select(c => c.Class!.Id).ToHashSet();

                ReturnResponse<IEnumerable<StudentClass>> existingResponse = await _studentClassService.GetAllAsync(
                    filters: [new FilterCondition<StudentClass>(sc => sc.StudentId, FilterOperator.Equals, Student.Id)]);

                if (existingResponse.Status != Status.Success)
                {
                    _messageService.Show("Failed to load existing enrollments.");
                    return;
                }

                List<StudentClass> existingEnrollments = (existingResponse.Value ?? []).ToList();
                HashSet<int> existingClassIds = existingEnrollments.Select(e => e.ClassId).ToHashSet();

                foreach (ClassCheckItem item in checkedItems)
                {
                    if (!existingClassIds.Contains(item.Class!.Id))
                    {
                        StudentClass enrollment = new()
                        {
                            StudentId = Student.Id,
                            ClassId = item.Class.Id,
                            StartDate = DateOnly.FromDateTime(DateTime.Now),
                            IsActive = true
                        };

                        ReturnResponse insertResponse = await _studentClassService.InsertAsync(enrollment);
                        if (insertResponse.Status != Status.Success)
                        {
                            _messageService.Show($"Failed to enroll in class '{item.Class.GetName()}': {insertResponse.Message}");
                        }
                    }
                }

                foreach (StudentClass enrollment in existingEnrollments)
                {
                    if (!checkedIds.Contains(enrollment.ClassId))
                    {
                        ReturnResponse deleteResponse = await _studentClassService.DeleteAsync(enrollment);
                        if (deleteResponse.Status != Status.Success)
                        {
                            _messageService.Show($"Failed to remove class enrollment (ID: {enrollment.ClassId}): {deleteResponse.Message}");
                        }
                    }
                }

                _messageService.Show("Class assignment saved successfully!", "Success", MessageButton.OK, MessageIcon.Success);

                await GoBack();
            }
            catch (Exception ex)
            {
                _messageService.Show($"An error occurred while saving: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            MessageResult result = _messageService.Show("Are you sure you want to cancel? Changes will not be saved.", "Confirm", MessageButton.YesNo, MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            await GoBack();
        }

        private async Task GoBack()
        {
            IViewModel? previous = _navigationService.PreviousViewModel;

            await _navigationService.NavigateAsync<StudentListViewModel>();
        }
    }
}
