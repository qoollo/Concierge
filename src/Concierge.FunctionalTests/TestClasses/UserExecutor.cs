using System.Linq;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;

namespace Qoollo.Concierge.FunctionalTests.TestClasses
{
    [DefaultExecutor]
    public class UserExecutor : IUserExecutable
    {
        public IWindowsServiceConfig WinServiceConfig
        {
            get { return new WinServiceConfig();}
        }
        
        private string _args = string.Empty;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }

        [StartupParametersHandler]
        public void StartupMethod(string[] args)
        {
            _args = args.Aggregate("", (current, s) => current + (s + " "));
        }

        [CommandHandler("GetStartupArgs")]
        public string GetStartupArgsCommand(UserCommand command)
        {
            return _args;
        }

        [CommandHandler("Get")]
        public string Get(UserCommand command)
        {
            return "Test string";
        }

        [CommandHandler("FloatAndDouble")]
        public string FloatAndDouble(DoubleAndFloatCommand command)
        {
            return string.Format("{0} {1}", command.AField, command.BField);
        }

        [CommandHandler("WrongDefaultParam")]
        public string WrongDefaultParam(WrongDefaultParamCommand command)
        {
            return "Wrong";
        }

        public IWindowsServiceConfig Configuration { get{return new WinServiceConfig();}}
    }
}