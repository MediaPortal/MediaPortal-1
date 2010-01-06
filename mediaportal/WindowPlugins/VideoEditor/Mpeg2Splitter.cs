#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#define ENABLE_FAST_SEEK
#define ENABLE_SCR_CORRECT_TIME_STAMP
#define ENABLE_PTS_DTS_CORRECT_TIME_STAMP

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using MediaPortal.GUI.Library;

#if (!STAND_ALONE)

#endif

// Required documents: Iso-Iec-13818-1 2000 Mpeg2 Systems 

/* To Do:
 
Functional:
- performance optimalisation (binary search to first cut-point)
- split functionality (is only a special kind of a trim/cut function)
- done ==> multi-cut points (typical for commercials) ==> bug-fixing ???, also use e_mode cut/trim !!!
- commercial detection ... (bit in stream ?, like VCR's ?)
- join-functionality
- object-oriented design: dvr-ms & mpeg, maybe move mpeg2-handling to core (DShowNet/Helper) ?

Source Code:
- remove variable iLeftBufferSize, and use a combination of iReadCounter/READ_FIFO_SIZE to detect iLeftBufferSize
- done ==> Merge SplitProgramStreamRip & SplitProgramStreamCut
- Merge Rip & Cut function
- AdjustTimeStampOffset: usage of this function to check for multiple timestamps (counts != 0)
- AdjustTimeStampOffset: my test-mpeg contains as the first pack-header timestamp zero, while the next contains offset 43, so in general to overcome problems in the future, more times the offset should be checked
- 2 structs required for SPLITTER_TIME_STAMP --> one for  the interface and one to be used for GetScrTimeStamp
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
- progress-bar joining not shown
- fix seeking in player in multi-cutted mpegs
- done ==> logging split into Log.Debug and Log.Info
- progress-bar doesn't work when mpeg2 file is not starting from a zero time-stamp
- to be tested in CUT mode, scene mode is tested because it is used already with mediaportal
- use return of GetScrTimeStamp function
- find different location to close log-file: swLog.Close(), currently in CloseInOutFile, doesn't work at destructor !!!! 
- make setbits function
- recorded dvr-ms/mpeg should not contain 0x00001B9 end code somewhere in the middle of the stream, to be checked in mediaportal core
- jump fixed offset, all mpeg pack header are all 0x800 long, to be checked in standard, if ( iLeftBufferSize > 0x800 ) // make FIFO buffer a multiple of 0x800 ???
- this check "if (iLeftBufferSize == 0)" doesn't it spoil 1 byte ???
- error handling to Client --> InfoLogging("Buffer-End inside Header, for now stop splitting and return error "), in this way the client doesn't remove the original file, using throw-exception mechanism
- need kind of memcopy/blockcopy to do following: for (int i = 0; i < iLeftBufferSize; i++) --> ptBuffers[i] = ptBuffers[iReadCounter + i]
- read a few pack-headers (pack-length probably 0x800, need to check spec)
- SeekStepForward = 1024 * 1024 * 10; // Parameters are hard dependent on mpeg bit-rate, so need calibration algorithm
- implement FAST-SEEK for CUT-mode
- lFileSizeSaved can be changed by fsIn.Length - fsIn.Position
- seek forward and backward need to check boundary of file
- handling illegal timestamps, for example timestamps larger then mpeg file
*/

namespace Mpeg2SplitterPackage
{
  internal enum SplitMode
  {
    E_MODE_CUT,
    E_MODE_SCENE,
    E_MODE_JOIN
  }

  internal enum PtsDtsFlags
  {
    E_FLAGS_PTS_ONLY = 2,
    E_FLAGS_PTS_DTS = 3
  }

  internal struct SPLITTER_TIME_STAMP
  {
    public DateTime start;
    public DateTime end;
  } ;

  internal class Mpeg2Splitter
  {
    private const int iVersion = 23; // Version: 05-11-2006
    private const int PACKET_HEADER_START_CODE = 0x000001BA;
    private const int MPEG_PROGRAM_END_CODE = 0x000001B9;
    private const int PACKET_START_CODE_AUDIO_BEGIN = 0x000001C0;
    private const int PACKET_START_CODE_AUDIO_END = 0x000001DF;
    private const int PACKET_START_CODE_VIDEO_BEGIN = 0x000001E0;
    private const int PACKET_START_CODE_VIDEO_END = 0x000001EF;

    private const int PACKET_HEADER_INCR = 14;
    private const int PES_HEADER_INCR = 19;
    private const int READ_BUF_INCR = PES_HEADER_INCR;
    private const int NR_OF_SPILTER_TIME_STAMPS = 40;
    private SPLITTER_TIME_STAMP[] tSplitterTime = new SPLITTER_TIME_STAMP[NR_OF_SPILTER_TIME_STAMPS];
    private FileStream fsIn;
    private FileStream fsOut;
#if (STAND_ALONE)
        private StreamWriter swLog;
#endif
    private BinaryWriter bwOut;
    private Int64 iSourceBitAddress;

    private const int SEEK_FIFO_SIZE = 10 * 0x800;
    // read a few pack-headers (pack-length 0x800), 3 consecutive timestamps need to be read 

    private const int READ_FIFO_SIZE = 1024 * 1024 * 10;

#if (STAND_ALONE)
        public byte[] ptBuffers = new byte[READ_FIFO_SIZE];
    // made public to do some hacks from outside this class
#else
    private byte[] ptBuffers = new byte[READ_FIFO_SIZE];
#endif

    private Int64 g_iReadCounter = 0;

    private int iPointCounter;

#if (STAND_ALONE)
        private bool bLogEnabled = true;
#endif

    private Timer progressTime;

    public delegate void Finished();

    public event Finished OnFinished;

    public delegate void Progress(int percentage);

    public event Progress OnProgress;

    private int percent = 0;
    private long lBlockRead, lTotalBlockRead; // required to calc progress        
    private TimeSpan TotalTimeRead, TimeReadTemp, TimeRead; // required to calc progress 

    private SplitMode splitMode;

    private TimeSpan DeltaNewTimeStamp;
    private DateTime NewTimeStamp;

    private TimeSpan SeekHysteresis = new TimeSpan(0, 0, 10);
    // Make Hysteresis not to small, the seek algo: big steps forward, small steps backwards

    private const long SeekStepForward = 1024 * 1024 * 20;
    // Parameters are hard dependent on mpeg bit-rate, so need calibration algorithm

    private const long SeekStepBackward = 1024 * 1024 * 5;

    private DateTime zeroTime = new DateTime(1900, 1, 1, 0, 0, 0, 0);

    public Mpeg2Splitter()
    {
      progressTime = new Timer(1000);
      progressTime.Elapsed += new ElapsedEventHandler(progressTime_Elapsed);
    }

    ~Mpeg2Splitter() {}

    private void progressTime_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (splitMode == SplitMode.E_MODE_JOIN)
      {
        percent = (int)(lBlockRead * 100 / lTotalBlockRead);
      }
      else
      {
        percent = (int)((TimeReadTemp.TotalSeconds + TimeRead.TotalSeconds) * 100 / TotalTimeRead.TotalSeconds);
      }
      if (percent > 100)
      {
        percent = 100;
      }
#if (STAND_ALONE)
      //DebugLogging("Percent: " + percent.ToString() + " TimeReadTemp " + TimeReadTemp.TotalSeconds + " TimeRead " + TimeRead.TotalSeconds + " TotalTimeRead " + TotalTimeRead.TotalSeconds);
#endif
      if (OnProgress != null)
      {
        OnProgress(percent);
      }
    }

    private bool OpenInOutFile(string sInFilename, string sOutFilename, FileMode fileMode)
    {
      bool result = true;

      try
      {
        // Create the new, empty data file
#if (STAND_ALONE)
                swLog = new StreamWriter("log.txt");
                InfoLogging("Start log-file version " + iVersion);
#endif
        // Create the reader for data
        fsIn = new FileStream(sInFilename, FileMode.Open, FileAccess.Read);
        if (fsIn == null)
        {
          InfoLogging("Error opening fsIn");
          result = false;
        }
        // Create the new, empty data file
        fsOut = new FileStream(sOutFilename, fileMode, FileAccess.Write);
        if (fsOut == null)
        {
          InfoLogging("Error opening fsOut");
          result = false;
        }
        // Create the writer for data.
        bwOut = new BinaryWriter(fsOut);
        if (bwOut == null)
        {
          InfoLogging("Error opening bwOut");
          result = false;
        }
      }
      catch (Exception e)
      {
        InfoLogging("Exception occured in OpenInOutFile: " + e);
        result = false;
      }
      return result;
    }

    private void CloseInOutFile()
    {
      progressTime.Stop();
      fsIn.Close();
      fsOut.Close();
      bwOut.Close();
#if (STAND_ALONE)
            swLog.Close();
#endif
    }

    public void Split(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tSplitTime) {}

    public void Join(List<FileInfo> fileList, string sOutFilename)
    {
      int i = 0;
      lBlockRead = lTotalBlockRead = 0; // to calc progress

      splitMode = SplitMode.E_MODE_JOIN;

      foreach (FileInfo file in fileList)
      {
        if (OpenInOutFile(file.FullName, sOutFilename, FileMode.Create))
        {
          lTotalBlockRead += (fsIn.Length / READ_FIFO_SIZE);
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
      splitMode = SplitMode.E_MODE_CUT;

      if (OpenInOutFile(sInFilename, sOutFilename, FileMode.Create))
      {
        DateTime timeStart = DateTime.Now;

        iPointCounter = iCounts;
        iSourceBitAddress = 0;

        for (int i = 0; i < iCounts; i++)
        {
          tSplitterTime[i] = tSplitTime[i];
          InfoLogging("Cut-points " + (i + 1) + "/" + iCounts + " start " + tSplitterTime[i].start.ToLongTimeString() +
                      " stop " + tSplitterTime[i].end.ToLongTimeString());
          TotalTimeRead += tSplitTime[i].end.Subtract(tSplitTime[i].start);
        }
        InfoLogging("TotalTime to Cut " + TotalTimeRead + " Seconds " + TotalTimeRead.TotalSeconds);
        SplitProgramStream(splitMode);
        DateTime timeStop = DateTime.Now;
        TimeSpan duration = timeStop.Subtract(timeStart);
        InfoLogging("Duration " + duration);
        CloseInOutFile();
      }
    }

    public void Scene(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP[] tTrimTime, int iCounts)
    {
      splitMode = SplitMode.E_MODE_SCENE;

      TotalTimeRead = new TimeSpan();

      if (OpenInOutFile(sInFilename, sOutFilename, FileMode.Create))
      {
        DateTime timeStart = DateTime.Now;

        iPointCounter = iCounts;
        iSourceBitAddress = 0;

        for (int i = 0; i < iCounts; i++)
        {
          tSplitterTime[i] = tTrimTime[i];
          InfoLogging("Copy-points " + (i + 1) + "/" + iCounts + " start " + tSplitterTime[i].start.ToLongTimeString() +
                      " stop " + tSplitterTime[i].end.ToLongTimeString());
          TotalTimeRead += tTrimTime[i].end.Subtract(tTrimTime[i].start);
        }
        InfoLogging("TotalTime to Copy " + TotalTimeRead + " Seconds " + TotalTimeRead.TotalSeconds);
        SplitProgramStream(splitMode);
        DateTime timeStop = DateTime.Now;
        TimeSpan duration = timeStop.Subtract(timeStart);
        InfoLogging("Duration " + duration);
        CloseInOutFile();
      }
    }

    public void Scene(string sInFilename, string sOutFilename, ref SPLITTER_TIME_STAMP tTrimTime)
    {
      SPLITTER_TIME_STAMP[] tStamp = new SPLITTER_TIME_STAMP[1];
      tStamp[0] = tTrimTime;
      Scene(sInFilename, sOutFilename, ref tStamp, 1);
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
      Int64 iBitAddress = iSourceBitAddress;
      int offset, b;

      while (bits != 0)
      {
        offset = (int)(iBitAddress & 7);
        b = Math.Min(bits, 8 - offset);
        data <<= b;
        data |= ShiftAndMask(ptBuffers[g_iReadCounter + iBitAddress / 8], 7 - offset, b);
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

    private void AdjustTimeStampOffset(int counts, ref DateTime tTimeStampOffset)
    {
      string oldTimeStamp, newTimeStamp;

      TimeSpan Offset = tTimeStampOffset.Subtract(zeroTime);

      oldTimeStamp = "Start Offset correction timestamp " + counts + " old " +
                     tSplitterTime[counts].start.ToLongTimeString();
      newTimeStamp = "End   Offset correction timestamp " + counts + " old " +
                     tSplitterTime[counts].end.ToLongTimeString();

      tSplitterTime[counts].start = tSplitterTime[counts].start.Add(Offset);
      tSplitterTime[counts].end = tSplitterTime[counts].end.Add(Offset);

      oldTimeStamp += " new " + tSplitterTime[counts].start.ToLongTimeString();
      newTimeStamp += " new " + tSplitterTime[counts].end.ToLongTimeString();
      InfoLogging(oldTimeStamp);
      InfoLogging(newTimeStamp);
    }

    private bool CompareTimeStampStartPoint(int counts, ref DateTime tTimeStamp)
    {
      return (tSplitterTime[counts].start == tTimeStamp);
    }

    private bool CompareTimeStampEndPoint(int counts, ref DateTime tTimeStamp)
    {
      return (tSplitterTime[counts].end == tTimeStamp);
    }

    private void CorrectPesTimeStamp(Int64 counter, ref TimeSpan tTimeSpan)
    {
      int pes_base_0;
      int pes_base_1;
      int pes_base_2;
      Int64 old_pes_base, new_pes_base;
      DateTime tTimeStamp;
      double time_in_sec;

      g_iReadCounter = counter;
      iSourceBitAddress = 0;

      // PES timestamp
      //4 + 3 + 1 + 15 + 1 + 15 + 1  =  40/8 = 5 bytes

      // ??? 
      Getbits(4);
      //system_clock_reference_base [32..30]	3
      pes_base_0 = Getbits(3);
      //marker_bit	1
      Getbits(1);
      //system_clock_reference_base [29..15]	15
      pes_base_1 = Getbits(15);
      //marker_bit	1
      Getbits(1);
      //system_clock_reference_base [14..0]	15
      pes_base_2 = Getbits(15);

      old_pes_base = ((long)pes_base_0 << 30) |
                     ((long)pes_base_1 << 15) |
                     (uint)pes_base_2;

      new_pes_base = (Int64)tTimeSpan.TotalSeconds * 90000;

      new_pes_base = old_pes_base - new_pes_base;

      pes_base_0 = (int)((new_pes_base >> 30) & 0x3);
      pes_base_1 = (int)((new_pes_base >> 15) & 0x7FFF);
      pes_base_2 = (int)(new_pes_base & 0x7FFF);

#if (STAND_ALONE)
            DebugLogging("~~~PES1 " + ptBuffers[g_iReadCounter + 0].ToString("X2") + ptBuffers[g_iReadCounter + 1].ToString("X2") + ptBuffers[g_iReadCounter + 2].ToString("X2") + ptBuffers[g_iReadCounter + 3].ToString("X2") + ptBuffers[g_iReadCounter + 4].ToString("X2"));
#endif

      // Clear the values to be changed
      ptBuffers[g_iReadCounter + 0] = (byte)(ptBuffers[g_iReadCounter + 0] & ~0x0E);
      ptBuffers[g_iReadCounter + 1] = (byte)(ptBuffers[g_iReadCounter + 1] & ~0xFF);
      ptBuffers[g_iReadCounter + 2] = (byte)(ptBuffers[g_iReadCounter + 2] & ~0xFE);
      ptBuffers[g_iReadCounter + 3] = (byte)(ptBuffers[g_iReadCounter + 3] & ~0xFF);
      ptBuffers[g_iReadCounter + 4] = (byte)(ptBuffers[g_iReadCounter + 4] & ~0xFE);

      ptBuffers[g_iReadCounter + 0] = (byte)(ptBuffers[g_iReadCounter + 0] | (pes_base_0) << 1);
      ptBuffers[g_iReadCounter + 1] = (byte)(ptBuffers[g_iReadCounter + 1] | (pes_base_1 >> 7) & 0xFF);
      ptBuffers[g_iReadCounter + 2] = (byte)(ptBuffers[g_iReadCounter + 2] | (pes_base_1 & 0x7F) << 1);
      ptBuffers[g_iReadCounter + 3] = (byte)(ptBuffers[g_iReadCounter + 3] | (pes_base_2 >> 7) & 0xFF);
      ptBuffers[g_iReadCounter + 4] = (byte)(ptBuffers[g_iReadCounter + 4] | (pes_base_2 & 0x7F) << 1);

      time_in_sec = (Int64)new_pes_base;
      time_in_sec = time_in_sec / (Int64)90000;

      tTimeStamp = zeroTime;
      tTimeStamp = tTimeStamp.AddSeconds((int)time_in_sec);

#if (STAND_ALONE)
            DebugLogging("~~~PES2 " + ptBuffers[g_iReadCounter + 0].ToString("X2") + ptBuffers[g_iReadCounter + 1].ToString("X2") + ptBuffers[g_iReadCounter + 2].ToString("X2") + ptBuffers[g_iReadCounter + 3].ToString("X2") + ptBuffers[g_iReadCounter + 4].ToString("X2") +
                                " - " + old_pes_base + " ==> " + pes_base_0 + " " + pes_base_1 + " " + pes_base_2 + " = " + new_pes_base + " offset " + tTimeSpan.ToString() + " New TimeStamp " + tTimeStamp.ToLongTimeString());
#endif
    }

    private void CorrectDtsTimeStamp(Int64 counter, ref TimeSpan tTimeSpan)
    {
      CorrectPesTimeStamp(counter, ref tTimeSpan);
    }

    private void CorrectPtsTimeStamp(Int64 counter, ref TimeSpan tTimeSpan)
    {
      CorrectPesTimeStamp(counter, ref tTimeSpan);
    }

    private void CorrectScrTimeStamp(Int64 counter, ref TimeSpan tTimeSpan)
    {
#if (ENABLE_SCR_CORRECT_TIME_STAMP)
      int system_clock_reference_base_0;
      int system_clock_reference_base_1;
      int system_clock_reference_base_2;
      Int64 old_scr_base, new_scr_base;
      Int64 x;
      DateTime tTimeStamp;
      double time_in_sec;

      g_iReadCounter = counter;
      iSourceBitAddress = 0;

      // Pack header length
      //32 + 2 + 3 + 1 + 15 + 1 + 15 + 1 + 9 + 1 + 22 +  1 + 1 + 5 + 3  =  112/8 = 14 bytes

      // pack header start code 
      x = Getbits(32);
      //'01'	2 say the layer - here it is mpeg2, 0x2 means mpeg1
      x = Getbits(2);
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

      old_scr_base = ((long)system_clock_reference_base_0 << 30) |
                     ((long)system_clock_reference_base_1 << 15) |
                     (uint)system_clock_reference_base_2;

      new_scr_base = (Int64)tTimeSpan.TotalSeconds * 27000000 / 300;

      new_scr_base = old_scr_base - new_scr_base;

      system_clock_reference_base_0 = (int)((new_scr_base >> 30) & 0x3);
      system_clock_reference_base_1 = (int)((new_scr_base >> 15) & 0x7FFF);
      system_clock_reference_base_2 = (int)(new_scr_base & 0x7FFF);

      // Clear the values to be changed
      ptBuffers[g_iReadCounter + 4] = (byte)(ptBuffers[g_iReadCounter + 4] & ~0x3B);
      ptBuffers[g_iReadCounter + 5] = (byte)(ptBuffers[g_iReadCounter + 5] & ~0xFF);
      ptBuffers[g_iReadCounter + 6] = (byte)(ptBuffers[g_iReadCounter + 6] & ~0xFB);
      ptBuffers[g_iReadCounter + 7] = (byte)(ptBuffers[g_iReadCounter + 7] & ~0xFF);
      ptBuffers[g_iReadCounter + 8] = (byte)(ptBuffers[g_iReadCounter + 8] & ~0xF8);

      ptBuffers[g_iReadCounter + 4] =
        (byte)
        (ptBuffers[g_iReadCounter + 4] |
         ((system_clock_reference_base_0) << 3) + ((system_clock_reference_base_1 >> 13) & 0x03));
      ptBuffers[g_iReadCounter + 5] =
        (byte)(ptBuffers[g_iReadCounter + 5] | ((system_clock_reference_base_1 >> 5) & 0xFF));
      ptBuffers[g_iReadCounter + 6] =
        (byte)
        (ptBuffers[g_iReadCounter + 6] |
         (((system_clock_reference_base_1 & 0x1F) << 3) + ((system_clock_reference_base_2 >> 13) & 0x03)));
      ptBuffers[g_iReadCounter + 7] =
        (byte)(ptBuffers[g_iReadCounter + 7] | ((system_clock_reference_base_2 >> 5) & 0xFF));
      ptBuffers[g_iReadCounter + 8] =
        (byte)(ptBuffers[g_iReadCounter + 8] | ((system_clock_reference_base_2 & 0x1F) << 3));

      time_in_sec = (Int64)new_scr_base * 300;
      time_in_sec = time_in_sec / (Int64)27000000;

      tTimeStamp = zeroTime;
      tTimeStamp = tTimeStamp.AddSeconds((int)time_in_sec);

#if (STAND_ALONE)
      //DebugLogging("$$$SCR " + ptBuffers[g_iReadCounter + 4].ToString("X2") + ptBuffers[g_iReadCounter + 5].ToString("X2") + ptBuffers[g_iReadCounter + 6].ToString("X2") + ptBuffers[g_iReadCounter + 7].ToString("X2") + ptBuffers[g_iReadCounter + 8].ToString("X2") + ptBuffers[g_iReadCounter + 9].ToString("X2") +
      //                    " - " + system_clock_reference_base_0 + " " + system_clock_reference_base_1 + " " + system_clock_reference_base_2 + " = " + new_scr_base + " offset " + tTimeSpan.ToString() + " New TimeStamp " + tTimeStamp.ToLongTimeString() );
#endif
#endif
    }

#if (STAND_ALONE)
        public bool GetScrTimeStamp(Int64 counter, ref DateTime tTimeStamp)
#else
    private bool GetScrTimeStamp(Int64 counter, ref DateTime tTimeStamp)
#endif
    {
      int system_clock_reference_base_0;
      int system_clock_reference_base_1;
      int system_clock_reference_base_2;
      Int64 scr_base;
      double time_in_sec;
      Int64 x;

      g_iReadCounter = counter;
      iSourceBitAddress = 0;

      // Pack header length
      //32 + 2 + 3 + 1 + 15 + 1 + 15 + 1 + 9 + 1 + 22 +  1 + 1 + 5 + 3  =  112/8 = 14 bytes

      // pack header start code 
      x = Getbits(32);
      //'01'	2 say the layer - here it is mpeg2, 0x2 means mpeg1
      x = Getbits(2);
      if (x != 0x1)
      {
        DebugLogging("Warning : MPEG2 bit is not signaled");
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

      time_in_sec = (Int64)scr_base * 300;
      time_in_sec = time_in_sec / (Int64)27000000;

      tTimeStamp = zeroTime;
      tTimeStamp = tTimeStamp.AddSeconds((int)time_in_sec);

#if (STAND_ALONE)
            DebugLogging("###SCR " + ptBuffers[g_iReadCounter + 4].ToString("X2") + ptBuffers[g_iReadCounter + 5].ToString("X2") + ptBuffers[g_iReadCounter + 6].ToString("X2") + ptBuffers[g_iReadCounter + 7].ToString("X2") + ptBuffers[g_iReadCounter + 8].ToString("X2") + ptBuffers[g_iReadCounter + 9].ToString("X2") +
                                " - " + system_clock_reference_base_0 + " " + system_clock_reference_base_1 + " " + system_clock_reference_base_2 + " = " + scr_base + " " + time_in_sec + " s TimeStamp " + tTimeStamp.ToLongTimeString());
#endif
/* Not Required
            Getbits(1);
            Getbits(9);  // system clock reference extension
            Getbits(1);
            Getbits(22); // program mux rate 
            Getbits(7);
            // stuffing
            x = Getbits(3);
            for (int i = 0; i < x; i++)
            {
                //stuffing_byte	8
                Getbits(8);
            }
*/
      return true;
    }

    private void InfoLogging(string sLog)
    {
#if (STAND_ALONE)
            swLog.WriteLine(sLog);
#else
      Log.Info(sLog);
#endif
    }

    private void DebugLogging(string sLog)
    {
#if (STAND_ALONE)
            if (bLogEnabled)
            {
                swLog.WriteLine(sLog);
            }
#else
      Log.Debug(sLog);
#endif
    }

    private bool SplitProgramStream(SplitMode splitMode)
    {
      int iLeftBufferSize = 0;
      int iBufferSize = 0;
      Int64 x = 0;
      int c_times = 0;
      Int64 iReadCounter = 0;
      Int64 iWriteCounter = 0;
      long lFileSizeSaved;
      long lFilePosition;
      bool bAdjustedTimeStampOffset = false;
      DateTime tStamp = new DateTime();
#if (STAND_ALONE)
            DateTime tPreviousStamp;
#endif
      int iValidTargetCounter;

      InfoLogging("Start SplitProgramStream " + splitMode);

      progressTime.Start();

      lFileSizeSaved = fsIn.Length;
#if (STAND_ALONE)
            tPreviousStamp = new DateTime(1900, 1, 1, 23, 59, 59, 0);
      // make the TimeStamp invalid so that the first timestamp is always printed
#endif
      NewTimeStamp = zeroTime;

      while (c_times < iPointCounter)
      {
        if (splitMode == SplitMode.E_MODE_SCENE)
        {
          DeltaNewTimeStamp = tSplitterTime[c_times].start.Subtract(NewTimeStamp);
        }

        #region Fill the buffer

        if (iLeftBufferSize == 0)
        {
          if (splitMode == SplitMode.E_MODE_CUT)
          {
            fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
          }
          iBufferSize = iLeftBufferSize = (int)Math.Min(lFileSizeSaved, SEEK_FIFO_SIZE);
          if (fsIn.Read(ptBuffers, 0, iBufferSize) != iBufferSize)
          {
            return false; //ERROR_READING_FROM_INPUT_FILE
          }
          lFileSizeSaved -= iBufferSize;
          iWriteCounter = iReadCounter = 0;
        }

        #endregion

        #region FastFast Seek to the Start-Point

#if ENABLE_FAST_SEEK
        if ((splitMode == SplitMode.E_MODE_SCENE) && (tSplitterTime[c_times].start != zeroTime))
        {
          iValidTargetCounter = 0;
          while (iLeftBufferSize != 0)
          {
            if (iLeftBufferSize >= READ_BUF_INCR) // prevent out-of-bound
            {
              x = (ptBuffers[iReadCounter + 0] << 24) + (ptBuffers[iReadCounter + 1] << 16) +
                  (ptBuffers[iReadCounter + 2] << 8) + (ptBuffers[iReadCounter + 3] << 0);
              //*(DWORD *)(ptBuffers + counter); 
              if (x == PACKET_HEADER_START_CODE)
              {
                GetScrTimeStamp(iReadCounter, ref tStamp);
                if (!bAdjustedTimeStampOffset)
                {
                  // use the first time stamp to see what is the zero time, and wait untill we for the first time see a correct pack-header                            
                  // why ? some mpeg2 could have a start timestamp different from zero
                  // and it could be that the mpeg file not nicely starts with a pack-header
                  for (int t = 0; t < iPointCounter; t++)
                  {
                    AdjustTimeStampOffset(t, ref tStamp);
                  }
                  bAdjustedTimeStampOffset = true;
                }
                if ((tSplitterTime[c_times].start.Subtract(SeekHysteresis) < tStamp) &&
                    (tStamp < tSplitterTime[c_times].start))
                {
                  iValidTargetCounter++;
                  DebugLogging("Valid Target Found: " + iValidTargetCounter + " iLeftBufferSize " + iLeftBufferSize +
                               " " + tStamp.ToLongTimeString());
                  if (iValidTargetCounter >= 3)
                  {
                    break;
                  }
                  iReadCounter += PACKET_HEADER_INCR;
                  iLeftBufferSize -= PACKET_HEADER_INCR;
                }
                else
                {
                  iBufferSize = iLeftBufferSize = (int)Math.Min(lFileSizeSaved, SEEK_FIFO_SIZE);
                  if (tStamp < tSplitterTime[c_times].start)
                  {
                    if ((fsIn.Position + SeekStepForward) < fsIn.Length)
                    {
                      lFilePosition = fsIn.Seek(SeekStepForward, SeekOrigin.Current);
                    }
                    else
                    {
                      lFilePosition = fsIn.Seek(-SeekStepBackward, SeekOrigin.End);
                    }
                    DebugLogging("Seek forward");
                  }
                  else
                  {
                    if ((fsIn.Position - SeekStepBackward) > 0)
                    {
                      lFilePosition = fsIn.Seek(-SeekStepBackward, SeekOrigin.Current);
                    }
                    else
                    {
                      lFilePosition = fsIn.Seek(0, SeekOrigin.Begin);
                    }
                    DebugLogging("Seek backward");
                  }
                  if (fsIn.Read(ptBuffers, 0, iBufferSize) != iBufferSize)
                  {
                    return false; //ERROR_READING_FROM_INPUT_FILE
                  }
                  lFileSizeSaved = fsIn.Length - fsIn.Position;
                  DebugLogging("TimeStamp: " + tStamp.ToLongTimeString() + " lFileSizeSaved: " + lFileSizeSaved);
                  iWriteCounter = iReadCounter = 0;
                }
              }
              else
              {
                iReadCounter += 1;
                iLeftBufferSize--;
              }
            }
            else
            {
              InfoLogging("Seek-data-buffer empty, quit splitting... (bad mpeg ???)");
              return false;
            }
          }
        }
#endif

        #endregion

        #region Handling Start-point, cut and scene method

        // Untill we found the start point we need to write the file into the output
        while (iLeftBufferSize != 0)
        {
          if (iLeftBufferSize >= READ_BUF_INCR) // prevent out-of-bound
          {
            x = (ptBuffers[iReadCounter + 0] << 24) + (ptBuffers[iReadCounter + 1] << 16) +
                (ptBuffers[iReadCounter + 2] << 8) + (ptBuffers[iReadCounter + 3] << 0); //Big-endian
            if (x == MPEG_PROGRAM_END_CODE) // it terminates the Program Stream
            {
              DebugLogging("End-of-program stream detected " + tStamp.ToLongTimeString()); //return true; 				
            }
            if (x == PACKET_HEADER_START_CODE) // start of pack header
            {
              GetScrTimeStamp(iReadCounter, ref tStamp);
              if (!bAdjustedTimeStampOffset)
              {
                // use the first time stamp to see what is the zero time, and wait untill we for the first time see a correct pack-header                            
                // why ? some mpeg2 could have a start timestamp different from zero
                // and it could be that the mpeg file not nicely starts with a pack-header
                for (int t = 0; t < iPointCounter; t++)
                {
                  AdjustTimeStampOffset(t, ref tStamp);
                }
                bAdjustedTimeStampOffset = true;
              }
              CorrectScrTimeStamp(iReadCounter, ref DeltaNewTimeStamp);
              NewTimeStamp = tStamp.Subtract(DeltaNewTimeStamp);
              iReadCounter += PACKET_HEADER_INCR;
              iLeftBufferSize -= PACKET_HEADER_INCR;
              if (splitMode == SplitMode.E_MODE_CUT)
              {
                TimeReadTemp = tSplitterTime[c_times].start.Subtract(tStamp);
              }
#if (STAND_ALONE)
                            if (tStamp != tPreviousStamp)
                            {
                                DebugLogging("Searching Start-point " + tSplitterTime[c_times].start.ToLongTimeString() + " Current Time " + tStamp.ToLongTimeString() + " New Time " + tStamp.Subtract(DeltaNewTimeStamp).ToLongTimeString());
                            }
                            tPreviousStamp = tStamp;
#endif
              if (CompareTimeStampStartPoint(c_times, ref tStamp))
              {
                if (splitMode == SplitMode.E_MODE_SCENE)
                {
                  iWriteCounter = iReadCounter - PACKET_HEADER_INCR;
                  // From here we start writing, write only the pack-header
                }
                else
                {
                  iReadCounter -= PACKET_HEADER_INCR;
                }
                fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                iWriteCounter = iReadCounter;
                DebugLogging("Start-point found Time " + tStamp.ToLongTimeString());
                break;
              }
              else
              {
                if (splitMode == SplitMode.E_MODE_CUT)
                {
                  fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                  iWriteCounter = iReadCounter;
                }
              }
            }
#if (ENABLE_PTS_DTS_CORRECT_TIME_STAMP)
            else if (((x >= PACKET_START_CODE_AUDIO_BEGIN) && (x <= PACKET_START_CODE_AUDIO_END)) ||
                     ((x >= PACKET_START_CODE_VIDEO_BEGIN) && (x <= PACKET_START_CODE_VIDEO_END)))
            {
#if (STAND_ALONE)
              //DebugLogging("Code " + ptBuffers[iReadCounter + 0].ToString("X2") + " " + ptBuffers[iReadCounter + 1].ToString("X2") + " " + ptBuffers[iReadCounter + 2].ToString("X2") + " " + ptBuffers[iReadCounter + 3].ToString("X2") + " - " + ptBuffers[iReadCounter + 4].ToString("X2")
              //             + " " + ptBuffers[iReadCounter + 5].ToString("X2") + " " + ptBuffers[iReadCounter + 6].ToString("X2") + " " + ptBuffers[iReadCounter + 7].ToString("X2") + " " + ptBuffers[iReadCounter + 8].ToString("X2") + " - " + ptBuffers[iReadCounter + 9].ToString("X2")
              //             + " " + ptBuffers[iReadCounter + 10].ToString("X2") + " " + ptBuffers[iReadCounter + 11].ToString("X2") + " " + ptBuffers[iReadCounter + 12].ToString("X2") + " " + ptBuffers[iReadCounter + 13].ToString("X2") + " " + ptBuffers[iReadCounter + 14].ToString("X2")
              //             + " " + ptBuffers[iReadCounter + 15].ToString("X2") + " " + ptBuffers[iReadCounter + 16].ToString("X2") + " " + ptBuffers[iReadCounter + 17].ToString("X2") + " " + ptBuffers[iReadCounter + 18].ToString("X2") + " " + ptBuffers[iReadCounter + 19].ToString("X2"));
#endif
              PtsDtsFlags ePtsDtsFlag = (PtsDtsFlags)((ptBuffers[iReadCounter + 7] >> 6) & 0x03);
              int iEScrFlag = (ptBuffers[iReadCounter + 7] >> 5) & 0x01;
              if (iEScrFlag == 1)
              {
                DebugLogging("iEScrFlag detected");
              }
#if (STAND_ALONE)
                            DebugLogging("PtsDtsFlags " + ePtsDtsFlag);
#endif
              if (ePtsDtsFlag == PtsDtsFlags.E_FLAGS_PTS_ONLY) // DTS information only
              {
                CorrectPtsTimeStamp(iReadCounter + 9, ref DeltaNewTimeStamp);
                iReadCounter += PES_HEADER_INCR;
                iLeftBufferSize -= PES_HEADER_INCR;
              }
              else if (ePtsDtsFlag == PtsDtsFlags.E_FLAGS_PTS_DTS) // PTS and DTS information
              {
                CorrectPtsTimeStamp(iReadCounter + 9, ref DeltaNewTimeStamp);
                CorrectDtsTimeStamp(iReadCounter + 14, ref DeltaNewTimeStamp);
                iReadCounter += PES_HEADER_INCR;
                iLeftBufferSize -= PES_HEADER_INCR;
              }
              else
              {
                iReadCounter += 1;
                iLeftBufferSize--;
              }
            }
#endif
            else
            {
              iReadCounter += 1;
              iLeftBufferSize--;
            }
          }
          if (iLeftBufferSize < READ_BUF_INCR) // Buffer ends in the middle of the READ_BUF_INCR size
          {
            if (splitMode == SplitMode.E_MODE_CUT)
            {
              fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
            }
            DebugLogging("Buffer ends in the middle of the READ_BUF_INCR, size left " + iLeftBufferSize);
            // Copy the unhandled data from the end to the beginning of the buffer, and then append new data
            for (int i = 0; i < iLeftBufferSize; i++)
            {
              ptBuffers[i] = ptBuffers[iReadCounter + i];
            }
            iBufferSize = (int)Math.Min(lFileSizeSaved, READ_FIFO_SIZE);
            if (iBufferSize == READ_FIFO_SIZE) // prevent out-of-bound
            {
              iBufferSize -= iLeftBufferSize; // iLeftBufferSize is the unhandled data
            }
            if (fsIn.Read(ptBuffers, iLeftBufferSize, iBufferSize) != iBufferSize)
            {
              return false; //ERROR_READING_FROM_INPUT_FILE
            }
            iLeftBufferSize += iBufferSize; // Calc the size of the buffer, which is normally FIFO_SIZE
            lFileSizeSaved -= iBufferSize;
            DebugLogging("lFileSizeSaved1 " + lFileSizeSaved);
            iWriteCounter = iReadCounter = 0;
          }
        }

        #endregion

        #region Refill buffer

        if (iLeftBufferSize == 0)
        {
          // In Cut mode we don't need to write the buffer, and in scene mode the buffer is already written
          iBufferSize = iLeftBufferSize = (int)Math.Min(lFileSizeSaved, READ_FIFO_SIZE);
          if (fsIn.Read(ptBuffers, 0, iBufferSize) != iBufferSize)
          {
            return false; //ERROR_READING_FROM_INPUT_FILE
          }
          lFileSizeSaved -= iBufferSize;
          DebugLogging("lFileSizeSaved2 " + lFileSizeSaved);
          iWriteCounter = iReadCounter = 0;
        }

        #endregion

        #region FastFast Seek to the End-Point

#if ENABLE_FAST_SEEK
        if (splitMode == SplitMode.E_MODE_CUT)
        {
          iValidTargetCounter = 0;
          while (iLeftBufferSize != 0)
          {
            if (iLeftBufferSize >= READ_BUF_INCR) // prevent out-of-bound
            {
              x = (ptBuffers[iReadCounter + 0] << 24) + (ptBuffers[iReadCounter + 1] << 16) +
                  (ptBuffers[iReadCounter + 2] << 8) + (ptBuffers[iReadCounter + 3] << 0);
              //*(DWORD *)(ptBuffers + counter); 
              if (x == PACKET_HEADER_START_CODE) // start of pack header
              {
                GetScrTimeStamp(iReadCounter, ref tStamp);

                if ((tSplitterTime[c_times].end.Subtract(SeekHysteresis) < tStamp) &&
                    (tStamp < tSplitterTime[c_times].end))
                {
                  iValidTargetCounter++;
                  DebugLogging("Valid Target Found: " + iValidTargetCounter + " iLeftBufferSize " + iLeftBufferSize);
                  if (iValidTargetCounter >= 3)
                  {
                    break;
                  }
                  iReadCounter += PACKET_HEADER_INCR;
                  iLeftBufferSize -= PACKET_HEADER_INCR;
                }
                else
                {
                  iBufferSize = iLeftBufferSize = (int)Math.Min(lFileSizeSaved, SEEK_FIFO_SIZE);
                  if (tStamp < tSplitterTime[c_times].start)
                  {
                    if ((fsIn.Position + SeekStepForward) < fsIn.Length)
                    {
                      lFilePosition = fsIn.Seek(SeekStepForward, SeekOrigin.Current);
                    }
                    else
                    {
                      lFilePosition = fsIn.Seek(-SeekStepBackward, SeekOrigin.End);
                    }
                    DebugLogging("Seek forward");
                  }
                  else
                  {
                    if ((fsIn.Position - SeekStepBackward) > 0)
                    {
                      lFilePosition = fsIn.Seek(-SeekStepBackward, SeekOrigin.Current);
                    }
                    else
                    {
                      lFilePosition = fsIn.Seek(0, SeekOrigin.Begin);
                    }
                    DebugLogging("Seek backward");
                  }
                  if (fsIn.Read(ptBuffers, 0, iBufferSize) != iBufferSize)
                  {
                    return false; //ERROR_READING_FROM_INPUT_FILE
                  }
                  lFileSizeSaved = fsIn.Length - fsIn.Position;
                  DebugLogging("TimeStamp: " + tStamp.ToLongTimeString() + " lFileSizeSaved: " + lFileSizeSaved);
                  iWriteCounter = iReadCounter = 0;
                }
              }
              else
              {
                iReadCounter += 1;
                iLeftBufferSize--;
              }
            }
            else
            {
              InfoLogging("Seek-data-buffer empty, quit splitting... (bad mpeg ???)");
              return false;
            }
          }
        }
#endif

        #endregion

        #region Handling End-point, cut and scene method

        // We found the start point a we skip the input till we found the end point
        while (iLeftBufferSize != 0)
        {
          if (iLeftBufferSize >= READ_BUF_INCR) // prevent out-of-bound
          {
            x = (ptBuffers[iReadCounter + 0] << 24) + (ptBuffers[iReadCounter + 1] << 16) +
                (ptBuffers[iReadCounter + 2] << 8) + (ptBuffers[iReadCounter + 3] << 0); //Big-endian
            if (x == MPEG_PROGRAM_END_CODE) // it terminates the Program Stream
            {
              DebugLogging("End-of-program stream detected " + tStamp.ToLongTimeString()); //return true; 				
            }
            if (x == PACKET_HEADER_START_CODE) // start of pack header
            {
              GetScrTimeStamp(iReadCounter, ref tStamp);
              CorrectScrTimeStamp(iReadCounter, ref DeltaNewTimeStamp);
              NewTimeStamp = tStamp.Subtract(DeltaNewTimeStamp);
              iReadCounter += PACKET_HEADER_INCR;
              iLeftBufferSize -= PACKET_HEADER_INCR;
              if (splitMode == SplitMode.E_MODE_SCENE)
              {
                TimeReadTemp = tStamp.Subtract(tSplitterTime[c_times].start);
              }
#if (STAND_ALONE)
                            if (tStamp != tPreviousStamp)
                            {
                                DebugLogging("Searching End-point " + tSplitterTime[c_times].end.ToLongTimeString() + " Current Time " + tStamp.ToLongTimeString() + " New Time " + tStamp.Subtract(DeltaNewTimeStamp).ToLongTimeString());
                            }
                            tPreviousStamp = tStamp;
#endif
              if (CompareTimeStampEndPoint(c_times, ref tStamp))
              {
                if (splitMode == SplitMode.E_MODE_CUT)
                {
                  iWriteCounter = iReadCounter - PACKET_HEADER_INCR;
                  // From here we start writing, write only the pack-header
                }
                else
                {
                  iReadCounter -= PACKET_HEADER_INCR;
                }
                fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                iWriteCounter = iReadCounter;
                DebugLogging("End-point found Time " + tStamp.ToLongTimeString());
                break;
              }
              else
              {
                if (splitMode == SplitMode.E_MODE_SCENE)
                {
                  fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
                  iWriteCounter = iReadCounter;
                }
              }
            }
#if (ENABLE_PTS_DTS_CORRECT_TIME_STAMP)
            else if (((x >= PACKET_START_CODE_AUDIO_BEGIN) && (x <= PACKET_START_CODE_AUDIO_END)) ||
                     ((x >= PACKET_START_CODE_VIDEO_BEGIN) && (x <= PACKET_START_CODE_VIDEO_END)))
            {
#if (STAND_ALONE)
              //DebugLogging("Code " + ptBuffers[iReadCounter + 0].ToString("X2") + " " + ptBuffers[iReadCounter + 1].ToString("X2") + " " + ptBuffers[iReadCounter + 2].ToString("X2") + " " + ptBuffers[iReadCounter + 3].ToString("X2") + " - " + ptBuffers[iReadCounter + 4].ToString("X2")
              //             + " " + ptBuffers[iReadCounter + 5].ToString("X2") + " " + ptBuffers[iReadCounter + 6].ToString("X2") + " " + ptBuffers[iReadCounter + 7].ToString("X2") + " " + ptBuffers[iReadCounter + 8].ToString("X2") + " - " + ptBuffers[iReadCounter + 9].ToString("X2")
              //             + " " + ptBuffers[iReadCounter + 10].ToString("X2") + " " + ptBuffers[iReadCounter + 11].ToString("X2") + " " + ptBuffers[iReadCounter + 12].ToString("X2") + " " + ptBuffers[iReadCounter + 13].ToString("X2") + " " + ptBuffers[iReadCounter + 14].ToString("X2")
              //             + " " + ptBuffers[iReadCounter + 15].ToString("X2") + " " + ptBuffers[iReadCounter + 16].ToString("X2") + " " + ptBuffers[iReadCounter + 17].ToString("X2") + " " + ptBuffers[iReadCounter + 18].ToString("X2") + " " + ptBuffers[iReadCounter + 19].ToString("X2"));
#endif
              PtsDtsFlags ePtsDtsFlag = (PtsDtsFlags)((ptBuffers[iReadCounter + 7] >> 6) & 0x03);
              int iEScrFlag = (ptBuffers[iReadCounter + 7] >> 5) & 0x01;
              if (iEScrFlag == 1)
              {
                DebugLogging("iEScrFlag detected");
              }

              if (ePtsDtsFlag == PtsDtsFlags.E_FLAGS_PTS_ONLY) // DTS information only
              {
                CorrectPtsTimeStamp(iReadCounter + 9, ref DeltaNewTimeStamp);
                iReadCounter += PES_HEADER_INCR;
                iLeftBufferSize -= PES_HEADER_INCR;
              }
              else if (ePtsDtsFlag == PtsDtsFlags.E_FLAGS_PTS_DTS) // PTS and DTS information
              {
                CorrectPtsTimeStamp(iReadCounter + 9, ref DeltaNewTimeStamp);
                CorrectDtsTimeStamp(iReadCounter + 14, ref DeltaNewTimeStamp);
                iReadCounter += PES_HEADER_INCR;
                iLeftBufferSize -= PES_HEADER_INCR;
              }
              else
              {
                iReadCounter += 1;
                iLeftBufferSize--;
              }
            }
#endif
            else
            {
              iReadCounter += 1;
              iLeftBufferSize--;
            }
          }
          if (iLeftBufferSize < READ_BUF_INCR)
          {
            if (splitMode == SplitMode.E_MODE_SCENE)
            {
              fsOut.Write(ptBuffers, (int)iWriteCounter, (int)(iReadCounter - iWriteCounter));
            }
            DebugLogging("Buffer ends in the middle of the READ_BUF_INCR, size left " + iLeftBufferSize);
            // Copy the unhandled data from the end to the beginning of the buffer, and then append new data
            for (int i = 0; i < iLeftBufferSize; i++)
            {
              ptBuffers[i] = ptBuffers[iReadCounter + i];
            }
            iBufferSize = (int)Math.Min(lFileSizeSaved, READ_FIFO_SIZE);
            if (iBufferSize == READ_FIFO_SIZE) // prevent out-of-bound
            {
              iBufferSize -= iLeftBufferSize; // iLeftBufferSize is the unhandled data
            }
            if (fsIn.Read(ptBuffers, iLeftBufferSize, iBufferSize) != iBufferSize)
            {
              return false; //ERROR_READING_FROM_INPUT_FILE
            }
            iLeftBufferSize += iBufferSize; // Calc the size of the buffer, which is normally FIFO_SIZE
            lFileSizeSaved -= iBufferSize;
            DebugLogging("lFileSizeSaved3 " + lFileSizeSaved);
            iWriteCounter = iReadCounter = 0;
          }
        }

        #endregion

        if (splitMode == SplitMode.E_MODE_SCENE)
        {
          TimeReadTemp = new TimeSpan(0, 0, 0);
          TimeRead += tStamp.Subtract(tSplitterTime[c_times].start);
        }
        if (splitMode == SplitMode.E_MODE_CUT)
        {
          DeltaNewTimeStamp = tSplitterTime[c_times].end.Subtract(NewTimeStamp);
        }

        c_times++;
      }
      if (splitMode == SplitMode.E_MODE_CUT)
      {
        /* now copy the entire file to output */
        fsOut.Write(ptBuffers, (int)iReadCounter, iLeftBufferSize);
        while (lFileSizeSaved != 0)
        {
          iBufferSize = (int)Math.Min(lFileSizeSaved, READ_FIFO_SIZE);
          lFileSizeSaved -= iBufferSize;
          if (fsIn.Read(ptBuffers, 0, iBufferSize) != iBufferSize)
          {
            return false; //ERROR_READING_FROM_INPUT_FILE
          }
          fsOut.Write(ptBuffers, 0, iBufferSize);
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
      int iBufferSize;
      long lFileSizeSaved;

      InfoLogging("JoinProgramStream");

      progressTime.Start();

      lFileSizeSaved = fsIn.Length;

      while (lFileSizeSaved != 0)
      {
        iBufferSize = (int)Math.Min(lFileSizeSaved, READ_FIFO_SIZE);
        lFileSizeSaved -= iBufferSize;
        if (fsIn.Read(ptBuffers, 0, iBufferSize) != iBufferSize)
        {
          return false; //ERROR_READING_FROM_INPUT_FILE
        }
        else
        {
          lBlockRead++; // to calc progress
        }
        fsOut.Write(ptBuffers, 0, iBufferSize);
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