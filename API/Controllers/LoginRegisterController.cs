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
        ///   "password": "Password123!"
        /// }
        /// ```
        /// or
        /// ```json
        /// POST /api/login
        /// {
        ///   "login": "User",
        ///   "password": "Password123!"
        /// }
        /// ```
        /// 
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

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <remarks>
        /// Description
        /// Creates a new user account with the provided username, email, and password.
        /// After successful registration, a confirmation email is sent to the user's email address.
        /// 
        /// Example request body
        /// ```json
        /// {
        ///   "userName": "JohnDope",
        ///   "email": "john.dope@example.com",
        ///   "password": "John!23",
        ///   "confirmPassword": "John!23"
        /// }
        /// ```
        ///
        /// Example response (success)
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "User registered successfully. Please check your email to confirm your account."
        /// }
        /// ```
        ///
        /// Example response (error)
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "User with this email: john.dope@example.com already exists",
        ///   "errors": []
        /// }
        /// ```
        ///
        /// Notes
        /// - Password must be at least 6 characters long and contain:
        ///   - At least one uppercase letter
        ///   - At least one number
        ///   - At least one special character
        /// - A confirmation email will be sent to the provided email address
        /// - The user must confirm their email before they can log in
        /// - Username and email must be unique
        /// </remarks>
        /// <response code="201">User registered successfully — confirmation email sent</response>
        /// <response code="400">Validation errors — email/username already exists or invalid input</response>
        /// <response code="500">Server error — something went wrong in the backend</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterNewUserAsync([FromBody] RegisterRequest request)
        {
            var result = await _login.RegisterUserAsync(request);

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

        /// <summary>
        /// Confirm user email address
        /// </summary>
        /// <remarks>
        /// Description:
        /// Confirms a user's email address using the token sent via email during registration.
        /// 
        /// Example request:
        /// ```json
        /// {
        ///   "email": "john.dope@example.com",
        ///   "token": "CfDJ8Abc123..."
        /// }
        /// ```
        ///
        /// Example response (success):
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Email confirmed successfully. You can now log in."
        /// }
        /// ```
        ///
        /// Example response (error):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Invalid or expired confirmation token",
        ///   "errors": ["Invalid token"]
        /// }
        /// ```
        ///
        /// Notes:
        /// - The token is sent to the user's email during registration
        /// - Tokens typically expire after 24 hours
        /// - Once confirmed, the user can log in to the application
        /// - If email is already confirmed, a 400 error will be returned
        /// </remarks>
        /// <response code="200">Email confirmed successfully</response>
        /// <response code="400">Invalid or expired token, or email already confirmed</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
        {
            var result = await _login.ConfirmEmailAsync(request);

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

        /// <summary>
        /// Request password reset - sends an email with reset token
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>Result indicating if password reset email was sent successfully</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/reset-password?email=user@example.com
        ///     
        /// This endpoint:
        /// - Validates if the user exists
        /// - Checks if email is confirmed
        /// - Generates a password reset token
        /// - Sends an email with reset instructions
        /// 
        /// The token expires after 24 hours.
        /// </remarks>
        /// <response code="200">Password reset email sent successfully</response>
        /// <response code="400">Email is not confirmed</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ForgotPasswordAsync(string email)
        {
            var result = await _login.ForgotPasswordAsync(email);

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

        /// <summary>
        /// Set a new password using reset token
        /// </summary>
        /// <param name="request">Password reset details including email, token, and new password</param>
        /// <returns>Result indicating if password was reset successfully</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/set-new-password
        ///     {
        ///         "email": "user@example.com",
        ///         "token": "CfDJ8IdAXN6s0V1Cl4t834jHBx...",
        ///         "password": "NewSecure123!",
        ///         "confirmPassword": "NewSecure123!"
        ///     }
        ///     
        /// Password requirements:
        /// - Minimum 6 characters
        /// - At least one uppercase letter (A-Z)
        /// - At least one number (0-9)
        /// - At least one special character (!@#$%^&amp;*(),.?":{}|&lt;&gt;)
        /// 
        /// The token must be the one received via email from the reset-password endpoint.
        /// Token expires after 24 hours.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("set-new-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
        {
            var result = await _login.ResetPasswordAsync(request);
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

