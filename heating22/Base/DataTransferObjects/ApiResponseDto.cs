
using System.Collections.Generic;
using System.Linq;

namespace Base.DataTransferObjects
{
    public class ApiResponseDto
    {
        public bool IsSuccessful { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public string ErrorsText => string.Join(", ", Errors.ToArray());
    }

    public class ApiResponseDto<ResultType> : ApiResponseDto
    {
        public ResultType Result { get; set; }
    }
}
