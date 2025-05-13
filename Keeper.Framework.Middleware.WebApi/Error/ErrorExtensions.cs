using Keeper.Framework.Validations;

namespace Keeper.Framework.Middleware;

public static class ErrorExtensions
{
    /// <summary>
    /// Convert list of validation issues to error response.
    /// </summary>
    /// <param name="issues">The issues.</param>
    /// <returns>The error response.</returns>
    public static ErrorResponse ToValidationResponse(this IList<ValidationIssue> issues)
    {
        var details = new List<ErrorDetail>();

        if (issues == null)
            return new ErrorResponse(details);

        foreach (var issue in issues)
        {
            details.Add(new ErrorDetail
            {
                Code = ErrorCodes.Validation.BuildErrorCode(issue.Message),
                Message = issue.Message.SanitizeMessage()
            });
        }

        return new ErrorResponse(details);
    }

    /// <summary>
    /// Convert string to error response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>The error response.</returns>
    public static ErrorResponse ToValidationResponse(this string message)
    {
        return new ErrorResponse(new ErrorDetail
        {
            Code = ErrorCodes.Validation.BuildErrorCode(message),
            Message = message.SanitizeMessage()
        });
    }

    /// <summary>
    /// Convert message string to an application error response.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The error response.</returns>
    public static ErrorResponse ToApplicationResponse(this string message)
    {
        return new ErrorResponse(new ErrorDetail
        {
            Code = ErrorCodes.Application.BuildErrorCode(message),
            Message = message.SanitizeMessage()
        });
    }

    /// <summary>
    /// Convert message string to a security error response.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The error response.</returns>
    public static ErrorResponse ToSecurityResponse(this string message)
    {
        return new ErrorResponse(new ErrorDetail
        {
            Code = ErrorCodes.Security.BuildErrorCode(message),
            Message = message.SanitizeMessage()
        });
    }

    /// <summary>
    /// Convert message string to a system error response.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The error response.</returns>
    public static ErrorResponse ToSystemResponse(this string message)
    {
        return new ErrorResponse(new ErrorDetail
        {
            Code = ErrorCodes.System.BuildErrorCode(message),
            Message = message.SanitizeMessage()
        });
    }

    private const char TokenStart = '{';
    private const char TokenEnd = '}';

    private static int BuildErrorCode(this int baseCode, string message)
    {
        try
        {
            var errorCode = baseCode;

            if (message == null)
                return errorCode;

            var codes = message.Split(TokenStart, TokenEnd);

            foreach (var c in codes)
            {
                if (!int.TryParse(c, out int code)) continue;
                errorCode += code;
                break;
            }

            return Math.Min(errorCode, baseCode + 999);
        }
        catch
        {
            return baseCode;
        }
    }

    private static string SanitizeMessage(this string message)
    {
        try
        {
            var santizedMessage = message;
            var codes = message.Split(TokenStart, TokenEnd);

            foreach (var c in codes)
            {
                if (!int.TryParse(c, out int code)) continue;
                santizedMessage = message.Replace($"{TokenStart}{c}{TokenEnd}", string.Empty);
                break;
            }

            return santizedMessage.Trim();
        }
        catch
        {
            return message;
        }
    }

    public static KeeperValidationException ToKeeperValidationException(this FluentValidation.ValidationException fvException)
    {
        var validationIssues = fvException.Errors.Select(static e => new ValidationIssue(e.ErrorMessage)).ToList();
        return new KeeperValidationException(validationIssues);
    }
}
