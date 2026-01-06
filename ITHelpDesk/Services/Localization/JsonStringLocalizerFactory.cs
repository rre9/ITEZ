using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ITHelpDesk.Services.Localization;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;
    private readonly ILoggerFactory _loggerFactory;

    public JsonStringLocalizerFactory(string resourcesPath, ILoggerFactory loggerFactory)
    {
        _resourcesPath = resourcesPath;
        _loggerFactory = loggerFactory;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var baseName = resourceSource.FullName ?? resourceSource.Name;
        var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();
        return new JsonStringLocalizer(_resourcesPath, baseName, logger);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();
        return new JsonStringLocalizer(_resourcesPath, baseName, logger);
    }
}

