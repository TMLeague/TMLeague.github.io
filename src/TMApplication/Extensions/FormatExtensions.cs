using System.Text.RegularExpressions;

namespace TMApplication.Extensions;

public static class FormatExtensions
{
    public static string FillParameters(this string format, object source) =>
        Regex.Replace(format, @"{(\w+)}", match =>
        {
            var propertyName = match.Groups[1].Value;
            var property = source.GetType().GetProperties()
                .FirstOrDefault(info => info.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (property == null)
                return $"{{{propertyName}}}";
            return property.GetValue(source)?.ToString() ?? string.Empty;
        });
}