using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.UniversalExecution.CommandLineArguments
{
    internal abstract class StartupParametersManager
    {
        private readonly List<CmdArgumentSpec> _specs = new List<CmdArgumentSpec>();

        #region Init

        public IEnumerable<string> Help()
        {
            return _specs.Where(x => x.IsVisible).Select(x => x.Key + " " + x.Description);
        }

        public void Add(CmdArgumentSpec spec)
        {
            CmdArgumentSpec sp = FindSpec(spec);
            if (sp != null && !sp.IsDisabled)
                throw new DuplicateNameException("Duplicate arguments name");

            if (sp != null && sp.IsDisabled)
                sp.IsDisabled = false;
            else
                _specs.Add(spec);
        }

        public void AddRange(IEnumerable<CmdArgumentSpec> specs)
        {
            foreach (CmdArgumentSpec spec in specs)
            {
                Add(spec);
            }
        }

        public void Disable(string key)
        {
            CmdArgumentSpec spec = FindKey(key);
            if (spec == null)
                throw new KeyNotFoundException(string.Format("Key {0} not found", key));

            spec.IsDisabled = true;
        }

        public void DisableRange(IEnumerable<string> keys)
        {
            foreach (string key in keys)
            {
                Disable(key);
            }
        }

        public void Enable(string key)
        {
            CmdArgumentSpec sp = FindKey(key);
            if (sp == null)
                throw new KeyNotFoundException(string.Format("Key {0} not found", key));

            sp.IsDisabled = false;
        }

        public void EnableRange(IEnumerable<string> keys)
        {
            foreach (string key in keys)
            {
                Enable(key);
            }
        }

        public void ChangeName(string oldKey, string newKey)
        {
            CmdArgumentSpec spec = FindKey(oldKey);
            if (spec == null)
                throw new KeyNotFoundException(string.Format("Key {0} not found", oldKey));

            spec.ChangeName(newKey);
        }

        private CmdArgumentSpec FindSpec(CmdArgumentSpec spec)
        {
            return FindKey(spec.Key);
        }

        private CmdArgumentSpec FindKey(string key)
        {
            return _specs.Find(x => x.TryParseKey(key));
        }

        #endregion

        #region Process Arguments

        public bool ProcessArguments(string[] args)
        {
            return ProcessArguments(args, _specs.Where(x => !x.IsDisabled));
        }

        protected abstract bool ProcessArguments(string[] args, IEnumerable<CmdArgumentSpec> specs);

        #endregion

        #region args

        public static string[] TransformArgs(string args)
        {
            return args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ConcatArgs(string[] args)
        {
            return string.Join(" ", args);
        }

        public static CommandSpec Split(string str)
        {
            var ret = new Dictionary<string, string>();

            List<string> split = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
            string name = split.First();

            for (int i = 1; i < split.Count; i++)
            {
                string key = split[i];
                string value = string.Empty;

                if (i < split.Count - 1 && !value.StartsWith("-") && !value.StartsWith("--"))
                    value = split[++i];

                ret.Add(key, value);
            }

            return new CommandSpec(name, ret);
        }

        #endregion
    }
}