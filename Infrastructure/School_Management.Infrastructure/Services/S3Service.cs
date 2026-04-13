using Amazon.S3;
using Amazon.S3.Model;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Infrastructure.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ISettingsService _settingsService;
        private readonly string _bucketName;

        public S3Service(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            string accessKey = Env.Get("ACCESS_KEY");
            string secretKey = Env.Get("SECRET_KEY");

            Settings settings = _settingsService.GetAllSettings();

            _s3Client = new AmazonS3Client(
                    accessKey,
                    secretKey,
                    new AmazonS3Config()
                    {
                        ServiceURL = $"https://t3.storage.dev",
                        ForcePathStyle = true
                    }
                );
            _bucketName = settings.BucketName;
        }

        /// <summary>
        /// Uploads a file to the online bucket
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<ReturnResponse> UploadFile(string filePath, string? folder = null)
        {
            string fileName = Path.GetFileName(filePath);
            PutObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = folder == null ? fileName : $"{folder}/{fileName}",
                FilePath = filePath,
            };

            try
            {
                await _s3Client.PutObjectAsync(request);
                return new() { Status = ReturnStatus.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = ReturnStatus.Failed, Message = $"There's an error when trying to upload the file!\n{ex.Message}" };
            }
        }

        /// <summary>
        /// Deletes a file with a specified key
        /// </summary>
        /// <param name="fileKey"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<ReturnResponse> DeleteFile(string fileKey, string? folder = null)
        {
            DeleteObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = folder == null ? fileKey : $"{folder}/{fileKey}",
            };
            try
            {
                await _s3Client.DeleteObjectAsync(request);
                return new() { Status = ReturnStatus.Success };
            }
            catch (Exception ex)
            {
                return new() { Status = ReturnStatus.Failed, Message = $"There's an error when trying to delete the file!\n{ex.Message}" };
            }
        }

        /// <summary>
        /// Download a file from the bucket
        /// </summary>
        /// <param name="fileKey">Key of the file</param>
        /// <param name="savePath">Downloaded file will be placed inside the specified save path</param>
        /// <param name="folder">Folder that contains the file with the specified key</param>
        /// <returns></returns>
        public async Task<ReturnResponse> DownloadFile(string fileKey, string savePath, string? folder = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileKey) || string.IsNullOrWhiteSpace(savePath))
                {
                    return new() { Status = ReturnStatus.Failed, Message = "File key or save path cannot be empty." };
                }

                Directory.CreateDirectory(savePath);

                string fullFilePath = Path.Combine(savePath, fileKey);

                GetObjectRequest request = new()
                {
                    BucketName = _bucketName,
                    Key = folder == null ? fileKey : $"{folder}/{fileKey}",
                };

                using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
                await response.WriteResponseStreamToFileAsync(fullFilePath, false, default);
                return new() { Status = ReturnStatus.Success };
            }
            catch (UnauthorizedAccessException ex)
            {
                return new() { Status = ReturnStatus.Failed, Message = $"Access denied when downloading file. Check folder permissions.\n{ex.Message}" };
            }
            catch (Exception ex)
            {
                return new() { Status = ReturnStatus.Failed, Message = $"There's an error when trying to download the file\n{ex.Message}" };
            }
        }
    }
}
