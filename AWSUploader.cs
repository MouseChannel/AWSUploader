using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;

namespace AWSUploader
{
    public class AWSConfig
    {

    }
    public class Worker
    {
        public ManualResetEvent signal;
        private string filePath;
        private string AWSKey;
        private FileInfo file;


        public Worker(FileInfo file)
        {
            this.file = file;
        }

        public void Work()
        {
            try
            {
                var putRequest1 = new PutObjectRequest
                {
                    BucketName = AWSUtil.BucketName,
                    Key = AWSUtil.AWSKeyPath + "/" + Path.GetFileName(file.FullName),
                    FilePath = file.FullName

                };


                AWSUtil.client.PutObjectAsync(putRequest1).Wait();
            }
            catch (AmazonS3Exception e)
            {
                throw new System.Exception(
                       "Error encountered ***. Message:'{0}' when writing an object   "
                       + e.Message);
            }
            catch (Exception e)
            {
                throw new System.Exception(
                   "Unknown encountered on server. Message:'{0}' when writing an object   "
                   + e.Message);
            }


            signal.Set();
            AWSUtil.UpdateProgress();

        }


    }
    public static class AWSUtil
    {
        public static AmazonS3Client client = null;
        public static string BucketName = string.Empty;
        private static string awsAccessKeyId = string.Empty;
        private static string awsSecretAccessKey = string.Empty;
        public static string AWSKeyPath = string.Empty;
        private static int workerThreads;


        public static void Init()
        {
            client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.CNNorthWest1);
            ThreadPool.GetMinThreads(out workerThreads, out int completionPorts);
        }
        public static void Init(AWSConfig configData)
        {

        }
        /// <summary>
        /// init config
        /// </summary>
        /// <param name="BucketName_">bucketName example :meta-bz11-static</param>
        /// <param name="AWSKeyPath_">remote root path,example : mochengTest </param>
        /// <param name="awsAccessKeyId_"></param>
        /// <param name="awsSecretAccessKey_"></param>
        public static void Init(string BucketName_, string AWSKeyPath_, string awsAccessKeyId_, string awsSecretAccessKey_)
        {
            BucketName = BucketName_;
            AWSKeyPath = AWSKeyPath_;
            awsAccessKeyId = awsAccessKeyId_;
            awsSecretAccessKey = awsSecretAccessKey_;
            Init();
        }

        public static Task UploadSingleFile(FileInfo file, string dir)
        {
            if (client == null)
            {
                throw new System.Exception("AWS has NOT Inited, Please Init first!!");
            }
            try
            {
                var putRequest1 = new PutObjectRequest
                {
                    BucketName = AWSUtil.BucketName,
                    Key = AWSKeyPath + file.FullName.Substring(dir.Length, file.FullName.Length - dir.Length).Replace('\\', '/'),
                    FilePath = file.FullName

                };


                return AWSUtil.client.PutObjectAsync(putRequest1);
            }
            catch (AmazonS3Exception e)
            {
                throw new System.Exception(
                       "Error encountered ***. Message:'{0}' when writing an object   "
                       + e.Message);
            }
            catch (Exception e)
            {
                throw new System.Exception(
                   "Unknown encountered on server. Message:'{0}' when writing an object   "
                   + e.Message);
            }





        }

        /// <summary>
        /// please use full path please
        /// </summary>
        /// <param name="dirPath"></param>
        public static void UploadDir(string dirPath)
        {
            if (client == null)
            {
                throw new System.Exception("AWS has NOT Inited, Please Init first!!");
            }

            DirectoryInfo direction = new DirectoryInfo(dirPath);

            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            List<Task> uploaders = new List<Task>();

            totalFiles = files.Length;
            curFileIndex = 0;


            for (int i = 0; i < files.Length; i += workerThreads)
            {

                uploaders.Clear();
                for (int j = 0; j < workerThreads; j++)
                {
                    if (i + j >= files.Length) break;

                    var curFile = files[i + j];
                    if (curFile.Attributes == FileAttributes.Directory) continue;
                    Task t = UploadSingleFile(curFile, dirPath);
                    // t.Start();
                    uploaders.Add(t);
                }


                Task.WaitAll(uploaders.ToArray());
                UpdateProgress();
            }





        }
        static int curFileIndex = 0;
        static int totalFiles = 1;
        public static float GetProgress()
        {
            Console.WriteLine((float)curFileIndex + " " + (float)totalFiles);
            return (float)curFileIndex / (float)totalFiles;
        }

        internal static void UpdateProgress()
        {
            curFileIndex += workerThreads;
            GetProgress();
        }
    }
}