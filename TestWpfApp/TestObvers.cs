using System.Collections.ObjectModel;

namespace TestWpfApp
{
    public class TestObvers : ObservableCollection<TestData>
    {
        protected override void InsertItem(int index, TestData item)
        {
            base.InsertItem(index, item);
        }
    }
}