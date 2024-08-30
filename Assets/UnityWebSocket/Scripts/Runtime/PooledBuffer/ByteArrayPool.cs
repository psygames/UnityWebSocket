using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace UnityWebSocket
{
    public class ByteArrayPool
    {
        public static readonly ByteArrayPool Shared = new ByteArrayPool();

        private static readonly string TName = typeof(ByteArrayPool).Name;

        // The pool is divided into 18 buckets, each bucket is a queue.
        private readonly Queue<byte[]>[] m_Pool;
        // The stack Max Length of each bucket.
        const int kQueueSize = 32;

        #region 
        // Only Support Kinds Of byte[] Lengths.
        const int kByteLenKindCount = 18;
        const int kMinByteLen = 8;
        const int kMaxByteLen = 1048576;
        static readonly int[] KindToByteLen = new int[kByteLenKindCount]
        {
            8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096,
            8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576
        };

        static readonly Dictionary<int, int> ByteLenToKind = new Dictionary<int, int>()
        {
            { 8, 0 }, { 16, 1 }, { 32, 2 }, { 64, 3 }, { 128, 4 }, { 256, 5 }, { 512, 6 }, { 1024, 7 },
            { 2048, 8 }, { 4096, 9 }, { 8192, 10 }, { 16384, 11 }, { 32768, 12 }, { 65536, 13 },
            { 131072, 14 }, { 262144, 15 }, { 524288, 16 }, { 1048576, 17 }
        };
        #endregion

        static int GetPoolIndex(int length)
        {
            if (length < kMinByteLen || length > kMaxByteLen)
            {
                return -1;
            }

            return ByteLenToKind[length];
        }

        static int CalcByteLen(int size)
        {
            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            size += 1;

            if (size < kMinByteLen) size = kMinByteLen;
            return size;
        }

        public ByteArrayPool()
        {
            m_Pool = new Queue<byte[]>[kByteLenKindCount];
        }

        /// <summary>
        /// The array length is not always accurate.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte[] Get(int len)
        {
            if (len < 0) len = 0;

            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            int size = CalcByteLen(len);
            int poolIndex = GetPoolIndex(size);

            if (poolIndex != -1)
            {
                if (m_Pool[poolIndex] == null)
                {
                    Profiler.BeginSample("ByteArrayPool.Get Alloc Queue");
                    m_Pool[poolIndex] = new Queue<byte[]>(kQueueSize);
                    Profiler.EndSample();
                }

                var pool = m_Pool[poolIndex];

                if (pool.Count > 0)
                {
                    var array = pool.Dequeue();
                    return array;
                }
            }

            Profiler.BeginSample("ByteArrayPool.Get Alloc byte[]");
            var allocArr = new byte[size];
            Profiler.EndSample();
            return allocArr;
        }

        /// <summary>
        /// <para> Return the array to the pool. </para>
        /// <para> The length of the array must be greater than or equal to 8 and a power of 2. </para>
        /// </summary>
        /// <param name="array"> The length of the array must be greater than or equal to 8 and a power of 2. </param>
        public void Return(byte[] array)
        {
            if ((array == null) || (array.Length == 0))
            {
                return;
            }

            int poolIndex = GetPoolIndex(array.Length);
            if (poolIndex == -1)
            {
                return;
            }

            if (m_Pool[poolIndex] == null)
            {
                Profiler.BeginSample("ByteArrayPool.Return Alloc Queue");
                m_Pool[poolIndex] = new Queue<byte[]>();
                Profiler.EndSample();
            }

            var pool = m_Pool[poolIndex];

            if (pool.Count >= kQueueSize)
            {
                // If the pool is full, we will not return the array to the pool.
                UnityEngine.Debug.LogWarning($"ByteArrayPool Return byte[{array.Length}] Too Many {pool.Count} Max {kQueueSize}");
                array = null;
            }
            else
            {
                pool.Enqueue(array);
            }
        }

        /// <summary>
        /// <para> Return the array to the pool and set array reference to null. </para>
        /// <para> The length of the array must be greater than or equal to 8 and a power of 2. </para>
        /// </summary>
        /// <param name="array"> The length of the array must be greater than or equal to 8 and a power of 2. </param>
        public void Return(ref byte[] array)
        {
            Return(array);
            array = null;
        }

        public void Cleanup()
        {
            // Release instances from each buckets.
            for (int i = 0; i < m_Pool.Length; i++)
            {
                var bucket = m_Pool[i];
                bucket.Clear();
            }
        }
    }
}