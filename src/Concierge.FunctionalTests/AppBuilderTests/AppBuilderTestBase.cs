using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    public class AppBuilderTestBase
    {
        protected bool ExitReceived = false;
        protected InstallerBase Installer;
        protected AppBuilder AppBuilder;
        protected Assembly ExecutingAccembly = Assembly.GetExecutingAssembly();

        protected virtual AppBuilder GetAppBuilder()
        {
            Installer = GetExecutor();
            return new AppBuilder(true);
        }

        protected virtual InstallerBase GetExecutor()
        {
            return new InstallerBase();
        }

        protected virtual void RunAsync(string startupArgs = ":debug")
        {
            ExitReceived = false;
            Task.Factory.StartNew(() =>
            {
                AppBuilder.Run(startupArgs.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                ExitReceived = true;
            });

            Thread.Sleep(500);
        }


        [TestInitialize]
        public void Initialize()
        {
            AssemblyHelper.EntryAssembly = ExecutingAccembly;
            AppBuilder = GetAppBuilder();
        }
    }
}