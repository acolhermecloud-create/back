using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Util;

namespace Service
{
    public class S3Service : IS3Service
    {
        private readonly IConfiguration _configuration;
        private readonly AmazonS3Client _s3Client;

        public S3Service(IConfiguration configuration)
        {
            _configuration = configuration;

            var credentials = new BasicAWSCredentials(
                _configuration["S3:KEY_ACCESS_1"],
                _configuration["S3:KEY_ACCESS_2"]
            );

            var config = new AmazonS3Config
            {
                ServiceURL = _configuration["S3:ENDPOINT"], // 👈 URL do MinIO
                ForcePathStyle = true,                      // 👈 obrigatório pro MinIO
                UseHttp = true                              // 👈 true se não tiver SSL
            };

            _s3Client = new AmazonS3Client(credentials, config);
        }

        public async Task<string> GetFileUrlByFileNameKey(string fileNameKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _configuration["S3:BUCKET_NAME"],
                Key = fileNameKey,
                Expires = DateTime.UtcNow.AddDays(1),
                Verb = HttpVerb.GET
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public async Task<string> SendStreamFileToS3(Stream stream, string extension)
        {
            var bucketName = _configuration["S3:BUCKET_NAME"];

            var transferUtility = new TransferUtility(_s3Client);

            string timestamp = Functions.GenerateTimeStampStrUnique()
                .Replace(",", "")
                .Replace(".", "");

            string filenameKey = $"{timestamp}{extension}";

            var request = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = bucketName,
                Key = filenameKey,
                ContentType = "application/octet-stream"
            };

            await transferUtility.UploadAsync(request);

            return filenameKey;
        }

        public async Task DeleteFileByFileNameKey(string fileNameKey)
        {
            await _s3Client.DeleteObjectAsync(
                _configuration["S3:BUCKET_NAME"],
                fileNameKey
            );
        }
    }
}