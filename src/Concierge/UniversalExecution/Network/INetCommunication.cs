using System.ServiceModel;

namespace Qoollo.Concierge.UniversalExecution.Network
{
    [ServiceContract]
    internal interface INetCommunication
    {
        [OperationContract]
        string SendCommand(CommandSpec command);
    }
}