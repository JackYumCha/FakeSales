using System;
using System.Collections.Generic;
using System.Text;
using Jack.DataScience.Common;
using Jack.DataScience.Storage.SFTP;
using Jack.DataScience.Storage.AWSS3;
using Autofac;
using Renci.SshNet;

namespace FakeSales.DataTransfer.Core
{
    public class DataTransferCoreModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<AWSS3Module>();
            builder.RegisterModule<SshModule>();
            builder.RegisterType<DataTransferCore>();
            base.Load(builder);
        }
    }
}
