using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    /// <summary>
    /// IExecutable decorator for startup argument processing
    /// </summary>
    internal class StartupParametersDecorator : IExecutable, IDecoratorChain
    {
        private readonly string[] _args;
        private readonly IExecutable _executable;
        private readonly IExecutable _userExecutable;

        public StartupParametersDecorator(IExecutable userExecutable, IExecutable executable, string[] args)
        {
            Contract.Requires(executable != null);
            Contract.Requires(userExecutable != null);
            Contract.Requires(args != null);
            _executable = executable;
            _userExecutable = userExecutable;
            _args = args;
        }

        public IExecutable FindDecorator(Type type)
        {
            if (_executable.GetType() == type)
                return _executable;

            var search = _executable as IDecoratorChain;
            if (search != null) return search.FindDecorator(type);
            return null;
        }

        public void Start()
        {
            MethodInfo method = FindParamsHandlingMethod();

            try
            {
                if (method != null)
                    method.Invoke(_userExecutable, new object[] {_args});
            }
            catch (TargetException e)
            {
                throw new InvalidDataException(WinServiceMessages.CallMethodWithAttribute("StartupParametersHandler"), e);
            }
            catch (ArgumentException e)
            {
                throw new InvalidDataException(WinServiceMessages.CallMethodWithAttribute("StartupParametersHandler"), e);
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidDataException(WinServiceMessages.CallMethodWithAttribute("StartupParametersHandler"), e);
            }
            catch (MethodAccessException e)
            {
                throw new InvalidDataException(WinServiceMessages.CallMethodWithAttribute("StartupParametersHandler"), e);
            }
            _executable.Start();
        }

        public void Stop()
        {
            _executable.Stop();
        }

        public void Dispose()
        {
            _executable.Dispose();
        }

        private MethodInfo FindParamsHandlingMethod()
        {
            MethodInfo[] methods = _userExecutable.GetType().GetMethods();
            MethodInfo method = null;
            foreach (MethodInfo methodInfo in methods.Where(
                methodInfo => Attribute.IsDefined(methodInfo, typeof (StartupParametersHandlerAttribute))))
            {
                if (method != null)
                    throw new InvalidDataException("Multiple methods with attribute StartupParametersHandler");
                method = methodInfo;
            }

            if (method == null)
                return null;

            if (!CommandReflector.CheckFirstParameterType<string[]>(method))
                throw new InvalidDataException(WinServiceMessages.MethodParametersAreWrong(method.Name));

            return method;
        }
    }
}