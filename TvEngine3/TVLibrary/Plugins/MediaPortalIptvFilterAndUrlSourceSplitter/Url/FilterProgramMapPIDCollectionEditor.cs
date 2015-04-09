using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    internal class FilterProgramMapPIDCollectionEditor : CollectionEditor
    {
        public FilterProgramMapPIDCollectionEditor()
            : base(typeof(FilterProgramMapPIDCollection))
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(FilterProgramMapPID) };
        }
    }
}
