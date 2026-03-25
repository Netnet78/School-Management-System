using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using School_Management.Core.Interfaces;

namespace School_Management.Infrastructure.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        IConfiguration config = new ConfigurationBuilder()
            .AddUserSecrets<S3Service>()
            .Build();

        public S3Service()
        {
            _s3Client = new AmazonS3Client(
                    config["ACCESS_KEY"],
                    config["SECRET_KEY"],
                    new AmazonS3Config()
                    {
                        ServiceURL = $"https://t3.storage.dev",
                        ForcePathStyle = true
                    }
                );
            _bucketName = config["BucketName"] ?? "";
        }

        /// <summary>
        /// Uploads a file to the online bucket
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task UploadFile(string filePath, string? folder = null)
        {
            string fileName = Path.GetFileName(filePath);
            PutObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = folder == null ? fileName : $"{folder}/{fileName}",
                FilePath = filePath,
            };

            await _s3Client.PutObjectAsync(request);
        }

        /// <summary>
        /// Deletes a file with a specified key
        /// </summary>
        /// <param name="fileKey"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task DeleteFile(string fileKey, string? folder = null)
        {
            DeleteObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = folder == null ? fileKey : $"{folder}/{fileKey}",
            };
            await _s3Client.DeleteObjectAsync(request);
        }

        /// <summary>
        /// Download a file from the bucket
        /// </summary>
        /// <param name="fileKey">Key of the file</param>
        /// <param name="savePath">Downloaded file will be placed inside the specified save path</param>
        /// <param name="folder">Folder that contains the file with the specified key</param>
        /// <returns></returns>
        public async Task DownloadFile(string fileKey, string savePath, string? folder = null)
        {
            GetObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = folder == null ? fileKey : $"{folder}/{fileKey}",
            };
            using var response = await _s3Client.GetObjectAsync(request);

            await response.WriteResponseStreamToFileAsync(savePath, false, default);
        }
    }
}
