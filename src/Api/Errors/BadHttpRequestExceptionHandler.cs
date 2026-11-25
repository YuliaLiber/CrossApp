using Microsoft.AspNetCore.Diagnostics;

namespace Api.Errors;

public sealed class BadHttpRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest ||
            badRequest.StatusCode != 400)
            return false;

        await Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["body"] = new[]
                {
                    "Некоректний JSON або неправильний тип поля."
                }
            }).ExecuteAsync(context);

        return true;
    }
}
