namespace Qoollo.Concierge.UniversalExecution.ParamsContext
{
    internal class ParamContainer
    {
        public ParamContainer()
        {
            ServiceHostParameters = new ServiceHostParameters();
        }

        public ServiceHostParameters ServiceHostParameters { get; private set; }
    }
}