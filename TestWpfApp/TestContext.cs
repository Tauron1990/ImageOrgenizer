using System.Collections.Generic;
using ImageOrganizer.Views.ImageEditorHelper;
using Syncfusion.UI.Xaml.Grid;

namespace TestWpfApp
{
    public class TestContext
    {
        public TestObvers TestDatas { get; set; }

        public LinkedCollection<TestData> TestLinkedCollection { get; set; }

        public string[] Names { get; set; }

        public TestContext()
        {
            TestDatas = new TestObvers();
            TestLinkedCollection = new LinkedCollection<TestData>(TestDatas);
            var mixer = new RandomNames();
            Types = new[] {"Male", "Female"};
            Names = mixer.Names;

            for (int i = 0; i < 100; i++)
            {
                var data = new TestData();
                mixer.Fill(data, Types);
                TestDatas.Add(data);
            }
        }

        public IEnumerable<string> Types { get; set; }

        public void OnAddNew(object sender, AddNewRowInitiatingEventArgs addNewRowInitiatingEventArgs) => ((TestData) addNewRowInitiatingEventArgs.NewObject).Names = Names;
    }
}