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
				Log.Write("Stop Rec :{0}-{1}-{2} {3}:{4}:{5} card:{6} channel:{7} title:{8}",
					dtTime.Day,
					dtTime.Month,
					dtTime.Year,
					dtTime.Hour,
					dtTime.Minute,
					dtTime.Second,
					ID,
					m_recording.Channel,
					m_recording.Title);
			}

			public void Record(DateTime dtTime, TVRecording recording)
			{
				m_State=State.Recording;
				m_recording=recording;
				Log.Write("Record   :{0}-{1}-{2} {3}:{4}:{5} card:{6} channel:{7} title:{8}",
										dtTime.Day,
										dtTime.Month,
										dtTime.Year,
										dtTime.Hour,
										dtTime.Minute,
										dtTime.Second,
										ID,
										m_recording.Channel,
										m_recording.Title);
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
		Hashtable programCache = new Hashtable();

		public void CheckRecordingsForConflicts(ref ArrayList conflicts)
		{
			Log.Write("Conflict manager: check for conflicts");
			conflicts=new ArrayList();
			int iPreRecordInterval =0;
			int iPostRecordInterval=0;
			
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
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
//TEST
//Card card1 = new Card(1,1);
//cards.Add(card1);
//TEST

			//collect all recordings
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);	

			//check conflicts between now & the next 2 months
			
			DateTime dtTime = DateTime.Now;
			DateTime dtStop = dtTime.AddMonths(2);
			ArrayList programs = new ArrayList();
			TVDatabase.GetPrograms(Util.Utils.datetolong(dtTime), Util.Utils.datetolong(dtStop),ref programs);
			while (dtTime < dtStop)
			{
				//check each recording
				foreach (TVRecording rec in recordings)
				{
					if (rec.Canceled != 0) continue;
					bool bIsRecording=false;
					TVProgram prog=GetProgramAt(ref programs, rec.Channel,dtTime.AddMinutes(iPreRecordInterval) );
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
								if (TVDatabase.CanCardViewTVChannel(rec.Channel,card.ID) || cards.Count==1)
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
							Log.Write("Conflict :{0}-{1}-{2} {3}:{4}:{5} card:- channel:{6} title:{7}",
																				dtTime.Day,
																				dtTime.Month,
																				dtTime.Year,
																				dtTime.Hour,
																				dtTime.Minute,
																				dtTime.Second,
																				conflict.Recording.Channel,
																				conflict.Recording.Title);
							foreach (ConflictManager.CardAllocation alloc in conflict.CardAllocations)
							{
								Log.Write("    card:{0} channel{1} title:{2}",alloc.ID,alloc.Recording.Channel,alloc.Recording.Title);
							}
							conflicts.Add(conflict);
						}
						else
						{
							usecard.Record(dtTime,rec);
						}
					}//if ( rec.IsRecordingProgramAtTime(dtCurrentTime,null,iPreRecordInterval, iPostRecordInterval) )
				}//foreach (TVRecording rec in recordings)

				foreach (Card card in cards)
				{
					if (card.IsRecording)
					{
						TVProgram progRec=GetProgramAt(ref programs, card.Recording.Channel,dtTime.AddMinutes(iPreRecordInterval) );
						card.Process(dtTime,progRec, iPreRecordInterval, iPostRecordInterval);
					}
				}
				dtTime = dtTime.AddMinutes(1);
			}//while (dtTime < dtStop)
			
			Log.Write("Conflict manager: found {0} conflicts", conflicts.Count);
		}//public void CheckRecordingsForConflicts()
		
		TVProgram GetProgramAt(ref ArrayList programs, string channel,DateTime dtTime)
		{
			if (programCache.ContainsKey(channel))
			{
				TVProgram prog =(TVProgram)programCache[channel] ;
				if ( prog.IsRunningAt(dtTime) ) 
				{
					return prog;
				}
				programCache.Remove(channel);
			}
				
			bool cont=false;
			do
			{
				cont=false;
				foreach (TVProgram prog in programs)
				{
					if (prog.Channel == channel)
					{
						if (prog.EndTime <= dtTime) 
						{
							cont=true;
							programs.Remove(prog);
							break;
						}
						else
						{															
							if ( prog.IsRunningAt(dtTime) )
							{
								programCache[channel]=prog;
								return prog;
							}
						}
					}
				}
			} while (cont);
			return null;
		}
	}//public class ConflictManager
}//namespace MediaPortal.TV.Recording