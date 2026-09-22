namespace Buckl.Infrastructure.Persistence;

/// <summary>How domain enumerations are stored: the member name in lower case (ADR-0016). The
/// same text builds the check constraints, so adding an enum member changes the model and the
/// "no pending model changes" test fails until a migration updates the constraint.</summary>
public static class EnumText
{
    public static string ToText<TEnum>(TEnum value)
        where TEnum : struct, Enum => value.ToString().ToLowerInvariant();

    /// <summary>Strict inverse of <see cref="ToText{TEnum}"/>: only the exact stored text is
    /// accepted, never a number or a different casing.</summary>
    public static TEnum Parse<TEnum>(string text)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(text);

        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (ToText(value) == text)
            {
                return value;
            }
        }

        throw new InvalidOperationException($"'{text}' is not a stored {typeof(TEnum).Name} value.");
    }

    /// <summary>Every value as a quoted SQL list, for <c>check (column in (...))</c>.</summary>
    public static string SqlList<TEnum>()
        where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetValues<TEnum>().Select(value => $"'{ToText(value)}'"));
}
