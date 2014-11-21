using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for creating RTMP arbitrary data objects.
    /// </summary>
    internal static class RtmpArbitraryDataFactory
    {
        #region Methods

        /// <summary>
        /// Creates RTMP arbitrary data object from parameters.
        /// </summary>
        /// <param name="parameters">The list of parameters to create RTMP arbitrary data.</param>
        /// <returns>RTMP arbitrary data object.</returns>
        /// <remarks>
        /// From list of parameters are removed parsed RTMP arbitrary data objects.
        /// </remarks>
        public static RtmpArbitraryData CreateArbitraryData(ref List<String> parameters)
        {
            RtmpArbitraryData result = null;

            if (result == null)
            {
                result = RtmpBooleanArbitraryData.Parse(ref parameters);
            }
            if (result == null)
            {
                result = RtmpNullArbitraryData.Parse(ref parameters);
            }
            if (result == null)
            {
                result = RtmpNumberArbitraryData.Parse(ref parameters);
            }
            if (result == null)
            {
                result = RtmpObjectArbitraryData.Parse(ref parameters);
            }
            if (result == null)
            {
                result = RtmpStringArbitraryData.Parse(ref parameters);
            }

            if (result == null)
            {
                // unknown parameter or end of RtmpObjectArbitraryData, skip
                parameters.RemoveAt(0);
            }

            return result;
        }

        #endregion
    }
}
