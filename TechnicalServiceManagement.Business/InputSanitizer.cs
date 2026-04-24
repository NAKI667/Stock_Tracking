using System.Text.RegularExpressions;

namespace TechnicalServiceManagement.Business;

/// <summary>
/// Provides centralized input validation and sanitization methods.
/// Prevents injection attacks and enforces consistent data quality
/// across all business operations.
/// </summary>
public static partial class InputSanitizer
{
    private const int MinPhoneLength = 7;
    private const int MaxPhoneLength = 15;
    private const int MaxInputLength = 500;

    /// <summary>
    /// Removes control characters and trims whitespace from user input.
    /// </summary>
    public static string SanitizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var sanitized = new string(input.Where(c => !char.IsControl(c) || c == '\n').ToArray());
        return sanitized.Trim();
    }

    /// <summary>
    /// Validates a required field and returns the sanitized value.
    /// Throws if the field is empty or exceeds the maximum length.
    /// </summary>
    public static string ValidateRequired(string value, string fieldName)
    {
        var sanitized = SanitizeText(value);

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        if (sanitized.Length > MaxInputLength)
        {
            throw new InvalidOperationException($"{fieldName} exceeds maximum allowed length ({MaxInputLength} characters).");
        }

        return sanitized;
    }

    /// <summary>
    /// Validates email format — must contain exactly one '@' followed by a domain with a dot.
    /// </summary>
    public static string ValidateEmail(string email)
    {
        var sanitized = SanitizeText(email);

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return string.Empty; // email is optional but validated when provided
        }

        if (!EmailPattern().IsMatch(sanitized))
        {
            throw new InvalidOperationException("Email format is invalid. Example: user@example.com");
        }

        return sanitized;
    }

    /// <summary>
    /// Validates phone number — must be digits only, between 7 and 15 characters.
    /// </summary>
    public static string ValidatePhone(string phone)
    {
        var sanitized = SanitizeText(phone);

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new InvalidOperationException("Phone number is required.");
        }

        if (!sanitized.All(char.IsDigit))
        {
            throw new InvalidOperationException("Phone number can only contain digits.");
        }

        if (sanitized.Length < MinPhoneLength || sanitized.Length > MaxPhoneLength)
        {
            throw new InvalidOperationException($"Phone number must be between {MinPhoneLength} and {MaxPhoneLength} digits.");
        }

        return sanitized;
    }

    /// <summary>
    /// Validates that a decimal value is positive (greater than zero).
    /// </summary>
    public static void ValidatePositiveAmount(decimal value, string fieldName)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{fieldName} must be greater than zero.");
        }
    }

    /// <summary>
    /// Validates that a decimal value is non-negative (zero or greater).
    /// </summary>
    public static void ValidateNonNegativeAmount(decimal value, string fieldName)
    {
        if (value < 0)
        {
            throw new InvalidOperationException($"{fieldName} cannot be negative.");
        }
    }

    /// <summary>
    /// Validates that an integer value is positive.
    /// </summary>
    public static void ValidatePositiveInteger(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{fieldName} must be greater than zero.");
        }
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailPattern();
}
