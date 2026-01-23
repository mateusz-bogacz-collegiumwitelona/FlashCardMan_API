using Data.Interfaces;
using DTO.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller responsible for managing flashcards operations.
    /// </summary>
    [Route("api/flashcard")]
    [ApiController]
    public class FlashCardController : ControllerBase
    {
        private readonly IFlashCardService _flashCardService;

        public FlashCardController(IFlashCardService flashCardService)
        {
            _flashCardService = flashCardService;
        }
        /// <summary>
        /// Adds a list of new flashcards to an existing deck identified by its token.
        /// </summary>
        /// <remarks>
        /// This endpoint allows bulk creation of flashcards associated with a specific deck. 
        /// The target deck is identified by the `deckToken` path parameter. The request body accepts a JSON array of card objects containing questions and answers.
        /// 
        /// Example successful request:
        /// ```json
        /// POST /api/flashcard/add/1sP0X8Rcg6u3bigqABxmJw
        /// [
        ///   {
        ///     "question": "Co to jest?",
        ///     "answer": "To jest string."
        ///   },
        ///   {
        ///     "question": "Jaka to liczba?",
        ///     "answer": "To jest 42."
        ///   }
        /// ]
        /// ```
        /// Example success response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Cards added to the deck successfully.",
        ///   "data": true
        /// }
        /// ```
        /// 
        /// Example error request (empty list - 400 Bad Request):
        /// ```json
        /// POST /api/flashcard/add/1sP0X8Rcg6u3bigqABxmJw
        /// []
        /// ```
        /// Example error response (400 Bad Request):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "No cards to add.",
        ///   "errors": [
        ///     "NoCardsProvided"
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (invalid token - 404 Not Found):
        /// ```json
        /// POST /api/flashcard/add/INVALID_TOKEN_XXX
        /// [ ... ]
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
        /// <param name="deckToken">The unique token identifying the specified deck. Passed in the URL path.</param>
        /// <param name="requests">A JSON array of objects, each containing a 'Question' and 'Answer'.</param>
        /// <returns>A JSON object indicating success or failure.</returns>
        /// <response code="200">Cards were successfully added to the deck.</response>
        /// <response code="400">The request body was empty or null (no cards provided).</response>
        /// <response code="404">The deck with the provided token was not found.</response>
        /// <response code="500">An internal server error occurred while saving cards to the database.</response>
        [HttpPost("add/{deckToken}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCardsToDeckAsync(
            [FromRoute] string deckToken, 
            [FromBody] List<AddCardsRequest> requests
            )
        {
            var result = await _flashCardService.AddCardsToDeckAsync(deckToken, requests);

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
