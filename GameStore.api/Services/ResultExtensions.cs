namespace GameStore.api;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this ServiceResult result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return MapError(result);
    }

    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Data);

        return MapError(result);
    }

    public static IResult ToCreatedHttpResult<T>(this ServiceResult<T> result, string routeName, Func<T, object> routeValuesFactory)
    {
        if (result.IsSuccess)
            return Results.CreatedAtRoute(routeName, routeValuesFactory(result.Data!), result.Data);

        return MapError(result);
    }

    private static IResult MapError(ServiceResult result) => result.ErrorType switch
    {
        ServiceErrorType.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
        ServiceErrorType.ValidationError => Results.BadRequest(new { error = result.ErrorMessage }),
        ServiceErrorType.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
        ServiceErrorType.Forbidden => Results.Json(new { error = result.ErrorMessage }, statusCode: 403),
        _ => Results.StatusCode(500)
    };
}
