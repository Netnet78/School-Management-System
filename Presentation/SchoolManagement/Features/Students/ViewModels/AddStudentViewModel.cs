using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Presentation.Shared.Features.Classes.Observables;
using SchoolManagement.Presentation.Shared.Features.Students.Observables;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public partial class AddStudentViewModel : StudentFormViewModelBase
    {
        public AddStudentViewModel(
            IStudentService studentService,
            ISkillService skillService,
            IAuthorizationService authorizationService,
            IMessageService messageService,
            INavigationService navigationService,
            IPhotoUploadService photoUploadService,
            IClassService classService,
            IStudentClassService studentClassService,
            IFileDialogService fileDialogService,
            IPhotoDeleteService photoDeleteService,
            IPhotoFetchService photoFetchService)
            : base(
                authorizationService,
                messageService,
                navigationService,
                photoUploadService,
                photoDeleteService,
                photoFetchService,
                skillService,
                classService,
                studentClassService,
                studentService,
                fileDialogService)
        {
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (StudentForm.Skill == null)
            {
                _messageService.Show("សូមជ្រើសរើសជំនាញ មុននឹងធ្វើការរក្សាទុក!", "ឈប់សិន! មួយៗ!", MessageButton.OK, MessageIcon.Information);
                return;
            }

            IsLoading = true;

            try
            {
                Student student = StudentForm.ToStudentModel();

                await UploadStudentPhotoAsync(student);

                student.StudentQR = new(student);

                var response = await _studentService.InsertAsync(student);

                if (response.Status == Status.Success)
                {
                    User? user = _authorizationService.CurrentUser;
                    if (user == null)
                    {
                        _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                        return;
                    }

                    if (user.IsValidRole(RoleType.Teacher) && user.EmployeeId.HasValue)
                    {
                        var classesResponse = await _classService.GetAllAsync(
                            filters: [new FilterCondition<Class>(c => c.TeacherId, FilterOperator.Equals, user.EmployeeId)]);

                        if (classesResponse.Status == Status.Success && classesResponse.Value != null)
                        {
                            foreach (Class cls in classesResponse.Value)
                            {
                                await _studentClassService.InsertAsync(new StudentClass
                                {
                                    StudentId = student.Id,
                                    ClassId = cls.Id,
                                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                                    IsActive = true
                                });
                            }
                        }
                    }
                    else if (user.IsHeadTeacher())
                    {
                        foreach (ClassCheckItem item in AvailableClasses.Where(c => c.IsChecked))
                        {
                            if (item.Class != null)
                            {
                                await _studentClassService.InsertAsync(new StudentClass
                                {
                                    StudentId = student.Id,
                                    ClassId = item.Class.Id,
                                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                                    IsActive = true
                                });
                            }
                        }
                    }

                    _messageService.Show("បានបង្កើតសិស្សថ្មីដោយជោគជ័យ!", "ជោគជ័យ", icon: MessageIcon.Success);
                    await GoBackAsync();
                }
                else
                {
                    _messageService.Show(response.Message ?? "មានកំហុសក្នុងការបង្កើតសិស្ស", "Oops!", icon: MessageIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _messageService.Show($"មានកំហុសបច្ចេកទេស: {ex.Message}", "Oops!", icon: MessageIcon.Error);
            }
            finally
            {
                StudentForm = new();
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            MessageResult result = _messageService.Show("តើអ្នកប្រាកដទេថានឹងបោះបង់នៃការបញ្ចូលទិន្នន័យ?", "ឈប់សិន!", MessageButton.YesNo, MessageIcon.Question);

            if (result != MessageResult.Yes) return;

            StudentForm = new();
            await GoBackAsync();
        }

        public override async Task LoadAsync()
        {
            User? user = _authorizationService.CurrentUser;
            if (user == null)
            {
                _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            await LoadCommonAuthorizationAsync(PermissionType.InsertStudents);

            if (!CanEdit)
            {
                return;
            }

            await LoadSkillsAsync();

            CanSetSkill = false;
            CanAssignClass = false;

            if (user.IsAdmin())
            {
                CanSetSkill = true;
                return;
            }

            StudentForm.Skill = user.Employee?.Department?.Skill;

            if (user.IsHeadTeacher())
            {
                CanAssignClass = true;
                await LoadAvailableClassesAsync(user.Employee?.DepartmentId);
            }
        }
    }
}
