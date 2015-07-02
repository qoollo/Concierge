using System;
using System.Collections.Generic;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge
{
    /// <summary>
    /// Main class for Concierge work
    /// </summary>
    public class AppBuilder : IAppBuilder
    {
        private readonly ExecutableBuilder _executableBuilder;
        private readonly ModeManager _modeManager;
        private readonly StartupParametersManager _parametersManager;        
        private string _defaultStartupString = "-help";
        private readonly AggregateLogger _logger;

        #region Constructors

        internal AppBuilder(ExecutableBuilder executableBuilder, bool userDefaultArgs = false,
            IEnumerable<string> disableArgs = null)
        {
            _logger = new AggregateLogger();
            _logger.AddLogger(new ConsoleLogger());

            _executableBuilder = executableBuilder;
            _parametersManager = new StartupParametersManagerSimpleParseLogic();
            _modeManager = new ModeManager();

            _parametersManager.AddRange(
                _modeManager.DefaultArgumentSpecsForModes(_executableBuilder, _parametersManager));

            if (userDefaultArgs)
            {
                _parametersManager.AddRange(_modeManager.DefaultArgumentSpecs(_executableBuilder));

                if (disableArgs != null)
                    _parametersManager.DisableRange(disableArgs);
            }
        }

        /// <summary>
        /// Initialization with empty arguments collection
        /// </summary>
        public AppBuilder()
            :this(new ExecutableBuilder(new CommandExecutorProxy()))
        {                 
        }

        /// <summary>
        /// Initialization with default arguments collection
        /// </summary>
        /// <param name="userDefaultArgs">Add arguments</param>
        /// <param name="disableArgs">Disabled argument names</param>
        public AppBuilder(bool userDefaultArgs, IEnumerable<string> disableArgs = null)
            : this(new ExecutableBuilder(new CommandExecutorProxy()), userDefaultArgs, disableArgs)
        {            
        }

        /// <summary>
        /// Initialization with default arguments collection and user executor type
        /// </summary>
        /// <param name="executorType">User executor type</param>
        /// <param name="userDefaultArgs">Add arguments</param>
        /// <param name="disableArgs">Disabled argument names</param>
        public AppBuilder(bool userDefaultArgs, Type executorType, IEnumerable<string> disableArgs = null)
            : this(new ExecutableBuilder(executorType, new CommandExecutorProxy()), userDefaultArgs, disableArgs)

        {
        }

        /// <summary>
        /// Initialization with default arguments collection and user executor instance
        /// </summary>  
        /// <param name="executor">User excutor instance</param>
        /// <param name="userDefaultArgs">Add arguments</param>
        /// <param name="disableArgs">Disabled argument names</param>
        public AppBuilder(bool userDefaultArgs, IUserExecutable executor, IEnumerable<string> disableArgs = null)
            : this(new ExecutableBuilder(executor, new CommandExecutorProxy()), userDefaultArgs, disableArgs)
        {
        }

        /// <summary>
        /// Initialization with empty arguments collection and user executor type
        /// </summary>
        /// <param name="executorType">User executor type</param>
        public AppBuilder(Type executorType)
            : this(new ExecutableBuilder(executorType, new CommandExecutorProxy()))
        {
        }

        /// <summary>
        /// Initialization with empty arguments collection and user executor instance
        /// </summary>
        /// <param name="executor">User executor instance</param>
        public AppBuilder(IUserExecutable executor)
            : this(new ExecutableBuilder(executor, new CommandExecutorProxy()))
        {
        }


        #endregion        

        #region Initialization

        internal ExecutableBuilder ExecutableBuilder
        {
            get { return _executableBuilder; }
        }

        internal AggregateLogger Logger { get { return _logger; } }

        /// <summary>
        /// Default startup arguments (without mode-set startup argument)
        /// </summary>
        public string DefaultStartupString
        {
            get { return _defaultStartupString; }
            set { _defaultStartupString = value; }
        }
        
        /// <summary>
        /// Configuration
        /// </summary>
        public IWindowsServiceConfig WindowsServiceConfig
        {
            get { return _executableBuilder.WindowsServiceConfig; }
        }

        /// <summary>
        /// Create user executor with fluent initailization
        /// </summary>
        public FluentExecutor FluentExecutor
        {
            get { return _executableBuilder.FluentExecutor; }
        }

        /// <summary>
        /// Run Concierge
        /// </summary>
        /// <param name="args">Startup arguments. Use args from Main</param>
        public void Run(string[] args)
        {
            try
            {
                _executableBuilder.Logger = _logger;
                _executableBuilder.PrepareExecutableBuilder();

                InstallerBase.Logger = _logger;
                InstallerBase.WinServiceConfig = WindowsServiceConfig;

                //if mode was not set
                if (!_parametersManager.ProcessArguments(args))
                {
                    string[] defArgs = DefaultStartupString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _parametersManager.ProcessArguments(defArgs);
                }
            }
            catch (Exception e)
            {
                _logger.Log(e.ToString());
                throw;
            }            
        }

        /// <summary>
        /// Fast run without initialization
        /// </summary>
        /// <param name="args">Startup arguments. Use args from Main</param>
        /// <param name="userDefaultArgs">Add arguments</param>
        /// <param name="disableArgs">Disabled argument names</param>
        public static void FastRun(string[] args, bool userDefaultArgs, IEnumerable<string> disableArgs = null)
        {
            var helper = new AppBuilder(userDefaultArgs, disableArgs);
            helper.Run(args);
        }

        /// <summary>
        /// Set default action (without mode-set startup argument)
        /// </summary>
        /// <param name="argument"></param>
        public void UseDefaultStartupString(DefaultStatupArguments argument)
        {
            switch (argument)
            {
                case DefaultStatupArguments.Attach:
                    _defaultStartupString = ":attach";
                    break;
                case DefaultStatupArguments.Debug:
                    _defaultStartupString = ":debug";
                    break;
                case DefaultStatupArguments.Help:
                    _defaultStartupString = "-help";
                    break;
                case DefaultStatupArguments.Interactive:
                    _defaultStartupString = ":interactive";
                    break;
                case DefaultStatupArguments.ServiceInstall:
                    _defaultStartupString = ":service install";
                    break;
                case DefaultStatupArguments.ServiceUnstall:
                    _defaultStartupString = ":service unstall";
                    break;
            }
        }

        /// <summary>
        /// Add logger
        /// </summary>
        /// <param name="logger"></param>
        public void AddLogger(IConciergeLogger logger)
        {
            _logger.AddLogger(logger);
        }

        internal TMode GetMode<TMode>(string modeName)
            where TMode : AppMode
        {
            return _modeManager.GetMode<TMode>(modeName);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Add command without arguments
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Action action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Func<string> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with UserCommand
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Action<UserCommand> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with UserCommand and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Func<UserCommand, string> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with Dictionary
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Action<Dictionary<string, string>> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with Dictionary and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Func<Dictionary<string, string>, string> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with string[]
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Action<string[]> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with UserCommand and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand(string name, Func<string[], string> action, string description = "")
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description, ""));
        }

        /// <summary>
        /// Add command with user defined description
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand<T>(string name, Action<T> action, string description = "")
            where T : UserCommand
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command with user defined description and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public void AddCommand<T>(string name, Func<T, string> action, string description = "")
            where T : UserCommand
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, action, description));
        }

        /// <summary>
        /// Add command from instance
        /// </summary>
        /// <param name="instance">Instance</param>
        public void AddCommandsFromInstance(object instance)
        {
            _executableBuilder.AddCommandsFromInstance(instance);
        }

        internal void AddCommand<TMode>(string name, Func<TMode, string, string[], string> func, string description = "")
            where TMode : AppMode
        {
            _executableBuilder.AddCommand(CommandExecutorProxy.Build(name, func, description));
        }

        #endregion

        #region Start Parameters

        /// <summary>
        /// Disable starup argument
        /// </summary>
        /// <param name="key">Argument name</param>
        public void DisableStartupParameter(string key)
        {
            _parametersManager.Disable(key);
        }

        /// <summary>
        /// Disable startup arguments
        /// </summary>        
        /// <param name="keys">Arguments names</param>
        public void DisableStartupParameter(string[] keys)
        {
            _parametersManager.DisableRange(keys);
        }

        /// <summary>
        /// Enable starup argument
        /// </summary>
        /// <param name="key">Argument name</param>
        public void EnableStartupParameter(string key)
        {
            _parametersManager.Enable(key);
        }

        /// <summary>
        /// Enable startup arguments
        /// </summary>        
        /// <param name="keys">Arguments names</param>
        public void EnableStartupParameter(string[] keys)
        {
            _parametersManager.EnableRange(keys);
        }

        /// <summary>
        /// Change argument name
        /// </summary>
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        public void ChangeStartupParameterName(string oldName, string newName)
        {
            _parametersManager.ChangeName(oldName, newName);
        }

        /// <summary>
        /// Add startup argument
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="action">Argument handler</param>
        /// <param name="description">Argument description</param>
        /// <param name="valueHint">Value description</param>
        public void AddStartupParameter(string name, Action action, string description = "User startup parameter", string valueHint ="value")
        {
            _parametersManager.Add(new CmdArgumentSpec(name, description, action, false, true, valueHint));
        }

        /// <summary>
        /// Add startup argument with parameter
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="action">Argument handler</param>
        /// <param name="description">Argument description</param>
        /// <param name="valueHint">Value description</param>
        public void AddStartupParameter(string name, Action<string> action, string description = "User startup parameter", string valueHint = "value")
        {
            _parametersManager.Add(new CmdArgumentSpec(name, description, action, false, true, valueHint));
        }

        #endregion
    }
}