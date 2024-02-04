namespace EventSourcedPM.Configuration;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

public static class StringBuilderExtensions
{
    private const string Indent = "  ";
    private const string DefaultValue = "<NOT SET>";

    public static StringBuilder AppendSettingTitle(this StringBuilder stringBuilder, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be specified", nameof(name));
        stringBuilder.AppendLine($"{name} settings".ToUpperInvariant());
        stringBuilder.AppendLine("=====");
        return stringBuilder;
    }

    public static StringBuilder AppendSettingSectionTitle(
        this StringBuilder stringBuilder,
        string name
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be specified", nameof(name));
        stringBuilder.AppendLine(name);
        stringBuilder.AppendLine("---");
        return stringBuilder;
    }

    public static StringBuilder AppendSettingValue<TResult>(
        this StringBuilder stringBuilder,
        Expression<Func<TResult>> propertyExpression
    )
    {
        var body = (MemberExpression)propertyExpression.Body;
        var value = propertyExpression.Compile()();

        var formattedValue = value != null ? value.ToString() : DefaultValue;
        stringBuilder.AppendLine($"{Indent}{body.Member.Name}: {formattedValue}");

        return stringBuilder;
    }

    public static StringBuilder AppendConfigLine(
        this StringBuilder stringBuilder,
        Expression<Func<TimeSpan>> propertyExpression
    )
    {
        var body = (MemberExpression)propertyExpression.Body;
        var timeSpan = propertyExpression.Compile()();

        var formattedValue = timeSpan != default ? timeSpan.ToString() : DefaultValue;
        stringBuilder.AppendLine($"{Indent}{body.Member.Name}: {formattedValue}");
        return stringBuilder;
    }

    public static StringBuilder AppendSubSection<TResult>(
        this StringBuilder stringBuilder,
        Expression<Func<TResult>> propertyExpression
    )
    {
        var body = (MemberExpression)propertyExpression.Body;
        stringBuilder.AppendSettingSectionTitle(body.Member.Name);

        var value = propertyExpression.Compile()();
        var formattedValue = value?.ToString() ?? DefaultValue;

        var lines = formattedValue.Split('\n');
        foreach (var line in lines)
        {
            stringBuilder.AppendLine($"{Indent}{Indent}{line}");
        }

        return stringBuilder;
    }

    public static StringBuilder AppendDictionarySettings<TKey, TValue>(
        this StringBuilder stringBuilder,
        Expression<Func<Dictionary<TKey, TValue>>> propertyExpression
    )
    {
        var body = (MemberExpression)propertyExpression.Body;
        var dictionary = propertyExpression.Compile()();

        if (dictionary == null)
        {
            stringBuilder.AppendLine($"{Indent}{body.Member.Name}: {DefaultValue}");
        }
        else
        {
            stringBuilder.AppendLine($"{Indent}{body.Member.Name}:");
            foreach (var item in dictionary)
            {
                stringBuilder.AppendLine($"{Indent}{Indent}{item.Key}:{item.Value}");
            }
        }

        return stringBuilder;
    }

    public static StringBuilder AppendEnumerableSettings<T>(
        this StringBuilder stringBuilder,
        Expression<Func<IEnumerable<T>>> propertyExpression
    )
    {
        var body = (MemberExpression)propertyExpression.Body;
        var enumerable = propertyExpression.Compile()();

        if (enumerable == null)
        {
            stringBuilder.AppendLine($"{Indent}{body.Member.Name}: {DefaultValue}");
        }
        else
        {
            stringBuilder.AppendLine($"{Indent}{body.Member.Name}:");
            foreach (var item in enumerable)
            {
                stringBuilder.AppendLine($"{Indent}{Indent}{item}");
            }
        }

        return stringBuilder;
    }

    public static string ToUrlPart(this string s) =>
        s != null ? Uri.EscapeDataString(s) : string.Empty;
}
