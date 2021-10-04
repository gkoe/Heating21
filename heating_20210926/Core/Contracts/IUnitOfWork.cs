using Common.Contracts;

namespace Core.Contracts
{
    public interface IUnitOfWork : ICommonUnitOfWork
    {
        ISensorRepository Sensors { get; }
        IMeasurementRepository Measurements { get; }
        IFsmTransitionRepository FsmTransitions { get; }
    }

}