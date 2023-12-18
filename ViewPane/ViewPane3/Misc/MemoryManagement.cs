using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ViewPane.Misc
{
	public class MemoryManagement
	{
		static List<object> requesters = new List<object>();
		static object locker = new object();
		static System.Timers.Timer timer = null;
		static volatile bool gcCollectScheduled = false;


		public static void Create(object requester)
		{
			lock (locker)
			{
				if (requesters.Contains(requester) == false)
					requesters.Add(requester);

				if (timer == null)
				{
					timer = new System.Timers.Timer(20000);
					timer.AutoReset = false;
					timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
				}
			}
		}

		public static void ReleaseUnusedMemory()
		{
			lock (locker)
			{
				if (timer != null)
				{
					if (gcCollectScheduled == false)
						timer.Start();

					gcCollectScheduled = true;
				}
			}
		}

		public static void Dispose(object requester)
		{

			lock (locker)
			{
				if (requesters.Contains(requester))
					requesters.Remove(requester);

				if (requesters.Count == 0 && timer != null)
				{
					gcCollectScheduled = false;
					timer.AutoReset = false;
					timer.Stop();
					timer.Dispose();
					timer = null;
				}
			}
		}

		public static void HardDispose()
		{
			lock (locker)
			{
				requesters.Clear();

				if (timer != null)
				{
					gcCollectScheduled = false;
					timer.AutoReset = false;
					timer.Stop();
					timer.Dispose();
					timer = null;
				}
			}
		}

		static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			lock (locker)
			{
				gcCollectScheduled = false;
				GC.Collect();
			}			
		}

	}
}
