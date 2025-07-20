using Application.Resources;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Presentation.Extentions
{
    public class ValidationExtensions
    {
        public static void CheckModelState(ModelStateDictionary modelState)
        {
            var entries = modelState
                .Where(mod => mod.Value.Errors.Count > 0)
                .GroupBy(mod => mod.Key.Split(".")[0])
                .Select(mod => mod.First());

            var errors = entries
                .SelectMany(pair => pair.Value.Errors.Take(1), (pair, error) => new ValidationFailure
                {
                    ErrorMessage = error.ErrorMessage, // No separator splitting here
                    PropertyName = pair.Key
                }).ToList();

            throw new ValidationException(errors);
        }
    }
}
