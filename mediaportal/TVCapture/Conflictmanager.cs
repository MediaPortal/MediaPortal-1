using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	public class ConflictManager
	{
		public class Card
		{
			enum State
			{
				Free,
				Recording,
			}
			int					m_ID;
			State				m_State=State.Free;
			int         m_priority;
			TVRecording m_recording;

			public Card(int id, int priority)
			{
				ID=id;
				m_priority=priority;
			}

			public int ID
			{
				get { return m_ID;}
				set { m_ID=value;}
			}

			public int Priority
			{
				get { return m_priority;}
				set { m_priority=value;}
			}

			public int RecordingID
			{
				get { return m_recording.ID;}
			}

			public TVRecording Recording
			{
				get { return m_recording;}
			}

			public bool IsRecording
			{
				get 
				{
					return (m_State!=State.Free);
				}
			}
			public void Process(DateTime dtTime, TVProgram prog, int iPreRecordInterval, int iPostRecordInterval)
			{
				if (m_State==State.Free) return ;
				if (m_recording.IsRecordingAtTime(dtTime,prog, iPreRecordInterval,iPostRecordInterval)) return ;
				m_State=State.Free;
			}

			public void Record(TVRecording recording)
			{
				m_State=State.Recording;
				m_recording=recording;
			}
		}//public class Card


		public class CardAllocation
		{
			public int				 ID;
			public TVRecording Recording;
		}

		public class RecordingConflict
		{
			public TVRecording Recording;
			public ArrayList	 CardAllocations = new ArrayList();
		}


		ArrayList cards = new ArrayList();

		public void CheckRecordingsForConflicts(ref ArrayList conflicts)
		{
			conflicts=new ArrayList();
			int iPreRecordInterval =0;
			int iPostRecordInterval=0;
			
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				iPreRecordInterval = xmlreader.GetValueAsInt("capture","prerecord", 5);
				iPostRecordInterval= xmlreader.GetValueAsInt("capture","postrecord", 5);
			}

			//collect all cards...
			cards.Clear();
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);
				if (dev.UseForRecording)
				{
					Card card = new Card(dev.ID,dev.Priority);
					cards.Add(card);
				}
			}
			//collect all recordings
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);	

			//check conflicts between now & the next 2 months
			TVUtil tvUtil= new TVUtil();
			DateTime dtTime = DateTime.Now;
			DateTime dtStop = dtTime.AddMonths(2);
			while (dtTime < dtStop)
			{
				//check each recording
				foreach (TVRecording rec in recordings)
				{
					if (rec.Canceled != 0) continue;
					bool bIsRecording=false;
					TVProgram prog=tvUtil.GetProgramAt(rec.Channel,dtTime.AddMinutes(iPreRecordInterval) );
					//Should this recording be running ?
					if ( rec.IsRecordingProgramAtTime(dtTime,prog,iPreRecordInterval, iPostRecordInterval) )
					{
						//yes, check if a card is already recording it
						foreach (Card card in cards)
						{
							if (card.IsRecording && card.RecordingID==rec.ID) 
							{
								//yep, then we can skip this
								bIsRecording=true;
								break;
							}
						}
						if (bIsRecording) continue;//recording is already being recorded


						//no card is recording this. find card with highest prio and use that one to record this recording
						int highestPrio=-1;
						Card usecard=null;
						foreach (Card card in cards)
						{
							//is card free?
							if (!card.IsRecording)
							{
								//and can it record the channel
								if (TVDatabase.CanCardViewTVChannel(rec.Channel,card.ID))
								{
									//is its priority higher?
									if (card.Priority>highestPrio)
									{
										//then use this card
										highestPrio=card.Priority;
										usecard=card;
									}
								}
							}
						}

						if (usecard==null)
						{
							//no card found, we have a conflict!
							rec.Canceled=1;
							RecordingConflict conflict = new RecordingConflict();
							conflict.Recording = rec;
							conflict.CardAllocations.Clear();
							foreach (Card card in cards)
							{
								if (card.IsRecording)
								{
									CardAllocation allocation = new CardAllocation();
									allocation.ID=card.ID;
									allocation.Recording=card.Recording;
									conflict.CardAllocations.Add(allocation);
								}
							}
							Log.Write("Recording on channel {0} title:'{1}' at {2} {3} conflicts with the following shows:",conflict.Recording.Channel,conflict.Recording.Title, dtTime.ToLongDateString(), dtTime.ToLongTimeString());
							foreach (ConflictManager.CardAllocation alloc in conflict.CardAllocations)
							{
								Log.Write("  card:{0} channel{1} title:{2}",alloc.ID,alloc.Recording.Channel,alloc.Recording.Title);
							}
							conflicts.Add(conflict);
						}
						else
						{
							usecard.Record(rec);
						}
					}//if ( rec.IsRecordingProgramAtTime(dtCurrentTime,null,iPreRecordInterval, iPostRecordInterval) )
				}//foreach (TVRecording rec in recordings)
				foreach (Card card in cards)
				{
					if (card.IsRecording)
					{
						TVProgram progRec=tvUtil.GetProgramAt(card.Recording.Channel,dtTime.AddMinutes(iPreRecordInterval) );
						card.Process(dtTime,progRec, iPreRecordInterval, iPostRecordInterval);
					}
				}
				dtTime = dtTime.AddMinutes(1);
			}//while (dtTime < dtStop)
		}//public void CheckRecordingsForConflicts()
	}//public class ConflictManager
}//namespace MediaPortal.TV.Recording