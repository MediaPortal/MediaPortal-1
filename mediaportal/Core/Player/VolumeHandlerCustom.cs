using System;
using System.Collections;

namespace MediaPortal.Player
{
	internal class VolumeHandlerCustom : VolumeHandler
	{
		#region Constructors

		public VolumeHandlerCustom()
		{
			using(MediaPortal.Profile.Xml reader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string text = reader.GetValueAsString("volume", "table", string.Empty);

				if(text == string.Empty)
					return;

				ArrayList array = new ArrayList();

				try
				{
					foreach(string volume in text.Split(new char[] { ',', ';' }))
					{
						if(volume == string.Empty)
							continue;

						array.Add(Math.Max(this.Minimum, Math.Min(this.Maximum, int.Parse(volume))));
					}

					array.Sort();

					this.Table = (int[])array.ToArray(typeof(int));
				}
				catch
				{
					// heh, its undocumented remember, no fancy logging going on here
				}
			}
		}

		#endregion Constructors
	}
}
