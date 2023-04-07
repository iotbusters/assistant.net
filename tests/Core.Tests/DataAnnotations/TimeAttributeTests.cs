using Assistant.Net.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Core.Tests.DataAnnotations;

public class TimeAttributeTests
{
    [TestCase("0")]
    [TestCase("0:1")]
    [TestCase("0:0:1")]
    [TestCase("0:0:0.01")]
    public void GetValidationResult_returnsValidationResultSuccess_greaterThenMinStringValue(string value) =>
        new TimeAttribute(minStringValue: value)
            .GetValidationResult(TimeSpan.FromHours(1), new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [TestCase("1")]
    [TestCase("1:0")]
    [TestCase("1:0:0")]
    [TestCase("1:0:0.01")]
    public void GetValidationResult_returnsValidationResultSuccess_lessThanMaxStringValue(string value) =>
        new TimeAttribute(maxStringValue: value)
            .GetValidationResult(TimeSpan.FromMinutes(1), new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [TestCase("0")]
    [TestCase("0:0")]
    [TestCase("0:0:0")]
    [TestCase("0:0:0.00")]
    public void GetValidationResult_returnsValidationResultSuccess_zeroStringValues(string value) =>
        new TimeAttribute(minStringValue: value, maxStringValue: value)
            .GetValidationResult(TimeSpan.Zero, new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [Test]
    public void GetValidationResult_returnsValidationResultSuccess_betweenMinAndMaxStringValues() =>
        new TimeAttribute(minStringValue: "00:00:01", maxStringValue: "00:00:03")
            .GetValidationResult(TimeSpan.FromSeconds(2), new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [Test]
    public void GetValidationResult_returnsValidationResult_lessThanMinStringValue() =>
        new TimeAttribute(minStringValue: "00:00:02")
            .GetValidationResult(TimeSpan.FromSeconds(1), new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult("The value 00:00:01 is less than 00:00:02.", new[] {"property"}));

    [Test]
    public void GetValidationResult_returnsValidationResult_greaterThanMaxStringValue() =>
        new TimeAttribute(maxStringValue: "00:00:01")
            .GetValidationResult(TimeSpan.FromSeconds(2), new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult("The value 00:00:02 is greater than 00:00:01.", new[] {"property"}));

    [TestCase("")]
    [TestCase("0.0")]
    [TestCase("0:f")]
    [TestCase("f:0")]
    public void GetValidationResult_returnsValidationResult_invalidArguments(string invalidValue) =>
        new TimeAttribute(minStringValue: invalidValue, maxStringValue: invalidValue)
            .GetValidationResult(null, new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult(
                $"Invalid time string: '{invalidValue}'."
                + Environment.NewLine
                + $"Invalid time string: '{invalidValue}'.",
                new[] {"property"}));

    [Test]
    public void GetValidationResult_returnsValidationResult_invalidMaxStringValue() =>
        new TimeAttribute(minStringValue: "-0:1", maxStringValue: "-0:2")
            .GetValidationResult(TimeSpan.Zero, new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult(
                "The value -00:01:00 is below zero."
                + Environment.NewLine
                + "The value -00:02:00 is below zero."
                + Environment.NewLine
                + "The max value -00:02:00 is less than min value -00:01:00.",
                new[] { "property" }));

    [Test]
    public void GetValidationResult_returnsValidationResultSuccess_greaterThenMinSeconds() =>
        new TimeAttribute(minSeconds: 1)
            .GetValidationResult(TimeSpan.FromHours(1), new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [Test]
    public void GetValidationResult_returnsValidationResultSuccess_lessThanMaxSeconds() =>
        new TimeAttribute(maxSeconds: 2 * 60)
            .GetValidationResult(TimeSpan.FromMinutes(1), new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [Test]
    public void GetValidationResult_returnsValidationResultSuccess_zeroSeconds() =>
        new TimeAttribute(minSeconds: 0f, maxSeconds: 0f)
            .GetValidationResult(TimeSpan.Zero, new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [Test]
    public void GetValidationResult_returnsValidationResultSuccess_betweenMinAndMaxSeconds() =>
        new TimeAttribute(minStringValue: "00:00:01", maxStringValue: "00:00:03")
            .GetValidationResult(TimeSpan.FromSeconds(2), new(new()))
            .Should().BeEquivalentTo(ValidationResult.Success);

    [Test]
    public void GetValidationResult_returnsValidationResult_lessThanMinSeconds() =>
        new TimeAttribute(minSeconds: 2)
            .GetValidationResult(TimeSpan.FromSeconds(1), new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult("The value 00:00:01 is less than 00:00:02.", new[] {"property"}));

    [Test]
    public void GetValidationResult_returnsValidationResult_greaterThanMaxSeconds() =>
        new TimeAttribute(maxSeconds: 1)
            .GetValidationResult(TimeSpan.FromSeconds(2), new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult("The value 00:00:02 is greater than 00:00:01.", new[] {"property"}));

    [Test]
    public void GetValidationResult_returnsValidationResult_invalidSecondsArguments() =>
        new TimeAttribute(minSeconds: -1, maxSeconds: -2)
            .GetValidationResult(TimeSpan.Zero, new(new()) {MemberName = "property"})
            .Should().BeEquivalentTo(new ValidationResult(
                "The value -1s is below zero."
                + Environment.NewLine
                + "The value -2s is below zero."
                + Environment.NewLine
                + "The max value -00:00:02 is less than min value -00:00:01.",
                new[] {"property"}));
}
