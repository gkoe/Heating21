namespace Core.Entities
{
    //public partial class Locker : IDatabaseValidatableObject
    //{
    //    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //    {
    //        if (validationContext.GetService(typeof(IUnitOfWork)) is not IUnitOfWork unitOfWork)
    //        {
    //            yield break;
    //            //throw new AccessViolationException("UnitOfWork is not injected!");
    //        }
    //        var validationResults = ExistsLockerNumber(this, unitOfWork);
    //        if (validationResults != ValidationResult.Success)
    //        {
    //            yield return validationResults;
    //        }
    //    }

    //    public static ValidationResult ExistsLockerNumber(Locker locker, IUnitOfWork unitOfWork)
    //    {
    //        if (unitOfWork.LockerRepository.HasDuplicate(locker))
    //        {
    //            return new ValidationResult($"Locker {locker.Number} existiert bereits!",
    //                new List<string> { nameof(locker.Number) });
    //        }

    //        return ValidationResult.Success;
    //    }
    //}
}
