using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessPlugins.EpgGrabber
{
   public enum ChannelSortBy
   {
      ChannelNumber,
      ChannelName
   }

   #region Channel Info Class
   /// <summary>
   /// Basic Channel Information Used in Sorting
   /// </summary>
   public class ChannelInfo
   {
      private int    _ID;
      private int    _MajorChannel;
      private int    _MinorChannel;
      private string _Name = String.Empty;

      #region Constructor
      public ChannelInfo(int id, int majorChannel, int minorChannel, string name)
      {
         _ID           = id;
         _MajorChannel = majorChannel;
         _MinorChannel = minorChannel;
         _Name         = name;
      }
      #endregion

      #region Getters and Setters
      public int ID
      {
         get { return _ID; }
         set { _ID = value; }
      }

      public int MajorChannel
      {
         get { return _MajorChannel; }
         set { _MajorChannel = value; }
      }

      public int MinorChannel
      {
         get { return _MinorChannel; }
         set { _MinorChannel = value; }
      }

      public string Name
      {
         get { return _Name; }
         set { _Name = value; }
      }
      #endregion
   }
   #endregion

   public class ChannelSorter
   {
      public System.Collections.IComparer comparer;

      /// <summary>
      /// Specifies the IList and Comparer
      /// </summary>
      /// <param name="array">The array to sort.</param>
      /// <param name="comparer">The comparer to use.</param>
      public ChannelSorter(System.Collections.IList array, System.Collections.IComparer comparer)
      {
         this.comparer = comparer;
         Sort(array, 0, array.Count - 1);
      }

      private void Sort(System.Collections.IList array, int lower, int upper)
      {
         // Check for non-base case
         if (lower < upper)
         {
            // Split and sort partitions
            int split = Pivot(array, lower, upper);
            Sort(array, lower, split - 1);
            Sort(array, split + 1, upper);
         }
      }

      private int Pivot(System.Collections.IList array, int lower, int upper)
      {
         // Pivot with first element
         int left = lower + 1;
         int right = upper;

         object pivot = array[lower];

         // Partition array elements
         while (left <= right)
         {
            // Find item out of place
            while ((left <= right) && (comparer.Compare(array[left], pivot) <= 0))
            {
               ++left;
            }

            while ((left <= right) && (comparer.Compare(array[right], pivot) > 0))
            {
               --right;
            }

            // Swap values if necessary
            if (left < right)
            {
               Swap(array, left, right);
               ++left;
               --right;
            }
         }

         // Move pivot element
         Swap(array, lower, right);
         return right;
      }

      private void Swap(System.Collections.IList array, int left, int right)
      {
         object swap = array[left];
         array[left] = array[right];
         array[right] = swap;
      }
   }

   public class ChannelNumberComparer : System.Collections.IComparer // IComparer<Channel>
   {

      #region IComparer Members

      public int Compare(object x, object y)
      {
         if (x == null || y == null)
            return 0;

         if (x is ChannelInfo && y is ChannelInfo)
         {
            ChannelInfo ChX = (ChannelInfo)x;
            ChannelInfo ChY = (ChannelInfo)y;

            //int majorChX = 0;
            //int minorChX = 0;
            //int majorChY = 0;
            //int minorChY = 0;

            //ATSCChannel atscChX = new ATSCChannel();
            //if (GetATSCChannel(x, ref atscChX))
            //{
            //   majorChX = atscChX.MajorChannel;
            //   minorChX = atscChX.MinorChannel;
            //}
            //else
            //{
            //   System.Collections.IList chDtlX = x.ReferringTuningDetail();
            //   TuningDetail chTuneDtlX = (TuningDetail)chDtlX[0];
            //   majorChX = chTuneDtlX.ChannelNumber;
            //}

            //ATSCChannel atscChY = new ATSCChannel();
            //if (GetATSCChannel(y, ref atscChY))
            //{
            //   majorChY = atscChY.MajorChannel;
            //   minorChY = atscChY.MinorChannel;
            //}
            //else
            //{
            //   System.Collections.IList chDtlY = y.ReferringTuningDetail();
            //   TuningDetail chTuneDtlY = (TuningDetail)chDtlY[0];
            //   majorChY = chTuneDtlY.ChannelNumber;
            //}

            if (ChX.MajorChannel < ChY.MajorChannel)
               return -1;
            else if (ChX.MajorChannel > ChY.MajorChannel)
               return 1;
            else
            {
               if (ChX.MinorChannel < ChY.MinorChannel)
                  return -1;
               else if (ChX.MinorChannel > ChY.MinorChannel)
                  return 1;
               else
                  return 0;
            }
         }
         else
            return 0;
      }

      #endregion
   }

}
