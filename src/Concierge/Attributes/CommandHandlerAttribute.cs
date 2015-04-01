using System;

namespace Qoollo.Concierge.Attributes
{
    /// <summary>
    ///  Attribute for command description in IUserExecutable
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandHandlerAttribute : Attribute
    {
        public CommandHandlerAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Command name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Command description
        /// </summary>
        public string Description { get; private set; }
    }
}