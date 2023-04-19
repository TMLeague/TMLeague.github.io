namespace TMModels.Extensions;

public static class MathExtensions
{
    public static double Std(this ICollection<double> values)
    {
        var avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }

    public static double Std(this ICollection<int> values)
    {
        var avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }
}