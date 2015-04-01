using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;

namespace Qoollo.Concierge.FunctionalTests.TestClasses
{
    internal class StringCommand : UserCommand
    {
        [Parameter(ShortKey = 'a', LongKey = "aaaaaa", DefaultValue = "test")]
        public string AField { get; set; }
    }

    internal class IntCommand : UserCommand
    {
        [Parameter(ShortKey = 'b', LongKey = "bb", DefaultValue = "4", IsRequired = true)]
        public int AField { get; set; }
    }

    public class DoubleAndFloatCommand : UserCommand
    {
        [Parameter(ShortKey = 'a', LongKey = "bb", DefaultValue = "4.45", IsRequired = true)]
        public double AField { get; set; }

        [Parameter(ShortKey = 'b', LongKey = "bb", DefaultValue = "3.45", IsRequired = true)]
        public float BField { get; set; }
    }

    public class WrongDefaultParamCommand : UserCommand
    {
        [Parameter(ShortKey = 'a', LongKey = "bb", DefaultValue = "4.4", IsRequired = false)]
        public int AField { get; set; }
    }
}