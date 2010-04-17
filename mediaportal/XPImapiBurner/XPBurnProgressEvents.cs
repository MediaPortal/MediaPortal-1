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

using XPBurn.COM;

namespace XPBurn
{
  internal class XPBurnProgressEvents : IDiscMasterProgressEvents
  {
    #region Private fields

    private XPBurnMessageQueue fOwner;

    #endregion

    #region Constructors

    public XPBurnProgressEvents(XPBurnMessageQueue owner)
    {
      fOwner = owner;
    }

    #endregion

    #region IDiscMasterProgressEvents Members

    public void QueryCancel(out bool pbCancel)
    {
      pbCancel = fOwner.Cancel;
    }

    public void NotifyPnPActivity()
    {
      fOwner.BeginInvoke(new NotifyPnPActivity(fOwner.OnRecorderChange));
    }

    public void NotifyAddProgress(int nCompletedSteps, int nTotalSteps)
    {
      fOwner.BeginInvoke(new NotifyCDProgress(fOwner.OnAddProgres), new object[] {nCompletedSteps, nTotalSteps});
    }

    public void NotifyBlockProgress(int nCompleted, int nTotal)
    {
      fOwner.BeginInvoke(new NotifyCDProgress(fOwner.OnBlockProgress), new object[] {nCompleted, nTotal});
    }

    public void NotifyTrackProgress(int nCurrentTrack, int nTotalTrack)
    {
      fOwner.BeginInvoke(new NotifyCDProgress(fOwner.OnTrackProgress), new object[] {nCurrentTrack, nTotalTrack});
    }

    public void NotifyPreparingBurn(int nEstimatedSeconds)
    {
      fOwner.BeginInvoke(new NotifyEstimatedTime(fOwner.OnPreparingBurn), new object[] {nEstimatedSeconds});
    }

    public void NotifyClosingDisc(int nEstimatedSeconds)
    {
      fOwner.BeginInvoke(new NotifyEstimatedTime(fOwner.OnClosingDisc), new object[] {nEstimatedSeconds});
    }

    public void NotifyBurnComplete(uint statusHR)
    {
      fOwner.BeginInvoke(new NotifyCompletionStatus(fOwner.OnBurnComplete), new object[] {statusHR});
    }

    public void NotifyEraseComplete(uint statusHR)
    {
      //fOwner.Invoke(new NotifyCompletionStatus(fOwner.OnEraseComplete), new object[] { statusHR });
    }

    #endregion
  }
}