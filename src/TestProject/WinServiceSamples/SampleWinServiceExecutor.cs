using System;
using System.ComponentModel;
using ConsoleHelpers.WindowsService;

namespace ConsoleHelpers.WindowsServiceSamples
{
    /// <summary>
    ///     RunInstaller(true) is a very important attribute.
    /// </summary>
    [RunInstaller(true)]
    [WinService(InstallName = "SampleService",
        DisplayName = "Sample Service",
        Description = "Sample description for sample service")]
    public class SampleWinServiceExecutor : ExecutorBase
    {
        private readonly EmptyLogger _logger = new EmptyLogger();
        private SampleBuilder _builder;

        /// <summary>
        ///     Use base(true) if your start is long  (5-10 seconds is long), otherwise base(false)
        /// </summary>
        public SampleWinServiceExecutor()
            : base(true)
        {
            _builder = new SampleBuilder();
            _builder.Build();
        }

        /// <summary>
        ///     If your start is long (5-10 seconds is long) inherit constructor from  base(true) , otherwise base(false)
        /// </summary>
        public override void Start()
        {
            try
            {
                _logger.Info("Service Starting");
                _builder.Start();
            }
            catch (Exception ex)
            {
                _logger.Fatal("Service Start failed");
                throw;
            }
        }

        public override void Stop()
        {
            try
            {
                _logger.Info("Service Stops");
                _builder.Stop();
            }
            catch (Exception ex)
            {
                _logger.Fatal("Service Stop failed");
                throw;
            }
            finally
            {
                _builder.Dispose();
                _builder = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing && _builder != null)
            {
                _builder.Dispose();
                _builder = null;
            }
            base.Dispose(disposing);
        }
    }
}