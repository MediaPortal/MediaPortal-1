using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MediaPortal.GUI.Library;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaPortal.Player.Subtitles
{
    public enum SubtitleType
    {
        Teletext = 0,
        Bitmap = 1,
        Auto = 2
    }

    // TODO: Have an AUTO subtitle option!

    public class SubtitleOption
    {
        public SubtitleType type;
        public TeletextPageEntry entry; // only for teletext
        public int bitmapIndex; // index among bitmap subs, only for bitmap subs :)
        public string language;

        public override string ToString()
        {
            if (type == SubtitleType.Bitmap)
            {
                return "Bitmap Lang " + language;
            }
            else if (type == SubtitleType.Teletext)
            {
                return "Teletext Lang\t" + entry.language + "\tpage : " + entry.page;
            }
            else if (type == SubtitleType.Auto)
            {
                return "Auto select";
            }
            else
            {
                return "???";
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (o == null) return false;
            if (o is SubtitleOption)
            {
                SubtitleOption other = o as SubtitleOption;
                if (other.type != this.type) return false;
                else if (other.bitmapIndex != this.bitmapIndex) return false;
                else if (!other.language.Equals(this.language)) return false;
                else if ((this.entry != null && !this.entry.Equals(other.entry)) || this.entry == null && other.entry != null) return false;
                else return true;
            }
            else return false;
        }
    }

    class SubtitleSelector
    {
        private SubtitleOption autoSelectOption;
        private delegate int SubtitleStreamEventCallback(int event_type, UInt64 event_value);
        private SubtitleStreamEventCallback subStreamCallback;
        private object syncLock = new object();

        public SubtitleSelector(MediaPortal.Player.TSReaderPlayer.ISubtitleStream dvbStreams, SubtitleRenderer subRender)
        {
            Log.Debug("SubtitleSelector ctor");
            if (dvbStreams == null)
            {
                throw new Exception("Nullpointer input not allowed ( ISubtitleStream )");
            }
            if (subRender == null)
            {
                throw new Exception("Nullpointer input not allowed ( SubtitleRenderer)");
            }
            else
            {
                this.dvbStreams = dvbStreams;
                this.subRender = subRender;
            }

            // load preferences
            using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
            {
                preferedLanguages = new List<string>();
                string languages = reader.GetValueAsString("mytv", "sublangs", "");
                Log.Debug("SubtitleSelector: sublangs entry content: " + languages);
                StringTokenizer st = new StringTokenizer(languages, ",");
                while (st.HasMore)
                {
                    string lang = st.NextToken();
                    if (lang.Length != 3)
                    {
                        Log.Warn("Language {0} is not in the correct format!", lang);
                    }
                    else
                    {
                        preferedLanguages.Add(lang);
                        Log.Info("Prefered language {0} is {1}", preferedLanguages.Count, lang);
                    }
                }
            }

            pageEntries = new Dictionary<int, TeletextPageEntry>();
            

            bitmapSubtitleCache = new List<SubtitleOption>();

            RetrieveBitmapSubtitles();

            if (preferedLanguages.Count > 0)
            {
                autoSelectOption = new SubtitleOption();
                autoSelectOption.language = "Auto";
                autoSelectOption.type = SubtitleType.Auto;

                SetOption(0); // the autoselect mode will have index 0 (ugly)
            }

            subRender.SetPageInfoCallback(new PageInfoCallback(OnPageInfo));
            subStreamCallback = new SubtitleStreamEventCallback(OnSubtitleStreamEvent);
            IntPtr pSubStreamCallback = Marshal.GetFunctionPointerForDelegate(subStreamCallback);
            Log.Debug("Calling SetSubtitleStreamEventCallback");
            dvbStreams.SetSubtitleStreamEventCallback(pSubStreamCallback);

            Log.Debug("End SubtitleSelector ctor");
        }

        private void RetrieveBitmapSubtitles()
        {
            bitmapSubtitleCache.Clear();
            try
            {
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
                    bitmapSubtitleCache.Add(option);
                    Log.Debug("Retrieved bitmap option Lang : " + option.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private int OnSubtitleStreamEvent(int eid, UInt64 evalue)
        {
            return 0;
            //lock (syncLock)
            //{
            //    if (eid == 0)
            //    {
            //        Log.Debug("SubtitleSelector: ISubtitleStream alerts of update");
            //        if (currentOption == autoSelectOption)
            //        {
            //            //ThreadStart rt = new ThreadStart(RetrieveBitmapSubtitles);
            //            //Thread t = new Thread(rt);
            //            //t.Start();
            //            //t.Join();
            //            RetrieveBitmapSubtitles();
            //            CheckForPreferedLanguage();
            //        }
            //    }
            //    else
            //    {
            //        Log.Error("OnSubtitleStreamUpdate: Unknown event id {0}", eid);
            //    }
            //    return 0;
            //}
        }

        private void OnPageInfo(TeletextPageEntry entry)
        {
            lock (syncLock)
            {
                if (!pageEntries.ContainsKey(entry.page))
                {
                    pageEntries.Add(entry.page, entry);
                    if (currentOption == autoSelectOption)
                    {
                        //Log.Debug("New subtitle page, check prefered");
                        CheckForPreferedLanguage();
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to auto choose a subtitle option
        /// based on the prefered languages
        /// </summary>
        private void CheckForPreferedLanguage()
        {
            Log.Debug("SubtitleSelector: CheckForPreferedLanguage");
            List<SubtitleOption> options = CollectOptions();

            SubtitleOption prefered = null;
            int priority = int.MaxValue;
            int prefOptIndex = -1;

            for (int optIndex = 1; optIndex < options.Count; optIndex++)
            {
                SubtitleOption opt = options[optIndex];

                int index = preferedLanguages.IndexOf(opt.language);

                if (index >= 0 && index < priority)
                {
                    prefered = opt;
                    priority = index;
                    prefOptIndex = optIndex;
                }
                optIndex++;
            }

            if (prefered != null && prefOptIndex != lastSubtitleIndex)
            { // FIXME: dont use index for comparison
                Log.Debug("Detected subtitle in prefered language : " + prefered.language);
                subRender.SetSubtitleOption(options[prefOptIndex]);
                lastSubtitleIndex = prefOptIndex;
                // dont change currentOption field, it should remain set to autoSelectOption
            }
        }

        private List<SubtitleOption> CollectOptions()
        {
            //Log.Debug("SubtitleSelector: CollectOptions");
            List<SubtitleOption> options = new List<SubtitleOption>();

            if (autoSelectOption != null)
            {
                options.Add(autoSelectOption);
            }

            options.AddRange(bitmapSubtitleCache);

            // collect teletext options
            foreach (KeyValuePair<int, TeletextPageEntry> p in pageEntries)
            {
                SubtitleOption option = new SubtitleOption();
                option.type = SubtitleType.Teletext;
                option.language = p.Value.language;
                option.entry = p.Value;
                options.Add(option);
                Log.Debug("Added Teletext option Lang : " + option.ToString());
            }
            return options;
        }


        public int CountOptions()
        {
            return CollectOptions().Count;
        }

        public int GetCurrentOption()
        {
            return lastSubtitleIndex;
        }

        public void SetOption(int index)
        {
            Log.Debug("SetOption {0}", index);
            List<SubtitleOption> options = CollectOptions();
            if (index >= options.Count)
            {
                Log.Error("SetOption with too large index!");
                return;
            }
            SubtitleOption option = options[index];
            lastSubtitleIndex = index;
            currentOption = option;
            
            if (option == autoSelectOption)
            {
                Log.Debug("SubtitleSelector : Set autoselect mode");
                CheckForPreferedLanguage();
                return; // nothing more to do
            }
            else if (option.type == SubtitleType.Bitmap)
            {
                dvbStreams.SetSubtitleStream(option.bitmapIndex);
            }
            Log.Debug("Subtitle is now " + currentOption.ToString());
            subRender.SetSubtitleOption(option);
        }

        public string GetCurrentLanguage()
        {
            if (currentOption == null)
            {
                Log.Error("Calling GetCurrentLanguage with no subtitle set!");
                return Strings.Unknown;
            }
            else if (currentOption.type == SubtitleType.Teletext && currentOption.entry.language.Trim().Length == 0)
            {
                return "p" + currentOption.entry.page;
            }
            else return currentOption.language;
        }

        private MediaPortal.Player.TSReaderPlayer.ISubtitleStream dvbStreams;
        private SubtitleRenderer subRender;
        private List<string> preferedLanguages;
        private int lastSubtitleIndex; // in auto mode this is auto selected index
        private SubtitleOption currentOption; // in auto mode this will be the auto option itself, not the autoselected option (really bad style :)
        private Dictionary<int, TeletextPageEntry> pageEntries;

        private List<SubtitleOption> bitmapSubtitleCache;
    }
}
