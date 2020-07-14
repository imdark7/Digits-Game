using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
    public class Tests
    {
        // A Test behaves as an ordinary method
        [TestCase(new int[0], 3, 0, new[] {3})]
        [TestCase(new[] {1}, 3, 0, new[] {3, 1})]
        [TestCase(new[] {1}, 3, 1, new[] {1, 3})]
        [TestCase(new[] {1, 2, 3}, 5, 1, new[] {1, 5, 2, 3})]
        public void TestAddAt(int[] arr, int next, int index, int[] expected)
        {
            var list = InitList(arr);
            list.AddAt(index, next);
            CheckList(list, expected);
        }

        [TestCase(new[] {1, 2, 3}, 0, 2, new[] {3})]
        [TestCase(new[] {1, 2, 3}, 2, 1, new[] {1, 2})]
        [TestCase(new[] {1, 2, 3}, 2, 2, new[] {2})]
        [TestCase(new[] {1, 2, 3, 4, 5, 6}, 4, 4, new[] {3, 4})]
        public void TestRemoveAt(int[] arr, int index, int count, int[] expected)
        {
            var list = InitList(arr);
            list.RemoveAt(index, count);
            CheckList(list, expected);
        }

        [TestCase(new [] {1, 1, 1, 1, 1}, 1, 5, 0, 5)]
        [TestCase(new [] {2, 1, 1, 1, 1, 1, 1, 1}, 1, 5, 1, 7)]
        [TestCase(new [] {1, 2, 1, 1, 1, 1, 1, 1}, 1, 5, 2, 7)]
        [TestCase(new [] {1, 1, 1, 1, 1, 1, 2, 1}, 1, 5, 7, 7)]
        [TestCase(new [] {1, 1, 1, 1, 1, 1, 1, 2}, 1, 5, 0, 7)]
        [TestCase(new [] {1, 3, 1, 1, 1, 1, 1, 2}, 1, 5, 2, 5)]
        public void TestTryGetRowOfDigits(int[] arr, int searchValue, int countForRow, int expectedStartIndex, int expectedCount)
        {
            var list = InitList(arr);
            Assert.True(list.TryGetRowOfDigits(searchValue, countForRow, out var startIndex, out var count));
            Assert.True(startIndex == expectedStartIndex, $"startIndex = {startIndex}, expectedStartIndex = {expectedStartIndex}");
            Assert.True(count == expectedCount, $"count = {count}, expectedCount = {expectedCount}");
        }

        [TestCase(new [] {1, 3, 1, 1, 1, 1, 1, 2}, 1, 5, new [] {1,3,10,2})]
        [TestCase(new [] {1, 1, 1, 2, 2, 1, 1, 1}, 1, 5, new [] {10,2,2})]
        public void TestScenario(int[] arr, int searchValue, int countForRow, int[] expectedArr)
        {
            var list = InitList(arr);
            Assert.True(list.TryGetRowOfDigits(searchValue, countForRow, out var startIndex, out var count));
            list.RemoveAt(startIndex, count);
            list.AddAt(startIndex, 10);
            CheckList(list, expectedArr);
        }

        private static void CheckList(CircleList list, int[] expected)
        {
            Assert.True(list.Count == expected.Length, "list.Count not equal expected.Length");
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(list.InnerList[i] == expected[i], $"list.InnerList[i] = {list.InnerList[i]}, expected[i] = {expected[i]}");
            }
        }

        private static CircleList InitList(int[] arr)
        {
            var list = new CircleList();
            for (var i = 0; i < arr.Length; i++)
            {
                list.AddAt(i, arr[i]);
            }

            return list;
        }
    }
}