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