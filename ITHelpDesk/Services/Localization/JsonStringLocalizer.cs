using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ITHelpDesk.Services.Localization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly string _resourcesPath;
    private readonly string _baseName;
    private readonly ILogger<JsonStringLocalizer>? _logger;

    public JsonStringLocalizer(string resourcesPath, string baseName, ILogger<JsonStringLocalizer>? logger = null)
    {
        _resourcesPath = resourcesPath;
        _baseName = baseName;
        _logger = logger;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = GetString(name);
            var value = format != null ? string.Format(format, arguments) : name;
            return new LocalizedString(name, value, resourceNotFound: format == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture;
        var resourceFile = GetResourceFile(culture.Name);
        
        if (File.Exists(resourceFile))
        {
            var json = File.ReadAllText(resourceFile);
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    yield return new LocalizedString(kvp.Key, kvp.Value);
                }
            }
        }
    }


    private string? GetString(string name)
    {
        var culture = CultureInfo.CurrentUICulture;
        var resourceFile = GetResourceFile(culture.Name);
        
        if (File.Exists(resourceFile))
        {
            try
            {
                var json = File.ReadAllText(resourceFile);
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                
                if (dictionary != null && dictionary.TryGetValue(name, out var value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reading localization file: {ResourceFile}", resourceFile);
            }
        }

        // Fallback to English if not found
        if (!culture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            var enResourceFile = GetResourceFile("en");
            if (File.Exists(enResourceFile))
            {
                try
                {
                    var json = File.ReadAllText(enResourceFile);
                    var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (dictionary != null && dictionary.TryGetValue(name, out var value))
                    {
                        return value;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error reading English fallback file: {ResourceFile}", enResourceFile);
                }
            }
        }

        return null;
    }

    private string GetResourceFile(string cultureName)
    {
        return Path.Combine(_resourcesPath, $"{cultureName}.json");
    }
}

