using System;

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
