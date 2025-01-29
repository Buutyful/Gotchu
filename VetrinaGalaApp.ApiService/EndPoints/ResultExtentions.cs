﻿using ErrorOr;

namespace VetrinaGalaApp.ApiService.EndPoints;

public static class ResultExtentions
{
    public static IResult ToResult(this List<Error> errors) =>
        errors switch
        {
            null => Results.Problem(),
            [] => Results.Problem(),
            [var error] when error.Type == ErrorType.NotFound => Results.NotFound(),
            [var error] when error.Type == ErrorType.Forbidden => Results.Forbid(),
            [var error] when error.Type == ErrorType.Conflict => Results.Conflict(),
            [.. var err] when err.All(e => e.Type == ErrorType.Validation) =>
                Results.ValidationProblem(
                    err.GroupBy(e => e.Code)
                       .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Description).ToArray()
                        )),

            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An unexpected error occurred")
        };
}

