namespace Zen.IdentityServer.Validation;

/// <summary>
/// Context for custom authorize request validation.
/// </summary>
public class CustomAuthorizeRequestValidationContext
{
    /// <summary>
    /// The result of custom validation. 
    /// </summary>
    public required AuthorizeRequestValidationResult Result { get; set; }
}
