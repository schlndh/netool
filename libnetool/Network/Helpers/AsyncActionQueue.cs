using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Netool.Network.Helpers
{
    public class AsyncActionQueue
    {
        private static Action stopAction = delegate () { };
        private BlockingCollection<Action> queue = new BlockingCollection<Action>();
        private Thread worker;

        public AsyncActionQueue()
        {
            this.worker = new Thread(execute);
            this.worker.IsBackground = true;
            this.worker.Start();
        }

        /// <summary>
        /// Adds action into the execution queue.
        /// </summary>
        /// <param name="a"></param>
        /// <remarks>
        /// Action will be executed asynchronously in another thread so be careful about thread-safety.
        /// </remarks>
        public void Add(Action a)
        {
            queue.Add(a);
        }

        /// <summary>
        /// Places stop sign into the queue - worker will terminate after all actions finish
        /// </summary>
        /// <remarks>
        /// This method doesn't block.
        /// </remarks>
        public void Stop()
        {
            queue.Add(stopAction);
        }

        private void execute()
        {
            while (true)
            {
                var action = queue.Take();
                if (!object.ReferenceEquals(action, stopAction))
                {
                    try
                    {
                        action();
                    }
                    catch
                    { }
                }
                else
                {
                    return;
                }
            }
        }
    }
}