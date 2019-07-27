using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LandonApi.Models
{
    public class ApiError
    {
        public ApiError()
        {

        }

        public ApiError(ModelStateDictionary modelState)
        {
            Message = "Invalid parameters";
            Details = modelState.FirstOrDefault(x => x.Value.Errors.Any()).Value.Errors
                .FirstOrDefault()?.ErrorMessage;

        }

        public string Message { get; set; }
        public string Details { get; set; }
    }
}
