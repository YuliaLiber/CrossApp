
using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveOrderRequestValidator
    : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {

RuleFor(x => x.Customer)
    .Must(value => !string.IsNullOrWhiteSpace(value)
        && value.Length <= 100)
    .WithMessage("Клієнт обов'язковий, максимум 100 символів.")
    .OverridePropertyName("customer");



RuleFor(x => x.Lines)
    .Must(lines => lines is { Count: > 0 })
    .WithMessage("Замовлення повинно містити хоча б один товар.")
    .OverridePropertyName("lines");


        RuleFor(x => x.Lines).Custom((lines, context) =>
        {
            if (lines is null)
                return;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line is null)
                {
                    context.AddFailure($"lines[{i}]",
                        "Позиція не може бути null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line.ProductId)
                    || line.ProductId.Length > 100)
                    context.AddFailure($"lines[{i}].productId",
                        "Артикул обов'язковий, максимум 100 символів.");

                if (string.IsNullOrWhiteSpace(line.Name)
                    || line.Name.Length > 200)
                    context.AddFailure($"lines[{i}].name",
                        "Назва обов'язкова, максимум 200 символів.");

                if (line.Price < 0)
                    context.AddFailure($"lines[{i}].price",
                        "Ціна не може бути від'ємною.");

                if (line.Quantity <= 0)
                    context.AddFailure($"lines[{i}].quantity",
                        "Кількість має бути більшою за нуль.");
            }
        });
    }
}

public sealed class SaveOrderValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments
            .OfType<SaveOrderRequest>()
            .FirstOrDefault();

        if (request is null)
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["body"] = ["Тіло запиту обов'язкове."]
                });

        var validator = context.HttpContext.RequestServices
            .GetRequiredService<IValidator<SaveOrderRequest>>();

        var result = await validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted);

        if (!result.IsValid)
            return Results.ValidationProblem(result.ToDictionary());

        return await next(context);
    }
}
