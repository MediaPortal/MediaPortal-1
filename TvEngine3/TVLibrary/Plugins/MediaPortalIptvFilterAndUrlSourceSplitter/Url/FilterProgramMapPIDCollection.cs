using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents collection of objects for filtering program elements in transport stream program map with specified PIDs.
    /// </summary>
    internal class FilterProgramMapPIDCollection : Collection<FilterProgramMapPID>
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FilterProgramMapPIDCollection"/> class.
        /// </summary>
        public FilterProgramMapPIDCollection()
            : base()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        /// <summary>
        /// Inserts a filter program map PID into collection at specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The filter program map PID to insert.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <paramref name="item"/> is <see langword="null"/>.</para>
        /// </exception>
        protected override void InsertItem(int index, FilterProgramMapPID item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the filter program map PID at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to replace.param>
        /// <param name="item">The new value for the filter program map PID at the specified index.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <paramref name="item"/> is <see langword="null"/>.</para>
        /// </exception>
        protected override void SetItem(int index, FilterProgramMapPID item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.SetItem(index, item);
        }

        ///// <summary>
        ///// Gets canonical string representation for the arbitrary data collection.
        ///// </summary>
        ///// <returns>
        ///// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data collection.
        ///// </returns>
        ///// <remarks>
        ///// Returns empty string ("") if no arbitrary data is in collection.
        ///// </remarks>
        //public override string ToString()
        //{
        //    StringBuilder builder = new StringBuilder();

        //    foreach (var arbitraryData in this)
        //    {
        //        builder.AppendFormat((builder.Length == 0) ? "{0}" : "{1}{0}", arbitraryData.ToString(), RtmpArbitraryDataCollection.ArbitraryDataSeparator);
        //    }

        //    return builder.ToString();
        //}

        ///// <summary>
        ///// Parses arbitrary data collection parameter from URL to current instance.
        ///// </summary>
        ///// <param name="parameter">The parameter to parse.</param>
        //public virtual void Parse(String parameter)
        //{
        //    List<String> splitted = parameter.Split(new String[] { RtmpArbitraryDataCollection.ArbitraryDataSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();

        //    while (splitted.Count != 0)
        //    {
        //        RtmpArbitraryData objectArbitraryData = RtmpArbitraryDataFactory.CreateArbitraryData(ref splitted);

        //        if (objectArbitraryData == null)
        //        {
        //            break;
        //        }

        //        this.Add(objectArbitraryData);
        //    }
        //}

        #endregion

        #region Constants

        ///// <summary>
        ///// Specifies arbitrary data separator.
        ///// </summary>
        //public static String ArbitraryDataSeparator = " ";

        #endregion
    }
}
