using Core.Contracts;

using Persistence;

namespace IotServices.Services
{
    public class NoTrackingPersistenceService
    {
        private static readonly Lazy<NoTrackingPersistenceService> lazy = new(() => new NoTrackingPersistenceService());
        public static NoTrackingPersistenceService Instance { get { return lazy.Value; } }
        private NoTrackingPersistenceService() 
        {
            UnitOfWork = new UnitOfWork();
            UnitOfWork.SetNoTracking();
        }

        public IUnitOfWork UnitOfWork { get; set; }


    }
}
