using DTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller responsible for user authentication and registration operations.
    /// </summary>
    [EnableCors("AllowClient")]
    [Route("api/")]
    [ApiController]
    public class LoginRegisterController : ControllerBase
    {
        private readonly ILoginRegisterServices _login;
        public LoginRegisterController(ILoginRegisterServices login)
        {
            _login = login;
        }

        /// <summary>
        /// Authenticates a user and issues HTTP-only cookies containing JWT and refresh tokens.
        /// </summary>
        /// <remarks>
        /// Allows an existing user to log in by providing their login (email or username) and password.
        /// Upon successful authentication, the server sets `HttpOnly` cookies (`jwt` and `refresh_token`) used for subsequent authenticated requests.
        /// The response body includes basic user information.
        /// 
        /// Example successful request:
        /// ```json
        /// POST /api/login
        /// {
        ///   "login": "user@example.com",
        ///   "password": "SecurePassword123!"
        /// }
        /// ```
        /// Example success response (Cookies are set in headers):
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Login successful",
        ///   "data": {
        ///     "message": "Login successful",
        ///     "email": "user@example.com",
        ///     "userName": "User",
        ///     "roles": [
        ///       "User"
        ///     ]
        ///   }
        /// }
        /// ```
        /// 
        /// Example error request (wrong credentials - 401 Unauthorized):
        /// ```json
        /// POST /api/login
        /// {
        ///   "login": "user@example.com",
        ///   "password": "WrongPassword"
        /// }
        /// ```
        /// Example error response (401 Unauthorized):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Invalid login attempt",
        ///   "errors": [
        ///     "Invalid login attempt"
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (email not confirmed - 403 Forbidden):
        /// ```json
        /// POST /api/login
        /// {
        ///   "login": "unconfirmed@example.com",
        ///   "password": "Password123!"
        /// }
        /// ```
        /// Example error response (403 Forbidden):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Please confirm your email address before logging in. Check your inbox for the confirmation link.",
        ///   "errors": [
        ///     "Please confirm your email address before logging in. Check your inbox for the confirmation link."
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">The login credentials (login and password).</param>
        /// <returns>A JSON object indicating success or failure, along with user data on success.</returns>
        /// <response code="200">Login successful. HttpOnly cookies are set.</response>
        /// <response code="400">Invalid request body format.</response>
        /// <response code="401">Invalid login credentials.</response>
        /// <response code="403">Email not confirmed or user has no roles assigned.</response>
        /// <response code="404">User with the provided login not found.</response>
        /// <response code="500">An internal server error occurred.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleLoginAsync(LoginRequest request)
        {
            var result = await _login.HandleLoginAsync(request);

            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }

        /// <summary>
        /// Logs out the current user and clears authentication cookies.
        /// </summary>
        /// <remarks>
        /// Requires the user to be currently authenticated via cookies. This operation signs the user out and instructs the browser to delete the `jwt` and `refresh_token` cookies.
        /// 
        /// Example successful request:
        /// ```
        /// POST /api/logout
        /// ```
        /// (Request must include valid authentication cookies)
        /// 
        /// Example success response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Logout successful."
        /// }
        /// ```
        /// 
        /// Example error request (not logged in - 401 Unauthorized):
        /// ```
        /// POST /api/logout
        /// ```
        /// (Request without valid cookies)
        /// </remarks>
        /// <returns>A JSON object indicating success or failure of the logout operation.</returns>
        /// <response code="200">Logout successful. Cookies are cleared.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="500">An internal server error occurred during logout.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleLogoutAsync()
        {
            var result = await _login.HandleLogoutAsync();
            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }
    }
}
