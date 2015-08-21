using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url;
using TvDatabase;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    internal enum FilterUrlState
    {
        NotTested,
        Pending,
        Failed,
        Correct
    }

    internal class FilterUrl
    {
        #region Private fields
        #endregion

        #region Constructors

        public FilterUrl()
        {
            this.ChannelName = String.Empty;
            this.Url = null;
            this.PlaylistUrl = String.Empty;
            this.TransportStreamId = Mpeg2TsParser.DefaultMpeg2TsTransportStreamID;
            this.ProgramNumber = Mpeg2TsParser.DefaultMpeg2TsProgramNumber;
            this.ProgramMapPID = Mpeg2TsParser.DefaultMpeg2TsProgramMapPID;
            this.Detail = null;
            this.State = FilterUrlState.NotTested;
            this.Error = String.Empty;
        }

        #endregion

        #region Properties

        /* common fields */

        public String ChannelName { get; set; }

        public SimpleUrl Url { get; set; }

        /* playlist fields */

        public String PlaylistUrl { get; set; }

        /* database fields */

        public int TransportStreamId { get; set; }

        public int ProgramNumber { get; set; }

        public int ProgramMapPID { get; set; }

        public TuningDetail Detail { get; set; }

        /* test state */

        public FilterUrlState State { get; set; }

        public String Error { get; set; }

        #endregion

        #region Methods
        #endregion
    }
}
