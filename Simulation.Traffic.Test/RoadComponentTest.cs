using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Simulation.Traffic;

namespace Simulation.TrafficTest
{
    [TestClass]
    public class RoadComponentTest
    {
        private class TestComponent : RoadComponent<string>
        {
            private TaskCompletionSource<string> cts;

            public bool? CompleteTest(string result)
            {
                return cts?.TrySetResult(result);
            }

            public Action AssertAction { get; set; }

            protected override async Task<string> GetValueAsync(CancellationToken cancel)
            {
                cts?.TrySetCanceled();
                cts = new TaskCompletionSource<string>();
                AssertAction?.Invoke();
                using (cancel.Register(() => cts.TrySetCanceled(cancel)))
                {
                    return await cts.Task;
                }
            } 
        }

        [TestMethod]
        public void TestInvalidation()
        {
            var component = new TestComponent();

            var task1 = component.Result.Task;

            var result1 = "value1";
            component.CompleteTest(result1);

            Assert.IsTrue(task1.IsCompleted, "completed 1");
            Assert.AreEqual(result1, task1.Result);

            var task2 = component.Result.Task;

            Assert.IsTrue(task2.IsCompleted, "completed 2");
            Assert.AreEqual(result1, task2.Result);

            component.Invalidate();

            var task3 = component.Result.Task;

            Assert.IsFalse(task3.IsCanceled, "task3.IsCanceled before .CompleteTest()");
            Assert.IsFalse(task3.IsCompleted, "task3.IsCompleted before .CompleteTest()");

            var result2 = "value1";
            component.CompleteTest(result1);

            Assert.IsFalse(task3.IsCanceled, "task3.IsCanceled after .CompleteTest()");
            Assert.IsTrue(task3.IsCompleted, "task3.IsCompleted after .CompleteTest()");

            Assert.AreEqual(result2, task3.Result);
        }

        [TestMethod]
        public void TestComponentOrder()
        {

            var component = new TestComponent();

            bool creating = false;

            component.AssertAction = () => creating = true;

            Assert.IsFalse(creating, "creating before .Value call");
            var task = component.Result.Task;
            Assert.IsTrue(creating, "creating after .Value call");

            Assert.IsFalse(task.IsCanceled, "task.IsCanceled before .CompleteTest()");
            Assert.IsFalse(task.IsCompleted, "task.IsCompleted before .CompleteTest()");
             

            var result = "done";

            var completeResult = component.CompleteTest(result);

            Assert.IsNotNull(completeResult, "Complete Result");
            Assert.IsTrue(completeResult.Value, "Complete Result");

            Assert.IsFalse(task.IsCanceled, "task.IsCanceled after .CompleteTest()");
            Assert.IsTrue(task.IsCompleted, "task.IsCompleted after .CompleteTest()");

            Assert.AreEqual(result, task.Result, "actual result");
        }
    }
}
