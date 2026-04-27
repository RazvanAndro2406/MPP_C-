using System;
using System.Collections.Generic;

namespace Org.Example.Domain
{
    public class Artist : Entity<long>
    {
        public string Name { get; set; }

        public Artist(long id, string name) : base(id)
        {
            Name = name;
        }

        public Artist(string name) : base(default)
        {
            Name = name;
        }
        
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not Artist other) return false;
            
           
            if (!base.Equals(obj)) return false;

            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Name);
        }

        public override string ToString()
        {
            return $"Artist{{id={Id}, name='{Name}'}}";
        }
    }
}