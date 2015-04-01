namespace Qoollo.Concierge.UniversalExecution
{
    internal interface IAppBuilder
    {
        /// <summary>
        /// IExecutabel creation from fluent
        /// </summary>
        FluentExecutor FluentExecutor { get; }

        /// <summary>
        /// Default startup argument if no mode in args
        /// </summary>
        string DefaultStartupString { get; set; }

        /// <summary>
        /// Configuartion
        /// </summary>
        IWindowsServiceConfig WindowsServiceConfig { get; }
    }
}