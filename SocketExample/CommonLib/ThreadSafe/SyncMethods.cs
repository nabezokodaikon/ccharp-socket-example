using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CommonLib.ThreadSafe
{
    // 参考: http://www.boyet.com/Articles/LockfreeQueue.html
    public static class SyncMethods
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
}
