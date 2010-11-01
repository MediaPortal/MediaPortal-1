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

using System.Windows.Forms;

namespace XPBurn
{
  internal class XPBurnMessageQueue : UserControl
  {
    private XPBurnCD fXPBurnCD;

    public bool Cancel
    {
      get { return fXPBurnCD.fCancel; }
    }

    public XPBurnMessageQueue(XPBurnCD xpBurnCD)
    {
      if (!IsHandleCreated)
      {
        CreateHandle();
      }

      fXPBurnCD = xpBurnCD;
    }

    internal void OnRecorderChange()
    {
      fXPBurnCD.OnRecorderChange();
    }

    internal void OnAddProgres(int nCompletdSteps, int nTotalSteps)
    {
      fXPBurnCD.OnAddProgres(nCompletdSteps, nTotalSteps);
    }

    internal void OnBlockProgress(int nCompletedSteps, int nTotalSteps)
    {
      fXPBurnCD.OnBlockProgress(nCompletedSteps, nTotalSteps);
    }

    internal void OnTrackProgress(int nCompletedSteps, int nTotalSteps)
    {
      fXPBurnCD.OnTrackProgress(nCompletedSteps, nTotalSteps);
    }

    internal void OnPreparingBurn(int nEstimatedSeconds)
    {
      fXPBurnCD.OnPreparingBurn(nEstimatedSeconds);
    }

    internal void OnClosingDisc(int nEstimatedSeconds)
    {
      fXPBurnCD.OnClosingDisc(nEstimatedSeconds);
    }

    internal void OnBurnComplete(uint status)
    {
      fXPBurnCD.OnBurnComplete(status);
    }

    internal void OnEraseComplete(uint status)
    {
      fXPBurnCD.OnEraseComplete(status);
    }
  }
}