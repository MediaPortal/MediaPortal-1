using System;

namespace MediaPortal.Core.Transcoding
{
	/// <summary>
	/// Summary description for TranscodeFactory.
	/// </summary>
	public class TranscodeFactory
	{
		public TranscodeFactory()
		{
		}

		public ITranscode GetTranscoder(TranscodeInfo info,VideoFormat format)
		{
			string ext=System.IO.Path.GetExtension(info.file).ToLower();
			if (ext==".dvr-ms") return new Dvrms2Mpeg();
			else
			{
				if (format==VideoFormat.Wmv)
					return new TranscodeToWMV();
			}
			return null;
		}

	}
}
