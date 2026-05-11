using SchoolManagement.Core.Application.Interfaces;
using SchoolManagement.Core.Enums;
using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Core.Shared.Models;

namespace SchoolManagement.Application.Services
{
    public class PhotoFetchService : IPhotoFetchService
    {
        private readonly ISettingsService _settings;
        private readonly IS3Service _s3Service;

        public PhotoFetchService(ISettingsService settings, IS3Service s3Service)
        {
            _settings = settings;
            _s3Service = s3Service;
        }

        public async Task<ReturnResponse<FileObject>> GetStudentPhoto(string photoKey, FileLocationOptions location = FileLocationOptions.LocalAndOnline, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(photoKey))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "ទិន្នន័យសម្ងាត់នៃរូបភាពសិស្សមិនអាចទទេបានទេ!"
                };
            }

            Settings config = _settings.GetAllSettings();

            if (string.IsNullOrWhiteSpace(config.StudentPhotoFolderPath))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "ទីតាំងទៅរកបណ្ដុំនៃរូបថតសិស្សមិនអាចទទេបានទេ! សូមធ្វើការកំណត់ទីតាំងទៅរករូបថតសិស្ស ដើម្បីដោះស្រាយបញ្ហានេះ។",
                };
            }

            return await GetPhoto(
                photoKey,
                config.StudentPhotoFolderPath,
                config.StudentPhotoFolderBucketPath,
                location,
                cancellationToken
            );
        }

        public async Task<ReturnResponse<FileObject>> GetEmployeePhoto(
            string photoKey,
            FileLocationOptions location = FileLocationOptions.LocalAndOnline,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(photoKey))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "ទិន្នន័យសម្ងាត់នៃរូបភាពបុគ្គលិកមិនអាចទទេបានទេ!"
                };
            }

            Settings config = _settings.GetAllSettings();

            if (string.IsNullOrWhiteSpace(config.EmployeePhotoFolderPath))
            {
                return new()
                {
                    Status = Status.Rejected,
                    Message = "ទីតាំងទៅរកបណ្ដុំនៃរូបថតបុគ្គលិកមិនអាចទទេបានទេ!"
                };
            }

            return await GetPhoto(
                photoKey,
                config.EmployeePhotoFolderPath,
                config.EmployeePhotoFolderBucketPath,
                location,
                cancellationToken);
        }

        private async Task<ReturnResponse<FileObject>> GetPhoto(string photoKey, string saveFolderPath, string bucketPath, FileLocationOptions location, CancellationToken token)
        {
            Directory.CreateDirectory(saveFolderPath);

            string path = Path.Combine(saveFolderPath, photoKey);

            if (!File.Exists(path) && location != FileLocationOptions.LocalOnly)
            {
                ReturnResponse returnResponse = await _s3Service.DownloadFile(
                    photoKey,
                    saveFolderPath,
                    bucketPath,
                    token
                );

                return new()
                {
                    Message = returnResponse.Message,
                    Status = returnResponse.Status,
                    Value = new(path)
                };
            }

            if (!File.Exists(path))
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = "មិនអាចរកឃើញឯកសាររូបភាពបុគ្គលិកបានទេ!"
                };
            }

            return new()
            {
                Status = Status.Success,
                Value = new(path)
            };
        }
    }
}