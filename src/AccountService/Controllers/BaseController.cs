using AccountService.Results;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

public class BaseController : ControllerBase
{
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return result.StatusCode switch
            {
                201 => Created("", result.Value),
                _ => new ObjectResult(result.Value) { StatusCode = result.StatusCode }
            };
        return new ObjectResult(new { error = result.Error }) { StatusCode = result.StatusCode };
    }
}