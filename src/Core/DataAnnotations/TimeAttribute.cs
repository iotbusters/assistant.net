using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Assistant.Net.DataAnnotations;

/// <summary>
///     TimeSpan range value validation attribute.
/// </summary>
public class TimeAttribute : ValidationAttribute
{
    private readonly TimeSpan? minValue;
    private readonly TimeSpan? maxValue;
    /// <summary>
    ///     
    /// </summary>
    private readonly List<string> invalidArgumentMessages = new(3);

    /// <summary/>
    /// <param name="minStringValue">Minimum time value (including) presented as string with format: '00:00:00' or '00:00:00.0'.</param>
    /// <param name="maxStringValue">Maximum time value (including) presented as string with format: '00:00:00' or '00:00:00.0'.</param>
    public TimeAttribute(string? minStringValue = null, string? maxStringValue = null)
    {
        this.minValue = ConvertOrDefault(minStringValue) ?? TimeSpan.MinValue;
        this.maxValue = ConvertOrDefault(maxStringValue) ?? TimeSpan.MaxValue;

        if (maxValue < minValue)
            invalidArgumentMessages.Add($"The max value {maxValue} is less than min value {minValue}.");
    }

    /// <summary/>
    /// <param name="minSeconds">Minimum time value (including) in seconds.</param>
    /// <param name="maxSeconds">Maximum time value (including) in seconds.</param>
    public TimeAttribute(float? minSeconds = null, float? maxSeconds = null)
    {
        this.minValue = ConvertOrDefault(minSeconds) ?? TimeSpan.MinValue;
        this.maxValue = ConvertOrDefault(maxSeconds) ?? TimeSpan.MaxValue;

        if (maxValue < minValue)
            invalidArgumentMessages.Add($"The max value {maxValue} is less than min value {minValue}.");
    }

    /// <summary/>
    public TimeAttribute(TimeSpan? minValue, TimeSpan? maxValue)
    {
        this.minValue = minValue ?? TimeSpan.MinValue;
        this.maxValue = maxValue ?? TimeSpan.MaxValue;

        if (this.maxValue < this.minValue)
            invalidArgumentMessages.Add($"The max value {maxValue} is less than min value {minValue}.");
    }

    /// <summary>
    ///     Determines whether infinite value is allowed.
    /// </summary>
    public bool AllowInfinite { get; set; } = false;

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

        if (AllowInfinite && time == Timeout.InfiniteTimeSpan)
            return ValidationResult.Success;

        var isLessThan = time < minValue;
        var isGreaterThan = time > maxValue;
        if (isLessThan || isGreaterThan)
        {
            var messages = new List<string>(2);
            if(isLessThan)
                messages.Add($"less than {minValue}");
            if (isGreaterThan)
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

        if (!TimeSpan.TryParseExact(stringValue, "c", new DateTimeFormatInfo(), out var value))
        {
            invalidArgumentMessages.Add($"Invalid time string: '{stringValue}'.");
            return null;
        }

        if (value < TimeSpan.Zero)
            invalidArgumentMessages.Add($"The value {value} is below zero.");

        return value;
    }

    private TimeSpan? ConvertOrDefault(float? seconds)
    {
        if (seconds == null)
            return null;

        if (seconds < 0f)
            invalidArgumentMessages.Add($"The value {seconds}s is below zero.");

        return TimeSpan.FromSeconds(seconds.Value);
    }
}
