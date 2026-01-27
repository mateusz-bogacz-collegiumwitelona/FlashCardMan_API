using DTO.Request;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller responsible for managing card decks operations.
    /// </summary>
    [EnableCors("AllowClient")]
    [Route("api/deck")]
    [ApiController]
    public class DeckController : AuthControllerBase
    {
        private readonly IDeckServices _deckServices;

        public DeckController(IDeckServices deckServices)
        {
            _deckServices = deckServices;
        }

        /// <summary>
        /// Creates a new card deck.
        /// </summary>
        /// <remarks>
        /// Accepts a deck name and description. If successful, returns status 201 Created.
        /// If an error occurs during database saving, returns status 500 Internal Server Error.
        /// Example request:
        /// ```json
        /// {
        ///  "name": "string",
        ///  "description": "string"
        /// }
        /// ```
        /// Example response:
        /// ```json
        /// {
        ///  "success": true,
        ///  "message": "Deck created successfully."
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">The DTO containing the Name and Description for the new deck.</param>
        /// <returns>A JSON object indicating success or failure of the operation.</returns>
        /// <response code="201">The deck was successfully created.</response>
        /// <response code="500">An internal server error occurred while creating the deck.</response>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDeck([FromBody] AddNewDeckRequest request)
        {
            var (userEmail, error) = GetAuthenticatedUser();

            var result = await _deckServices.CreateDeckAsync(request, userEmail);
            
            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message,
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }

        /// <summary>
        /// Retrieves a list of all available card decks.
        /// </summary>
        /// <remarks>
        /// Returns a JSON object containing a list of decks (name, description, and token) in the 'data' field.
        /// If no decks exist in the database, returns status 404 Not Found.
        /// Example request:
        /// ```json
        /// GET /api/deck/all
        /// ```
        /// Example response: 
        /// ```json
        /// {
        ///  "success": true,
        ///  "message": "Decks retrieved successfully.",
        ///  "data": [
        ///    {
        ///     "name": "Test",
        ///      "description": "chuj",
        ///      "token": "_n7XucasIDb8TyEofP4FTA"
        ///    },
        ///    {
        ///      "name": "string1",
        ///      "description": "string2",
        ///      "token": "1sP0X8Rcg6u3bigqABxmJw"
        ///    }
        ///  ]
        ///}
        ///'''
        /// </remarks>
        /// <returns>A JSON object containing the list of decks or error information.</returns>
        /// <response code="200">Successfully retrieved the list of decks.</response>
        /// <response code="404">No decks found in the database.</response>
        /// <response code="500">An internal server error occurred while retrieving data.</response>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDecks()
        {
            var (userEmail, error) = GetAuthenticatedUser();
            var result = await _deckServices.GetAllDecksAsync(userEmail);
            
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
        /// Updates an existing deck's name or description.
        /// </summary>
        /// <remarks>
        /// Performs a partial update. A valid token is required. You can provide 'name', 'description', or both. Fields left null will not be changed.
        /// 
        /// Example request (update name and description):
        /// ```json
        /// PATCH /api/deck/edit
        /// {
        ///   "token": "_n7XucasIDb8TyEofP4FTA",
        ///   "name": "string6",
        ///   "description": "string7"
        /// }
        /// ```
        /// Example success response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Deck updated successfully."
        /// }
        /// ```
        /// 
        /// Example error request (no fields to update sent):
        /// ```json
        /// PATCH /api/deck/edit
        /// {
        ///   "token": "_n7XucasIDb8TyEofP4FTA"
        /// }
        /// ```
        /// Example error response (Bad Request):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "No fields to update.",
        ///   "errors": [
        ///     "NoUpdateFields"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">The DTO containing the token and optional fields to update.</param>
        /// <returns>A JSON object indicating success or failure.</returns>
        /// <response code="200">The deck was updated successfully.</response>
        /// <response code="400">Invalid request: either no fields were provided for update, or the values provided are identical to the existing ones (no changes detected).</response>
        /// <response code="404">The deck with the provided token was not found.</response>
        /// <response code="500">An internal server error occurred while trying to update the database.</response>
        [HttpPatch("edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditDeckAsync(EditDeckRequest request)
        {
            var (userEmail, error) = GetAuthenticatedUser();
            var result = await _deckServices.EditDeckAsync(request, userEmail);
            
            return result.IsSuccess
                ? StatusCode(result.StatusCode, result.Data)
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }

        /// <summary>
        /// Deletes a specific card deck based on its token.
        /// </summary>
        /// <remarks>
        /// Performs a permanent deletion of a deck. A valid token must be provided as a query parameter.
        /// 
        /// Example successful request:
        /// ```json
        /// DELETE /api/deck/delete?token=_n7XucasIDb8TyEofP4FTA
        /// ```
        /// Example success response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Deck deleted successfully."
        /// }
        /// ```
        /// 
        /// Example error request (deck not found):
        /// ```json
        /// DELETE /api/deck/delete?token=INVALID_TOKEN_XXX
        /// ```
        /// Example error response (Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Deck not found.",
        ///   "errors": [
        ///     "DeckNotFound"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="token">The unique security token identifying the deck to be deleted. Passed as a query string parameter.</param>
        /// <returns>A JSON object indicating success or failure of the deletion operation.</returns>
        /// <response code="200">The deck was deleted successfully.</response>
        /// <response code="400">The token parameter was missing or empty.</response>
        /// <response code="404">The deck with the provided token was not found.</response>
        /// <response code="500">An internal server error occurred while trying to delete the deck from the database.</response>
        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDeckAsync([FromQuery] string token)
        {
            var (userEmail, error) = GetAuthenticatedUser();
            var result = await _deckServices.DeleteDeckAsync(token, userEmail);

            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message,
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }

        /// <summary>
        /// Retrieves all flashcards belonging to a specific deck.
        /// </summary>
        /// <remarks>
        /// Returns a list of flashcards associated with the deck token provided in the query parameters.
        /// 
        /// Example successful request:
        /// ```
        /// GET /api/deck/cards?token=1sP0X8Rcg6u3bigqABxmJw
        /// ```
        /// Example success response (200 OK):
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Carnd in deck reviewed successfuly.",
        ///   "data": [
        ///     {
        ///       "question": "Przykładowe pytanie 1",
        ///       "answer": "Przykładowa odpowiedź 1",
        ///       "createdAt": "2024-01-25T12:00:00Z",
        ///       "updatedAt": "2024-01-25T12:00:00Z",
        ///       "token": "card_token_abc123"
        ///     },
        ///     {
        ///       "question": "Przykładowe pytanie 2",
        ///       "answer": "Przykładowa odpowiedź 2",
        ///       "createdAt": "2024-01-26T15:30:00Z",
        ///       "updatedAt": "2024-01-26T15:30:00Z",
        ///       "token": "card_token_xyz789"
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (missing token - 400 Bad Request):
        /// ```
        /// GET /api/deck/cards?token=
        /// ```
        /// Example error response (400 Bad Request):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "No token.",
        ///   "errors": [
        ///     "NoToken"
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (deck not found - 404 Not Found):
        /// ```
        /// GET /api/deck/cards?token=INVALID_TOKEN
        /// ```
        /// Example error response (404 Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Deck not found.",
        ///   "errors": [
        ///     "DeckNotFound"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="token">The unique token of the deck whose cards you want to retrieve (passed as query parameter).</param>
        /// <returns>A JSON object containing the list of flashcards or error information.</returns>
        /// <response code="200">Successfully retrieved the list of cards for the deck.</response>
        /// <response code="400">The token parameter was missing or empty.</response>
        /// <response code="404">Either the deck with the provided token does not exist, or the deck exists but contains no cards.</response>
        /// <response code="500">An internal server error occurred while retrieving data.</response>
        [HttpGet("cards")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDeckCardsAsync([FromQuery]string token)
        {
            var (userEmail, error) = GetAuthenticatedUser();
            var result = await _deckServices.GetDeckCardsAsync(token, userEmail);

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
        /// Retrieves all flashcards belonging to a specific deck that are due for review.
        /// </summary>
        /// <remarks>
        /// Returns a list of flashcards associated with the deck token provided in the route path.
        /// Only includes cards where the 'NextReviewAt' date is in the past or present.
        /// The user must be authenticated and must be the owner of the deck.
        /// 
        /// Example successful request:
        /// ```
        /// GET /api/deck/1sP0X8Rcg6u3bigqABxmJw/due-cards
        /// ```
        /// Example success response (200 OK):
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Carnd in deck reviewed successfuly.",
        ///   "data": [
        ///     {
        ///       "question": "Przykładowe pytanie do powtórki 1",
        ///       "answer": "Przykładowa odpowiedź 1",
        ///       "createdAt": "2024-01-20T12:00:00Z",
        ///       "updatedAt": "2024-01-25T12:00:00Z",
        ///       "token": "card_token_abc123"
        ///     },
        ///     {
        ///       "question": "Przykładowe pytanie do powtórki 2",
        ///       "answer": "Przykładowa odpowiedź 2",
        ///       "createdAt": "2024-01-22T15:30:00Z",
        ///       "updatedAt": "2024-01-26T15:30:00Z",
        ///       "token": "card_token_xyz789"
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (Unauthorized access - 401 Unauthorized):
        /// ```
        /// GET /api/deck/NOT_MY_DECK_TOKEN/due-cards
        /// ```
        /// Example error response (401 Unauthorized):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "User is no authorize to interact with this deck",
        ///   "errors": null
        /// }
        /// ```
        /// 
        /// Example error request (deck not found - 404 Not Found):
        /// ```
        /// GET /api/deck/INVALID_TOKEN/due-cards
        /// ```
        /// Example error response (404 Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Deck not found.",
        ///   "errors": [
        ///     "DeckNotFound"
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (no due cards in deck - 404 Not Found):
        /// ```
        /// GET /api/deck/VALID_TOKEN_NO_DUE_CARDS/due-cards
        /// ```
        /// Example error response (404 Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Cards in deck not found.",
        ///   "errors": [
        ///     "CardsNotFound"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="token">The unique token of the deck whose due cards you want to retrieve (passed as route parameter).</param>
        /// <returns>A JSON object containing the list of due flashcards or error information.</returns>
        /// <response code="200">Successfully retrieved the list of due cards for the deck.</response>
        /// <response code="400">The token parameter was empty.</response>
        /// <response code="401">The authenticated user is not authorized to access this deck.</response>
        /// <response code="404">Either the deck with the provided token does not exist, or the deck exists but contains no cards due for review.</response>
        /// <response code="500">An internal server error occurred while retrieving data.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{token}/due-cards")]
        public async Task<IActionResult> GetDueCardForDeckAsync([FromRoute] string token)
        {
            var (userEmail, error) = GetAuthenticatedUser();
            var result = await _deckServices.GetDueCardForDeckAsync(token, userEmail);

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
        /// Exports a flashcard deck as a downloadable JSON file.
        /// </summary>
        /// <remarks>
        /// Generates a JSON file containing deck metadata and all flashcards
        /// associated with the provided deck token.  
        /// The exported file includes the deck name, description, and a list of cards
        /// with their questions and answers.
        /// 
        /// The user must be authenticated and must be the owner of the deck.
        /// The response is returned as a file download with the
        /// <c>application/json</c> content type.
        /// 
        /// Example successful request:
        /// ```
        /// GET /api/deck/export?deckToken=5zbqgVtplir225hqMHkLYQ
        /// ```
        ///
        /// Example success response (200 OK – file download):
        /// ```json
        /// {
        ///   "name": "My Flashcards Deck",
        ///   "description": "Deck for daily vocabulary practice",
        ///   "cards": [
        ///     {
        ///       "question": "What is dependency injection?",
        ///       "answer": "A design pattern used to implement IoC."
        ///     },
        ///     {
        ///       "question": "What is ASP.NET Core?",
        ///       "answer": "A cross-platform framework for building web APIs."
        ///     }
        ///   ]
        /// }
        /// ```
        ///
        /// Example error request (Unauthorized access – 401 Unauthorized):
        /// ```
        /// GET /api/deck/export?deckToken=NOT_MY_DECK_TOKEN
        /// ```
        ///
        /// Example error response (401 Unauthorized):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "User is not authorized to export this deck.",
        ///   "errors": null
        /// }
        /// ```
        ///
        /// Example error request (deck not found – 404 Not Found):
        /// ```
        /// GET /api/deck/export?deckToken=INVALID_TOKEN
        /// ```
        ///
        /// Example error response (404 Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Deck not found.",
        ///   "errors": [
        ///     "DeckNotFound"
        ///   ]
        /// }
        /// ```
        ///
        /// Example error response (500 Internal Server Error):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "An error occurred while exporting the deck.",
        ///   "errors": [
        ///     "CannotExportDeck"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="deckToken">
        /// The unique token of the deck to export (passed as a query parameter).
        /// </param>
        /// <returns>
        /// A downloadable JSON file containing the exported deck data,
        /// or an error response if the export fails.
        /// </returns>
        /// <response code="200">The deck was successfully exported and returned as a JSON file.</response>
        /// <response code="400">The deckToken query parameter was missing or empty.</response>
        /// <response code="401">The authenticated user is not authorized to export this deck.</response>
        /// <response code="404">The deck with the provided token does not exist.</response>
        /// <response code="500">An internal server error occurred while exporting the deck.</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("export")]
        public async Task<IActionResult> GetDeckToJsonAsync([FromQuery] string deckToken)
        {
            var result = await _deckServices.GetDeckToJsonAsync(deckToken);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }

            return result.Data!;
        }

        [Consumes("multipart/form-data")]
        [HttpPost("import")]
        public async Task<IActionResult> ImportDeckFromJson(IFormFile file)
        {
            var (userEmail, error) = GetAuthenticatedUser();

            var result = await _deckServices.ImportDeckFromJson(file, userEmail);

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
    }
}
