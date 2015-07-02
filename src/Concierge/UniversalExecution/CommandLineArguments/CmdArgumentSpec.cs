using System;
using System.Diagnostics.Contracts;

namespace Qoollo.Concierge.UniversalExecution.CommandLineArguments
{
    /// <summary>
    /// Command line argument description
    /// </summary>
    public class CmdArgumentSpec
    {
        #region Constructors

        private CmdArgumentSpec(string key, string description, Action<string, string[]> processAction,
            bool isValueRequired, bool isSetModeArgument, bool isVisible, string valueHint)
        {
            Contract.Requires(key != null);
            Contract.Requires(description != null);

            IsVisible = isVisible;
            _processAction = processAction;
            IsValueRequired = isValueRequired;
            Description = description;
            Key = NormalizeKey(key);
            IsDisabled = false;
            IsSetModeArgument = isSetModeArgument;
            ValueSeparator = " ";
            ValueHint = valueHint;

            if (Key.Length == 0)
                throw new ArgumentException("Key should be presented");
        }

        public CmdArgumentSpec(string key, string description, Action<string[]> processAction,
            bool isLastArgument = false, bool isVisible = false, string valueHint="value")
            : this(key, description, (value, args) => processAction(args), false, isLastArgument, isVisible, valueHint)
        {
        }

        public CmdArgumentSpec(string key, string description, Action<string, string[]> processAction,
            bool isLastArgument = false, bool isVisible = false, string valueHint = "value")
            : this(key, description, processAction, true, isLastArgument, isVisible, valueHint)
        {
        }

        public CmdArgumentSpec(string key, string description, Action processAction,
            bool isLastArgument = false, bool isVisible = false, string valueHint = "value")
            : this(key, description, (value, args) => processAction(), false, isLastArgument, isVisible, valueHint)
        {
        }

        public CmdArgumentSpec(string key, string description, Action<string> processAction,
            bool isLastArgument = false, bool isVisible = false, string valueHint = "value")
            : this(key, description, (value, args) => processAction(value), true, isLastArgument, isVisible, valueHint)
        {
        }

        public CmdArgumentSpec(CmdArgumentSpec spec)
        {
            IsVisible = spec.IsVisible;
            _processAction = spec._processAction;
            IsValueRequired = spec.IsValueRequired;
            Description = spec.Description;
            Key = spec.Key;
            IsDisabled = false;
            IsSetModeArgument = spec.IsSetModeArgument;
        }

        #endregion

        #region Members

        /// <summary>
        /// Action for argument processing
        /// </summary>
        private readonly Action<string, string[]> _processAction;

        /// <summary>
        /// View argument in help
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Is value for argument required
        /// </summary>
        public bool IsValueRequired { get; private set; }

        /// <summary>
        /// If argument is last for execution (for modes)
        /// </summary>
        public bool IsSetModeArgument { get; private set; }

        public bool IsDisabled { get; set; }

        #endregion

        public string Value { get; set; }
        /// <summary>
        /// Used in help. If argument is -c [count], then this value is 'count'
        /// </summary>
        public string ValueHint { get; private set; }
        /// <summary>
        /// Separator for key and value. WARNING: Used only for help right now.
        /// </summary>
        public string ValueSeparator { get; private set; }

        public void CallAction(string[] args)
        {
            _processAction(Value, args);
        }

        #region Parse

        private string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is empty", "key");

            key = key.Trim().ToLower();

            if (key.Length == 1)
                throw new ArgumentException("Key is empty", "key");

            if (!key.StartsWith("-") && !key.StartsWith(":"))
                key = "-" + key;

            return key;            
        }

        public bool TryParseKey(string arg)
        {
            string copy = Key.Substring(1);
            return (arg != null) && (copy.Equals(arg.Trim().ToLower().Substring(1)));
        }

        public void ChangeName(string newKey)
        {
            Key = newKey;
        }

        public virtual bool IsArgName(string value)
        {
            return false;
        }

        #endregion
    }
}