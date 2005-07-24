using System;
//using System.Collections.Generic;
using System.Text;

namespace MediaPortal.TV.Database
{
    public class ProgramData
    {
        public string ChannelID = String.Empty;
        public string Title = String.Empty;
		public string SubTitle = String.Empty;
        public string Description = String.Empty;
		public string Month = String.Empty;
		public string Genre = String.Empty;
		public int Day = 0;
        public int[] StartTime;
        public int[] EndTime;

    }
}
