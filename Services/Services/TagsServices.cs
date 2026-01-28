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

        public TagsServices(ITagsRepo tagsRepo)
        {
            _tagsRepo = tagsRepo;
        }

        public async Task<ResultHandler<bool>> AddNewTagAsync(string name)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(name))
                    return ResultHandler<bool>.Failure(
                        "Tag name cannot be empty.",
                        StatusCodes.Status400BadRequest,
                        new List<string> { "InvalidTagName" });

                var result = await _tagsRepo.AddNewTagAsync(name);

                if (!result)
                {
                    return ResultHandler<bool>.Failure(
                        "Failed to add new tag.",
                        StatusCodes.Status500InternalServerError,
                        new List<string> { "AddTagFailed" });
                }

                return ResultHandler<bool>.Success(
                    "Tag added successfully.",
                    StatusCodes.Status200OK,
                    true);
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
