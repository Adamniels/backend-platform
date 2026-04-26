using FluentValidation;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Context;

public sealed class GetMemoryContextV1RequestValidator : AbstractValidator<GetMemoryContextV1Request>
{
    public const int MaxTaskLength = 16_384;
    public const int MaxIdLength = 256;

    public GetMemoryContextV1RequestValidator()
    {
        RuleFor(x => x.TaskDescription)
            .MaximumLength(MaxTaskLength);
        RuleFor(x => x.WorkflowType)
            .MaximumLength(256);
        RuleFor(x => x.ProjectId)
            .MaximumLength(MaxIdLength);
        RuleFor(x => x.Domain)
            .MaximumLength(256);
    }
}
