using System;
using System.Collections;
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


	}
}
