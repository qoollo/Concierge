using System;
using System.Collections.Generic;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution;

namespace Qoollo.Concierge
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Change configuration
        /// </summary>        
        public static AppBuilder WithWinServiceProps(this AppBuilder appBuilder, Action<IWindowsServiceConfig> action)
        {
            appBuilder.ExecutableBuilder.WinServiceConfigBuilder.Modify(action);
            return appBuilder;
        }

        /// <summary>
        /// Set user executor
        /// </summary>
        public static AppBuilder UseExecutor(this AppBuilder appBuilder, Action<FluentExecutor> action)
        {
            var executor = new FluentExecutor();
            action(executor);
            appBuilder.ExecutableBuilder.FluentExecutor = executor;
            return appBuilder;
        }

        /// <summary>
        /// Default startup arguments (without mode-set startup argument)
        /// </summary>
        public static AppBuilder WithDefaultStartupString(this AppBuilder appBuilder, string defaultStartupString)
        {
            appBuilder.DefaultStartupString = defaultStartupString;
            return appBuilder;
        }

        /// <summary>
        /// Set default action (without mode-set startup argument)
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static AppBuilder WithDefaultStartupString(this AppBuilder appBuilder, DefaultStatupArguments argument)
        {
            appBuilder.UseDefaultStartupString(argument);
            return appBuilder;
        }

        /// <summary>
        /// Add logger
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <param name="logger"></param>
        public static AppBuilder WithNewLogger(this AppBuilder appBuilder, IConciergeLogger logger)
        {
            appBuilder.AddLogger(logger);
            return appBuilder;
        }


        #region Commands

        /// <summary>
        /// Add command without arguments
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name, Action action, string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name, Func<string> action,
            string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with UserCommand
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name, Action<UserCommand> action,
            string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with UserCommand and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name, Func<UserCommand, string> action,
            string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with UserCommand and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name,
            Action<Dictionary<string, string>> action, string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with Dictionary and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name,
            Func<Dictionary<string, string>, string> action, string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with string[]
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name, Action<string[]> action,
            string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with UserCommand and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, string name, Func<string[], string> action,
            string description = "")
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with user defined description
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand<T>(this AppBuilder appBuilder, string name, Action<T> action,
            string description = "")
            where T : UserCommand
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command with user defined description and return value
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="action">Command handler</param>
        /// <param name="description">Command description</param>
        public static AppBuilder AddCommand<T>(this AppBuilder appBuilder, string name, Func<T, string> action,
            string description = "")
            where T : UserCommand
        {
            appBuilder.AddCommand(name, action, description);
            return appBuilder;
        }

        /// <summary>
        /// Add command from instance
        /// </summary>
        /// <param name="instance">Instance</param>
        public static AppBuilder AddCommand(this AppBuilder appBuilder, object instance)
        {
            appBuilder.AddCommandsFromInstance(instance);
            return appBuilder;
        }

        #endregion

        #region Start Parameters

        /// <summary>
        /// Disable starup argument
        /// </summary>
        /// <param name="key">Argument name</param>
        public static AppBuilder DisableStartupParameter(this AppBuilder appBuilder, string key)
        {
            appBuilder.DisableStartupParameter(key);
            return appBuilder;
        }

        /// <summary>
        /// Disable startup arguments
        /// </summary>        
        /// <param name="keys">Arguments names</param>
        public static AppBuilder DisableStartupParameter(this AppBuilder appBuilder, string[] keys)
        {
            appBuilder.DisableStartupParameter(keys);
            return appBuilder;
        }

        /// <summary>
        /// Enable starup argument
        /// </summary>
        /// <param name="key">Argument name</param>
        public static AppBuilder EnableStartupParameter(this AppBuilder appBuilder, string key)
        {
            appBuilder.EnableStartupParameter(key);
            return appBuilder;
        }

        /// <summary>
        /// Enable startup arguments
        /// </summary>        
        /// <param name="keys">Arguments names</param>
        public static AppBuilder EnableStartupParameter(this AppBuilder appBuilder, string[] keys)
        {
            appBuilder.EnableStartupParameter(keys);
            return appBuilder;
        }

        /// <summary>
        /// Change argument name
        /// </summary>
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        public static AppBuilder ChangeStartupParameterName(this AppBuilder appBuilder, string oldName, string newName)
        {
            appBuilder.ChangeStartupParameterName(oldName, newName);
            return appBuilder;
        }

        /// <summary>
        /// Add startup argument
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="action">Argument handler</param>
        /// <param name="description">Argument description</param>
        public static AppBuilder AddStartupParameter(this AppBuilder appBuilder, string name, Action action, string description = "User startup parameter")
        {
            appBuilder.AddStartupParameter(name, action,description);
            return appBuilder;
        }

        /// <summary>
        /// Add startup argument with parameter
        /// </summary>
        /// <param name="name">Argument name</param>
        /// <param name="action">Argument handler</param>
        /// <param name="description">Argument description</param>
        public static AppBuilder AddStartupParameter(this AppBuilder appBuilder, string name, Action<string> action, string description = "User startup parameter")
        {
            appBuilder.AddStartupParameter(name, action, description);
            return appBuilder;
        }

        #endregion
    }
}