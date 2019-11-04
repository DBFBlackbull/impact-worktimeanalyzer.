using System.Collections.Generic;

namespace Impact.Core.Model
{
    public class Project
    {
        public int Id { get; }
        public string Name { get; }
        public IList<Task> Tasks { get; }

        public Project(int id, string name)
        {
            Id = id;
            Name = name;
            Tasks = new List<Task>();
        }
    }

    public class Task
    {
        public int Id { get; }
        public string Name { get; }

        public Task(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}