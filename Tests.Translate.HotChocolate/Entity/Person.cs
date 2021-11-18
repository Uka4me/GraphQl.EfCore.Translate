using System;

namespace Tests.Translate.HotChocolate.Entity
{
    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Company? Company { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
