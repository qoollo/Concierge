using System;

namespace Qoollo.Concierge.UniversalExecution.Core
{
    internal static class ConsoleLogs
    {
        private static bool _useLocal = false;
        private static string _message;

        public static void WriteLine(string str)
        {
            if (_useLocal)

            {
                _message = str;
            }
            else
            {
                Console.WriteLine(str);
            }
        }

        public static void UseStream()
        {
            _useLocal = true;
        }

        public static string ReadLine()
        {
            if (_useLocal)
            {
                return _message;
            }

            throw new NotImplementedException();
        }
    }
}