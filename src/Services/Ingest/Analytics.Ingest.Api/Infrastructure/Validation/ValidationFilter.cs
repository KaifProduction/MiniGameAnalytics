using FluentValidation;

namespace Analytics.Ingest.Api.Infrastructure.Validation;

public sealed class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(ctx);

        var arg = ctx.Arguments.FirstOrDefault(a => a is T);
        if (arg is null) return Results.BadRequest(new { error = "Invalid payload" });

        var result = await validator.ValidateAsync((T)arg, ctx.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }

        return await next(ctx);
    }
}