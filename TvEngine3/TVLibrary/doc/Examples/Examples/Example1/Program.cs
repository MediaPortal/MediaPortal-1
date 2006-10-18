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
using System.Text;
using TvControl;          // include the tvserver remote control interfaces
using TvLibrary.Channels; // include tv-channel types

namespace Example1
{
  /// <summary>
  /// Example which connects to a tv-server
  /// and shows the status for each tv-card
  /// </summary>
  class Program
  {
    static void ShowCardStatus(int cardIndex)
    {
      Console.WriteLine("card:{0}", cardIndex);
      int cardId=RemoteControl.Instance.CardId(cardIndex);

      VirtualCard card = new VirtualCard(cardId, RemoteControl.HostName);
      Console.WriteLine("  Type          :{0}", card.Type);
      Console.WriteLine("  Name          :{0}", card.Name);
      Console.WriteLine("  Device        :{0}", card.Device);
      Console.WriteLine("  IsTimeShifting:{0}", card.IsTimeShifting);
      Console.WriteLine("  IsRecording   :{0} {1}", card.IsRecording,card.RecordingFileName);
      Console.WriteLine("  IsScanning    :{0} {1}", card.IsScanning,card.TimeShiftFileName);
      Console.WriteLine("  IsGrabbingEpg :{0}", card.IsGrabbingEpg);
      Console.WriteLine("  IsScrambled   :{0}", card.IsScrambled);
      Console.WriteLine("  ChannelName   :{0}", card.ChannelName);
      Console.WriteLine("  Channel       :{0}", card.Channel);
      Console.WriteLine("  SignalQuality :{0}", card.SignalQuality);
      Console.WriteLine("  SignalLevel   :{0}", card.SignalLevel);
      Console.WriteLine("  IsTunerLocked :{0}", card.IsTunerLocked);
      User user;
      bool isLocked = card.IsLocked(out user);
      if (isLocked)
        Console.WriteLine("  IsLocked by   :{0}", user.Name);
      
    }
    static void Main(string[] args)
    {
      //set the hostname of the tvserver
      RemoteControl.HostName = "localhost";

      //Enumerate all cards installed on the tvserver
      int cardCount = RemoteControl.Instance.Cards;
      for (int i = 0; i < cardCount; ++i)
      {
        ShowCardStatus(i);
      }

      Console.ReadLine();
    }
  }
}
