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

            var threadStartFlagDic = new Dictionary<int, ThreadSafeBoolean>();
            for (var i = 0; i < threadCount * 2; i++)
            {
                threadStartFlagDic.Add(i, new ThreadSafeBoolean(false));
            }

            var threadList = new List<Thread>();
            for (var i = 0; i < threadCount; i++)
            {
                threadList.Add(new Thread(obj =>
                {
                    var index = (int)obj;
                    while (!threadStartFlagDic[index].Value)
                    {
                        Thread.Sleep(0);
                        continue;
                    }

                    var limit = nodeCount / threadCount;
                    var count = 0;
                    while (count < limit)
                    {
                        var node = new Node(this.getNextIndex(), DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                        srcQueue.Enqueue(node);
                        count++;
                    }
                }));
            }

            for (var i = 0; i < threadCount; i++)
            {
                threadList.Add(new Thread(obj =>
                {
                    var index = (int)obj;
                    while (!threadStartFlagDic[index].Value)
                    {
                        Thread.Sleep(0);
                        continue;
                    }

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
                    }
                }));
            }

            foreach (var t in threadList.Select((Value, Index) => new { Index, Value }))
            {
                t.Value.Start(t.Index);
                while (!t.Value.IsAlive)
                {
                    continue;
                }
            }

            foreach (var i in threadStartFlagDic.Keys.OrderBy(i => i))
            {
                threadStartFlagDic[i].Value = true;
            }
            //foreach (var i in threadStartFlagDic.Keys.OrderByDescending(i => i))
            //{
            //    threadStartFlagDic[i].Value = true;
            //}

            threadList.ForEach(t => t.Join());

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

        [Test]
        public void DefaultQueueTest()
        {
            const int threadCount = 10;
            const int nodeCount = 100000;

            var srcQueue = new Queue<Node>();
            var destQueue = new Queue<Node>();

            var threadStartFlagDic = new Dictionary<int, ThreadSafeBoolean>();
            for (var i = 0; i < threadCount * 2; i++)
            {
                threadStartFlagDic.Add(i, new ThreadSafeBoolean(false));
            }

            var threadList = new List<Thread>();
            for (var i = 0; i < threadCount; i++)
            {
                threadList.Add(new Thread(obj =>
                {
                    var index = (int)obj;
                    while (!threadStartFlagDic[index].Value)
                    {
                        Thread.Sleep(0);
                        continue;
                    }

                    var limit = nodeCount / threadCount;
                    var count = 0;
                    while (count < limit)
                    {
                        var node = new Node(this.getNextIndex(), DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                        srcQueue.Enqueue(node);
                        count++;
                    }
                }));
            }

            for (var i = 0; i < threadCount; i++)
            {
                threadList.Add(new Thread(obj =>
                {
                    var index = (int)obj;
                    while (!threadStartFlagDic[index].Value)
                    {
                        Thread.Sleep(0);
                        continue;
                    }

                    var limit = nodeCount / threadCount;
                    var count = 0;
                    while (count < limit)
                    {
                        Node node;
                        try
                        {
                            node = srcQueue.Dequeue();
                        }
                        catch (InvalidOperationException)
                        {
                            continue;
                        }

                        try
                        {
                            destQueue.Enqueue(node);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        count++;
                    }
                }));
            }

            foreach (var t in threadList.Select((Value, Index) => new { Index, Value }))
            {
                t.Value.Start(t.Index);
                while (!t.Value.IsAlive)
                {
                    continue;
                }
            }

            foreach (var i in threadStartFlagDic.Keys.OrderBy(i => i))
            {
                threadStartFlagDic[i].Value = true;
            }
            //foreach (var i in threadStartFlagDic.Keys.OrderByDescending(i => i))
            //{
            //    threadStartFlagDic[i].Value = true;
            //}

            threadList.ForEach(t => t.Join());

            var nodeList = new List<Node>();
            while (true)
            {
                if (destQueue.Count == 0)
                {
                    break;
                }

                var node = destQueue.Dequeue();
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
