using Base.DataTransferObjects;
using Base.Validations;

using System.ComponentModel.DataAnnotations;

namespace Wasm.DataTransferObjects
{
    public class WasmUserPutDto : UserPutDto
    {

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password and confirm password is not matched")]
        public string ConfirmPassword { get; set; } = string.Empty;


    }
}
