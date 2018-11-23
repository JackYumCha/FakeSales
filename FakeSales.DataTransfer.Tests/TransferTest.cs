using System;
using Xunit;
using FakeSales.DataTransfer.Core;
using Jack.DataScience.Common;
using Jack.DataScience.Storage.SFTP;
using Jack.DataScience.Storage.AWSS3;
using Autofac;

namespace FakeSales.DataTransfer.Tests
{
    public class TransferTest
    {
        [Fact(DisplayName = "Transfer File")]
        public async void TransferFile()
        {
            AutoFacContainer autoFacContainer = new AutoFacContainer();
            autoFacContainer.RegisterOptions<AWSS3Options>();
            autoFacContainer.RegisterOptions<SshOptions>();
            autoFacContainer.ContainerBuilder.RegisterModule<DataTransferCoreModule>();
            var services = autoFacContainer.ContainerBuilder.Build();
            var dataTransfer = services.Resolve<DataTransferCore>();
            var filename = await dataTransfer.Transfer();
        }
    }
}
