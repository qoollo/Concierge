using System;

namespace Qoollo.Concierge.Attributes
{
    /// <summary>
    /// Attribute to identify Default Executor.
    /// If Exector has Property, that returns IWindowsServiceConfig, it will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DefaultExecutorAttribute : Attribute
    {}
}