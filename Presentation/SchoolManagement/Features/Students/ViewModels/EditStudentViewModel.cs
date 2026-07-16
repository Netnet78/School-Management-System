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

        [ObservableProperty]
        private byte[]? _currentQrCodeImage;

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
            IStudentQRService studentQRService,
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
                fileDialogService,
                studentQRService)
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

                if (_student.StudentQR == null)
                {
                    var qrResponse = await _studentQRService.GetByIdAsync(_student.Id);
                    if (qrResponse.Status == Status.Success)
                    {
                        _student.StudentQR = qrResponse.Value;
                    }
                }

                LoadStudentToForm(_student);
                StudentName = $"{StudentForm.LastName} {StudentForm.FirstName}";

                if (StudentForm.StudentQr != null && !string.IsNullOrWhiteSpace(StudentForm.StudentQr.QRCodeValue))
                {
                    CurrentQrCodeImage = SchoolManagement.Infrastructure.Features.Reports.Export.QRCodeExporter.GenerateQrPng(
                        StudentForm.StudentQr.QRCodeValue, 200, 200);
                }
                else
                {
                    CurrentQrCodeImage = null;
                }

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
                StudentForm.ApplyToStudentModel(_student);

                if (_student.Candidate == null)
                {
                    _messageService.Show("មិនអាចរក្សាទុកបាន ព្រោះព័ត៌មានសិស្សខ្វះ Candidate ទេ!", "មានកំហុសបច្ចេកទេស!", MessageButton.OK, MessageIcon.Hand);
                    return;
                }

                ReturnResponse candidateResponse = await _candidateService.UpdateCandidateAsync(_student.Candidate);

                if (candidateResponse.Status != Status.Success)
                {
                    _messageService.Show(candidateResponse.Message, "ឈប់សិន!", MessageButton.OK, MessageIcon.Hand);
                    return;
                }
                await DeleteStudentPhotoAsync(_student.Candidate);

                await UploadStudentPhotoAsync(_student);

                // Handle QR persistence separately to avoid EF Core conflicts
                StudentQR? pendingQR = StudentForm.StudentQr;
                _student.StudentQR = null;
                ReturnResponse studentUpdateResponse = await _studentService.UpdateAsync(_student);

                if (studentUpdateResponse.Status != Status.Success)
                {
                    _messageService.Show(
                        studentUpdateResponse.Message ?? "មិនអាចកែប្រែទិន្នន័យសិស្សបានទេ!",
                        "មានកំហុសបច្ចេកទេស!",
                        MessageButton.OK,
                        MessageIcon.Error);
                    return;
                }

                if (pendingQR != null)
                {
                    pendingQR.Id = _student.Id;
                    var existingQR = await _studentQRService.GetByIdAsync(_student.Id);
                    if (existingQR.Status == Status.Success && existingQR.Value != null)
                    {
                        existingQR.Value.QRCodeValue = pendingQR.QRCodeValue;
                        existingQR.Value.GeneratedAt = DateTime.UtcNow;
                        await _studentQRService.UpdateAsync(existingQR.Value);
                    }
                    else
                    {
                        await _studentQRService.InsertAsync(pendingQR);
                    }
                    _student.StudentQR = pendingQR;
                }

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

        [RelayCommand]
        private void UpdateStudentQR()
        {
            string newValue = Guid.NewGuid().ToString();

            if (StudentForm.StudentQr == null)
            {
                // We need the student Id so we can correctly save it later.
                // _student may not be set if OnNavigatedTo hasn't fired yet, guard just in case.
                StudentForm.StudentQr = new StudentQR()
                {
                    Id = _student?.Id ?? 0,
                    QRCodeValue = newValue,
                    IsActive = true,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            else
            {
                StudentForm.StudentQr.QRCodeValue = newValue;
                StudentForm.StudentQr.GeneratedAt = DateTime.UtcNow;
            }

            CurrentQrCodeImage = Infrastructure.Features.Reports.Export.QRCodeExporter.GenerateQrPng(
                newValue, 200, 200);

            OnPropertyChanged(nameof(StudentForm));
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
