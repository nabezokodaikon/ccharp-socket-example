using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CommonLib.ThreadSafe
{
    // 参考: http://www.boyet.com/Articles/LockfreeQueue.html
    public sealed class LockFreeQueue<T>
    {
        private sealed class Node<U>
        {
            public Node<U> Next;
            public U Item;
        }

        Node<T> head;
        Node<T> tail;

        public LockFreeQueue()
        {
            this.head = new Node<T>();
            this.tail = this.head;
        }

        public void Enqueue(T item)
        {
            Node<T> oldTail = null;
            Node<T> oldNext = null;
            var node = new Node<T>();
            node.Item = item;

            var updatedNewLink = false;
            while (!updatedNewLink)
            {
                oldTail = this.tail;
                oldNext = oldTail.Next;
                if (SyncMethods.IsMatch(ref this.tail, oldTail))
                {
                    if (SyncMethods.IsNull(ref oldNext))
                    {
                        updatedNewLink = SyncMethods.TryUpdate(ref this.tail.Next, null, node);
                    }
                    else
                    {
                        SyncMethods.TryUpdate(ref this.tail, oldTail, oldNext);
                    }
                }
            }

            SyncMethods.TryUpdate(ref this.tail, oldTail, node);
        }

        public T Dequeue()
        {
            T result = default(T);
            var haveAdvancedHead = false;
            while (!haveAdvancedHead)
            {
                var oldHead = this.head;
                var oldTail = this.tail;
                var oldHeadNext = oldHead.Next;

                if (SyncMethods.IsMatch(ref oldHead, this.head))
                {
                    if (SyncMethods.IsMatch(ref oldHead, oldTail))
                    {
                        if (SyncMethods.IsNull(ref oldHeadNext))
                        {
                            return default(T);
                        }

                        SyncMethods.TryUpdate(ref this.tail, oldTail, oldHeadNext);
                    }
                    else
                    {
                        result = oldHeadNext.Item;
                        haveAdvancedHead = SyncMethods.TryUpdate(ref this.head, oldHead, oldHeadNext);
                    }
                }
            }

            return result;
        }

        public IList<T> DequeueAll()
        {
            var list = new List<T>();
            while (true)
            {
                var node = this.Dequeue();
                if (node == null)
                {
                    break;
                }

                list.Add(node);
            }

            return list;
        }

        public void Clear()
        {
            while (this.Dequeue() != null) { }
        }
    }
}
