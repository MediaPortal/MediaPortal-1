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

#region Usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace TvDatabase
{
  /// <summary>
  /// Holds program list partition info. A partition is a contiguous series of programs on the same channel
  /// </summary>
  public struct ProgramListPartition
  {
    public int IdChannel;
    public DateTime Start;
    public DateTime End;

    public ProgramListPartition(int idChannel, DateTime start, DateTime end)
    {
      IdChannel = idChannel;
      Start = start;
      End = end;
    }
  }

  public class ProgramList
    : List<Program>, IComparer<Program>
  {
    #region variables

    private bool _alreadySorted;

    #endregion

    #region ctor

    public ProgramList()
    {
      //_alreadySorted = false;
    }

    public ProgramList(int capacity)
      : base(capacity)
    {
      //_alreadySorted = false;
    }

    public ProgramList(IList<Program> source)
      : base(source)
    {
      //_alreadySorted = false;
    }

    #endregion

    #region public properties

    /// <summary>
    /// Indicates whether the <see cref="ProgramList"/> is already sorted or not.
    /// </summary>
    /// <remarks>
    /// When this property is <value>true</value> methods that 
    /// need the list to be sorted, will assume it is already sorted
    /// and will not sort it again
    /// </remarks>
    public bool AlreadySorted
    {
      get { return _alreadySorted; }
      set { _alreadySorted = value; }
    }

    #endregion

    #region public methods

    public void SortIfNeeded()
    {
      if (!_alreadySorted)
      {
        Sort(this);
      }
    }

    /// <summary>
    /// Removes overlappin programs withing the list
    /// </summary>
    /// <remarks>
    /// This method will try to remove programs without leaving gaps in the list.
    /// </remarks>
    public void RemoveOverlappingPrograms()
    {
      if (Count == 0) return;
      SortIfNeeded();
      Program prevProg = this[0];
      for (int i = 1; i < Count; i++)
      {
        Program newProg = this[i];
        if (newProg.StartTime < prevProg.EndTime) // we have an overlap here
        {
          // let us find out which one is the correct one
          if (newProg.StartTime > prevProg.StartTime) // newProg will create hole -> delete it
          {
            Remove(newProg);
            i--; // stay at the same position
            continue;
          }

          List<Program> prevList = new List<Program>();
          List<Program> newList = new List<Program>();
          prevList.Add(prevProg);
          newList.Add(newProg);
          Program syncPrev = prevProg;
          Program syncProg = newProg;
          for (int j = i + 1; j < Count; j++)
          {
            Program syncNew = this[j];
            if (syncPrev.EndTime == syncNew.StartTime)
            {
              prevList.Add(syncNew);
              syncPrev = syncNew;
              if (syncNew.StartTime > syncProg.EndTime)
              {
                // stop point reached => delete Programs in newList
                foreach (Program prog in newList) Remove(prog);
                i = j - 1;
                prevProg = syncPrev;
                newList.Clear();
                prevList.Clear();
                break;
              }
            }
            else if (syncProg.EndTime == syncNew.StartTime)
            {
              newList.Add(syncNew);
              syncProg = syncNew;
              if (syncNew.StartTime > syncPrev.EndTime)
              {
                // stop point reached => delete Programs in prevList
                foreach (Program prog in prevList) Remove(prog);
                i = j - 1;
                prevProg = syncProg;
                newList.Clear();
                prevList.Clear();
                break;
              }
            }
          }
          // check if a stop point was reached => if not delete newList
          if (newList.Count > 0)
          {
            foreach (Program prog in prevList) Remove(prog);
            i = Count;
            break;
          }
        }
        prevProg = newProg;
      }
    }

    /// <summary>
    /// Removes programs from the list that overlap programs in <paramref name="existingPrograms"/>.
    /// </summary>
    /// <param name="existingPrograms">a list of programs to check against</param>
    /// <remarks>
    /// The list will be sorted if needed according to <seealso cref="AlreadySorted"/> property.
    /// The <paramref name="existingPrograms"/> list is assumed unsorted and will be sorted.
    /// Both list may contain programs from multiple channels.
    /// Programs are assumed to not overlap within each list.
    /// </remarks>
    public void RemoveOverlappingPrograms(List<Program> existingPrograms)
    {
      RemoveOverlappingPrograms(existingPrograms, false);
    }

    /// <summary>
    /// Removes programs from the list that overlap programs in <paramref name="existingPrograms"/>.
    /// </summary>
    /// <param name="existingPrograms">a list of programs to check against</param>
    /// <param name="alreadySorted">if <value>true</value> <paramref name="existsinPrograms"/> is assumed to be sorted</param>
    /// <remarks>
    /// The list will be sorted if needed according to <seealso cref="AlreadySorted"/> property.
    /// The <paramref name="existingPrograms"/> list will be sorted if <paramref name="alreadySorted"/> is <value>false</value>.
    /// Both list may contain programs from multiple channels.
    /// Programs are assumed to not overlap within each list.
    /// </remarks>
    public void RemoveOverlappingPrograms(List<Program> existingPrograms, bool alreadySorted)
    {
      if (Count == 0 || existingPrograms.Count == 0) return;

      if (!alreadySorted)
      {
        existingPrograms.Sort(this);
      }
      SortIfNeeded();
      IComparer<Program> comparer = this;

      // Traverse both lists in parallel in a "Merge Join" style.
      int i = 0;
      int j = 0;
      for (; i < Count && j < existingPrograms.Count;)
      {
        Program prog = this[i];
        Program existProg = existingPrograms[j];
        if (prog.IdChannel == existProg.IdChannel)
        {
          if (prog.EndTime <= existProg.StartTime)
          {
            i++;
            //prog = this[i];
          }
          else if (prog.StartTime >= existProg.EndTime)
          {
            j++;
            //existProg = existingPrograms[j];
          }
          else // prog.StratTime < existProg.EndTime && prog.EndTime > existProg.StartTime
          {
            RemoveAt(i);
            //prog = this[i];
          }
        }
        else
        {
          if (comparer.Compare(prog, existProg) < 0)
          {
            i++;
            //prog = this[i];
          }
          else
          {
            j++;
            //existProg = existingPrograms[j];
          }
        }
      }
    }

    /// <summary>
    /// Fill in gaps in the <see cref="ProgramList"/> using data from <paramref name="sourceList"/>.
    /// </summary>
    /// <param name="sourceList">The list to get data from</param>
    /// <remarks>
    /// Programs in <paramref name="sourceList"/> are assumed unsorted.
    /// </remarks>
    public void FillInMissingDataFromList(List<Program> sourceList)
    {
      FillInMissingDataFromList(sourceList, false);
    }

    /// <summary>
    /// Fill in gaps in the <see cref="ProgramList"/> using data from <paramref name="sourceList"/>.
    /// </summary>
    /// <param name="sourceList">The list to get data from</param>
    /// <param name="sourceAlreadySorted">If true <paramref name="sourceList"/> 
    /// is assumed to be already sorted and will not be sorted again</param>
    public void FillInMissingDataFromList(List<Program> sourceList, bool sourceAlreadySorted)
    {
      SortIfNeeded();
      if (!sourceAlreadySorted)
      {
        sourceList.Sort(this);
      }
      Program prevProg = this[0];
      for (int i = 1; i < Count; i++)
      {
        Program newProg = this[i];
        if (newProg.StartTime > prevProg.EndTime) // we have a gap here
        {
          // try to find data in the database
          foreach (Program dbProg in sourceList)
          {
            if ((dbProg.StartTime >= prevProg.EndTime) && (dbProg.EndTime <= newProg.StartTime))
            {
              Insert(i, dbProg.Clone());
              i++;
              prevProg = dbProg;
            }
            if (dbProg.StartTime >= newProg.EndTime) break; // no more data available
          }
        }
        prevProg = newProg;
      }
    }

    public void FixEndTimes()
    {
      SortIfNeeded();
      for (int i = 0; i < Count; ++i)
      {
        Program prog = this[i];
        if (i + 1 < Count)
        {
          //correct the times of the current program using the times of the next one
          Program progNext = this[i + 1];
          if (prog.IdChannel == progNext.IdChannel)
          {
            if (prog.StartTime >= prog.EndTime)
            {
              prog.EndTime = progNext.StartTime;
            }
            if (prog.EndTime > progNext.StartTime)
            {
              //if the endTime of this program is later that the start of the next program 
              //it probably needs to be corrected (only needed when the grabber )
              prog.EndTime = progNext.StartTime;
            }
          }
        }
      }
    }

    /// <summary>
    /// Parse the program list and find partitions of chained programs 
    /// (each program starts at the end of the previous one and is on the same channel).
    /// </summary>
    /// <returns>A list of partition data</returns>
    public List<ProgramListPartition> GetPartitions()
    {
      List<ProgramListPartition> partitions = new List<ProgramListPartition>();
      if (Count != 0)
      {
        SortIfNeeded();
        ProgramListPartition partition = new ProgramListPartition(this[0].IdChannel, this[0].StartTime, this[0].EndTime);
        for (int i = 1; i < Count; i++)
        {
          Program currProg = this[i];
          if (partition.IdChannel.Equals(currProg.IdChannel) && partition.End.Equals(currProg.StartTime))
          {
            partition.End = currProg.EndTime;
          }
          else
          {
            partitions.Add(partition);
            partition = new ProgramListPartition(currProg.IdChannel, currProg.StartTime, currProg.EndTime);
          }
        }
        partitions.Add(partition);
      }
      return partitions;
    }

    /// <summary>
    /// Parse the program list and find the unique channel IDs
    /// </summary>
    /// <returns>A list of channel IDs</returns>
    public List<int> GetChannelIds()
    {
      List<int> channelIds = new List<int>();
      if (Count != 0)
      {
        SortIfNeeded();
        int lastChannelId = this[0].IdChannel;
        channelIds.Add(lastChannelId);
        for (int i = 1; i < Count; i++)
        {
          Program currProg = this[i];
          if (lastChannelId != currProg.IdChannel)
          {
            lastChannelId = currProg.IdChannel;
            channelIds.Add(lastChannelId);
          }
        }
      }
      return channelIds;
    }

    #endregion

    #region Base overrides

    public new void Add(Program item)
    {
      _alreadySorted = false;
      base.Add(item);
    }

    public new void AddRange(IEnumerable<Program> collection)
    {
      _alreadySorted = false;
      base.AddRange(collection);
    }

    public new void Insert(int index, Program item)
    {
      _alreadySorted = false;
      base.Insert(index, item);
    }

    public new void InsertRange(int index, IEnumerable<Program> collection)
    {
      _alreadySorted = false;
      base.InsertRange(index, collection);
    }

    public new void Reverse(int index, int count)
    {
      _alreadySorted = false;
      base.Reverse(index, count);
    }

    public new void Reverse()
    {
      _alreadySorted = false;
      base.Reverse();
    }

    public new void Sort()
    {
      Sort(this);
      _alreadySorted = true;
    }

    #endregion

    #region IComparer<Program> Members

    int IComparer<Program>.Compare(Program x, Program y)
    {
      if (x == y) return 0;
      if (x == null) return -1;
      if (y == null) return -1;

      if (x.IdChannel != y.IdChannel)
      {
        int res = String.Compare(x.ReferencedChannel().DisplayName, y.ReferencedChannel().DisplayName, true);
        if (res == 0)
        {
          if (x.IdChannel > y.IdChannel)
          {
            return 1;
          }
          else
          {
            return -1;
          }
        }
      }
      if (x.StartTime > y.StartTime) return 1;
      if (x.StartTime < y.StartTime) return -1;
      return 0;
    }

    #endregion
  }
}