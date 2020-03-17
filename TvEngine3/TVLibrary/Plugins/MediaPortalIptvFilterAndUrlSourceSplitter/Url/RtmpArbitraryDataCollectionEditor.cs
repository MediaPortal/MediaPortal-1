using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    internal class RtmpArbitraryDataCollectionEditor : CollectionEditor
    {
        public RtmpArbitraryDataCollectionEditor()
            : base(typeof(RtmpArbitraryDataCollection))
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(RtmpBooleanArbitraryData), typeof(RtmpNullArbitraryData), typeof(RtmpNumberArbitraryData), typeof(RtmpObjectArbitraryData), typeof(RtmpStringArbitraryData) };
        }
    }
}
