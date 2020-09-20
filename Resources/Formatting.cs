using System.Globalization;
using System.Linq;

// This global class defines extension methods to numeric types
// where I don't want system globalization to come into play.

public static class Format
{
    private const string decimalFmt = "0.0000000";
    private static CultureInfo invariant => CultureInfo.InvariantCulture;

    private static string filterNan(string value, string replace = decimalFmt)
    {
        if (value.ToUpperInvariant() == "NAN")
            value = replace;

        return value;
    }

    public static string ToInvariantString(this float value)
    {
        string result = value.ToString(decimalFmt, invariant);
        return filterNan(result);
    }

    public static string ToInvariantString(this double value)
    {
        string result = value.ToString(decimalFmt, invariant);
        return filterNan(result);
    }

    public static string ToInvariantString(this int value)
    {
        return value.ToString(invariant);
    }

    public static string ToInvariantString(this object value)
    {
        switch (value)
        {
            case int i    : return i.ToInvariantString();
            case float f  : return f.ToInvariantString();
            case double d : return d.ToInvariantString();
            default       : return value?.ToString();
        }
    }

    public static float ParseFloat(string s)
    {
        return float.Parse(s, invariant);
    }

    public static double ParseDouble(string s)
    {
        return double.Parse(s, invariant);
    }

    public static int ParseInt(string s)
    {
        return int.Parse(s, invariant);
    }

    public static string FormatFloats(params float[] values)
    {
        string[] results = values
            .Select(value => value.ToInvariantString())
            .ToArray();

        return string.Join(" ", results);
    }
}