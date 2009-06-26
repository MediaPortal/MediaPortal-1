#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

namespace WindowPlugins.VideoEditor
{
  /*class MpegModifier
	{
		
		System.Timers.Timer cutProgresstime;
    public delegate void Finished();
    public event Finished OnFinished;
    public delegate void Progress(int percentage);
    public event Progress OnProgress;
    int percent = 0;
    double newDuration = 0;
    System.IO.FileInfo inFilename;
    List<TimeDomain> cutPoints;
		List<TimeDomain> splitterTime = new List<TimeDomain>();

		//const int NR_OF_SPILTER_TIME_STAMPS = 40;
		//SPLITTER_TIME_STAMP[] tSplitterTime = new SPLITTER_TIME_STAMP[NR_OF_SPILTER_TIME_STAMPS];
		//SPLITTER_TIME_STAMP tStamp;
		FileStream fsIn;
		FileStream fsOut;
		//StreamWriter swLog;
		BinaryReader bwIn;
		BinaryWriter bwOut;
		Int64 sourceBitAddress;

		const int FIFO_SIZE = 1024 * 1024 * 20;

		byte[] ptBuffers = new byte[FIFO_SIZE];

		Int64 g_iReadCounter = 0;

		int iPointCounter;

		const int RIPPER_TIME_STAMP_MODE_0 = 0;  // to min.sec
		const int RIPPER_TIME_STAMP_MODE_1 = 1;  // to mili 
		const int RIPPER_TIME_STAMP_MODE_2 = 2;  // to 33 bit time stamp

		bool bLogEnabled = false;


		public MpegModifier()
		{
			cutProgresstime = new System.Timers.Timer(1000);
			cutProgresstime.Elapsed += new System.Timers.ElapsedEventHandler(progressTime_Elapsed);
		}

		void progressTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			//int progress = 0;
			//percent = System.Convert.ToInt32((progress * 100) / newDuration);
			//// progressBar.Percentage = percent;
			////progressLbl.Label = percent.ToString();
			//if (OnProgress != null)
			//	OnProgress(percent);
		}

		public void CutMpeg(System.IO.FileInfo inFilename, List<TimeDomain> cutPoints)
		{
			this.inFilename = inFilename;
			this.cutPoints = cutPoints;

			

			CutMpeg();
		}

		private void CutMpeg()
		{
			try
			{
				sourceBitAddress = 0;

				TimeDomain timeStamp;
				System.IO.FileInfo outFilename;
				
				percent = 0;
				bool result = true;

				// Create the reader for data
				fsIn = new FileStream(inFilename.FullName, FileMode.Open, FileAccess.Read);
				
				// Create the writer for data.
				bwIn = new BinaryReader(fsIn);
				
				

				cutProgresstime.Start();
				string outPath = inFilename.FullName;
				//rename the source file ------------later this could be configurable to delete it
				//TODO behavior if the renamed sourcefile (_original) exists
				inFilename.MoveTo(inFilename.FullName.Replace("."+inFilename.Extension, "_original." + inFilename.Extension));
				//to not to change the database the outputfile has the same name 
				outFilename = new System.IO.FileInfo(outPath);

				if (outFilename.Exists)
				{
					outFilename.Delete();
				}

				// Create the new, empty data file
				fsOut = new FileStream(outFilename.FullName, FileMode.Create, FileAccess.ReadWrite);
				if (fsOut == null)
				// Create the writer for data.
				bwOut = new BinaryWriter(fsOut);

				long fileSizeSaved;
				int leftBufferSize;
				Int64 x = 0;
				int c_times = 0;
				Int64 readCounter = 0;
				Int64 writeCounter = 0;
				bool checkOffset = true;

				Log.Debug("Start SplitProgramStreamCut");
				fileSizeSaved = fsIn.Length;
				leftBufferSize = (int)Math.Min(fileSizeSaved, FIFO_SIZE);
				Log.Debug("step 0 " + leftBufferSize);

				if (bwIn.Read(ptBuffers, 0, leftBufferSize) != leftBufferSize)
					throw new Exception("ERROR_READING_FROM_INPUT_FILE");

				fileSizeSaved = fileSizeSaved - leftBufferSize;

				// Also get the first time stamp to see what is the zero time 
				GetTimeStamp(readCounter, c_times, ref timeStamp);
				AdjustTimeStampOffset(c_times, ref timeStamp);
				readCounter += 14; // 14 - 4 bytes of the first pack header minus the pack header start code 
				leftBufferSize -= 14;
				if (!CompareTimeStampStartPoint(c_times, ref timeStamp))
				{
					//ER		        if (_write(OutFileHnd , ptBuffers + iWriteCounter , iReadCounter) != iReadCounter) { 
					//ER			         return ERROR_WRITEING_TO_OUTPUT_FILE; 
					//ER		        }
					fsOut.Write(ptBuffers, (int)writeCounter, (int)(readCounter - writeCounter));
					writeCounter = readCounter;
				}
				Log.Debug("step 1");


				cutProgresstime.Stop();
				
				percent = 100;

				if (OnFinished != null)
					OnFinished();
				

			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
      finally
      {
        cutPoints = null;
        percent = 0;
        newDuration = 0;
      }
    }

		bool GetTimeStamp(Int64 counter, int counts, ref TimeDomain timeStamp)
		{
			int system_clock_reference_base_0;
			int system_clock_reference_base_1;
			int system_clock_reference_base_2;
			Int64 scr_base;
			double PTS_in_sec;
			Int64 x;
			int sec;
			int min;
			int hour;


			g_iReadCounter = counter;
			iSourceBitAddress = 0;

			// Pack header length 
			//32 + 2 + 3 + 1 + 15 + 1 + 15 + 1 + 9 + 1 + 22 +  1 + 1 + 5 + 3  =  112/8 = 14 bytes 
			// pack header start code 
			x = Getbits(32);


			//'01'	2 // say the layer - here its mpeg2 if   its 0x2 its mpeg1 
			x = Getbits(2);
			if (x != 0x1)
			{
				Log.Warn("Warning : MPEG2 bit is not signald\n");
				return false;
			}

			//system_clock_reference_base [32..30]	3
			system_clock_reference_base_0 = Getbits(3);
			//marker_bit	1
			Getbits(1);

			//system_clock_reference_base [29..15]	15
			system_clock_reference_base_1 = Getbits(15);


			//marker_bit	1
			Getbits(1);
			//system_clock_reference_base [14..0]	15
			system_clock_reference_base_2 = Getbits(15);


			scr_base = ((long)system_clock_reference_base_0 << 30) |
						 ((long)system_clock_reference_base_1 << 15) |
									(uint)system_clock_reference_base_2;

			PTS_in_sec = (Int64)scr_base * 300;
			PTS_in_sec = PTS_in_sec / (Int64)27000000;

			sec = (int)PTS_in_sec;
			min = sec / 60;
			hour = min / 60;
			min = min % 60;
			sec = sec % 60;

			timeStamp.StartTimeSp.Minutes = min;
			timeStamp.StartTimeSp.Seconds = sec;
			timeStamp.StartTimeSp.Hours = hour;
			timeStamp.StartTimeSp.Milliseconds = PTS_in_sec;
			//timeStamp.s_Mpeg2TimeStamp33Bit = scr_base;

			Getbits(11);
			Getbits(22); // program mux rate 
			Getbits(7);
			// stuffing 
			x = Getbits(3);
			for (int i = 0; i < x; i++)
			{
				//stuffing_byte	8
				Getbits(8);
			}
			return true;
		}

		void AdjustTimeStampOffset(int counts, ref TimeDomain timeStampOffset)
		{
			long ta;

			Log.Debug("Old S Offset correction timestamp " + counts + " time " + cutPoints[counts].StartTimeSp.Hours + " " + cutPoints[counts].StartTimeSp.Minutes + " " + cutPoints[counts].StartTimeSp.Seconds);
			Log.Debug("Old E Offset correction timestamp " + counts + " time " + cutPoints[counts].EndTimeSp.Hours + " " + cutPoints[counts].EndTimeSp.Minutes + " " + cutPoints[counts].EndTimeSp.Seconds);

			ta = cutPoints[counts].StartTimeSp.Hours * 3600 + cutPoints[counts].StartTimeSp.Minutes * 60 + cutPoints[counts].StartTimeSp.Seconds +
					 timeStampOffset.StartTimeSp.Hours * 3600 + timeStampOffset.StartTimeSp.Minutes * 60 + timeStampOffset.StartTimeSp.Seconds;

			//splitterTime[counts].s_min = (int)(ta / 60);
			//splitterTime[counts].s_hour = tSplitterTime[counts].s_min / 60;
			//splitterTime[counts].s_sec = (int)(ta % 60);
			cutPoints[counts].StartTimeSp = new TimeSpan(ta);

			ta = cutPoints[counts].EndTimeSp.Hours * 3600 + cutPoints[counts].EndTimeSp.Minutes * 60 + cutPoints[counts].EndTimeSp.Seconds +
					 timeStampOffset.ESp.Hours * 3600 + timeStampOffset.StartTimeSp.Minutes * 60 + timeStampOffset.StartTimeSp.Seconds;

			//tSplitterTime[counts].e_min = (int)(ta / 60);
			//tSplitterTime[counts].e_hour = tSplitterTime[counts].e_min / 60;
			//tSplitterTime[counts].e_sec = (int)(ta % 60);
			cutPoints[counts].EndTimeSp = new TimeSpan(ta);

			Log.Debug("New S Offset correction timestamp " + counts + " time " + cutPoints[counts].StartTimeSp.Hours + " " + cutPoints[counts].StartTimeSp.Minutes + " " + cutPoints[counts].StartTimeSp.Seconds);
			Log.Debug("New E Offset correction timestamp " + counts + " time " + cutPoints[counts].EndTimeSp.Hours + " " + cutPoints[counts].EndTimeSp.Minutes + " " + cutPoints[counts].EndTimeSp.Seconds);

		}
		
		public void Cut(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP[] tCutTime, int iCounts)
		{
			tSplitterTime[0].type = RIPPER_TIME_STAMP_MODE_0;
			tSplitterTime[1].type = RIPPER_TIME_STAMP_MODE_0;

			iPointCounter = iCounts;
			sourceBitAddress = 0;

			for (int i = 0; i < iCounts; i++)
			{
				tSplitterTime[i] = tCutTime[i];
			}

			if (OpenInOutFile(sInFilename, sOutFilename))
			{
				SplitProgramStreamCut();
				CloseInOutFile();
			}
		}

		bool SplitProgramStreamCut()
		{
			long lFileSizeSaved;
			int iLeftBufferSize;
			Int64 x = 0;
			int c_times = 0;
			Int64 iReadCounter = 0;
			Int64 iWriteCounter = 0;
			bool bCheckOffset = true;

			Log.Debug("Start SplitProgramStreamCut");

			lFileSizeSaved = fsIn.Length;
			iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
			Log.Debug("step 0 " + iLeftBufferSize);
			
			if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize) return false; //ERROR_READING_FROM_INPUT_FILE

			lFileSizeSaved = lFileSizeSaved - iLeftBufferSize;

			// Also get the first time stamp to see what is the zero time 
			GetTimeStamp(iReadCounter, c_times, ref tStamp);
			AdjustTimeStampOffset(c_times, ref tStamp);
			iReadCounter += 14; // 14 - 4 bytes of the first pack header minus the pack header start code 
			iLeftBufferSize -= 14;
			if (!CompareTimeStampStartPoint(c_times, ref tStamp))
			{
				//ER		        if (_write(OutFileHnd , ptBuffers + iWriteCounter , iReadCounter) != iReadCounter) { 
				//ER			         return ERROR_WRITEING_TO_OUTPUT_FILE; 
				//ER		        }
				fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
				iWriteCounter = iReadCounter;
			}
			Log.Debug("step 1");
		// Untill we found the start point a we need to write the file into the output 
		cut_more: // to do make while-loop
			while ((iLeftBufferSize != 0) && !CompareTimeStampStartPoint(c_times, ref tStamp))
			{
				if (iReadCounter <= (FIFO_SIZE - 4)) // prevent out-of-bound
				{
					x = (ptBuffers[iReadCounter + 0] << 24) + (ptBuffers[iReadCounter + 1] << 16) + (ptBuffers[iReadCounter + 2] << 8) + (ptBuffers[iReadCounter + 3] << 0); //*(DWORD *)(ptBuffers + counter); 
				}
				else
				{
					x = 0;
				}
				if (x == 0x000001BA)
				{
					//ER		           if (_write(OutFileHnd , ptBuffers + iWriteCounter , iReadCounter - iWriteCounter) != (iReadCounter - iWriteCounter)) { 
					//ER			            return ERROR_WRITEING_TO_OUTPUT_FILE; 
					//ER			        }
					fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
					iWriteCounter = iReadCounter;
					GetTimeStamp(iReadCounter, c_times, ref tStamp);
					if (bCheckOffset) // because the test mpeg I used contains the first packet header zero and then the real offset, so temp read out twice
					{
						for (int t = 0; t < iPointCounter; t++)
							AdjustTimeStampOffset(t, ref tStamp);
						bCheckOffset = false;
					}
					iReadCounter += 14;
					iLeftBufferSize -= 14;
					Log.Debug("TimeA " + tStamp.s_min + " " + tStamp.s_sec);
				}
				//ER               else if (x == 0x000001B9) // it terminates the Program Stream
				//ER               {
				//ER                   swLog.WriteLine("End of file received"); //return true; 							
				//ER		        } 
				else
				{
					iReadCounter += 1;
					iLeftBufferSize--;
				}
				if (iLeftBufferSize == 0)
				{
					iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
					//ER			        if (_read(InFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
					//ER				         return ERROR_READING_FROM_INPUT_FILE; 
					//ER			        }
					if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
					{
						return false; //ERROR_READING_FROM_INPUT_FILE
					}
					lFileSizeSaved -= iLeftBufferSize;
					iWriteCounter = iReadCounter = 0;
				}
			}

			if (iLeftBufferSize == 0)
			{
				iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
				//ER			        if (_read(InFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
				//ER				         return ERROR_READING_FROM_INPUT_FILE; 
				//ER			        }
				if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize) return false; //ERROR_READING_FROM_INPUT_FILE
				Log.Debug("Read-buffer length " + FIFO_SIZE + " size left " + iLeftBufferSize + " @ Time " + tStamp.s_min + " " + tStamp.s_sec);
				lFileSizeSaved -= iLeftBufferSize;
				iWriteCounter = iReadCounter = 0;
			}

			iWriteCounter = iReadCounter;

			while ((iLeftBufferSize != 0) && !CompareTimeStampEndPoint(c_times, ref tStamp))
			{
				if (iReadCounter <= (FIFO_SIZE - 4)) // prevent out-of-bound
				{
					x = (ptBuffers[iReadCounter + 0] << 24) + (ptBuffers[iReadCounter + 1] << 16) + (ptBuffers[iReadCounter + 2] << 8) + (ptBuffers[iReadCounter + 3] << 0); //*(DWORD *)(ptBuffers + counter); 
				}
				else
				{
					x = 0;
				}
				if (x == 0x000001BA)
				{
					GetTimeStamp(iReadCounter, c_times, ref tStamp);
					iReadCounter += 14;
					iLeftBufferSize -= 14;
					Log.Debug("TimeB " + tStamp.s_min + " " + tStamp.s_sec);
				}
				//ER                else if (x == 0x000001B9) // it terminates the Program Stream
				//ER                { 
				//ER			        return true; 				
				//ER		        } 
				else
				{
					iReadCounter += 1;
					iLeftBufferSize--;
				}

				if (iLeftBufferSize == 0)
				{
					iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
					//ER			        if (_read(InFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
					//ER				         return ERROR_READING_FROM_INPUT_FILE; 
					//ER			        }
					if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize) return false; //ERROR_READING_FROM_INPUT_FILE
					lFileSizeSaved -= iLeftBufferSize;
					iWriteCounter = iReadCounter = 0;
				}
			}

			c_times++;
			if (c_times < iPointCounter)
			{

				// write the pack header 
				iWriteCounter -= 14;
				//ER		        if (_write(OutFileHnd , ptBuffers + iWriteCounter , 14) != 14) { 
				//ER			        return ERROR_WRITEING_TO_OUTPUT_FILE; 
				//ER		        }
				fsOut.Write(ptBuffers, (int)iWriteCounter, 14);
				iWriteCounter = iReadCounter;
				goto cut_more;
			}

			iReadCounter -= 14;
			iLeftBufferSize += 14;
			// else copy the entire file to output 
			// start with the last header 
			//ER	        if (_write(OutFileHnd , ptBuffers + iReadCounter, 14) != 14) { 
			//ER		         return ERROR_WRITEING_TO_OUTPUT_FILE; 
			//ER	        }
			fsOut.Write(ptBuffers, (int)iReadCounter, 14);
			iReadCounter += 14;
			iLeftBufferSize -= 14;
			//ER	        if (_write(OutFileHnd , ptBuffers + iReadCounter , iLeftBufferSize) != iLeftBufferSize) { 
			//ER		         return ERROR_WRITEING_TO_OUTPUT_FILE; 
			//ER	        }
			fsOut.Write(ptBuffers, (int)iReadCounter, iLeftBufferSize);
			Log.Debug("test1");
			while (lFileSizeSaved != 0)
			{
				iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
				lFileSizeSaved -= iLeftBufferSize;
				//ER		        if (_read(InFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
				//ER			        return ERROR_READING_FROM_INPUT_FILE; 
				//ER		        }
				if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize) return false; //ERROR_READING_FROM_INPUT_FILE
				//ER	           if (_write(OutFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
				//ER		            return ERROR_WRITEING_TO_OUTPUT_FILE; 
				//ER		        }
				fsOut.Write(ptBuffers, 0, iLeftBufferSize);
			}

			return true;
		}



		public void Trim(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tTrimTime)
		{
			tSplitterTime[0].type = RIPPER_TIME_STAMP_MODE_0;
			tSplitterTime[1].type = RIPPER_TIME_STAMP_MODE_0;

			iPointCounter = 1;
			sourceBitAddress = 0;
			tSplitterTime[0] = tTrimTime;

			if (OpenInOutFile(sInFilename, sOutFilename))
			{
				SplitProgramStreamTrim();
				CloseInOutFile();
			}
		}
		int ShiftAndMask(byte val, int msb, int n)
		{
			int lsb = msb + 1 - n;
			int mask;

			mask = (1 << n) - 1;
			return (val >> lsb) & mask;
		}

		int Peekbits(int bits)
		{
			int data = 0;
			Int64 iBitAddress = sourceBitAddress; //__int64
			int offset, b;

			while (bits != 0)
			{
				offset = (int)(iBitAddress & 7);
				b = Math.Min(bits, 8 - offset);
				data <<= b;
				data |= ShiftAndMask(ptBuffers[g_iReadCounter + iBitAddress / 8], 7 - offset, b); //source_mmap	 = ptBuffers + counter; 
				iBitAddress += b;
				bits -= b;
			}
			return data;
		}

		int Getbits(int bits)
		{
			int data = Peekbits(bits);
			sourceBitAddress += bits;
			return data;
		}

		public System.IO.FileInfo InFilename
		{
			get
			{
				return inFilename;
			}
			set
			{
				inFilename = value;
			}
		}

		public List<TimeDomain> CutPoints
		{
			get
			{
				return cutPoints;
			}
			set
			{
				cutPoints = value;
			}
		}
	}*/
}