using CityServicesHub.BuildingBlocks.EventBus.Abstractions;

namespace CityServicesHub.BuildingBlocks.EventBus;

/// <summary>
/// Gestiona las suscripciones a eventos de integración.
/// </summary>
public interface IEventBusSubscriptionManager
{
    bool IsEmpty { get; }
    event EventHandler<string> OnEventRemoved;
    
    void AddSubscription<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
    
    void RemoveSubscription<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
    
    bool HasSubscriptionsForEvent<TEvent>() where TEvent : IntegrationEvent;
    bool HasSubscriptionsForEvent(string eventName);
    Type? GetEventTypeByName(string eventName);
    void Clear();
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>() where TEvent : IntegrationEvent;
    IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
    string GetEventKey<TEvent>() where TEvent : IntegrationEvent;
}

/// <summary>
/// Información de una suscripción a un evento.
/// </summary>
public class SubscriptionInfo
{
    public Type HandlerType { get; }

    private SubscriptionInfo(Type handlerType)
    {
        HandlerType = handlerType;
    }

    public static SubscriptionInfo Create(Type handlerType)
    {
        return new SubscriptionInfo(handlerType);
    }
}

/// <summary>
/// Implementación en memoria del gestor de suscripciones.
/// </summary>
public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
    private readonly List<Type> _eventTypes;

    public event EventHandler<string>? OnEventRemoved;

    public InMemoryEventBusSubscriptionManager()
    {
        _handlers = new Dictionary<string, List<SubscriptionInfo>>();
        _eventTypes = new List<Type>();
    }

    public bool IsEmpty => !_handlers.Any();

    public void AddSubscription<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = GetEventKey<TEvent>();
        
        if (!HasSubscriptionsForEvent(eventName))
        {
            _handlers.Add(eventName, new List<SubscriptionInfo>());
        }

        var handlerType = typeof(THandler);
        
        if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
        {
            throw new ArgumentException(
                $"Handler {handlerType.Name} already registered for '{eventName}'");
        }

        _handlers[eventName].Add(SubscriptionInfo.Create(handlerType));

        if (!_eventTypes.Contains(typeof(TEvent)))
        {
            _eventTypes.Add(typeof(TEvent));
        }
    }

    public void RemoveSubscription<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = GetEventKey<TEvent>();
        var handlerToRemove = FindSubscriptionToRemove<TEvent, THandler>(eventName);
        
        if (handlerToRemove != null)
        {
            _handlers[eventName].Remove(handlerToRemove);
            
            if (!_handlers[eventName].Any())
            {
                _handlers.Remove(eventName);
                var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                if (eventType != null)
                {
                    _eventTypes.Remove(eventType);
                }
                OnEventRemoved?.Invoke(this, eventName);
            }
        }
    }

    private SubscriptionInfo? FindSubscriptionToRemove<TEvent, THandler>(string eventName)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        if (!HasSubscriptionsForEvent(eventName))
        {
            return null;
        }

        return _handlers[eventName].SingleOrDefault(s => s.HandlerType == typeof(THandler));
    }

    public bool HasSubscriptionsForEvent<TEvent>() where TEvent : IntegrationEvent
    {
        return HasSubscriptionsForEvent(GetEventKey<TEvent>());
    }

    public bool HasSubscriptionsForEvent(string eventName)
    {
        return _handlers.ContainsKey(eventName);
    }

    public Type? GetEventTypeByName(string eventName)
    {
        return _eventTypes.SingleOrDefault(t => t.Name == eventName);
    }

    public void Clear()
    {
        _handlers.Clear();
        _eventTypes.Clear();
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent<TEvent>() where TEvent : IntegrationEvent
    {
        return GetHandlersForEvent(GetEventKey<TEvent>());
    }

    public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName)
    {
        return _handlers.TryGetValue(eventName, out var handlers) 
            ? handlers 
            : Enumerable.Empty<SubscriptionInfo>();
    }

    public string GetEventKey<TEvent>() where TEvent : IntegrationEvent
    {
        return typeof(TEvent).Name;
    }
}
