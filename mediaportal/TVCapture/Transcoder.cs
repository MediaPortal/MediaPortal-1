using System;
using System.Collections;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for Transcoder.
	/// </summary>
	public class Transcoder
	{
		static ArrayList queue = new ArrayList();
		static Thread WorkerThread =null;
		static Transcoder()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		static public void Transcode(TVRecorded rec)
		{
			lock(queue)
			{
				queue.Add(rec);
			}

			if (WorkerThread==null)
			{
				WorkerThread = new Thread(new ThreadStart(TranscodeWorkerThread));
				WorkerThread.Start();
			}

		}

		static public bool IsTranscoding(TVRecorded rec)
		{
			lock(queue)
			{
				foreach (TVRecorded rec1 in queue)
				{
					if (rec1.FileName==rec.FileName) return true;
				}
			}
			return false;
		}

		static void TranscodeWorkerThread()
		{
			System.Threading.Thread.CurrentThread.Priority=ThreadPriority.BelowNormal;

			while (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
			{
				if (queue.Count==0) 
				{
					System.Threading.Thread.Sleep(100);
				}
				else
				{
					TVRecorded recording=null;
					lock(queue)
					{
						recording = (TVRecorded)queue[0];
						queue.RemoveAt(0);
					}

					//transcode recording...
				}
			}
		}
	}
}
