using FluentValidation;
using RagApiServer.Models;

namespace RagApiServer.Validators;

public class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.CollectionName)
            .NotEmpty().WithMessage("collection_name must not be empty.");

        RuleFor(x => x.SourceType)
            .NotEmpty().WithMessage("source_type must not be empty.");

        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("query must not be empty.")
            .MaximumLength(4000).WithMessage("query must not exceed 4000 characters.");

        RuleFor(x => x.TopK)
            .GreaterThan(0).WithMessage("top_k must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("top_k must be less than or equal to 100.");
            
        RuleFor(x => x.TopN)
            .GreaterThan(0).WithMessage("top_n must be greater than 0.");

        RuleFor(x => x.ScoreThreshold)
            .GreaterThanOrEqualTo(0.0f).WithMessage("score_threshold must be greater than or equal to 0.0.")
            .LessThanOrEqualTo(1.0f).WithMessage("score_threshold must be less than or equal to 1.0.");
    }
}
