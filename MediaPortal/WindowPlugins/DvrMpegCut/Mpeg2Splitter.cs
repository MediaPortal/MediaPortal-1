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
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

// Original source-code available from www.becapture.com, written in C++
// This code refactored in C#

/* To Do:
 
Functional:
- performance optimalisation
- split functionality (is only a special kind of a trim/cut function)
- multi-cut points (typical for commercials)
- commercial detection ... (bit in stream ?, like VCR's ?)

Source Code:
- remove variable iLeftBufferSize, and use a combination of iReadCounter/FIFO_SIZE to detect iLeftBufferSize
- Merge SplitProgramStreamTrim & SplitProgramStreamCut
- AdjustTimeStampOffset: usage of this function to check for multiple timestamps (counts != 0)
- AdjustTimeStampOffset: my test-mpeg contains as the first pack-header timestamp zero, while the next contains offset 43, so in general to overcome problems in the future, more times the offset should be checked
- 2 structs required for SPLITTER_TIME_STAMP --> one for  the interface and one to be used for GetTimeStamp
*/

namespace Mpeg2SplitterPackage
{
  struct SPLITTER_TIME_STAMP
  {
    public int s_min;
    public int s_sec;
    public int s_hour;
    public double s_mili;
    public int e_min;
    public int e_sec;
    public int e_hour;
    public int type;
    public Int64 s_Mpeg2TimeStamp33Bit;
  };

  class Mpeg2Splitter
  {
    const int NR_OF_SPILTER_TIME_STAMPS = 40;
    SPLITTER_TIME_STAMP[] tSplitterTime = new SPLITTER_TIME_STAMP[NR_OF_SPILTER_TIME_STAMPS];
    SPLITTER_TIME_STAMP tStamp;
    FileStream fsIn;
    FileStream fsOut;
    //StreamWriter swLog;
    BinaryReader bwIn;
    BinaryWriter bwOut;
    Int64 iSourceBitAddress;

    const int FIFO_SIZE = 1024 * 1024 * 20;

    byte[] ptBuffers = new byte[FIFO_SIZE];

    Int64 g_iReadCounter = 0;

    int iPointCounter;

    const int RIPPER_TIME_STAMP_MODE_0 = 0;  // to min.sec
    const int RIPPER_TIME_STAMP_MODE_1 = 1;  // to mili 
    const int RIPPER_TIME_STAMP_MODE_2 = 2;  // to 33 bit time stamp

    bool bLogEnabled = false;

    bool OpenInOutFile(string sInFilename, string sOutFilename)
    {
      bool result = true;

      // Create the reader for data
      fsIn = new FileStream(sInFilename, FileMode.Open, FileAccess.Read);
      if (fsIn == null)
      {
        result = false;
      }
      // Create the writer for data.
      bwIn = new BinaryReader(fsIn);
      if (bwIn == null)
      {
        result = false;
      }
      // Create the new, empty data file
      fsOut = new FileStream(sOutFilename, FileMode.Create, FileAccess.ReadWrite);
      if (fsOut == null)
      {
        result = false;
      }
      // Create the writer for data.
      bwOut = new BinaryWriter(fsOut);
      if (bwOut == null)
      {
        result = false;
      }
      // Create the new, empty data file
      //swLog = new StreamWriter("log.txt");
      //swLog.WriteLine("Start log-file");

      return result;
    }

    void CloseInOutFile()
    {
      fsIn.Close();
      bwIn.Close();
      fsOut.Close();
      bwOut.Close();
      //swLog.Close();
    }

    public void Split(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tSplitTime)
    {


    }
    public void Cut(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tCutTime)
    {
      tSplitterTime[0].type = RIPPER_TIME_STAMP_MODE_0;
      tSplitterTime[1].type = RIPPER_TIME_STAMP_MODE_0;

      iPointCounter = 1;
      iSourceBitAddress = 0;
      tSplitterTime[0] = tCutTime;

      if (OpenInOutFile(sInFilename, sOutFilename))
      {
        SplitProgramStreamCut();
        CloseInOutFile();
      }
    }
    public void Cut(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP[] tCutTime, int iCounts)
    {
      tSplitterTime[0].type = RIPPER_TIME_STAMP_MODE_0;
      tSplitterTime[1].type = RIPPER_TIME_STAMP_MODE_0;

      iPointCounter = iCounts;
      iSourceBitAddress = 0;

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

    public void Trim(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tTrimTime)
    {
      tSplitterTime[0].type = RIPPER_TIME_STAMP_MODE_0;
      tSplitterTime[1].type = RIPPER_TIME_STAMP_MODE_0;

      iPointCounter = 1;
      iSourceBitAddress = 0;
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

    int Getbits(int bits)
    {
      int data = Peekbits(bits);
      iSourceBitAddress += bits;
      return data;
    }

    void AdjustTimeStampOffset(int counts, ref SPLITTER_TIME_STAMP tTimeStampOffset)
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

    bool CompareTimeStampStartPoint(int counts, ref SPLITTER_TIME_STAMP tTimeStamp)
    {
      return ((tSplitterTime[counts].s_min == tTimeStamp.s_min) && (tSplitterTime[counts].s_sec == tTimeStamp.s_sec));
    }
    bool CompareTimeStampEndPoint(int counts, ref SPLITTER_TIME_STAMP tTimeStamp)
    {
      return ((tSplitterTime[counts].e_min == tTimeStamp.s_min) && (tSplitterTime[counts].e_sec == tTimeStamp.s_sec));
    }
    bool GetTimeStamp(Int64 counter, int counts, ref SPLITTER_TIME_STAMP tTimeStamp)
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
      tTimeStamp.s_mili = PTS_in_sec;
      tTimeStamp.s_Mpeg2TimeStamp33Bit = scr_base;

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
    void Logging(string sLog)
    {
      if (bLogEnabled)
      {
        //swLog.WriteLine(sLog);
        ServiceProvider services = GlobalServiceProvider.Instance;
        ILog log = services.Get<ILog>();
        log.Info(sLog);
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

      Logging("Start SplitProgramStreamCut");

      lFileSizeSaved = fsIn.Length;
      iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);
      Logging("step 0 " + iLeftBufferSize);
      //ER	        if (_read(InFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
      //ER		         return ERROR_READING_FROM_INPUT_FILE; 
      //ER	        }
      if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize) return false; //ERROR_READING_FROM_INPUT_FILE

      lFileSizeSaved = lFileSizeSaved - iLeftBufferSize;

      /* Also get the first time stamp to see what is the zero time */
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
      Logging("step 1");
    /* Untill we found the start point a we need to write the file into the output */
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
          Logging("TimeA " + tStamp.s_min + " " + tStamp.s_sec);
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
        Logging("Read-buffer length " + FIFO_SIZE + " size left " + iLeftBufferSize + " @ Time " + tStamp.s_min + " " + tStamp.s_sec);
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
          Logging("TimeB " + tStamp.s_min + " " + tStamp.s_sec);
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

        /* write the pack header */
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
      /* else copy the entire file to output */
      /* start with the last header */
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
      Logging("test1");
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
    bool SplitProgramStreamTrim()
    {
      long lFileSizeSaved;
      int iLeftBufferSize;
      Int64 x = 0;
      int c_times = 0;
      Int64 iReadCounter = 0;
      Int64 iWriteCounter = 0;
      bool bCheckOffset = true;

      lFileSizeSaved = fsIn.Length;
      iLeftBufferSize = (int)Math.Min(lFileSizeSaved, FIFO_SIZE);

      //ER	        if (_read(InFileHnd , ptBuffers , iLeftBufferSize) != iLeftBufferSize) { 
      //ER		         return ERROR_READING_FROM_INPUT_FILE; 
      //ER	        }
      if (bwIn.Read(ptBuffers, 0, iLeftBufferSize) != iLeftBufferSize) return false; //ERROR_READING_FROM_INPUT_FILE

      lFileSizeSaved = lFileSizeSaved - iLeftBufferSize;

      /* Also get the first time stamp to see what is the zero time */
      GetTimeStamp(iReadCounter, c_times, ref tStamp);
      AdjustTimeStampOffset(c_times, ref tStamp);
      iReadCounter += 14; // 14 - 4 bytes of the first pack header minus the pack header start code 
      iLeftBufferSize -= 14;
      if (CompareTimeStampStartPoint(c_times, ref tStamp))
      {
        //ER		        if (_write(OutFileHnd , ptBuffers + iWriteCounter , iReadCounter) != iReadCounter) { 
        //ER			         return ERROR_WRITEING_TO_OUTPUT_FILE; 
        //ER		        }
        fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
        iWriteCounter = iReadCounter;
      }

      /* Untill we found the start point a we need to write the file into the output */

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
          //                    fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
          //                    iWriteCounter = iReadCounter;
          GetTimeStamp(iReadCounter, c_times, ref tStamp);
          if (bCheckOffset) // because the test mpeg I used contains the first packet header zero and then the real offset, so temp read out twice
          {
            AdjustTimeStampOffset(c_times, ref tStamp);
            bCheckOffset = false;
          }
          iReadCounter += 14;
          iLeftBufferSize -= 14;
          Logging("TimeA " + tStamp.s_min + " " + tStamp.s_sec);
        }
        //Temp disabled due to my test mpeg file, it contains somewhere in the stream this code (0x000001B9)
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
        Logging("Read-buffer length " + FIFO_SIZE + " size left " + iLeftBufferSize + " @ Time " + tStamp.s_min + " " + tStamp.s_sec);
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
          fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
          iWriteCounter = iReadCounter;
          GetTimeStamp(iReadCounter, c_times, ref tStamp);
          iReadCounter += 14;
          iLeftBufferSize -= 14;
          Logging("TimeB " + tStamp.s_min + " " + tStamp.s_sec);
        }
        //ER                else if (x == 0x000001B9) // it terminates the Program Stream
        //ER                {
        //ER                    return true;
        //ER                }
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

      return true;
    }


  }
}
