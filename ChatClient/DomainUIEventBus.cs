using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatClient
{
    public class DomainUiEventBus
    {
        public enum EventType
        {
            ArtistsChanged,
            SpectaclesChanged,
            TicketsChanged,
            BuyersChanged,
            TicketSalesChanged,
            ArtistSpectaclesChanged
        }

        public interface IListener
        {
            void OnDomainEvent(EventType type);
        }

        private readonly List<IListener> _listeners = new List<IListener>();
        private readonly object _lock = new object();

        public void Subscribe(IListener listener)
        {
            lock (_lock)
            {
                if (!_listeners.Contains(listener))
                    _listeners.Add(listener);
            }
        }

        public void Unsubscribe(IListener listener)
        {
            lock (_lock)
            {
                _listeners.Remove(listener);
            }
        }

        public void Publish(EventType type)
        {
            List<IListener> snapshot;
            lock (_lock)
            {
                snapshot = _listeners.ToList();
            }

            foreach (var listener in snapshot)
            {
                listener.OnDomainEvent(type);
            }
        }

        public void PublishMany(params EventType[] types)
        {
            var uniqueTypes = new HashSet<EventType>(types);
            foreach (var type in uniqueTypes)
            {
                Publish(type);
            }
        }

        public void PublishAll()
        {
            PublishMany((EventType[])Enum.GetValues(typeof(EventType)));
        }
    }
}