using System.Collections.Generic;

namespace Qoollo.Concierge.Commands
{
    public class UserCommand
    {
        public Dictionary<string, string> Arguments { get; internal set; }

        public string CommandName { get; internal set; }
    }
}