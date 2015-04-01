using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.UniversalExecution.Decorators;
using Qoollo.Concierge.UniversalExecution.ParamsContext;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal abstract class AppMode : IDecoratorChain
    {
        private readonly string[] _args;
        private readonly CommandExecutorProxy _commands;
        private readonly bool _setNewCommands;
        protected ParamContainer ParamContainer;
        private IExecutable _executable;
        private AppMode _nextMode;
        private Func<IExecutable, IExecutable> _wrapper;

        protected AppMode(string name, string description, string[] args = null, bool setNewCommands = true)
        {
            Contract.Requires(!string.IsNullOrEmpty(description));
            _args = args;
            _setNewCommands = setNewCommands;
            Description = description;
            Name = name;
            _nextMode = null;
            _commands = new CommandExecutorProxy();
        }

        #region Members

        public string Name { get; private set; }
        public string Description { get; private set; }

        public string StartInfo
        {
            get { return string.Format("Start {0} mode. Type help for addition info", Name); }
        }

        public IExecutable Executable
        {
            get { return _executable; }
        }

        public CommandExecutorProxy CommandExecutorProxy
        {
            get { return _commands; }
        }

        protected Func<IExecutable, IExecutable> Wrapper
        {
            get { return _wrapper; }
        }

        #endregion

        #region Prepare

        public virtual bool Prepare(string[] args, out string result)
        {
            result = HttpStatusCode.OK.ToString();
            return true;
        }

        public virtual bool Prepare(string args, out string result)
        {
            result = HttpStatusCode.OK.ToString();
            return true;
        }

        public void SetParamContainer(ParamContainer paramContainer)
        {
            Contract.Requires(paramContainer != null);
            ParamContainer = paramContainer;
        }

        #endregion

        #region Build

        /// <summary>
        /// Find decorator in decorators chain
        /// </summary>
        /// <param name="type">decorators type</param>
        /// <returns></returns>
        public IExecutable FindDecorator(Type type)
        {
            var search = _executable as IDecoratorChain;
            if (search != null)
            {
                return search.FindDecorator(type);
            }
            return null;
        }

        /// <summary>
        /// Find decorator in decorators chain
        /// </summary>
        /// <typeparam name="TDecorator">decorators type</typeparam>
        /// <returns></returns>
        public TDecorator FindDecorator<TDecorator>()
            where TDecorator : IExecutable
        {
            var search = _executable as IDecoratorChain;
            if (search != null)
            {
                return (TDecorator)search.FindDecorator(typeof(TDecorator));
            }
            return default(TDecorator);
        }

        public void RegistrateExecutable(IExecutable executable)
        {
            Contract.Requires(executable != null);
            _executable = executable;
        }

        public void RegistrateCommand(CommandExecutor command)
        {
            _commands.AddOrChangeCommand(command);
        }

        public void RegistrateCommandRange(IEnumerable<CommandExecutor> command)
        {
            _commands.AddOrChangeRange(command);
        }

        public AppMode SetIExecutableWrapper(Func<IExecutable, IExecutable> wrapper)
        {
            Contract.Requires(wrapper != null);
            _wrapper = wrapper;
            return this;
        }

        public void WrapExecutable()
        {
            if (_wrapper != null)
            {
                var executable = FindDecorator<TunnelDecorator>();
                executable.SetExecutable(_wrapper(executable.Executable));
            }
        }        

        public void CopyInfo(AppMode mode)
        {
            _wrapper = mode._wrapper;
        }

        protected void SetNextMode(AppMode mode)
        {
            _nextMode = mode;
        }

        #endregion

        #region Start

        public AppMode Start(string[] args, ExecutableBuilder executableBuilder = null)
        {
            string[] arguments = _args ?? args;
            return InnerStart(arguments, executableBuilder);
        }

        public AppMode Start(ExecutableBuilder executableBuilder)
        {
            return InnerStart(_args, executableBuilder);
        }

        protected abstract void Build(string[] args, ExecutableBuilder executableBuilder);

        protected AppMode InnerStart(string[] args, ExecutableBuilder executableBuilder)
        {
            if (executableBuilder != null && _setNewCommands)
                executableBuilder.SetNewCommandExecutorProxy(_commands, this);

            Build(args, executableBuilder);

            if (_executable != null)
            {
                WrapExecutable();
                _executable.Start();
            }

            return _nextMode;
        }

        #endregion
    }
}