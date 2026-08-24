using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedExpenseTrackerApp.Domain.Shared
{
    public class BaseController : ControllerBase
    {
        public IActionResult Execute<T>(Result<T> result)
        {
            var responseType = result.GetEnumType();

            return responseType switch
            {
                EnumResType.Success => Ok(result),
                EnumResType.ValidationError => BadRequest(result),
                EnumResType.SystemError => StatusCode(500,result),
                EnumResType.NotFound => NotFound(result),
                EnumResType.None => throw new Exception("EnumRespType is none. pls check your logic."),
                _ => throw new Exception("Out of scope in Execute (BaseController). pls check your logic.")
            };
        }
    }
}
