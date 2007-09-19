using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MediaPortal.GUI.Library;

namespace MediaPortal.Player
{
    public enum SubtitleType{
        Teletext = 0,
        Bitmap = 1
    }

    public class SubtitleOption{
        public SubtitleType type;
        public TeletextPageEntry entry; // only for teletext
        public int bitmapIndex; // index among bitmap subs, only for bitmap subs :)
        public string language;

        public string ToString() {
            if (type == SubtitleType.Bitmap)
            {
                return "Bitmap Lang " + language;
            }
            else
            {
                return "Teletext Lang\t" + entry.language + "\tpage : " + entry.page;
            }
        }
    }

    class SubtitleSelector
    {
        private Dictionary<int,TeletextPageEntry> pageEntries;

        public SubtitleSelector(MediaPortal.Player.TSReaderPlayer.ISubtitleStream dvbStreams, SubtitleRenderer subRender)
        {
            if (dvbStreams == null)
            {
                throw new Exception("Nullpointer input not allowed ( ISubtitleStream )");
            }
            if ( subRender == null)
            {
                throw new Exception("Nullpointer input not allowed ( SubtitleRenderer)");
            }
            else {
                this.dvbStreams = dvbStreams;
                this.subRender = subRender;
            }

            // load preferences
            using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
            {
                primaryLang = reader.GetValueAsString("MyTV", "PrimarySubLang", "eng");
                secondaryLang = reader.GetValueAsString("MyTV", "SecondarySubLang", "ger");
            }
            pageEntries = new Dictionary<int, TeletextPageEntry>();
            subRender.SetPageInfoCallback(new PageInfoCallback(OnPageInfo));
            MediaPortal.GUI.Library.Log.Info("Subtitle selector: Primary lang {0}, secondary {1}", primaryLang, secondaryLang);
        }

        private void OnPageInfo(TeletextPageEntry entry){
            lock (pageEntries)
            {
                if (!pageEntries.ContainsKey(entry.page))
                {
                    pageEntries.Add(entry.page, entry);
                }
            }
        }

        private List<SubtitleOption> CollectOptions() {
            Log.Debug("SubtitleSelector: CollectOptions");
            List<SubtitleOption> options = new List<SubtitleOption>();
            // collect dvb bitmap subtitle options
            int streamCount = 0;
            dvbStreams.GetSubtitleStreamCount(ref streamCount);
            Debug.Assert(streamCount >= 0 && streamCount <= 100);

            for (int i = 0; i < streamCount; i++)
            {
                TSReaderPlayer.SUBTITLE_LANGUAGE subLang = new TSReaderPlayer.SUBTITLE_LANGUAGE();
                dvbStreams.GetSubtitleStreamLanguage(i, ref subLang);
                SubtitleOption option = new SubtitleOption();
                option.type = SubtitleType.Bitmap;
                option.language = subLang.lang;
                option.bitmapIndex = i;
                options.Add(option);
                Log.Debug("Added bitmap option Lang : " + option.ToString());
            }

            // collect teletext options

            lock (pageEntries)
            {
                foreach (KeyValuePair<int, TeletextPageEntry> p in pageEntries)
                {
                    SubtitleOption option = new SubtitleOption();
                    option.type = SubtitleType.Teletext;
                    option.language = p.Value.language;
                    option.entry = p.Value;
                    options.Add(option);
                    Log.Debug("Added Teletext option Lang : " + option.ToString());
                }
            }
            return options;
        }


        public int CountOptions() {
            return CollectOptions().Count;
        }

        public int GetCurrentOption() {
            return lastSubtitleIndex;
        }

        public void SetOption(int index) {
            List<SubtitleOption> options = CollectOptions();
            if (index >= options.Count) {
                Log.Error("SetOption with too larger index!");
                return;
            }
            SubtitleOption option = options[index];
            subRender.SetSubtitleOption(option);
            if (option.type == SubtitleType.Bitmap) {
                dvbStreams.SetSubtitleStream(option.bitmapIndex);
            }
            lastSubtitleIndex = index;
            currentOption = option;
            Log.Debug("Subtitle is now " + currentOption.ToString());
        }

        public string GetCurrentLanguage() {
            if (currentOption == null)
            {
                Log.Error("Calling GetCurrentLanguage with no subtitle set!");
                return Strings.Unknown;
            }
            else if (currentOption.type == SubtitleType.Teletext && currentOption.entry.language.Trim().Length == 0)
            {
                return "p." + currentOption.entry.page;
            }
            else return currentOption.language;
        }

        private MediaPortal.Player.TSReaderPlayer.ISubtitleStream dvbStreams;
        private SubtitleRenderer subRender;

        private string primaryLang;
        private string secondaryLang;
        private int lastSubtitleIndex;
        private SubtitleOption currentOption;
    }
}
