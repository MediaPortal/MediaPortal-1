using System;

namespace MediaPortal.Core.Transcoding
{
	public enum VideoFormat
	{
		Dvr_ms,
		Mpeg2,
		Wmv,
		Divx,
		Xvid
	};

	public enum Quality
	{
		Low,
		Medium,
		High
	}

	/// <summary>
	/// Class giving all information for a file
	/// </summary>
	public class TranscodeInfo
	{
		public string file=String.Empty;					//local filename+path
		public string Author=String.Empty;				//author of file
		public string Copyright=String.Empty;		//copyright notice
		public string Description=String.Empty;	//description of file
		public string Rating=String.Empty;				//rating for file
		public string Title=String.Empty;				//title of file
	}

	/// <summary>
	/// interface for all transcoders.
	/// Any transcoder should implement this and make sure that TranscodeFactory can create such ITranscode instance
	/// </summary>
	public interface ITranscode
	{
		/// <summary>
		/// Check if the requested video format is supported by this transcoder
		/// </summary>
		/// <param name="format">Video format</param>
		/// <returns>true: transcoder can encode a file to the requested video format</returns>
		/// <returns>true: transcoder cannot encode a file to the requested video format</returns>
		bool Supports(VideoFormat format);

		/// <summary>
		/// Transcode a file
		/// </summary>
		/// <param name="info">instance of TranscodeInfo which gives all info of the file to transcode</param>
		/// <param name="format">Video format to which the file needs tobe converted</param>
		/// <param name="quality">Desired quality</param>
		/// <returns>
		/// true: transcoding has started
		/// false:failed to transcode file
		/// </returns>
		bool Transcode(TranscodeInfo info, VideoFormat format,Quality quality);

		/// <summary>
		/// Property to check if transcoding has finished
		/// </summary>
		/// <returns>
		/// false: transcoding still busy
		/// true: transcoding has ended
		/// </returns>
		bool IsFinished();

		/// <summary>
		/// Property to check if we're transcoding
		/// </summary>
		/// <returns>
		/// true: file is being transcoded
		/// false: idle
		/// </returns>
		bool IsTranscoding();
	}
}
