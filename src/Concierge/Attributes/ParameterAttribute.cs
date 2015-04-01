using System;
using Microsoft.Build.Framework;

namespace Qoollo.Concierge.Attributes
{
    /// <summary>
    /// Attribute for parameter description in UserCommand
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParameterAttribute : Attribute
    {
        public ParameterAttribute()
        {
            LongKey = string.Empty;
            IsRequired = false;
            DefaultValue = null;
            Description = string.Empty;
        }

        [Required]
        public char ShortKey { get; set; }

        public string LongKey { get; set; }
        public bool IsRequired { get; set; }
        public object DefaultValue { get; set; }
        public string Description { get; set; }
    }
}