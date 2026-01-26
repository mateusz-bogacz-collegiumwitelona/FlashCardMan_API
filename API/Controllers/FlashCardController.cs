using DTO.Request;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller responsible for managing flashcards operations.
    /// </summary>
    [EnableCors("AllowClient")]
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

        /// <summary>
        /// Updates an existing flashcard's question or answer.
        /// </summary>
        /// <remarks>
        /// Performs a partial update on a flashcard identified by its token. 
        /// Parameters are passed via query strings. You must provide at least one field to update (question or "answare"). If both are empty or whitespace, a 400 Bad Request will be returned.
        /// 
        /// Example successful request (updating just the answer):
        /// ```
        /// PATCH /api/flashcard/edit?cardToken=tokenKarty123&amp;answare=NowaOdpowiedz
        /// ```
        /// Example success response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Card edit successfully.",
        ///   "data": true
        /// }
        /// ```
        /// 
        /// Example error request (no update fields provided - 400 Bad Request):
        /// ```
        /// PATCH /api/flashcard/edit?cardToken=tokenKarty123
        /// ```
        /// Example error response (400 Bad Request):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "No fields to update.",
        ///   "errors": [
        ///     "NoUpdateFields"
        ///   ]
        /// }
        /// ```
        /// 
        /// Example error request (invalid token - 404 Not Found):
        /// ```
        /// PATCH /api/flashcard/edit?cardToken=NIEISTNIEJACY_TOKEN&amp;question=NowePytanie
        /// ```
        /// Example error response (404 Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Card not found",
        ///   "errors": [
        ///     "CardNotFound"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="cardToken">The unique token identifying the flashcard to be edited (required).</param>
        /// <param name="question">The new text for the question (optional).</param>
        /// <param name="answare">The new text for the answer (optional).</param>
        /// <returns>A JSON object indicating success or failure.</returns>
        /// <response code="200">The card was updated successfully.</response>
        /// <response code="400">No valid fields (question or answer) were provided for update.</response>
        /// <response code="404">The flashcard with the provided token was not found.</response>
        /// <response code="500">An internal server error occurred while updating the database.</response>
        [HttpPatch("edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditCardAsync(
            [FromQuery] string cardToken, 
            [FromQuery] string? question, 
            [FromQuery] string? answare)
        {
            var result = await _flashCardService.EditCardAsync(cardToken, question, answare);

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
        /// Deletes a specific flashcard based on its token.
        /// </summary>
        /// <remarks>
        /// Performs a permanent deletion of a flashcard identified by the provided token passed as a query parameter.
        /// 
        /// Example successful request:
        /// ```
        /// DELETE /api/flashcard/delete?cardToken=card_token_abc123
        /// ```
        /// Example success response:
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "Card deleted successfully.",
        ///   "data": true
        /// }
        /// ```
        /// 
        /// Example error request (card not found - 404 Not Found):
        /// ```
        /// DELETE /api/flashcard/delete?cardToken=INVALID_TOKEN_XXX
        /// ```
        /// Example error response (404 Not Found):
        /// ```json
        /// {
        ///   "success": false,
        ///   "message": "Card not found",
        ///   "errors": [
        ///     "CardNotFound"
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <param name="cardToken">The unique token identifying the flashcard to be deleted (passed as a query parameter).</param>
        /// <returns>A JSON object indicating success or failure of the deletion operation.</returns>
        /// <response code="200">The flashcard was deleted successfully.</response>
        /// <response code="404">The flashcard with the provided token was not found.</response>
        /// <response code="500">An internal server error occurred while trying to delete the card from the database.</response>
        [HttpDelete("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCardAsync([FromQuery]string cardToken)
        {
            var result = await _flashCardService.DeleteCardAsync(cardToken);

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
