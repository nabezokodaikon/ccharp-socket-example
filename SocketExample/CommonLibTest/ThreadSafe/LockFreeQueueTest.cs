using CommonLib.ThreadSafe;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CommonLibTest.ThreadSafe
{
    [TestFixture]
    class LockFreeQueueTest
    {
        private class Node
        {
            public int Index { get; set; }
            public string Value { get; set; }

            public Node(int index, string value)
            {
                this.Index = index;
                this.Value = value;
            }
        }

        private int index = 0;

        private int getNextIndex()
        {
            return Interlocked.Increment(ref this.index);
        }

        [Test]
        public void MultithreadAccessTest()
        {
            const int threadCount = 10;
            const int nodeCount = 100000;

            var srcQueue = new LockFreeQueue<Node>();
            var destQueue = new LockFreeQueue<Node>();

            var threadList = new List<Thread>();
            for (var i = 0; i < threadCount; i++)
            {
                var t = new Thread(() =>
                {
                    var limit = nodeCount / threadCount;
                    var count = 0;
                    while (count < limit)
                    {
                        var node = new Node(this.getNextIndex(), DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                        srcQueue.Enqueue(node);
                        count++;
                        Thread.Sleep(0);
                    }
                });

                threadList.Add(t);
            }

            for (var i = 0; i < threadCount; i++)
            {
                var t = new Thread(() =>
                {
                    var limit = nodeCount / threadCount;
                    var count = 0;
                    while (count < limit)
                    {
                        var node = srcQueue.Dequeue();
                        if (node != null)
                        {
                            destQueue.Enqueue(node);
                            count++;
                        }
                        
                        Thread.Sleep(0);
                    }
                });

                threadList.Add(t);
            }

            for (var i = threadList.Count - 1; i > -1; i--)
            {
                var t = threadList[i];
                t.Start();
                while (!t.IsAlive)
                {
                    break;
                }
            }

            foreach (var t in threadList)
            {
                t.Join();
            }

            var nodeList = new List<Node>();
            while (true)
            {
                var node = destQueue.Dequeue();
                if (node == null)
                {
                    break;
                }

                nodeList.Add(node);
            }

            var nodeDic = new Dictionary<int, Node>();
            foreach (var node in nodeList)
            {
                Assert.AreEqual(nodeDic.ContainsKey(node.Index), false);
                nodeDic.Add(node.Index, node);
            }

            Assert.AreEqual(nodeList.Max(i => i.Index), nodeCount);
            Assert.AreEqual(nodeList.Count, nodeCount);
        }
    }
}
