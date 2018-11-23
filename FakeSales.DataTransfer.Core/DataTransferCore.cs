using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Jack.DataScience.Common;
using Jack.DataScience.Storage.SFTP;
using Jack.DataScience.Storage.AWSS3;
using Autofac;
using Renci.SshNet;
using Jack.DataScience.Data.Parquet;
using System.IO;
using CsvHelper;
using System.Text.RegularExpressions;
using System.Text;

namespace FakeSales.DataTransfer.Core
{
    public class DataTransferCore
    {
        private readonly AWSS3Options awsS3Options;
        private readonly AWSS3API awsS3;
        private readonly SftpClient sftpClient;
        public DataTransferCore(AWSS3Options awsS3Options,
            AWSS3API awsS3, SftpClient sftpClient)
        {
            this.awsS3Options = awsS3Options;
            this.awsS3 = awsS3;
            this.sftpClient = sftpClient;
        }
        public async Task<string> Transfer()
        {
            //AutoFacContainer autoFac = new AutoFacContainer();
            //autoFac.RegisterOptions<SshOptions>();
            //autoFac.RegisterOptions<AWSS3Options>();
            //autoFac.ContainerBuilder.RegisterModule<SshModule>();
            //autoFac.ContainerBuilder.RegisterModule<AWSS3Module>();

            //var services = autoFac.ContainerBuilder.Build();

            //var sftpClient = services.Resolve<SftpClient>();

            //var awsS3 = services.Resolve<AWSS3API>();
            //var awsS3Options = services.Resolve<AWSS3Options>();

            var bucket = awsS3Options.Bucket;

            if (!await awsS3.BucketExists())
            {
                await awsS3.CreateBucket(bucket);
            }

            Regex nameReplacer = new Regex(@"\.csv$", RegexOptions.IgnoreCase);

            sftpClient.Connect();

            var files = sftpClient.ListDirectory("/root/").ToList();

            string filename = null;

            foreach (var file in files)
            {
                if (!await awsS3.FileExists(bucket, file.Name))
                {
                    filename = file.Name;
                    using (var sftpStream = sftpClient.OpenRead(file.FullName))
                    {
                        using (var sftpStreamReader = new StreamReader(sftpStream))
                        {
                            using(var csvReader = new CsvReader(sftpStreamReader))
                            {
                                var list = new List<SaleRecord>();
                                int limit = 10000;
                                int filePartIndex = 0;
                                foreach(var record in csvReader.GetRecords<SaleRecord>())
                                {
                                    list.Add(record);
                                    if(list.Count >= limit)
                                    {
                                        filePartIndex = await WriteItemsToParquet(filename, filePartIndex, nameReplacer, list, bucket);
                                    }
                                }

                                if (list.Count > 0)
                                {
                                    filePartIndex = await WriteItemsToParquet(filename, filePartIndex, nameReplacer, list, bucket);
                                }

                                using (var uploadSuccessStream = new MemoryStream(Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"))))
                                {
                                    using(var transferStream = new MemoryStream(uploadSuccessStream.ToArray()))
                                    {
                                        await awsS3.Upload(filename, transferStream, bucket);
                                    }
                                }
                            }
                        }
                            
                    }
                    break;
                }
            }

            return filename;
        }

        private async Task<int> WriteItemsToParquet<T>(string filename, int filePartIndex, Regex nameReplacer, 
            List<T> list, string bucket) where T: class
        {
            var s3Filename = "data-" + nameReplacer.Replace(filename, $"-{filePartIndex.ToString().PadLeft(3, '0')}.parquet");
            using (var parquetWriteStream = new MemoryStream())
            {
                parquetWriteStream.WriteParquet(list);
                using (var parquetUploadStream = new MemoryStream(parquetWriteStream.ToArray()))
                {
                    await awsS3.Upload(s3Filename, parquetUploadStream, bucket);
                }
            }
            list.Clear();
            return filePartIndex += 1;
        }
    }
}
