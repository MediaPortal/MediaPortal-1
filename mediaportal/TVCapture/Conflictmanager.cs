using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.TV.Recording
{
	public class ConflictManager
	{	
		static  int[] cards ;
		static  ArrayList recordings ;
		static  TVUtil						util =null;
		static  ConflictManager()
		{
			TVDatabase.OnRecordingsChanged += new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnRecordingsChanged);
		}
		static bool AllocateCard(string ChannelName)
		{
			int cardNo=-1;
			int minRecs=Int32.MaxValue;
			for (int i=0; i < cards.Length;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);
				if ( !dev.UseForRecording) continue;
				if ( cards.Length>1)
				{
					if(!TVDatabase.CanCardViewTVChannel(ChannelName,dev.ID)) continue;
				}
				if (cards[i]==0) 
				{
					cardNo=i;
					break;
				}
				if (cards[i] < minRecs)
				{
					minRecs=cards[i];
					cardNo=i;
				}
			}
			if (cardNo>=0)
			{
				cards[cardNo]++;
				if (cards[cardNo]>1) return true;
			}
			return false;
		}

		static void FreeCards()
		{
			for (int i=0; i < cards.Length;++i)
				cards[i]=0;
		}

		static void Initialize()
		{
			util = new TVUtil(14);
			recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);
		}

		static public bool IsConflict(TVRecording rec)
		{
			if (Recorder.Count<=0) 
				return false;
			
			if (recordings==null || util==null) 
			{
				Initialize();
			}
			cards = new int[Recorder.Count];
			if (recordings.Count==0) return false;
			
			ArrayList epsiodes = util.GetRecordingTimes(rec);
			foreach (TVRecording epsiode in epsiodes)
			{
				if (epsiode.Canceled!=0) continue;
				FreeCards();
				AllocateCard(epsiode.Channel);
				foreach (TVRecording otherRecording in recordings)
				{
					ArrayList otherEpisodes = util.GetRecordingTimes(otherRecording);
					foreach ( TVRecording otherEpisode in otherEpisodes)
					{
						if (otherEpisode.Canceled!=0) continue;
						if (otherEpisode.ID==epsiode.ID && 
							otherEpisode.Start==epsiode.Start && 
							otherEpisode.End==epsiode.End) continue;
						// episode        s------------------------e
						// other    ---------s-----------------------------
						// other ------------------e
						if ( (otherEpisode.Start >= epsiode.Start && otherEpisode.Start < epsiode.End) ||
							   (otherEpisode.Start <= epsiode.Start && otherEpisode.End >= epsiode.End)     ||
							(otherEpisode.End > epsiode.Start && otherEpisode.End <= epsiode.End) )
						{
							if (AllocateCard(otherEpisode.Channel))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		static public TVRecording[] GetConflictingRecordings(TVRecording rec)
		{
			if (Recorder.Count<=0) return null;
			
			if (recordings==null || util==null) 
			{
				Initialize();
			}
			cards = new int[Recorder.Count];
			if (recordings.Count==0) return null;
			
			ArrayList conflicts = new ArrayList();
			ArrayList epsiodes = util.GetRecordingTimes(rec);
			foreach (TVRecording epsiode in epsiodes)
			{
				if (epsiode.Canceled!=0) continue;
				
				FreeCards();
				AllocateCard(epsiode.Channel);
				foreach (TVRecording otherRecording in recordings)
				{
					ArrayList otherEpisodes = util.GetRecordingTimes(otherRecording);
					foreach ( TVRecording otherEpisode in otherEpisodes)
					{
						if (otherEpisode.Canceled!=0) continue;
						if (otherEpisode.ID==epsiode.ID && 
							otherEpisode.Start==epsiode.Start && 
							otherEpisode.End==epsiode.End) continue;
						// episode        s------------------------e
						// other    ---------s-----------------------------
						// other ------------------e
						if ( (otherEpisode.Start >= epsiode.Start && otherEpisode.Start < epsiode.End) ||
							(otherEpisode.Start <= epsiode.Start && otherEpisode.End >= epsiode.End)     ||
							(otherEpisode.End > epsiode.Start && otherEpisode.End <= epsiode.End) )
						{
							if (AllocateCard(otherEpisode.Channel))
							{
								conflicts.Add(otherRecording);
								break;
							}
						}
					}
				}
			}
			TVRecording[] conflictingRecordings = new TVRecording[conflicts.Count];
			for (int i=0; i < conflicts.Count;++i)
				conflictingRecordings[i] = (TVRecording)conflicts[i];
			return conflictingRecordings;
		}

		static public TVUtil Util
		{
			get { 
				if (util==null) 
					Initialize();
				return util;
				}
		}

		static private void TVDatabase_OnRecordingsChanged()
		{
			Initialize();
		}
	}
}