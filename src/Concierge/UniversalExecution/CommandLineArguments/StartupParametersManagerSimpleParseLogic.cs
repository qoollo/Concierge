using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Qoollo.Concierge.UniversalExecution.CommandLineArguments
{
    internal class StartupParametersManagerSimpleParseLogic : StartupParametersManager
    {        
        protected override bool ProcessArguments(string[] args, IEnumerable<CmdArgumentSpec> specs)
        {
            if (args.Length == 0)
                return false;

            bool ret = true;

            Action excAction = null;

            List<CmdArgumentSpec> sp = ExtractSpecs(ref args, specs, ref excAction);

            if (sp.Count == 0 || !sp.Last().IsSetModeArgument)
                ret = false;

            if (excAction != null)
                excAction();

            foreach (CmdArgumentSpec spec in sp)
            {
                spec.CallAction(args);
            }

            return ret;
        }

        private List<CmdArgumentSpec> ExtractSpecs(ref string[] args, IEnumerable<CmdArgumentSpec> specs,
            ref Action excAction)
        {
            Action lAct = excAction;
            Action<Exception> action = exc =>
            {
                if (lAct == null)
                    lAct = () => { throw exc; };
            };

            var ret = new List<CmdArgumentSpec>();
            int i = 0;
            for (; i < args.Length; i++)
            {
                CmdArgumentSpec spec = FindArgumentForSpec(args[i], specs);
                if (spec == null)
                {
                    action(new CmdValueForArgumentNotFoundException(string.Format("Unknown args: {0}", args[i])));
                    continue;
                }

                if (spec.IsValueRequired)
                {
                    if (i < args.Length - 1 && !spec.IsArgName(args[i + 1]))
                        spec.Value = args[++i];
                    else
                    {
                        action(
                            new CmdValueForArgumentNotFoundException(string.Format("Argument: {0} need value", args[i])));
                        continue;
                    }
                }

                ret.Add(spec);

                if (spec.IsSetModeArgument)
                {
                    i++;
                    break;
                }
            }

            args = args.ToList().GetRange(i, args.Length - i).ToArray();

            excAction = lAct;
            return ret;
        }

        private CmdArgumentSpec FindArgumentForSpec(string arg, IEnumerable<CmdArgumentSpec> specs)
        {
            Contract.Requires(arg != null);
            Contract.Requires(specs != null);

            CmdArgumentSpec sp = specs.FirstOrDefault(spec => spec.TryParseKey(arg));
            return sp != null ? new CmdArgumentSpec(sp) : null;
        }
    }
}