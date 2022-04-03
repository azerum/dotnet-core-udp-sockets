using System.Threading;

namespace UdpSockets
{
    public class InterlockedInt64
    {
        private long count = 0;

        public void Add(long value)
        {
            Interlocked.Add(ref count, value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref count);
        }

        public long Get()
        {
            return count;
        }
    }
}
