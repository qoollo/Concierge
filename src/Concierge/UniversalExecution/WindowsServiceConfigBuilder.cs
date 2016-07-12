using System;
using System.Collections.Generic;

namespace Qoollo.Concierge.UniversalExecution
{
    internal class WindowsServiceConfigBuilder
    {
        private  IWindowsServiceConfig _host;
        private readonly List<Action<IWindowsServiceConfig>>  _configActions; 

        public WindowsServiceConfigBuilder()
        {
            _host = new WinServiceConfig();
            _configActions = new List<Action<IWindowsServiceConfig>>();
        }

        public WindowsServiceConfigBuilder(IWindowsServiceConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            _host = config;
        }

        public void SetNewConfig(IWindowsServiceConfig config)
        {
            _host = config;
        }

        public void Modify(Action<IWindowsServiceConfig> action)
        {
            if (action == null) throw new ArgumentNullException("action");            
            
            _configActions.Add(action);
        }

        public IWindowsServiceConfig Get()
        {
            return _host;
        }

        public void ProcessActions()
        {
            _configActions.ForEach(x => x(_host));
        }
    }
}