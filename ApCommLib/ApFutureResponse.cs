using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApCommLib
{
    /// <summary>
    ///  异步回复句柄
    /// </summary>
    public class ApFutureResponse
    {
        private TimeSpan maxWait = TimeSpan.FromMilliseconds(Timeout.Infinite);
        /// <summary>
        /// 回复超时时间 单位毫秒
        /// </summary>
        public TimeSpan ResponseTimeout
        {
            get { return maxWait; }
            set { maxWait = value; }
        }

        private readonly CountDownLatch latch = new CountDownLatch(1);
        
        /// <summary>
        /// 等待句柄
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get { return latch.AsyncWaitHandle; }
        }

        private ApInvokeReturnMessage response;
        /// <summary>
        /// 回复信息
        /// </summary>
        public ApInvokeReturnMessage Response
        {
            // Blocks the caller until a value has been set
            get
            {
                lock (latch)
                {
                    if (null != response)
                    {
                        return response;
                    }
                }

                try
                {
                    if (!latch.await(maxWait) && response == null)
                    {
                        //给自己赋值，等待超时
                        lock (latch)
                        {
                            response = new ApInvokeReturnMessage
                            {
                                ReturnValue = null,
                                RemoteException = new Exception("等待时间超时！")
                            };
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                lock (latch)
                {
                    return response;
                }
            }

            set
            {
                lock (latch)
                {
                    response = value;
                }

                latch.countDown();
            }
        }
    }

    /// <summary>
    /// 异步通信等待工具
    /// </summary>
    public class CountDownLatch
    {
        private readonly ManualResetEvent mutex = new ManualResetEvent(false);
        private int remaining;

        public CountDownLatch(int i)
        {
            remaining = i;
        }

        /// <summary>
        /// Decrement the count, releasing any waiting Threads when the count reaches Zero.
        /// </summary>
        public void countDown()
        {
            lock (mutex)
            {
                if (remaining > 0)
                {
                    remaining--;
                    if (0 == remaining)
                    {
                        mutex.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current count for this Latch.
        /// </summary>
        public int Remaining
        {
            get
            {
                lock (mutex)
                {
                    return remaining;
                }
            }
        }

        /// <summary>
        /// Causes the current Thread to wait for the count to reach zero, unless
        /// the Thread is interrupted.
        /// </summary>
        public void await()
        {
            this.await(TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        /// <summary>
        /// Causes the current thread to wait until the latch has counted down to zero, unless
        /// the thread is interrupted, or the specified waiting time elapses.
        /// </summary>
        public bool await(TimeSpan timeout)
        {
            return mutex.WaitOne((int)timeout.TotalMilliseconds, false);
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return mutex; }
        }
    }
}
