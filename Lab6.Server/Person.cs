using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lab6.Server
{
    [Serializable]
    [DataContract]
    public class Person
    {
        Random random = new Random();

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Age { get; set; }
        [DataMember]
        public Date Date { get; set; }

        public Person()
        {
            Age = random.Next(1,100);
            Name = "Номер человека в списке: " + Age;
        }
        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }
        public Person(string name,int age,Date date)
        {
            Name = name;
            Age = age;
            Date = date;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
