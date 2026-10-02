using System.Globalization;

namespace LogMyOJT.Services;

// small helpers so dates and times always look the same on every machine
public static class Fmt
{
    private static readonly CultureInfo C = CultureInfo.InvariantCulture;

    public static string Date(DateOnly d) => d.ToString("ddd, MMM d", C);
    public static string LongDate(DateOnly d) => d.ToString("MMM d, yyyy", C);
    public static string Time(TimeOnly t) => t.ToString("hh:mm tt", C);
    public static string Hours(double h) => h.ToString("0.0", C);
}
