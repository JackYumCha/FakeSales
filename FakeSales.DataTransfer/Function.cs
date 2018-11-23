using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Jack.DataScience.Common;
using Jack.DataScience.Storage.SFTP;
using Jack.DataScience.Storage.AWSS3;
using FakeSales.DataTransfer.Core;
using Autofac;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FakeSales.DataTransfer
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(string input, ILambdaContext context)
        {
            AutoFacContainer autoFacContainer = new AutoFacContainer();
            autoFacContainer.RegisterOptions<AWSS3Options>();
            autoFacContainer.RegisterOptions<SshOptions>();
            autoFacContainer.ContainerBuilder.RegisterModule<DataTransferCoreModule>();
            var services = autoFacContainer.ContainerBuilder.Build();
            var dataTransfer = services.Resolve<DataTransferCore>();
            var filename = await dataTransfer.Transfer();

            return filename == null ? "No File Transferred." : $"File {filename} Transferred.";
        }
    }
}
