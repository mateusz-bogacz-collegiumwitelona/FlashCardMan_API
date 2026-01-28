using Data.Interfaces;
using Microsoft.AspNetCore.Http;
using Services.Helpers;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class TagsServices : ITagsServices
    {
        private readonly ITagsRepo _tagsRepo;
        private readonly IFlashCardRepo _flashCardRepo;

        public TagsServices(ITagsRepo tagsRepo, IFlashCardRepo flashCardRepo)
        {
            _tagsRepo = tagsRepo;
            _flashCardRepo = flashCardRepo;
        }

        public async Task<ResultHandler<bool>> AddTagToTokenIfNew(string name, string cardToken)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(name))
                    return ResultHandler<bool>.Failure(
                        "Tag name cannot be empty.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "InvalidTagName" });

               var card = await _flashCardRepo.GetCardByTokenAsync(cardToken);
                if (card == null)
                {
                    return ResultHandler<bool>.Failure(
                        "Flash card not found.",
                        StatusCodes.Status404NotFound,
                        new List<string> { "FlashCardNotFound" });
                }

                bool isTagExist = await _tagsRepo.IsCardHaveThisTag(card.Id, name);

                if (isTagExist) 
                    return ResultHandler<bool>.Failure(
                        "The flash card already has this tag.",
                        StatusCodes.Status409Conflict,
                        new List<string> { "TagAlreadyExists" });

                var resutl = await _tagsRepo.AddTagToTokenIfNew(name, card.Id);

                if (!resutl)
                    return ResultHandler<bool>.Failure(
                        "Failed to add the tag to the flash card.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "AddTagFailed" });

                return ResultHandler<bool>.Success(
                    "Tag added successfully.",
                    StatusCodes.Status200OK,
                    resutl);
            }
            catch (Exception ex)
            {
                return ResultHandler<bool>.Failure(
                    "An error occurred while adding the tag.",
                    StatusCodes.Status500InternalServerError,
                    new List<string> { ex.Message });
            }
        }
    }
}
