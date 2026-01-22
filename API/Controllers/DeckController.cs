using DTO.Request;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    /// <summary>
    /// Controller responsible for managing card decks operations.
    /// </summary>

    [Route("api/deck")]
    [ApiController]
    public class DeckController : ControllerBase
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
            var result = await _deckServices.CreateDeckAsync(request);
            
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
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDecks()
        {
            var result = await _deckServices.GetAllDecksAsync();
            
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
