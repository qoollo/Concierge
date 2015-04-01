using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Qoollo.Concierge.UniversalExecution.Network
{
    [DataContract]
    internal class CommandSpec
    {
        public CommandSpec(string name, Dictionary<string, string> arguments)
        {
            Name = name;
            Arguments = new Dictionary<string, string>(arguments);
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public Dictionary<string, string> Arguments { get; private set; }
    }
}