using System.Collections.Concurrent;
using System.Text;

namespace QingFeng.CoreUtils.Performance
{
    /// <summary>
    /// Static class for caching StringBuilders so we don't have to keep creating/destroying them all the time
    /// </summary>
    public static class StringBuilderPool
    {
        const int MaxPooledBuilders = 64;
        const int MaxBuilderCapacity = 512;

        static readonly ConcurrentQueue<StringBuilder> freeStringBuilders;

        static StringBuilderPool()
        {
            freeStringBuilders = new ConcurrentQueue<StringBuilder>();
        }

        public static StringBuilder Acquire()
        {
            return freeStringBuilders.TryDequeue(out var sb) ? sb : new StringBuilder(300);
        }

        public static void Release(StringBuilder sb)
        {
            if (sb != null && sb.Length <= MaxBuilderCapacity && freeStringBuilders.Count <= MaxPooledBuilders)
            {
                sb.Clear();

                freeStringBuilders.Enqueue(sb);
            }
        }

        public static string ReleaseToPool(this StringBuilder sb)
        {
            var s = sb?.ToString();

            Release(sb);

            return s;
        }
    }
}
