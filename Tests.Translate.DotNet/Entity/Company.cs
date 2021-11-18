using System.Collections.Generic;

namespace Tests.Translate.DotNet.Entity
{
    public class Company
    {
        public string? Name { get; set; }
        public IList<Person> Employees { get; set; } = null!;
    }
}
