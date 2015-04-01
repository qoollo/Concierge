using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.ServiceProcess;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution;

namespace Qoollo.Concierge.WindowsService
{
    internal class WinServiceBase : ServiceBase
    {
        private readonly IExecutable _executable;

        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private IContainer components;

        public IConciergeLogger Logger;

        public WinServiceBase(IExecutable executable, string serviceName)
        {
            Contract.Requires(executable != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(serviceName));
            _executable = executable;
            ServiceName = serviceName;
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            ServiceName = "Empty Service";
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            try
            {
                _executable.Start();
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                throw;
            }            
        }

        protected override void OnStop()
        {
            _executable.Stop();
        }
    }
}