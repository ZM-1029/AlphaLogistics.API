using Microsoft.AspNetCore.Mvc;
using WALMS.API.Common;
//using WALMS.API.DTO.Order;

namespace WALMS.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult SuccessResponse<T>(T data, string message = "")
        {
            var response = new ApiResponse<T>(data, true, message);
            return Ok(response); // HTTP 200 OK
        }

        protected IActionResult CreatedResponse<T>(T data, string message = "")
        {
            var response = new ApiResponse<T>(data, true, message);
            return Created(string.Empty, response); // HTTP 201 Created
        }

        protected IActionResult ErrorResponse<T>(string message, List<T> errors = null)
        {
            var response = new ApiResponse<T>(false, message, errors);
            return BadRequest(response); // HTTP 400 Bad Request
        }
		

		/*protected IActionResult NoContentResponse<T>(string message)
        {
            var response = new ApiResponse<T>(false, message);
            return NotFound(response); // HTTP 404 Not Found
        }*/
		protected IActionResult ConflictResponse<T>(string message)
        {
            var formattedMessage = message.Replace("\n", Environment.NewLine);
            var response = new ApiResponse<T>(false, message);
            return Conflict(response); // HTTP 409 Not Found
        }

        protected IActionResult InternalServerErrorResponse<T>(string message, List<T> errors = null)
        {
            var response = new ApiResponse<T>(false, message, errors);
            return StatusCode(500, response); // HTTP 500 Internal Server Error
        }

       /* protected IActionResult InternalServerErrorResponse<T>(string message, List<T> errors = null,T exceptionDetail)
        {
            if (exceptionDetail != null)
            {
                errors = errors ?? new List<T>();
                errors.Add(exceptionDetail);
            }

            var response = new ApiResponse<T>(false, message, errors);
            return StatusCode(500, response); // HTTP 500 Internal Server Error
        }*/
        protected IActionResult UnauthorizedResponse<T>(string message)
        {
            var response = new ApiResponse<T>(false, message);
            return Unauthorized(response); // HTTP 401 Unauthorized
        }

        public IActionResult NoContentResponse<T>(string message)
        {
            var response = new ApiResponse<T>(false, message);
            return Ok(response);
        }
    }
}
