using Qoollo.Concierge.Commands.Routers;

namespace Qoollo.Concierge.Commands.Sources
{
    internal class FakeCommandSource : CommandSource
    {
        public FakeCommandSource(CommandRouter commandRouter) : base(commandRouter)
        {
        }

        public override void StartListening()
        {
        }

        public override void StopListening()
        {
        }
    }
}