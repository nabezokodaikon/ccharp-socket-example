using System.Threading;

namespace CommonLib.ThreadSafe
{
    /// <summary>
    /// スレッドセーフにアクセス可能な bool 値を保持するクラス。
    /// </summary>
    public sealed class ThreadSafeBoolean
    {
        private const long FALSE_VALUE = 0L;
        private const long TRUE_VALUE = 1L;

        private long value;

        /// <summary>
        /// 保持している値。
        /// </summary>
        public bool Value
        {
            get
            {
                return (Interlocked.Read(ref this.value) == TRUE_VALUE);
            }
            set
            {
                if (value)
                {
                    Interlocked.Exchange(ref this.value, TRUE_VALUE);
                }
                else
                {
                    Interlocked.Exchange(ref this.value, FALSE_VALUE);
                }
            }
        }

        /// <summary>
        /// ThreadSafeBoolean クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="initValue">初期値。</param>
        public ThreadSafeBoolean(bool initValue)
        {
            if (initValue)
            {
                this.value = TRUE_VALUE;
            }
            else
            {
                this.value = FALSE_VALUE;
            }
        }

        /// <summary>
        /// 値の変更を試みます。
        /// </summary>
        /// <param name="newValue">設定する値。</param>
        /// <returns>値が変更できた場合はtrue。それ以外の場合はfalseを返します。</returns>
        public bool TryChange(bool newValue)
        {
            if (newValue)
            {
                return (Interlocked.CompareExchange(ref this.value, TRUE_VALUE, FALSE_VALUE) == FALSE_VALUE);
            }
            else
            {
                return (Interlocked.CompareExchange(ref this.value, FALSE_VALUE, TRUE_VALUE) == TRUE_VALUE);
            }
        }
    }
}
