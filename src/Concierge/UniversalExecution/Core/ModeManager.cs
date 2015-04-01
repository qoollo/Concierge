using System;
using System.Collections.Generic;
using System.IO;
using Qoollo.Concierge.Annotations;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.ParamsContext;
using Qoollo.Concierge.Whale;

namespace Qoollo.Concierge.UniversalExecution.Core
{
    internal class ModeManager
    {
        private readonly Dictionary<string, AppMode> _modes = new Dictionary<string, AppMode>();
        private readonly ParamContainer _paramContainer = new ParamContainer();        

        /// <summary>
        ///     Параметры запуска
        /// </summary>
        /// <param name="executableBuilder"></param>        
        /// <returns></returns>
        public IEnumerable<CmdArgumentSpec> DefaultArgumentSpecs(ExecutableBuilder executableBuilder)
        {
            var ret = new List<CmdArgumentSpec>
            {
                new CmdArgumentSpec("name", "Использовать другое имя сервиса",
                    str =>
                    {
                        executableBuilder.WindowsServiceConfig.InstallName = str;
                        executableBuilder.WindowsServiceConfig.DisplayName = str;
                    }, isVisible: true),
                new CmdArgumentSpec("user", "Запускать сервис от имени пользователя",
                    str => executableBuilder.WindowsServiceConfig.Username = str, isVisible: true),
                new CmdArgumentSpec("password", "Пароль для пользователя сервиса",
                    str => executableBuilder.WindowsServiceConfig.Password = str, isVisible: true),
                new CmdArgumentSpec("host", "Изменить адрес подключения к сервису",
                    str => _paramContainer.ServiceHostParameters.Add(new Uri(str)),
                    isVisible: true)
            };

            return ret;
        }

        public IEnumerable<CmdArgumentSpec> DefaultArgumentSpecsForModes(ExecutableBuilder executableBuilder,
             StartupParametersManager parametersManager)
        {
            executableBuilder.AddCommand(
                CommandExecutorProxy.Build("help", arg => executableBuilder.GetHelp(arg), "show all commands"));

            var ret = new List<CmdArgumentSpec>
            {
                CmdArgForMode(new DebugMode(), executableBuilder),
                CmdArgForMode(new InteractiveMode(), executableBuilder),
                CmdArgForModeWithArgument(new ServiceInstallerMode(), executableBuilder),
                CmdArgForModeWithArgument(new WinServiceMode(), executableBuilder),
                CmdArgForMode(new AttachMode(executableBuilder.WindowsServiceConfig), executableBuilder),

                new CmdArgumentSpec("help", "Показать аргументы",
                    () => Console.WriteLine(CustomConsoleHelpers.FormatHelp(parametersManager.Help())), true, true),
            };
            return ret;
        }

        public void Start(AppMode app, ExecutableBuilder executableBuilder, string[] args = null)
        {
            app.SetParamContainer(_paramContainer);
            if (app.GetType() != typeof(ServiceInstallerMode) && !IsValidAppMode(app, args))
                return;

            bool first = args != null;
            do
            {
                app.SetParamContainer(_paramContainer);
                Console.WriteLine(app.StartInfo);
                app = first ? app.Start(args, executableBuilder) : app.Start(executableBuilder);
                CopyCommands(app);
                first = false;
            } while (app != null);
        }

        public TMode GetMode<TMode>(string modeName)
            where TMode : AppMode
        {
            AppMode mode = GetMode(modeName);
            return mode == null ? null : (TMode) mode;
        }

        public AppMode GetMode(string modeName)
        {
            AppMode mode;
            if (!_modes.TryGetValue(modeName, out mode))
                return null;
            return mode;
        }

        private CmdArgumentSpec CmdArgForMode(AppMode mode, ExecutableBuilder executableBuilder)
        {
            _modes.Add(mode.Name, mode);
            return new CmdArgumentSpec(":"+mode.Name, mode.Description,
                args => Start(mode, executableBuilder, args), true, true);
        }

        private CmdArgumentSpec CmdArgForModeWithArgument(AppMode mode, ExecutableBuilder executableBuilder)
        {
            _modes.Add(mode.Name, mode);
            return new CmdArgumentSpec(":" + mode.Name, mode.Description,
                (value, args) =>
                {
                    string result;
                    if (!mode.Prepare(value, out result))
                        Console.WriteLine(result);
                    else
                        Start(mode, executableBuilder, args);
                }, true, true);
        }

        private bool IsValidAppMode(AppMode app, string[] args)
        {
            string result;
            bool ret = app.Prepare(args, out result);
            if (!ret)
                ConsoleLogs.WriteLine(result);
            return ret;
        }

        private void CopyCommands(AppMode app)
        {
            if (app != null)
            {
                AppMode mode = GetMode(app.Name);
                if (mode != null)
                {
                    app.RegistrateCommandRange(mode.CommandExecutorProxy.Commands);
                    app.CopyInfo(mode);
                }
            }
        }
    }
}