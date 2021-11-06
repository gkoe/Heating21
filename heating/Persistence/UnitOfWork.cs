
using Base.Persistence;

using Core.Contracts;

namespace Persistence
{
    public class UnitOfWork : BaseUnitOfWork, IUnitOfWork
    {
        public ApplicationDbContext ApplicationDbContext => BaseApplicationDbContext as ApplicationDbContext;

        public UnitOfWork(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            Actors = new ActorRepository(applicationDbContext);
            Sensors = new SensorRepository(applicationDbContext);
            Measurements = new MeasurementRepository(applicationDbContext);
            FsmTransitions = new FsmTransitionRepository(applicationDbContext);
        }

        public ISensorRepository Sensors { get; }
        public IActorRepository Actors { get; }
        public IMeasurementRepository Measurements { get; }
        public IFsmTransitionRepository FsmTransitions { get; }

    }
}
