using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Logger;

namespace Qoollo.Concierge.UniversalExecution.Network
{
    internal class ReceiverHost : ServiceHost
    {        
        public ReceiverHost(CommandRouter dep, IConciergeLogger logger, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            foreach (ContractDescription cd in ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new ReceiverInstanceProvider(dep, logger));
            }
        }

        public ReceiverHost(CommandRouter dep, IConciergeLogger logger, Type serviceType)
            : base(serviceType)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            foreach (ContractDescription cd in ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new ReceiverInstanceProvider(dep, logger));
            }
        }
    }

    internal class ReceiverInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly CommandRouter _dep;
        private readonly IConciergeLogger _logger;

        public ReceiverInstanceProvider(CommandRouter dep, IConciergeLogger logger)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _dep = dep;
            _logger = logger;
        }

        #region IInstanceProvider Members

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return new NetCommandReceiver(_dep, _logger);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        #endregion

        #region IContractBehavior Members

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}