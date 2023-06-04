using Apache.Ignite.Core.Cache.Configuration;

namespace IgniteAPI
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int DepartmentId { get; set; }

        public Person(int id, string name, int departmentId)
        {
            Id = id;
            Name = name;
            DepartmentId = departmentId;
        }

        public override string ToString()
        {
            return $"Person [Id={Id}, Name={Name}, DepartmentId={DepartmentId}]";
        }
    }   
}