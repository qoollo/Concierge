using System;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    /// <summary>
    /// Interface for decorator search
    /// </summary>
    internal interface IDecoratorChain
    {
        IExecutable FindDecorator(Type type);
    }
}