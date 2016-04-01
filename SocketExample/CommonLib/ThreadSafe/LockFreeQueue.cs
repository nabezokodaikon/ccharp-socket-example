using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

// 参考: http://www.boyet.com/Articles/LockfreeQueue.html
namespace CommonLib.ThreadSafe
{
    internal static class SyncMethods
    {
        public static bool TryUpdate<T>(ref T location, T comparand, T newValue)
            where T : class
        {
            return comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        public static bool IsMatch<T>(ref T location, T newValue)
            where T : class
        {
            return newValue == Interlocked.CompareExchange(ref location, newValue, newValue);
        }

        public static bool IsNull<T>(ref T location)
            where T : class
        {
            return null == Interlocked.CompareExchange(ref location, null, null);
        }
    }

    internal class SingleLinkNode<T>
    {
        public SingleLinkNode<T> Next;
        public T Item;
    }

    public class LockFreeQueue<T>
    {
        SingleLinkNode<T> head;
        SingleLinkNode<T> tail;

        public LockFreeQueue()
        {
            this.head = new SingleLinkNode<T>();
            this.tail = this.head;
        }

        public void Enqueue(T item)
        {
            SingleLinkNode<T> oldTail = null;
            SingleLinkNode<T> oldNext = null;
            var node = new SingleLinkNode<T>();
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

        public void Clear()
        {
            while (this.Dequeue() != null) { }
        }
    }
}
