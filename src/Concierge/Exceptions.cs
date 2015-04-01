using System;

namespace Qoollo.Concierge
{
    public class CmdArgumentParsingException : Exception
    {
        public CmdArgumentParsingException()
        {
        }

        public CmdArgumentParsingException(string message) : base(message)
        {
        }

        public CmdArgumentParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class CmdArgumentNotFoundException : CmdArgumentParsingException
    {
        public CmdArgumentNotFoundException()
        {
        }

        public CmdArgumentNotFoundException(string message) : base(message)
        {
        }

        public CmdArgumentNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class CmdValueForArgumentNotFoundException : CmdArgumentParsingException
    {
        public CmdValueForArgumentNotFoundException()
        {
        }

        public CmdValueForArgumentNotFoundException(string message) : base(message)
        {
        }

        public CmdValueForArgumentNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class CmdUnknownArgumentException : CmdArgumentParsingException
    {
        public CmdUnknownArgumentException()
        {
        }

        public CmdUnknownArgumentException(string message)
            : base(message)
        {
        }

        public CmdUnknownArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class InitializationException : Exception
    {
        public InitializationException()
        {
        }

        public InitializationException(string message) : base(message)
        {
        }

        public InitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}