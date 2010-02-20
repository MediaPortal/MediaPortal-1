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

namespace XPBurn
{
  /// <summary>
  /// This delegate is used internally to allow the current burn to be cancelled.  
  /// The system queries the method attached to this delegate periodically and if <see cref="XPBurnCD.Cancel"/> 
  /// is true, the burn is stopped.
  /// </summary>
  public delegate void QueryCancel(bool bCancel);

  /// <summary>
  /// This delegate is used as the type of the <see cref="XPBurnCD.RecorderChange"/> event.
  /// The method attached to it is invoked by the system when some plug and play acitvity has been detected, possibly 
  /// the removal or addition of a cd burner drive.
  /// </summary>
  public delegate void NotifyPnPActivity();

  /// <summary>
  /// This delegate is used as the type of the <see cref="XPBurnCD.BlockProgress"/>, 
  /// <see cref="XPBurnCD.AddProgress"/>, and <see cref="XPBurnCD.TrackProgress"/> events.  
  /// The methods attached to the delegate are invoked periodically by the system when progress has been made 
  /// with the burn.
  /// </summary>
  public delegate void NotifyCDProgress(int nCompletedSteps, int nTotalSteps);

  /// <summary>
  /// This delegate is used as the type of the <see cref="XPBurnCD.PreparingBurn"/>and 
  /// <see cref="XPBurnCD.ClosingDisc"/> events.  
  /// </summary>
  public delegate void NotifyEstimatedTime(int nEstimatedSeconds);

  /// <summary>
  /// This delegate is used as the type of the <see cref="XPBurnCD.EraseComplete"/> and 
  /// <see cref="XPBurnCD.BurnComplete"/> events.
  /// </summary>
  public delegate void NotifyCompletionStatus(uint status);
}