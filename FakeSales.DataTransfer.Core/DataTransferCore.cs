using System;
using System.Threading.Tasks;
using System.Linq;
using Jack.DataScience.Common;
using Jack.DataScience.Storage.SFTP;
using Jack.DataScience.Storage.AWSS3;
using Autofac;
using Renci.SshNet;

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
            AutoFacContainer autoFac = new AutoFacContainer();
            autoFac.RegisterOptions<SshOptions>();
            autoFac.RegisterOptions<AWSS3Options>();
            autoFac.ContainerBuilder.RegisterModule<SshModule>();
            autoFac.ContainerBuilder.RegisterModule<AWSS3Module>();

            var services = autoFac.ContainerBuilder.Build();

            var sftpClient = services.Resolve<SftpClient>();

            var awsS3 = services.Resolve<AWSS3API>();
            var awsS3Options = services.Resolve<AWSS3Options>();

            var bucket = awsS3Options.Bucket;

            if (!await awsS3.BucketExists())
            {
                await awsS3.CreateBucket(bucket);
            }

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
                        await awsS3.Upload(file.Name, sftpStream, bucket);
                    }
                    break;
                }
            }

            return filename;
        }
    }
}
