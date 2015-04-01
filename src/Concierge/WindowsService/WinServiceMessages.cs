using Qoollo.Concierge.Whale;

namespace Qoollo.Concierge.WindowsService
{
    internal class WinServiceMessages
    {
        private const int ExpectedWidth = 60;


        public static string ServiceStartedMessage(string name)
        {
            return CustomConsoleHelpers.PlaceStringAtCenter(name + " has started", ExpectedWidth) + "\n";
        }

        public static string ServiceStoppedMessage(string name)
        {
            return CustomConsoleHelpers.PlaceStringAtCenter(name + "has stopped", ExpectedWidth) + "\n";
        }

        public static string ServiceAlreadyStartedMessage(string serviceName)
        {
            return serviceName + " is already running as winService. Start anyway?";
        }

        public static string ServiceStartedParameterlessMessage()
        {
            return "Service was started without parameters.";
        }

        public static string MethodParametersAreWrong(string methodName)
        {
            return string.Format("Parameters in method {0} are wrong", methodName);
        }

        public static string CallMethodWithAttribute(string attributeName)
        {
            return string.Format("Cannot call method with {0} attribute", attributeName);
        }
    }
}