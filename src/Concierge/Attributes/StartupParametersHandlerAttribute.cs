using System;

namespace Qoollo.Concierge.Attributes
{
    /// <summary>
    /// Attribute for command with startup parameters in IUserExecutable. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class StartupParametersHandlerAttribute : Attribute
    {
    }
}