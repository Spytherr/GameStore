namespace GameStore.api;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this ServiceResult result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
            ServiceErrorType.ValidationError => Results.BadRequest(new { error = result.ErrorMessage }),
            ServiceErrorType.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Data);

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
            ServiceErrorType.ValidationError => Results.BadRequest(new { error = result.ErrorMessage }),
            ServiceErrorType.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
            _ => Results.StatusCode(500)
        };
    }

    public static IResult ToCreatedHttpResult<T>(this ServiceResult<T> result, string routeName, Func<T, object> routeValuesFactory)
    {
        if (result.IsSuccess)
            return Results.CreatedAtRoute(routeName, routeValuesFactory(result.Data!), result.Data);

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
            ServiceErrorType.ValidationError => Results.BadRequest(new { error = result.ErrorMessage }),
            ServiceErrorType.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
            _ => Results.StatusCode(500)
        };
    }
}
