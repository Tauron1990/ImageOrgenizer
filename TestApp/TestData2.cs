using System.Collections.Generic;

namespace TestApp
{
    public class TestData2
    {
        public int Id { get; set; }

        public string TestProp { get; set; }

        public ICollection<TestDataConnector> Connectors { get; set; }
    }
}