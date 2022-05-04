using Base.Validations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.DataTransferObjects
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Email"></param>
    /// <param name="Name"></param>
    /// <param name="NewPassword"></param>
    /// <param name="RoleName"></param>
    public record UpsertRequestDto(
         string Id,
        [Required, EmailAddress] string Email,
        [Required] string Name,
        [CheckPasswordRules] string NewPassword,
        string PhoneNumber,
        string RoleName);

}
