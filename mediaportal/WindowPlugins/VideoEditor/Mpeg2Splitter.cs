#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if (!STAND_ALONE)
using MediaPortal.GUI.Library;
#endif

// Original source-code available from www.becapture.com, written in C++
// This code refactored in C#

// Version: 0.5: 06-10-2006

/* To Do:
 
Functional:
- performance optimalisation (binary search to first cut-point)
- split functionality (is only a special kind of a trim/cut function)
- done ==> multi-cut points (typical for commercials) ==> bug-fixing ???, also use e_mode cut/trim !!!
- commercial detection ... (bit in stream ?, like VCR's ?)
- join-functionality
- object-oriented design: dvr-ms & mpeg, maybe move mpeg2-handling to core (DShowNet/Helper) ?

Source Code:
- remove variable iLeftBufferSize, and use a combination of iReadCounter/FIFO_SIZE to detect iLeftBufferSize
- done ==> Merge SplitProgramStreamRip & SplitProgramStreamCut
- Merge Rip & Cut function
- AdjustTimeStampOffset: usage of this function to check for multiple timestamps (counts != 0)
- AdjustTimeStampOffset: my test-mpeg contains as the first pack-header timestamp zero, while the next contains offset 43, so in general to overcome problems in the future, more times the offset should be checked
- 2 structs required for SPLITTER_TIME_STAMP --> one for  the interface and one to be used for GetTimeStamp
- remove ref's in cut and split functions
- done ==> remove goto !
- check if source time is also lineair (no gaps in time) and adapt target time lineair from the beginning
- add try and catch for function OpenInOutFile() and calls to read/write functions (disk-full messages,...)
- usage of core/mpeg2info.cs ???
- cut location, to be checked also with I,B and P frames, to get smooth transition
- add write buffer, so that larger blocks will be written 
- progress based on address (time-stamps) iso filesave
- progress event based on backgroundworker class
- add read/write/process thread
- use DateTime struct iso "struct TIME_STAMP"
- what is this for a notation/statement: List<System.IO.FileInfo> FileInfo ?????

*/

namespace Mpeg2SplitterPackage
{

    struct SPLITTER_TIME_STAMP
    {
        public int s_min;
        public int s_sec;
        public int s_hour;
        public int e_min;
        public int e_sec;
        public int e_hour;
    };

    class Mpeg2Splitter
    {
        private const int NR_OF_SPILTER_TIME_STAMPS = 40;
        private SPLITTER_TIME_STAMP[] tSplitterTime = new SPLITTER_TIME_STAMP[NR_OF_SPILTER_TIME_STAMPS];
        private SPLITTER_TIME_STAMP tStamp;
        private SPLITTER_TIME_STAMP tPreviousStamp;
        private FileStream fsIn;
        private FileStream fsOut;
#if (STAND_ALONE)
        private StreamWriter swLog;
#endif
        private BinaryReader bwIn;
        private BinaryWriter bwOut;
        private Int64 iSourceBitAddress;
        private long lBlockRead, lTotalBlockRead;        // required to calc progress        

        private const int FIFO_SIZE = 1024 * 1024 * 50; //50MB buffer is faster then 20MB

        private byte[] ptBuffers = new byte[FIFO_SIZE];

        private Int64 g_iReadCounter = 0;

        private int iPointCounter;

#if (STAND_ALONE)
        private bool bLogEnabled = true;
#else
        private bool bLogEnabled = true;
#endif

        System.Timers.Timer progressTime;
        public delegate void Finished();
        public event Finished OnFinished;
        public delegate void Progress(int percentage);
        public event Progress OnProgress;

        private int percent = 0;

        public Mpeg2Splitter()
        {
            progressTime = new System.Timers.Timer(1000);
            progressTime.Elapsed += new System.Timers.ElapsedEventHandler(progressTime_Elapsed);
        }

        void progressTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            percent = (int)(lBlockRead * 100 / lTotalBlockRead);
			if ( percent > 100 )
			{
				percent = 100;
			}
            //Logging("percent:"+percent.ToString());
            if (OnProgress != null)
            {
                OnProgress(percent);
            }
        }

        private bool OpenInOutFile(string sInFilename, string sOutFilename, FileMode  fileMode )
        {
            bool result = true;

            try
            {
                // Create the reader for data
                fsIn = new FileStream(sInFilename, FileMode.Open, FileAccess.Read);
                if (fsIn == null)
                {
                    Logging("Error opening fsIn");
                    result = false;
                }
                // Create the writer for data.
                bwIn = new BinaryReader(fsIn);
                if (bwIn == null)
                {
                    Logging("Error opening bwIn");
                    result = false;
                }
                // Create the new, empty data file
                fsOut = new FileStream(sOutFilename, fileMode, FileAccess.Write);
                if (fsOut == null)
                {
                    Logging("Error opening fsOut");
                    result = false;
                }
                // Create the writer for data.
                bwOut = new BinaryWriter(fsOut);
                if (bwOut == null)
                {
                    Logging("Error opening bwOut");
                    result = false;
                }
                // Create the new, empty data file
#if (STAND_ALONE)
                swLog = new StreamWriter("log.txt");
                Logging("Start log-file");
#endif
            }
            catch (Exception e)
            {
                Logging("Exception occured in OpenInOutFile: " + e);
                result = false;
            }
            return result;
        }

        private void CloseInOutFile()
        {
            progressTime.Stop();
            fsIn.Close();
            bwIn.Close();
            fsOut.Close();
            bwOut.Close();
#if (STAND_ALONE)
            swLog.Close();
#endif
        }

        public void Split(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tSplitTime)
        {
        }
        public void Join(List<System.IO.FileInfo> fileList, string sOutFilename)
        {
			int i = 0;
            lBlockRead = lTotalBlockRead = 0; // to calc progress

            foreach (FileInfo file in fileList)
            {
                if (OpenInOutFile(file.FullName, sOutFilename, FileMode.Create))
                {
                    lTotalBlockRead += (fsIn.Length / FIFO_SIZE);
                    CloseInOutFile();
                }
                i++;
            }
            i = 0;
			foreach (FileInfo file in fileList)
			{
                if (OpenInOutFile(file.FullName, sOutFilename, (i == 0) ? FileMode.Create : FileMode.Append))
	            {
	                JoinProgramStream();
	                CloseInOutFile();
	            }
				i++;
			}        
        }
        public void Cut(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tSplitTime)
        {
            SPLITTER_TIME_STAMP[] tStamp = new SPLITTER_TIME_STAMP[1];
            tStamp[0] = tSplitTime;
            Cut(sInFilename, sOutFilename, ref tStamp, 1);
        }
        public void Cut(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP[] tSplitTime, int iCounts)
        {
            lBlockRead = lTotalBlockRead = 0; // to calc progress
            if (OpenInOutFile(sInFilename, sOutFilename, FileMode.Create))
            {
                DateTime timeStart = DateTime.Now;

                iPointCounter = iCounts;
                iSourceBitAddress = 0;

                for (int i = 0; i < iCounts; i++)
                {
                    tSplitterTime[i] = tSplitTime[i];
                    Logging("Cut-points " + (i + 1) + "/" + iCounts + " start " + tSplitterTime[i].s_hour + "h:" + tSplitterTime[i].s_min + "m:" + tSplitterTime[i].s_sec + "s stop " + tSplitterTime[i].e_hour + "h:" + tSplitterTime[i].e_min + "m:" + tSplitterTime[i].e_sec + "s");
                }
                lTotalBlockRead = (fsIn.Length / FIFO_SIZE);
                SplitProgramStream(true);
                DateTime timeStop = DateTime.Now;
                TimeSpan duration = timeStop.Subtract(timeStart);
                Logging("Duration " + duration);
                CloseInOutFile();
            }
        }

        public void Rip(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP[] tTrimTime, int iCounts)
        {
            lBlockRead = lTotalBlockRead = 0; // to calc progress
            if (OpenInOutFile(sInFilename, sOutFilename, FileMode.Create))
            {
                DateTime timeStart = DateTime.Now;

                iPointCounter = iCounts;
                iSourceBitAddress = 0;

                for (int i = 0; i < iCounts; i++)
                {
                    tSplitterTime[i] = tTrimTime[i];
                    Logging("Rip-points " + (i + 1) + "/" + iCounts + " start " + tSplitterTime[i].s_hour + "h:" + tSplitterTime[i].s_min + "m:" + tSplitterTime[i].s_sec + "s stop " + tSplitterTime[i].e_hour + "h:" + tSplitterTime[i].e_min + "m:" + tSplitterTime[i].e_sec + "s");
                }
                lTotalBlockRead = (fsIn.Length / FIFO_SIZE);
                SplitProgramStream(false);
                DateTime timeStop = DateTime.Now;
                TimeSpan duration = timeStop.Subtract(timeStart);
                Logging("Duration " + duration);
                CloseInOutFile();
            }
        }
        public void Rip(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tTrimTime)
        {
            SPLITTER_TIME_STAMP[] tStamp = new SPLITTER_TIME_STAMP[1];
            tStamp[0] = tTrimTime;
            Rip(sInFilename, sOutFilename, ref tStamp, 1);
        }
        private int ShiftAndMask(byte val, int msb, int n)
        {
            int lsb = msb + 1 - n;
            int mask;

            mask = (1 << n) - 1;
            return (val >> lsb) & mask;
        }

        private int Peekbits(int bits)
        {
            int data = 0;
            Int64 iBitAddress = iSourceBitAddress; //__int64
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

        private int Getbits(int bits)
        {
            int data = Peekbits(bits);
            iSourceBitAddress += bits;
            return data;
        }

        private void AdjustTimeStampOffset(int counts, ref SPLITTER_TIME_STAMP tTimeStampOffset)
        {
            long ta;

            Logging("Old S Offset correction timestamp " + counts + " time " + tSplitterTime[counts].s_hour + " " + tSplitterTime[counts].s_min + " " + tSplitterTime[counts].s_sec);
            Logging("Old E Offset correction timestamp " + counts + " time " + tSplitterTime[counts].e_hour + " " + tSplitterTime[counts].e_min + " " + tSplitterTime[counts].e_sec);

            ta = tSplitterTime[counts].s_hour * 3600 + tSplitterTime[counts].s_min * 60 + tSplitterTime[counts].s_sec +
                 tTimeStampOffset.s_hour * 3600 + tTimeStampOffset.s_min * 60 + tTimeStampOffset.s_sec;

            tSplitterTime[counts].s_min = (int)(ta / 60);
            tSplitterTime[counts].s_hour = tSplitterTime[counts].s_min / 60;
            tSplitterTime[counts].s_sec = (int)(ta % 60);

            ta = tSplitterTime[counts].e_hour * 3600 + tSplitterTime[counts].e_min * 60 + tSplitterTime[counts].e_sec +
                 tTimeStampOffset.s_hour * 3600 + tTimeStampOffset.s_min * 60 + tTimeStampOffset.s_sec;

            tSplitterTime[counts].e_min = (int)(ta / 60);
            tSplitterTime[counts].e_hour = tSplitterTime[counts].e_min / 60;
            tSplitterTime[counts].e_sec = (int)(ta % 60);

            Logging("New S Offset correction timestamp " + counts + " time " + tSplitterTime[counts].s_hour + " " + tSplitterTime[counts].s_min + " " + tSplitterTime[counts].s_sec);
            Logging("New E Offset correction timestamp " + counts + " time " + tSplitterTime[counts].e_hour + " " + tSplitterTime[counts].e_min + " " + tSplitterTime[counts].e_sec);

        }

        private bool CompareTimeStampStartPoint(int counts, ref SPLITTER_TIME_STAMP tTimeStamp)
        {
            return ((tSplitterTime[counts].s_min == tTimeStamp.s_min) && (tSplitterTime[counts].s_sec == tTimeStamp.s_sec));
        }
        private bool CompareTimeStampEndPoint(int counts, ref SPLITTER_TIME_STAMP tTimeStamp)
        {
            return ((tSplitterTime[counts].e_min == tTimeStamp.s_min) && (tSplitterTime[counts].e_sec == tTimeStamp.s_sec));
        }
        private bool GetTimeStamp(Int64 counter, int counts, ref SPLITTER_TIME_STAMP tTimeStamp)
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

            /* Pack header length */
            //32 + 2 + 3 + 1 + 15 + 1 + 15 + 1 + 9 + 1 + 22 +  1 + 1 + 5 + 3  =  112/8 = 14 bytes 
            /* pack header start code */
            x = Getbits(32);

            //'01'	2 /* say the layer - here its mpeg2 if   its 0x2 its mpeg1 */
            x = Getbits(2);
            if (x != 0x1)
            {
                //ER		   TRACE("Warning : MPEG2 bit is not signald\n");
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

            tTimeStamp.s_min = min;
            tTimeStamp.s_sec = sec;
            tTimeStamp.s_hour = hour;

            Getbits(11);
            Getbits(22); // program mux rate 
            Getbits(7);
            /* stuffing */
            x = Getbits(3);
            for (int i = 0; i < x; i++)
            {
                //stuffing_byte	8
                Getbits(8);
            }
            return true;
        }
        private void Logging(string sLog)
        {
            if (bLogEnabled)
            {
#if (STAND_ALONE)
                swLog.WriteLine(sLog);
#else
                Log.Info(sLog);
#endif
            }
        }
        private bool SplitProgramStream(bool bRemoveBetweenPoints)
        {
            int iLeftBufferSize;
            Int64 x = 0;
            int c_times = 0;
            Int64 iReadCounter = 0;
            Int64 iWriteCounter = 0;
            long lFileSizeSaved;

            Logging("Start SplitProgramStream " + ((bRemoveBetweenPoints) ? "remove" : "hold") + " between points");

            progressTime.Start();
            lFileSizeSaved = fsIn.Length;
            iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
            if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
            {
                return false; //ERROR_READING_FROM_INPUT_FILE
            }
            else
            {
                lBlockRead++; // to calc progress
            }
            lFileSizeSaved -= iLeftBufferSize;
            /* Also get the first time stamp to see what is the zero time */
            GetTimeStamp(iReadCounter, c_times, ref tStamp);
            for (int t = 0; t < iPointCounter; t++)
            {
                AdjustTimeStampOffset(t, ref tStamp);
            }
            tPreviousStamp.s_min = -1; // make the TimeStamp invalid so that the first timestamp is always printed
            iReadCounter += 14; // 14 - 4 bytes of the first pack header minus the pack header start code 
            iLeftBufferSize -= 14;
            if (bRemoveBetweenPoints)
            {
                if (!CompareTimeStampStartPoint(c_times, ref tStamp))
                {
                    fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                    iWriteCounter = iReadCounter;
                }
            }
            else
            {
                if (CompareTimeStampStartPoint(c_times, ref tStamp))
                {
                    fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                    iWriteCounter = iReadCounter;
                }
            }
            while (c_times < iPointCounter)
            {
                // Untill we found the start point a we need to write the file into the output
                while (iLeftBufferSize != 0)
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
                        if (bRemoveBetweenPoints)
                        {
                            fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                            iWriteCounter = iReadCounter;
                        }
                        GetTimeStamp(iReadCounter, c_times, ref tStamp);
                        iReadCounter += 14;
                        iLeftBufferSize -= 14;
                        if ((tStamp.s_hour != tPreviousStamp.s_hour) || (tStamp.s_min != tPreviousStamp.s_min) || (tStamp.s_sec != tPreviousStamp.s_sec))
                        {
                            Logging("TimeA " + tStamp.s_hour + "h:" + tStamp.s_min + "m:" + tStamp.s_sec + "s");
                        }
                        tPreviousStamp = tStamp;
                        if (CompareTimeStampStartPoint(c_times, ref tStamp))
                        {
                            if (!bRemoveBetweenPoints)
                            {
                                /* write the pack header */
                                iWriteCounter = iReadCounter - 14;
                                fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                                iWriteCounter = iReadCounter;
                            }
                            break;
                        }
                    }
                    //ER               else if (x == 0x000001B9) // it terminates the Program Stream
                    //ER               {
                    //ER                   Logging("End of file received"); //return true; 							
                    //ER		        } 
                    else
                    {
                        iReadCounter += 1;
                        iLeftBufferSize--;
                    }
                    if (iLeftBufferSize == 0)
                    {
                        iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
                        if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
                        {
                            return false; //ERROR_READING_FROM_INPUT_FILE
                        }
                        else
                        {
                            lBlockRead++; // to calc progress
                        }
                        lFileSizeSaved -= iLeftBufferSize;
                        iWriteCounter = iReadCounter = 0;
                    }
                }

                if (iLeftBufferSize == 0)
                {
                    iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
                    if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
                    {
                        return false; //ERROR_READING_FROM_INPUT_FILE
                    }
                    else
                    {
                        lBlockRead++; // to calc progress
                    }
                    lFileSizeSaved -= iLeftBufferSize;
                    iWriteCounter = iReadCounter = 0;
                }

                // We found the start point a we skip the input till we found the end point
                while (iLeftBufferSize != 0)
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
                        if (!bRemoveBetweenPoints)
                        {
                            fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                            iWriteCounter = iReadCounter;
                        }
                        GetTimeStamp(iReadCounter, c_times, ref tStamp);
                        iReadCounter += 14;
                        iLeftBufferSize -= 14;
                        if ((tStamp.s_hour != tPreviousStamp.s_hour) || (tStamp.s_min != tPreviousStamp.s_min) || (tStamp.s_sec != tPreviousStamp.s_sec))
                        {
                            Logging("TimeB " + tStamp.s_hour + "h:" + tStamp.s_min + "m:" + tStamp.s_sec + "s");
                        }
                        tPreviousStamp = tStamp;
                        if (CompareTimeStampEndPoint(c_times, ref tStamp))
                        {
                            if (bRemoveBetweenPoints)
                            {
                                /* write the pack header */
                                iWriteCounter = iReadCounter - 14;
                                fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                                iWriteCounter = iReadCounter;
                            }
                            break;
                        }
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
                        if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
                        {
                            return false; //ERROR_READING_FROM_INPUT_FILE
                        }
                        else
                        {
                            lBlockRead++; // to calc progress
                        }
                        lFileSizeSaved -= iLeftBufferSize;
                        iWriteCounter = iReadCounter = 0;
                    }
                }
                c_times++;
            }
            if (bRemoveBetweenPoints)
            {
                /* now copy the entire file to output */
                fsOut.Write(ptBuffers, (int)iReadCounter, iLeftBufferSize);
                while (lFileSizeSaved != 0)
                {
                    iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
                    lFileSizeSaved -= iLeftBufferSize;
                    if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
                    {
                        return false; //ERROR_READING_FROM_INPUT_FILE
                    }
                    else
                    {
                        lBlockRead++; // to calc progress
                    }
                    fsOut.Write(ptBuffers, 0, iLeftBufferSize);
                }
            }

            progressTime.Stop();
            percent = 100;

            if (OnFinished != null)
            {
                OnFinished();
            }
            return true;
        }
        private bool JoinProgramStream()
        {
            int iLeftBufferSize;
            Int64 x = 0;
            int c_times = 0;
            Int64 iReadCounter = 0;
            Int64 iWriteCounter = 0;
            long lFileSizeSaved;

            Logging("JoinProgramStream");

            progressTime.Start();

            lFileSizeSaved = fsIn.Length;

            while (lFileSizeSaved != 0)
            {
                iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
                lFileSizeSaved -= iLeftBufferSize;
                if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize)
                {
                    return false; //ERROR_READING_FROM_INPUT_FILE
                }
                else
                {
                    lBlockRead++; // to calc progress
                }
                fsOut.Write(ptBuffers, 0, iLeftBufferSize);
            }

            progressTime.Stop();
            percent = 100;

            if (OnFinished != null)
            {
                OnFinished();
            }
            return true;
        }
    }
}
