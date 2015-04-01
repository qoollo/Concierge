using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands
{
    internal static class CommandReflector
    {
        #region Create UserCommand

        public static List<ParameterProperty> GetParameterList<TCommand>()
        {
            var ret = new List<ParameterProperty>();

            Type t = typeof (TCommand);

            //Get command fields
            PropertyInfo[] props =
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                BindingFlags.SetProperty);

            foreach (PropertyInfo propertyInfo in props)
            {
                //Get fields with attribute
                if (Attribute.IsDefined(propertyInfo, typeof (ParameterAttribute)))
                {
                    var attr = propertyInfo.GetCustomAttributes(typeof (ParameterAttribute), false)
                        .First() as ParameterAttribute;

                    ret.Add(new ParameterProperty
                    {
                        PropertyName = propertyInfo.Name,
                        PropertyType = propertyInfo.PropertyType,
                        Attribute = attr
                    });
                }
            }

            return ret;
        }

        public static TCommand CreateInstance<TCommand>() where TCommand : UserCommand
        {
            Type baseType = typeof (TCommand);
            object o = Activator.CreateInstance(baseType);
            return o as TCommand;
        }

        public static void PrepareInstance<TCommand>(TCommand tCommand, List<ParameterProperty> parameters,
            CommandSpec command)
            where TCommand : UserCommand
        {
            foreach (ParameterProperty parameter in parameters)
            {
                ProcessProperty(tCommand, command, parameter);
            }
        }

        private static void ProcessProperty<TCommand>(TCommand tCommand, CommandSpec command,
            ParameterProperty property) where TCommand : UserCommand
        {
            KeyValuePair<string, string> shortName =
                command.Arguments.FirstOrDefault(x => x.Key == "-" + property.Attribute.ShortKey);
            KeyValuePair<string, string> longName =
                command.Arguments.FirstOrDefault(x => x.Key == "--" + property.Attribute.LongKey);

            if (!shortName.Equals(default(KeyValuePair<string, string>)))
            {
                SetProperty(tCommand, property, shortName.Value);
                return;
            }

            if (!longName.Equals(default(KeyValuePair<string, string>)))
            {
                SetProperty(tCommand, property, longName.Value);
                return;
            }

            if (property.Attribute.IsRequired)
                throw new CmdValueForArgumentNotFoundException(string.Format("Parameter -{0} is requred",
                    property.Attribute.ShortKey));

            if (property.Attribute.DefaultValue != null)
                SetProperty(tCommand, property, property.Attribute.DefaultValue);
        }

        private static void SetProperty<TCommand>(TCommand tCommand, ParameterProperty property, object value)
            where TCommand : UserCommand
        {
            PropertyInfo prop = tCommand.GetType().GetProperty(property.PropertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            if (prop != null)
            {
                object propertyValue = Convert.ChangeType(value,
                    Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType,
                    CultureInfo.InvariantCulture);

                prop.SetValue(tCommand, propertyValue, null);
            }
        }

        #endregion

        #region Command Preparation

        public static Type GetCommandTypeFromMethod(MethodInfo methodInfo)
        {
            ParameterInfo[] parms = methodInfo.GetParameters();
            if (parms.Count() != 1 || !parms.First().ParameterType.IsSubclassOf(typeof (UserCommand)))
                return null;

            return parms.First().ParameterType;
        }

        public static bool CheckFirstParameterType<TType>(MethodInfo method)
        {
            return CheckFirstParameterType(method, typeof (TType));
        }

        public static bool CheckFirstParameterType(MethodInfo method, Type type)
        {
            ParameterInfo[] parms = method.GetParameters();
            if (parms.Length != 1 || parms[0].ParameterType != type)
                return false;

            return true;
        }

        public static CommandExecutor CreateExecutor(Type type, string commandName, string helpText,
            object instance, MethodInfo methodInfo)
        {
            Type baseType = typeof (CommandExecutorFromAttribute<>);
            Type destType = baseType.MakeGenericType(new[] {type});
            return
                Activator.CreateInstance(destType, new[] {commandName, helpText, instance, methodInfo}) as
                    CommandExecutor;
        }

        #endregion
    }
}