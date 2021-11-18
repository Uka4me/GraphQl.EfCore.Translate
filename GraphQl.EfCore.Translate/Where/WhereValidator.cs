using GraphQl.EfCore.Translate;
using System;

static class WhereValidator
{
    public static void ValidateObject(Type propertyType, Comparison comparison, CaseString? @case)
    {
        if (comparison == Comparison.Contains ||
            comparison == Comparison.StartsWith ||
            comparison == Comparison.EndsWith/* ||
            comparison == Comparison.Like*/)
        {
            throw new($"Cannot perform {comparison} on {propertyType.FullName}.");
        }

        if (@case is not null)
        {
            throw new($"Cannot use {nameof(CaseString)} when comparing {propertyType.FullName}.");
        }
    }

    public static void ValidateSingleObject(Type propertyType, Comparison comparison, CaseString? @case)
    {
        ValidateObject(propertyType, comparison, @case);
        if (comparison == Comparison.In)
        {
            throw new($"Cannot perform {comparison} on {propertyType.FullName}.");
        }
    }

    public static void ValidateString(Comparison comparison, CaseString? @case)
    {
        if (comparison == Comparison.GreaterThan ||
            comparison == Comparison.GreaterThanOrEqual ||
            comparison == Comparison.LessThanOrEqual ||
            comparison == Comparison.LessThan)
        {
            throw new($"Cannot perform {comparison} on a String.");
        }

        /*if (comparison == Comparison.Like && @case is not null)
        {
            throw new($"{nameof(Comparison.Like)} is not compatible with {nameof(CaseString)}.");
        }*/
    }

    public static void ValidateSingleString(Comparison comparison, CaseString? @case)
    {
        ValidateString(comparison, @case);
        if (comparison == Comparison.In)
        {
            throw new($"Cannot perform {comparison} on a single String.");
        }
    }
}