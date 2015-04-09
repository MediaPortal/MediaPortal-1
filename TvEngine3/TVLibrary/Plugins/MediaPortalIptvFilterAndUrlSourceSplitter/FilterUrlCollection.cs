using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    internal sealed class FilterUrlCollection : KeyedCollection<String, FilterUrl>
    {
        protected override string GetKeyForItem(FilterUrl item)
        {
            return item.ChannelName;
        }
    }
}
