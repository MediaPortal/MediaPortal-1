using System;
using System.Collections;
using System.Threading;
using MediaPortal.GUI.Library;
using Microsoft.Win32;

namespace MediaPortal.Dispatcher
{
	public sealed class JobDispatcher
	{
		#region Methods

		void Clear()
		{
			_jobs.Clear();
		}

		void Dispatch()
		{
			_isRunning = true;

			try
			{
				Job job = null;

				for(;;)
				{
					lock(_dispatcher)
					{
						if(_isPaused || _jobs.Count == 0)
							Monitor.Wait(_dispatcher);

						if(_isRunning == false)
							break;
						
						job = (Job)_jobs.Peek();
						
						if(job.IsReady == false)
							Monitor.Wait(_dispatcher, job.Next.Subtract(DateTime.Now));

						if(_isRunning == false)
							break;

						if(job.IsReady == false)
							continue;

						_jobs.Dequeue();
					}

					job.Run();
					job = null;
				}
			}
			catch(ThreadInterruptedException)
			{
				Log.Write("JobDispatcher.Dispatch: ThreadInterruptException");
			}
			catch(Exception e)
			{
				Log.Write("JobDispatcher.Dispatch: {0}", e.Message);
			}

			_isRunning = false;
		}

		internal static void Dispatch(Job job)
		{
			lock(_dispatcher)
			{
				_jobs.Enqueue(job);

				Monitor.Pulse(_dispatcher);
			}
		}

		public static void Init()
		{
			lock(_dispatcher)
			{
				if(_isRunning)
					throw new InvalidOperationException("Dispatcher is already initialized.");

				_dispatcherThread = new Thread(new ThreadStart(_dispatcher.Dispatch));
				_dispatcherThread.IsBackground = true;
				_dispatcherThread.Priority = ThreadPriority.AboveNormal;
				_dispatcherThread.Name = "Dispatcher";
				_dispatcherThread.Start();

				// wait for the dispatcher thread to get going
				while(_isRunning == false)
					Thread.Sleep(100);
			}
		}

		public static void Pause()
		{
			lock(_dispatcher)
			{
				if(_isRunning == false)
					throw new InvalidOperationException("Dispatcher is not running.");

				if(_isPaused)
					throw new InvalidOperationException("Dispatcher is already paused.");

				_isPaused = true;
			}
		}

		public static void Resume()
		{
			lock(_dispatcher)
			{
				if(_isRunning == false)
					throw new InvalidOperationException("Dispatcher is not running.");

				if(_isPaused == false)
					throw new InvalidOperationException("Dispatcher is already running.");

				_isPaused = false;

				Monitor.Pulse(_dispatcher);
			}
		}

		public static void Term()
		{
			lock(_dispatcher)
			{
				if(_isRunning == false)
					throw new InvalidOperationException("Dispatcher is already shutdown.");

				_isRunning = false;
				_dispatcher.Clear();

				Monitor.Pulse(_dispatcher);

				_dispatcherThread = null;
			}
		}

		#endregion Methods

		#region Fields

		static JobDispatcher		_dispatcher = new JobDispatcher();
		static Thread				_dispatcherThread;
		static volatile bool		_isRunning;
		static volatile bool		_isPaused;
		static PriorityQueue		_jobs = new PriorityQueue(new JobComparer());

		#endregion Fields
	}
}
