using System;
using System.Collections.Generic;

namespace Ticketing.Model.Domain
{
    [Serializable]
    public abstract class Entity<TId> : IIdentifiable<TId>
    {
        // Matches Java: protected ID id;
        // In C#, we use a property which generates the field automatically
        public TId Id { get; set; }

        // Matches Java: public Entity(ID id)
        protected Entity(TId id)
        {
            Id = id;
        }

        // Matches Java: public boolean equals(Object o)
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            
            // Matches Java: if (!(o instanceof Entity)) return false;
            if (obj is not Entity<TId> other) return false;

            // Matches Java: Objects.equals(getId(), entity.getId())
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        // Matches Java: public int hashCode()
        public override int GetHashCode()
        {
            return Id != null ? EqualityComparer<TId>.Default.GetHashCode(Id) : 0;
        }

        // Matches Java: public String toString()
        public override string ToString()
        {
            return $"Entity{{id={Id}}}";
        }
    }
}