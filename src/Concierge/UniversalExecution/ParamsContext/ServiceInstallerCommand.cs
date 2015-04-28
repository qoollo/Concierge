using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;

namespace Qoollo.Concierge.UniversalExecution.ParamsContext
{
    internal class ServiceInstallerCommand : UserCommand
    {
        [Parameter(ShortKey = 'm')]
        public string RunMode { get; set; }

        [Parameter(ShortKey = 't', LongKey = "timeout", IsRequired = false, DefaultValue = 30000)]
        public int Timeout { get; set; }
    }
}