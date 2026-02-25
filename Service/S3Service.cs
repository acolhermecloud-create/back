using Amazon;
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

        public S3Service(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetFileUrlByFileNameKey(string fileNameKey)
        {
            var credentials = new BasicAWSCredentials(_configuration["S3:KEY_ACCESS_1"], _configuration["S3:KEY_ACCESS_2"]);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.SAEast1);

            var preSignedRequest = new GetPreSignedUrlRequest
            {
                BucketName = _configuration["S3:BUCKET_NAME"],
                Key = fileNameKey,
                Expires = DateTime.Now.AddDays(1),
                Verb = HttpVerb.GET
            };

            var objectUrl = s3Client.GetPreSignedURL(preSignedRequest);

            return objectUrl;
        }

        public async Task<string> SendStreamFileToS3(Stream stream, string extension)
        {
            var credentials = new BasicAWSCredentials(
                   _configuration["S3:KEY_ACCESS_1"], _configuration["S3:KEY_ACCESS_2"]);

            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.SAEast1);
            var bucketName = _configuration["S3:BUCKET_NAME"];

            TransferUtility fileTransferUtility = new(s3Client);

            string timestamp = Functions.GenerateTimeStampStrUnique().Replace(",", "").Replace(".", "");
            string filenameKey = $"{timestamp}{extension}";

            var request = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = bucketName,
                Key = filenameKey
            };
            await fileTransferUtility.UploadAsync(request);

            return filenameKey;
        }

        public async Task DeleteFileByFileNameKey(string fileNameKey)
        {
            var credentials = new BasicAWSCredentials(_configuration["S3:KEY_ACCESS_1"], _configuration["S3:KEY_ACCESS_2"]);
            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.SAEast1);

            await s3Client.DeleteObjectAsync(_configuration["S3:BUCKET_NAME"], fileNameKey);
        }
    }
}
