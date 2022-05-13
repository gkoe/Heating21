using Core.Contracts;

using Persistence;

namespace IotServices.Services
{
    public class NoTrackingUnitOfWork : UnitOfWork
    {
        public NoTrackingUnitOfWork() : base()
        {
            SetNoTracking();
        }

    }
}
