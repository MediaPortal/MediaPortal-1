using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Filter
{
    /// <summary>
    /// Represents class for MediaPortal IPTV filter and url source splitter errors.
    /// </summary>
    internal static class FilterError
    {
        /// <summary>
        /// Gets error description for specified error code.
        /// </summary>
        /// <param name="filterState">The instance of MediaPortal IPTV filter and url source splitter.</param>
        /// <param name="errorCode">The error code to get description.</param>
        /// <returns>Error description or <see cref="String.Empty"/> is unknown.</returns>
        public static String ErrorDescription(IFilterStateEx filterState, int errorCode)
        {
            return FilterError.ErrorDescription(filterState, errorCode, true);
        }

        /// <summary>
        /// Gets error description for specified error code.
        /// </summary>
        /// <param name="filterState">The instance of MediaPortal IPTV filter and url source splitter.</param>
        /// <param name="errorCode">The error code to get description.</param>
        /// <param name="translateOtherErrors"><see langword="true"/> if not filter error codes have to be translated, <see langword="false"/> otherwise.</param>
        /// <returns>Error description or <see cref="String.Empty"/> is unknown.</returns>
        public static String ErrorDescription(IFilterStateEx filterState, int errorCode, bool translateOtherErrors)
        {
            if (filterState != null)
            {
                Boolean isFilterError = false;
                int result = filterState.IsFilterError(out isFilterError, errorCode);

                if ((result == 0) && isFilterError)
                {
                    String description = String.Empty;

                    result = filterState.GetErrorDescription(errorCode, out description);

                    if (result == 0)
                    {
                        return String.IsNullOrEmpty(description) ? String.Empty : description.Trim();
                    }
                }

                if ((result == 0) && (!isFilterError) && translateOtherErrors)
                {
                    String description = DirectShowLib.DsError.GetErrorText(errorCode);

                    return String.IsNullOrEmpty(description) ? String.Empty : description.Trim();
                }
            }

            return String.Empty;
        }
    }
}
