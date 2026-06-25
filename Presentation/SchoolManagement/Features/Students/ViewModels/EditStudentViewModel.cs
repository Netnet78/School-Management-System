using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolManagement.Application.Features.Classes.Authorization;
using SchoolManagement.Presentation.Shared.Features.Classes.Observables;
using SchoolManagement.Presentation.Shared.Features.Students.Observables;
using SchoolManagement.Presentation.Shared.Features.Students.Params;

namespace SchoolManagement.Presentation.Features.Students.ViewModels
{
    public partial class EditStudentViewModel : StudentFormViewModelBase, INavigationAware
    {
        private readonly ICandidateService _candidateService;

        private Student? _student;

        [ObservableProperty]
        private string _studentName = string.Empty;

        public EditStudentViewModel(
            ICandidateService candidateService,
            IStudentService studentService,
            IPhotoUploadService photoUploadService,
            IPhotoDeleteService photoDeleteService,
            IMessageService messageService,
            IAuthorizationService authorizationService,
            INavigationService navigationService,
            ISkillService skillService,
            IClassService classService,
            IStudentClassService studentClassService,
            IFileDialogService fileDialogService,
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
            _candidateService = candidateService;
        }

        public async Task OnNavigatedToAsync(INavigationParams @params)
        {
            if (@params is not EditStudentNavigationParams p || p.Student == null)
            {
                _messageService.Show("មិនអាចរកឃើញទិន្នន័យសិស្សដែលត្រូវកែប្រែនោះទេ!", "មានកំហុសបច្ចេកទេស!", icon: MessageIcon.Error);
                return;
            }

            _student = p.Student;
        }

        public override async Task LoadAsync()
        {
            if (_student == null)
            {
                return;
            }

            IsLoading = true;

            try
            {
                await LoadCommonAuthorizationAsync(PermissionType.EditStudents);

                if (!CanEdit)
                {
                    return;
                }

                await LoadSkillsAsync();
                LoadStudentToForm(_student);
                StudentName = $"{StudentForm.LastName} {StudentForm.FirstName}";

                IsPhotoLoading = true;
                CurrentPhoto = null;
                ReturnResponse<FileObject> photoResponse = await _photoFetchService.GetStudentPhoto(StudentForm.PhotoKey);

                if (photoResponse.Status == Status.Success)
                {
                    CurrentPhoto = photoResponse.Value?.FilePath;
                }

                IsPhotoLoading = false;

                User? user = _authorizationService.CurrentUser;
                if (user == null)
                {
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }
                CanAssignClass = false;

                if (user.IsAdmin())
                {
                    CanAssignClass = true;
                    CanSetSkill = true;

                    await LoadAvailableClassesAsync(departmentId: null);

                    HashSet<int> enrolledIds = await LoadExistingEnrollmentsAsync(_student.Id);

                    foreach (ClassCheckItem item in AvailableClasses)
                    {
                        if (item.Class != null && enrolledIds.Contains(item.Class.Id))
                        {
                            item.IsChecked = true;
                        }
                    }
                }

                if (user.IsHeadTeacher())
                {
                    CanAssignClass = true;
                    int? departmentId = user.Employee?.DepartmentId;

                    if (departmentId.HasValue)
                    {
                        await LoadAvailableClassesAsync(departmentId);

                        HashSet<int> enrolledIds = await LoadExistingEnrollmentsAsync(_student.Id);

                        foreach (ClassCheckItem item in AvailableClasses)
                        {
                            if (item.Class != null && enrolledIds.Contains(item.Class.Id))
                            {
                                item.IsChecked = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanEdit)
            {
                _messageService.Show("អ្នកគ្មានសិទ្ធិក្នុងការកែប្រែទិន្នន័យសិស្សបានទេ!", "ឈប់សិន!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            if (_student == null)
            {
                _messageService.Show("មិនអាចរក្សាទុកទិន្នន័យសិស្សដែលមិនមាននៅក្នុងប្រព័ន្ធនោះទេ!", "មានកំហុសបច្ចេកទេស!", MessageButton.OK, MessageIcon.Hand);
                return;
            }

            IsLoading = true;

            try
            {
                Student updatedStudent = StudentForm.ToStudentModel();

                if (updatedStudent.Candidate == null)
                {
                    _messageService.Show("មិនអាចរក្សាទុកបាន ព្រោះព័ត៌មានសិស្សខ្វះ Candidate ទេ!", "មានកំហុសបច្ចេកទេស!", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                ReturnResponse candidateResponse = await _candidateService.UpdateCandidateAsync(updatedStudent.Candidate);

                if (candidateResponse.Status != Status.Success)
                {
                    _messageService.Show(candidateResponse.Message, "ឈប់សិន!", MessageButton.OK, MessageIcon.Hand);
                    return;
                }
                await DeleteStudentPhotoAsync(updatedStudent.Candidate);

                await UploadStudentPhotoAsync(updatedStudent);

                await _studentService.UpdateAsync(updatedStudent);

                User? user = _authorizationService.CurrentUser;
                if (user == null)
                {
                    _messageService.Show("Unable to determine the current user.", "Error", MessageButton.OK, MessageIcon.Error);
                    return;
                }

                if (user.IsHeadTeacher() || user.IsAdmin())
                {
                    HashSet<int> existingClassIds = await LoadExistingEnrollmentsAsync(_student.Id);

                    foreach (ClassCheckItem item in AvailableClasses.Where(c => c.IsChecked))
                    {
                        if (item.Class != null && !existingClassIds.Contains(item.Class.Id))
                        {
                            DateTime now = DateTime.Now;

                            await _studentClassService.InsertAsync(new StudentClass
                            {
                                StudentId = _student.Id,
                                ClassId = item.Class.Id,
                                StartDate = DateOnly.FromDateTime(DateTime.Now),
                                IsActive = true
                            });
                        }
                    }

                    foreach (StudentClass sc in await GetExistingEnrollmentsAsync(_student.Id))
                    {
                        if (!AvailableClasses.Any(c => c.IsChecked && c.Class?.Id == sc.ClassId))
                        {
                            await _studentClassService.DeleteAsync(sc);
                        }
                    }
                }

                _messageService.Show("បានកែប្រែទិន្នន័យសិស្សដោយជោគជ័យ!", "ជោគជ័យ", icon: MessageIcon.Success);
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                _messageService.Show("មានបញ្ហាបច្ចេកទេសមួយ អំឡុងពេលដែលកែប្រែទិន្នន័យសិស្ស!\nError Message:" +
                    $"\n{ex.Message}", "ប្រព័ន្ធមានបញ្ហា!", icon: MessageIcon.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<IEnumerable<StudentClass>> GetExistingEnrollmentsAsync(int studentId)
        {
            var response = await _studentClassService.GetAllAsync(
                filters: [new FilterCondition<StudentClass>(sc => sc.StudentId, FilterOperator.Equals, studentId)]);

            return response.Status == Status.Success && response.Value != null
                ? response.Value
                : [];
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            _student = null;
            await GoBackAsync();
        }
    }
}
