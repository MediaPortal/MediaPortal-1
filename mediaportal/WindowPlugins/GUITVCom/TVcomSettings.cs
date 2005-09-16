using System;
using System.Text;

namespace MediaPortal.GUI.Video
{
	class TVcomSettings
	{
		static TVcomSettings()
		{
			loadSettings();
		}

		public static bool lookupIfNoSEinFilename = false;
		public static bool renameFiles = false;
		public static bool renameOnlyIfNoSEinFilename = true;
		public static string renameFormat = "[SHOWNAME] - [SEASONNO]x[EPISODENO] - [EPISODETITLE]";
		public static char replaceSpacesWith = ' ';

		public static string titleFormat = "[SHOWNAME] - [SEASONNO]x[EPISODENO] - [EPISODETITLE]";

		public static string genreFormat = "[SHOWNAME] ([GENRE])";

		private const string settingsFilePath = "Episode Guides/settings";



		private static bool loadSettings()
		{
			tvDotComParser.tvComLogWritelineStatic("Loading Settings...");
			try
			{
				System.IO.StreamReader r = new System.IO.StreamReader(settingsFilePath);
				lookupIfNoSEinFilename =  Convert.ToBoolean(r.ReadLine());
				renameFiles =  Convert.ToBoolean(r.ReadLine());
				renameOnlyIfNoSEinFilename  =  Convert.ToBoolean(r.ReadLine());
				renameFormat = r.ReadLine();
				replaceSpacesWith = Convert.ToChar(r.ReadLine());
				titleFormat = r.ReadLine();
				genreFormat = r.ReadLine();
				r.Close();
				tvDotComParser.tvComLogWritelineStatic("Settings loaded Succesfully");

				tvDotComParser.tvComLogWritelineStatic(lookupIfNoSEinFilename.ToString());
				tvDotComParser.tvComLogWritelineStatic(renameFiles.ToString());
				tvDotComParser.tvComLogWritelineStatic(renameOnlyIfNoSEinFilename.ToString());
				tvDotComParser.tvComLogWritelineStatic(renameFormat);
				tvDotComParser.tvComLogWritelineStatic(replaceSpacesWith.ToString());
				tvDotComParser.tvComLogWritelineStatic(titleFormat);
				tvDotComParser.tvComLogWritelineStatic(genreFormat);

				return true;
			}
			catch
			{
				tvDotComParser.tvComLogWritelineStatic("There was an error loading the Settings!");
				
				return false;
			}

		}


		public static void writeSettings()
		{
			if(!System.IO.Directory.Exists("Episode Guides"))
				System.IO.Directory.CreateDirectory("Episode Guides");

			System.IO.StreamWriter w = new System.IO.StreamWriter(settingsFilePath, false);
			w.WriteLine(lookupIfNoSEinFilename.ToString());
			w.WriteLine(renameFiles.ToString());
			w.WriteLine(renameOnlyIfNoSEinFilename.ToString());
			w.WriteLine(renameFormat);
			w.WriteLine(replaceSpacesWith.ToString());
			w.WriteLine(titleFormat);
			w.WriteLine(genreFormat);
			w.Close();

			tvDotComParser.tvComLogWritelineStatic("Settings updated:");

			tvDotComParser.tvComLogWritelineStatic(lookupIfNoSEinFilename.ToString());
			tvDotComParser.tvComLogWritelineStatic(renameFiles.ToString());
			tvDotComParser.tvComLogWritelineStatic(renameOnlyIfNoSEinFilename.ToString());
			tvDotComParser.tvComLogWritelineStatic(renameFormat);
			tvDotComParser.tvComLogWritelineStatic(replaceSpacesWith.ToString());
			tvDotComParser.tvComLogWritelineStatic(titleFormat);
			tvDotComParser.tvComLogWritelineStatic(genreFormat);

		}
	}



}
