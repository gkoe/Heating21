using Base.Contracts.Persistence;

namespace Core.Contracts
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IActorRepository Actors { get; }
        ISensorRepository Sensors { get; }
        IMeasurementRepository Measurements { get; }
        IFsmTransitionRepository FsmTransitions { get; }

    }
}
