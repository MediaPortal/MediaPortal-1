using System;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Interface definition for auto-tuning
	/// 
	/// MP now supports multiple TV capture cards like analog cable/antenne, DVB-S, DVB-T and DVB-C
	/// each type has its own specific auto-tuning to locate/find new TV Channels
	/// By implementing this ITuning interface for a specific card type the configuration.exe does not need to know
	/// all details about tuning. 
	/// The configuration.exe asks graphfactory for an ITuning interface for a specific card
	/// and when it gets it calls the AutoTune() method
	/// </summary>
	/// 
	public interface AutoTuneCallback
	{
		void OnNewChannel();
		void OnStatus(string description);
		void OnProgress(int percentDone);
		void OnEnded();
	}

	public interface ITuning
	{
		/// <summary>
		/// This method should do all auto-tuning
		/// It should locate & find all tv channels for the card specified and store them in the database
		/// </summary>
		/// <param name="card">specifies the tvcapture card for which tuning should occur</param>
		/// <param name="callback">specifies a callback interface to indicate status updates</param>
		void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback callback);
		void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback callback);
		void Continue();
		int  MapToChannel(string channel);
	}
}
