using System;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money From(decimal amount)
    {
        // decimal لا يدعم NaN/Infinity، لكن نتحقق من المنطق والأمان
        var rounded = Round2(amount);

        if (rounded < 0m)
            throw new BusinessRuleException("Money amount cannot be negative.", "MONEY_NEGATIVE_NOT_ALLOWED");

        return new Money(rounded);
    }

    public static Money Zero => new(0m);

    public Money Add(Money other) => From(Amount + other.Amount);
    public Money Subtract(Money other)
    {
        var result = Amount - other.Amount;
        if (result < 0m)
            throw new BusinessRuleException("Money result cannot be negative.", "MONEY_RESULT_NEGATIVE");
        return From(result);
    }

    public static Money operator +(Money a, Money b) => a.Add(b);
    public static Money operator -(Money a, Money b) => a.Subtract(b);

    public override string ToString() => Amount.ToString("0.00");

    private static decimal Round2(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}