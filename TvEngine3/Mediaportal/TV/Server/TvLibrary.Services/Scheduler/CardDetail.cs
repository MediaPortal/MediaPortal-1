#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Scheduler
{
  public static class IComparableExtensions
  {
    public static void SortStable<T>(this List<T> list) where T : IComparable<T>
    {
      var listStableOrdered = list.OrderBy<T, T>(x => x, new ComparableComparer<T>()).ToList();
      list.Clear();
      list.AddRange(listStableOrdered);
    }

    private class ComparableComparer<T> : IComparer<T> where T : IComparable<T>
    {
      public int Compare(T x, T y)
      {
        return x.CompareTo(y);
      }
    }
  }

  /// <summary>
  /// Class which can be used to sort Cards bases on priority
  /// </summary>
  public class CardDetail : IComparable<CardDetail>
  {
    private readonly int _cardId;
    private readonly Tuner _card;
    private readonly int _cardPriority;
    private readonly IChannel _detail;
    private readonly int _tuningDetailPriority;
    private bool _sameTransponder;
    private int _numberOfOtherUsers;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="id">card id</param>
    /// <param name="card">card dataaccess object</param>
    /// <param name="detail">tuning detail</param>
    /// <param name="tuningDetailPriority">the priority for the database tuning detail record (higher is more important)</param>
    /// <param name="sameTransponder">indicates whether it is the same transponder</param>
    /// <param name="numberOfOtherUsers"></param>
    public CardDetail(int id, Tuner card, IChannel detail, int tuningDetailPriority, bool sameTransponder, int numberOfOtherUsers)
    {
      _cardId = id;
      _card = card;
      _cardPriority = _card.Priority;
      _detail = detail;
      _tuningDetailPriority = tuningDetailPriority;
      _sameTransponder = sameTransponder;
      _numberOfOtherUsers = numberOfOtherUsers;
    }

    /// <summary>
    /// gets the id of the card
    /// </summary>
    public int Id
    {
      get { return _cardId; }
    }

    /// <summary>
    /// gets/sets the priority
    /// </summary>
    /// <value>The priority.</value>
    public int CardPriority
    {
      get { return _cardPriority; }
    }

    /// <summary>
    /// gets the card
    /// </summary>
    public Tuner Card
    {
      get { return _card; }
    }

    /// <summary>
    /// gets the tuning detail
    /// </summary>
    public IChannel TuningDetail
    {
      get { return _detail; }
    }

    /// <summary>
    /// gets the tuning detail priority
    /// </summary>
    public int TuningDetailPriority
    {
      get { return _tuningDetailPriority; }
    }

    /// <summary>
    /// returns if it is the same transponder
    /// </summary>
    public bool SameTransponder
    {
      get { return _sameTransponder; }
      set { _sameTransponder = value; }
    }

    /// <summary>
    /// gets the number of other users
    /// </summary>
    public int NumberOfOtherUsers
    {
      get { return _numberOfOtherUsers; }
      set { _numberOfOtherUsers = value; }
    }

    #region IComparable<CardInfo> Members

    /// <summary>
    /// Compare two CardDetails.
    /// </summary>
    /// <remarks>
    /// The preferred CardDetail is the one that should be tried earlier when
    /// tuning. This decision is based on:
    /// 1. Tuning detail priority [lower preferred, enables user to prefer HD tuning details].
    /// 2. Tuner tuned to same transponder or not [same transponder preferred, minimises the number of tuners used].
    /// 3. Number of users using the tuner [higher user count preferred, tends to minimise the number of tuners used].
    /// 4. Tuner priority [lower preferred, user preference].
    /// 
    /// If this function is used to sort a list, the most preferred CardDetail
    /// will be the first in the list.
    /// </remarks>
    public int CompareTo(CardDetail other)
    {
      if (TuningDetailPriority != other.TuningDetailPriority)
      {
        return TuningDetailPriority - other.TuningDetailPriority;
      }

      if (SameTransponder != other.SameTransponder)
      {
        if (SameTransponder)
        {
          return -1;
        }
        return 1;
      }

      if (NumberOfOtherUsers != other.NumberOfOtherUsers)
      {
        return other.NumberOfOtherUsers - NumberOfOtherUsers;
      }

      return CardPriority - other.CardPriority;
    }

    #endregion
  }
}