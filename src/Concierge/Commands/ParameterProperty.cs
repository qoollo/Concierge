using System;
using Qoollo.Concierge.Attributes;

namespace Qoollo.Concierge.Commands
{
    internal class ParameterProperty
    {
        public ParameterAttribute Attribute { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
    }
}