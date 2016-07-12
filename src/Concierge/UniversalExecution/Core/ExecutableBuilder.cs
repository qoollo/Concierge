using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Commands.Sources;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.Decorators;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.UniversalExecution.Network.Client;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.Core
{
    internal class ExecutableBuilder
    {
        private readonly Type _type;
        private readonly CommandExecutorProxyAggregate _commands;
        internal WindowsServiceConfigBuilder WinServiceConfigBuilder = new WindowsServiceConfigBuilder();
        private readonly IUserExecutable _executable;
        public const string ConsolePrefix = "->";
        public IConciergeLogger Logger { get; set; }

        #region Constructors

        public ExecutableBuilder(CommandExecutorProxy commands)
        {
            _commands = new CommandExecutorProxyAggregate(commands);
            FluentExecutor = new FluentExecutor();
        }

        public ExecutableBuilder(Type type, CommandExecutorProxy executorProxy)
            : this(executorProxy)
        {
            Contract.Requires(type != null);
            _type = type;
        }

        public ExecutableBuilder(IUserExecutable executable, CommandExecutorProxy executorProxy)
            : this(executorProxy)
        {
            Contract.Requires(executable != null);
            _executable = executable;
        }

        #endregion

        /// <summary>
        ///     Executor, that is initialized via Fluent syntax.
        ///     It's properties override Executor, found from attribute, if there is one.
        /// </summary>
        public FluentExecutor FluentExecutor { get; set; }

        public Type ExecutorType
        {
            get { return _type ?? FindUserServiceOrDefault(AssemblyHelper.EntryAssembly); }
        }

        public IWindowsServiceConfig WindowsServiceConfig
        {
            get { return WinServiceConfigBuilder.Get(); }
        }

        public void PrepareExecutableBuilder()
        {
            if (_type != null)
                WinServiceConfigBuilder.SetNewConfig(((IUserExecutable) Build(_type)).Configuration);
            else if (_executable != null)
                WinServiceConfigBuilder.SetNewConfig(_executable.Configuration);

            WinServiceConfigBuilder.ProcessActions();
        }

        public void SetNewCommandExecutorProxy(CommandExecutorProxy executorProxy, AppMode appMode)
        {
            _commands.SetCommandExecutorProxy(executorProxy, appMode);
        }

        public string GetHelp(string[] args)
        {
            return _commands.GetHelp(args);
        }

        #region Build Executable

        private IExecutable Build()
        {
            IExecutable ret;

            if (_executable != null) // instance
                ret = _executable;
            else if (_type != null) // type
                ret = Build(_type);
            else if (FluentExecutor != null && FluentExecutor.IsInitialized()) // fluent
                ret = new ActionsExecutableDecorator(FluentExecutor);
            else
                ret = Build(ExecutorType); //smth

            return ret;
        }

        private IExecutable Build(Type type)
        {
            return Activator.CreateInstance(type, null) as IExecutable;
        }

        /// <summary>
        /// Create and wrepp IUserExecutable
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="mode"></param>
        /// <param name="isAsyncStart"></param>
        /// <param name="args"></param>
        /// <param name="uri"></param>
        /// <param name="interactive">Indicates, that program will listen for commands</param>
        /// <returns></returns>
        public IExecutable Build(CommandExecutorProxy commands, RunMode mode, bool isAsyncStart = false,
            string[] args = null, Uri[] uri = null, bool interactive = true)
        {
            IExecutable userExecutable = Build();
            AddCommandsFromInstance(userExecutable, commands);
            IExecutable executable = new TunnelDecorator(userExecutable);

            if (args != null)
                executable = new StartupParametersDecorator(userExecutable, executable, args);

            if (isAsyncStart)
                executable = new AsyncDecorator(executable, Logger);

            switch (mode)
            {
                case RunMode.Debug:
                    if (interactive)
                        executable = new CommandSourceDecorator(executable, DebugSource(_commands));
                    break;
                case RunMode.Service:
                    if (interactive)
                        executable = new CommandSourceDecorator(executable, ServiceSource(_commands, uri));
                    executable = new ServiceRunDecorator(executable);
                    break;
            }
            return executable;
        }

        public virtual IExecutable BuildInteractiveMode()
        {
            return new CommandSourceDecorator(null, DebugSource(_commands));
        }

        public IExecutable BuildAttachToService(StableConcurrentConnection<INetCommunication> connection,
            bool isFake = false)
        {
            var router = new RemoteRouter(_commands, connection);

            var cmdSource = isFake
                ? new FakeCommandSource(router)
                : AttachToService(router, ConsolePrefix);

            return new CommandSourceDecorator(null, cmdSource);
        }

        protected virtual CommandSource AttachToService(RemoteRouter router, string prefix = "")
        {
            return new ConsoleCommandSource(router, prefix);
        }

        protected virtual CommandSource DebugSource(CommandExecutorProxy commands, string prefix = "")
        {
            return new ConsoleCommandSource(new LocalRouter(commands), prefix);
        }

        protected virtual CommandSource ServiceSource(CommandExecutorProxy commands, Uri[] baseAddress)
        {
            return new WcfCommandSource(new LocalRouter(commands), baseAddress,
                WindowsServiceConfig.IgnoreRemoteConnectionFail) {Logger = Logger};
        }

        #endregion

        #region GetConfig Type

        public static Type FindUserServiceOrDefault()
        {
            return FindUserServiceOrDefault(AssemblyHelper.EntryAssembly);
        }

        public static Type FindUserServiceOrDefault(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            return types.FirstOrDefault(type => Attribute.IsDefined(type, typeof (DefaultExecutorAttribute)));
        }

        #endregion

        #region Commands

        public void AddCommand(CommandExecutor command, CommandExecutorProxy executorProxy = null)
        {
            if (executorProxy == null)
                _commands.UserCommandExecutor.AddOrChangeCommand(command);
            else
                executorProxy.AddOrChangeCommand(command);
        }

        public void AddCommand(CommandExecutorWithRunMode command)
        {
            _commands.AddCommand(command);
        }

        public void AddCommandsFromInstance(object instance, CommandExecutorProxy executorProxy = null)
        {
            if (executorProxy == null)
                _commands.UserCommandExecutor.AddFromInstance(instance);
            else
                executorProxy.AddFromInstance(instance);
        }

        #endregion
    }
}