using System.ComponentModel.DataAnnotations;

namespace Core.Validations
{
    public class FromToValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //var booking = (Booking)validationContext.ObjectInstance;
            //if (booking.From < DateTime.Today)
            //{
            //    return new ValidationResult("From-Date darf nicht in der Vergangenheit liegen", new List<string> { "From" });
            //}
            //if (booking.To != null && booking.To <= booking.From)
            //{
            //    return new ValidationResult("To-Date muss hinter From-Date liegen", new List<string> { "To" });
            //}
            return ValidationResult.Success;
        }
    }
}
