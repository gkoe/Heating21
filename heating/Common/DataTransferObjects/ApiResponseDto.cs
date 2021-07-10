using System.Collections.Generic;

namespace Common.DataTransferObjects
{
    public class ApiResponseDto
    {
        public bool IsSuccessful { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
