using System;
using System.Diagnostics;
using System.Threading;
using BepuUtilities;
using BepuUtilities.Memory;

namespace TGC.MonoGame.Samples.Physics.Bepu;

/// <summary>
///     Provides a <see cref="IThreadDispatcher" /> implementation. Not reentrant.
/// </summary>
public class SimpleThreadDispatcher : IThreadDispatcher, IDisposable
{
    private readonly BufferPool[] bufferPools;

    private volatile bool disposed;
    private readonly AutoResetEvent finished;
    private int remainingWorkerCounter;

    private volatile Action<int> workerBody;

    private readonly Worker[] workers;

    /// <summary>
    ///     Creates a new thread dispatcher with the given number of threads. With default value in
    ///     threadPoolBlockAllocationSize = 16384
    /// </summary>
    /// <param name="threadCount">Number of threads to dispatch on each invocation.</param>
    public SimpleThreadDispatcher(int threadCount) : this(threadCount, 16384)
    {
    }

    /// <summary>
    ///     Creates a new thread dispatcher with the given number of threads.
    /// </summary>
    /// <param name="threadCount">Number of threads to dispatch on each invocation.</param>
    /// <param name="threadPoolBlockAllocationSize">Size of memory blocks to allocate for thread pools.</param>
    public SimpleThreadDispatcher(int threadCount, int threadPoolBlockAllocationSize)
    {
        ThreadCount = threadCount;
        workers = new Worker[threadCount - 1];
        for (var i = 0; i < workers.Length; ++i)
        {
            workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
            workers[i].Thread.IsBackground = true;
            workers[i].Thread.Start((workers[i].Signal, i + 1));
        }

        finished = new AutoResetEvent(false);
        bufferPools = new BufferPool[threadCount];
        for (var i = 0; i < bufferPools.Length; ++i)
        {
            bufferPools[i] = new BufferPool(threadPoolBlockAllocationSize);
        }
    }

    /// <summary>
    ///     Waits for all pending work to complete and then disposes all workers.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            SignalThreads(ThreadCount);
            foreach (var t in bufferPools)
            {
                t.Clear();
            }

            foreach (var worker in workers)
            {
                worker.Thread.Join();
                worker.Signal.Dispose();
            }
        }
    }

    /// <summary>
    ///     Gets the number of threads to dispatch work on.
    /// </summary>
    public int ThreadCount { get; }

    public void DispatchWorkers(Action<int> workerBody, int maximumWorkerCount = int.MaxValue)
    {
        if (maximumWorkerCount > 1)
        {
            Debug.Assert(this.workerBody == null);
            this.workerBody = workerBody;
            SignalThreads(maximumWorkerCount);
            //Calling thread does work. No reason to spin up another worker and block this one!
            DispatchThread(0);
            finished.WaitOne();
            this.workerBody = null;
        }
        else if (maximumWorkerCount == 1)
        {
            workerBody(0);
        }
    }

    public BufferPool GetThreadMemoryPool(int workerIndex)
    {
        return bufferPools[workerIndex];
    }

    private void DispatchThread(int workerIndex)
    {
        Debug.Assert(workerBody != null);
        workerBody(workerIndex);

        if (Interlocked.Decrement(ref remainingWorkerCounter) == -1)
        {
            finished.Set();
        }
    }

    private void WorkerLoop(object untypedSignal)
    {
        var (signal, workerIndex) = ((AutoResetEvent, int))untypedSignal;
        while (true)
        {
            signal.WaitOne();
            if (disposed)
            {
                return;
            }
            DispatchThread(workerIndex);
        }
    }

    private void SignalThreads(int maximumWorkerCount)
    {
        //Worker 0 is not signalled; it's the executing thread.
        //So if we want 4 total executing threads, we should signal 3 workers.
        var maximumWorkersToSignal = maximumWorkerCount - 1;
        var workersToSignal = maximumWorkersToSignal < workers.Length ? maximumWorkersToSignal : workers.Length;
        remainingWorkerCounter = workersToSignal;
        for (var i = 0; i < workersToSignal; ++i)
        {
            workers[i].Signal.Set();
        }
    }

    private struct Worker
    {
        public Thread Thread;
        public AutoResetEvent Signal;
    }
}
