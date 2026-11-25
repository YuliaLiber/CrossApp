
using Core.Domain;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
            return false;

        await Results.Problem(
            title: "Порушення бізнес-правила",
            detail: exception.Message,
            statusCode: StatusCodes.Status409Conflict
        ).ExecuteAsync(context);

        return true;
    }
}
