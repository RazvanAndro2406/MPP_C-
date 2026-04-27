using Org.Example.Domain;

namespace Lab2.domain
{
    public class Buyer : Entity<long>
    {
        public string Name { get; set; }

        public Buyer(long id, string name) : base(id)
        {
            Name = name;
        }

        public Buyer(string name) : base(default)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"Buyer{{id={Id}, name='{Name}'}}";
        }
    }
}

