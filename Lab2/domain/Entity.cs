using System;
using System.Collections.Generic;

namespace Org.Example.Domain
{
    [Serializable]
    public abstract class Entity<TId>
    {
        public TId Id { get; set; }

        protected Entity(TId id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not Entity<TId> other) return false;

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return $"Entity{{id={Id}}}";
        }
    }
}