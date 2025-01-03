using CollectionsZero;
using System.Diagnostics;

namespace TestProject
{
    [TestClass]
    public sealed class CollectionZeroTests
    {
        [TestMethod]
        public void TestInsert()
        {
            var collection = new ObsCollZero<int>(AttachItem, DetachItem);

            collection.Add(4);

            collection.Add(6);

            collection.Add(2);
            collection.Add(5);

            collection.RemoveAt(1);

            collection.Remove(2);

            collection.Clear();

        }
        private void AttachItem(int obj)
        {
            Debug.WriteLine($"Attached to {obj}");
        }

        private void DetachItem(int obj)
        {
            Debug.WriteLine($"Detached from {obj}");

        }
    }
}
