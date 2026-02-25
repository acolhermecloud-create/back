using Newtonsoft.Json.Serialization;
using System.Globalization;

public class CamelCasePropertyResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return propertyName;

        // Primeira letra minúscula e cada palavra subsequente começa com maiúscula
        return char.ToLower(propertyName[0], CultureInfo.InvariantCulture) +
               propertyName.Substring(1);
    }
}
