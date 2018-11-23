using System;
using System.IO;
using Xunit;
using System.Collections.Generic;
using Jack.DataScience;
using Jack.DataScience.Common;
using Jack.DataScience.Storage.SFTP;
using Troschuetz.Random.Generators;
using Troschuetz.Random.Distributions.Discrete;
using Troschuetz.Random;
using Renci.SshNet.Sftp;
using Renci.SshNet;
using Jack.DataScience.Data.CSV;
using Autofac;

namespace FakeSales.Generator
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            AutoFacContainer container = new AutoFacContainer();
            container.RegisterOptions<SshOptions>();
            container.ContainerBuilder.RegisterModule<SshModule>();
            var services = container.ContainerBuilder.Build();
            var sftpClient = services.Resolve<SftpClient>();
            sftpClient.Connect();
            Random rnd = new Random((int) (DateTime.Now - new DateTime(2018,1,1)).TotalMilliseconds );

            TRandom random = new TRandom();

            for(int i = 0; i < 50; i++)
            {
                var sales = new List<SaleRecord>();
                for(int j = 0; j< 50000; j++)
                {
                    SaleRecord saleRecord = new SaleRecord()
                    {
                        StoreNo = rnd.Next(99999).ToString().PadLeft(5, '0'),
                        HourOfDay = rnd.Next(24),
                        MinuteOfHour = rnd.Next(60),
                        NumberApple = random.Poisson(12),
                        NumberOfBanana = random.Poisson(5),
                        NumberOfBeer = random.Poisson(4),
                        NumberOfChicken = random.Poisson(8),
                        TimeStamp = DateTime.Now.AddSeconds(-rnd.Next(365 * 24 * 3600))
                    };

                    saleRecord.TotalValueEclGST =
                        saleRecord.NumberApple * 4 +
                        saleRecord.NumberOfBanana * 8 +
                        saleRecord.NumberOfBeer * 5 +
                        saleRecord.NumberOfChicken * 6;

                    sales.Add(saleRecord);
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    stream.WriteCsv(sales);
                    using(MemoryStream upload = new MemoryStream(stream.ToArray()))
                    {
                        sftpClient.UploadFile(upload, $"/root/{i.ToString().PadLeft(3,'0')}.csv");
                    }
                }
            }

            sftpClient.Disconnect();
            sftpClient.Dispose();
        }
    }
}
