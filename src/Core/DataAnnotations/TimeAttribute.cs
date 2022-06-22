using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Assistant.Net.DataAnnotations;

/// <summary>
///     TimeSpan range value validation attribute.
/// </summary>
public class TimeAttribute : ValidationAttribute
{
    private readonly TimeSpan? minValue;
    private readonly TimeSpan? maxValue;
    private readonly List<string> invalidArgumentMessages = new(2);

    /// <summary/>
    /// <param name="minStringValue">Minimum time value presented as string with format: '00:00:00' or '00:00:00.0'.</param>
    /// <param name="maxStringValue">Maximum time value presented as string with format: '00:00:00' or '00:00:00.0'.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public TimeAttribute(string? minStringValue = null, string? maxStringValue = null)
    {
        this.minValue = ConvertOrDefault(minStringValue) ?? TimeSpan.MinValue;
        this.maxValue = ConvertOrDefault(maxStringValue) ?? TimeSpan.MaxValue;
    }

    /// <summary/>
    /// <param name="minSeconds">Minimum time value in seconds.</param>
    /// <param name="maxSeconds">Maximum time value in seconds.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public TimeAttribute(float? minSeconds = null, float? maxSeconds = null)
    {
        this.minValue = ConvertOrDefault(minSeconds) ?? TimeSpan.MinValue;
        this.maxValue = ConvertOrDefault(maxSeconds) ?? TimeSpan.MaxValue;
    }

    /// <summary/>
    /// <exception cref="ArgumentException"/>
    public TimeAttribute(TimeSpan? minValue, TimeSpan? maxValue)
    {
        this.minValue = minValue ?? TimeSpan.MinValue;
        this.maxValue = maxValue ?? TimeSpan.MaxValue;
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var memberNames = new[] {validationContext.MemberName!};

        if (invalidArgumentMessages.Any())
            return new ValidationResult(string.Join(Environment.NewLine, invalidArgumentMessages), memberNames);

        if (value == null)
            return ValidationResult.Success;

        if (value is not TimeSpan time)
            return new ValidationResult("The value isn't a TimeSpan.", memberNames);

        var isLessThan = time < minValue;
        var isGreaterThan = time > maxValue;
        if (isLessThan || isGreaterThan)
        {
            var messages = new List<string>(2);
            if(isLessThan)
                messages.Add($"less than {minValue}");
            if (isLessThan)
                messages.Add($"greater than {maxValue}");
            var message = $"The value {time} is " + string.Join(" and ", messages) + ".";
            return new ValidationResult(message, memberNames);
        }

        return ValidationResult.Success;
    }

    private TimeSpan? ConvertOrDefault(string? stringValue)
    {
        if (stringValue == null)
            return null;

        if (!TimeSpan.TryParse(stringValue, out var value))
        {
            invalidArgumentMessages.Add($"Invalid time string: '{stringValue}'.");
            return null;
        }

        if (value < TimeSpan.Zero)
        {
            invalidArgumentMessages.Add($"The time value should be greater or equal to zero: '{value}'.");
            return null;
        }

        return value;
    }

    private TimeSpan? ConvertOrDefault(float? seconds)
    {
        if (seconds == null)
            return null;

        if (seconds < 0f)
        {
            invalidArgumentMessages.Add($"The value should be greater or equal to zero: '{seconds}'.");
            return null;
        }

        return TimeSpan.FromSeconds(seconds.Value);
    }
}
