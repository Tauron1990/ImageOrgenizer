using System.Collections.Generic;

namespace TestApp
{
    public class TestData
    {
        public int Id { get; set; }

        public string TestProp2 { get; set; }

        public ICollection<TestDataConnector> Connectors { get; set; }
    }
}