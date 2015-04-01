using System.Diagnostics.Contracts;
using System.Net;
using System.Reflection;

namespace Qoollo.Concierge.Commands.Executors
{
    internal class CommandExecutorFromAttribute<TCommand> : CommandExecutorWithClass<TCommand>
        where TCommand : UserCommand
    {
        private readonly object _instance;
        private readonly MethodInfo _methodInfo;

        public CommandExecutorFromAttribute(string commandName, string description, object instance, MethodInfo methodInfo)
            : base(commandName, description)
        {
            Contract.Requires(instance != null);
            _instance = instance;
            _methodInfo = methodInfo;
        }

        protected override string ExecuteGeneric(TCommand command)
        {
            // Call method in instance (IUserExecutable) with command as parameter
            object obj = _methodInfo.Invoke(_instance, new object[] {command});

            if (obj != null)
                return (string) obj;

            return HttpStatusCode.OK.ToString();
        }
    }
}