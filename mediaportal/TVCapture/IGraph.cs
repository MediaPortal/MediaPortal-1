using System;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
	public interface IGraph
	{
    bool CreateGraph();
    void DeleteGraph();
    bool StartTimeShifting(int iChannelNr, string strFileName);
    bool StopTimeShifting();
    bool StartRecording(string strFileName, bool bContentRecording, DateTime timeProgStart);
    void StopRecording();
    void TuneChannel(int iChannel);

    int GetChannelNumber();
	}
}
