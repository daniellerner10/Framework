using Serilog.Core;
using Serilog.Events;

namespace Keeper.Framework.Logging;

class PropertyEnricher(Dictionary<string, string> _properties) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_properties is not null)
        {
            foreach (var item in _properties)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(item.Key, item.Value));
            }
        }
    }
}
