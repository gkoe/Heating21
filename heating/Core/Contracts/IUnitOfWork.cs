
using Base.Contracts.Persistence;

namespace Core.Contracts
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        ISensorRepository Sensors { get; }
        IMeasurementRepository Measurements { get; }
        IFsmTransitionRepository FsmTransitions { get; }

        //IPupilRepository PupilRepository { get; }
        //IBookingRepository BookingRepository { get; }
        //ILockerRepository LockerRepository { get; }

    }
}
