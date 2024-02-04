namespace EventSourcedPM.Domain.Models;

using System;

public static class IsoDateTimeExtensions
{
    public static string ToIsoDate(this DateOnly date) => date.ToString("yyyy-MM-dd");

    public static string ToIsoDate(this DateTime date) => date.ToString("yyyy-MM-dd");

    public static string ToIsoTimestamp(this DateTime date) => date.ToString("O");
}
