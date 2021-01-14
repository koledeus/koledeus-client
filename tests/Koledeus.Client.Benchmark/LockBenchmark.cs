using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Koledeus.Client.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [MemoryDiagnoser]
    public class LockBenchmark
    {
        private List<int> _numbers = new List<int>()
        {
            213, 123, 12, 312, 321, 3, 12, 31, 23, 12, 31, 23,
            12, 3312, 3, 12, 312, 3, 123, 12, 31, 23, 12, 312, 312, 3, 123, 123, 12, 31, 23, 123, 12, 312, 31, 23, 12,
            3, 12, 312,
            312, 312, 31, 23, 12, 31, 23, 12, 31, 23, 12, 31, 23, 123, 21, 312, 312, 312, 3, 13, 1
        };

        private object _lock = new object();
        private Mutex _mutex = new Mutex();

        [Benchmark]
        public void ObjectLock()
        {
            int total = 0;
            Parallel.ForEach(_numbers, number =>
            {
                int a = number * 10;

                lock (_lock)
                {
                    total += a;
                }
            });
        }

        [Benchmark]
        public void MutexLock()
        {
            int total = 0;
            Parallel.ForEach(_numbers, number =>
            {
                int a = number * 10;

                _mutex.WaitOne();
                total += a;
                _mutex.ReleaseMutex();
            });
        }

        [Benchmark]
        public void ConcurrentBagLock()
        {
            var bag = new ConcurrentBag<int>();
            Parallel.ForEach(_numbers, number =>
            {
                int a = number * 10;

                bag.Add(a);
            });
            int total = bag.Sum();
        }

        [Benchmark]
        public void ThreadSafeWithoutLocking()
        {
            int[] numbers = new int[_numbers.Count];
            Parallel.ForEach(_numbers, (number, _, index) =>
            {
                int a = number * 10;

                numbers[index] = a;
            });

            int total = 0;
            foreach (var number in numbers)
            {
                total += number;
            }
        }
    }
}