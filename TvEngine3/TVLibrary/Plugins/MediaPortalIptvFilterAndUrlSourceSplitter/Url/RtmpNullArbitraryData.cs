using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents null RTMP arbitrary data.
    /// </summary>
    internal class RtmpNullArbitraryData : RtmpArbitraryData
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpNullArbitraryData"/> class.
        /// </summary>
        /// <param name="value">The specified object value.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpNullArbitraryData"/> class.
        /// </overloads>
        public RtmpNullArbitraryData()
            : this(RtmpArbitraryData.DefaultName)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpNullArbitraryData"/> class with specified name.
        /// </summary>
        /// <param name="name">The name of arbitrary data.</param>
        public RtmpNullArbitraryData(String name)
            : base(RtmpArbitraryDataType.Null, name)
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the null arbitrary data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data.
        /// </returns>
        public override string ToString()
        {
            if (this.Name != RtmpArbitraryData.DefaultName)
            {
                return String.Format("conn=NZ:{0}:", this.Name);
            }
            else
            {
                return String.Format("conn=Z:");
            }
        }

        /// <summary>
        /// Parses arbitrary data parameters for RTMP arbitrary data object.
        /// </summary>
        /// <param name="parameters">The list of parameters to parse.</param>
        /// <returns>An RTMP arbitrary data object or <see langword="null"/> if not RTMP arbitrary data object of this type.</returns>
        /// <remarks>
        /// From list of parameters is removed parsed RTMP arbitrary data object.
        /// </remarks>
        public static RtmpArbitraryData Parse(ref List<String> parameters)
        {
            String parameter = parameters[0];
            RtmpNullArbitraryData arbitraryData = null;

            if (parameter.StartsWith("conn="))
            {
                parameter = parameter.Substring(5);

                String[] splitted = parameter.Split(new String[] { ":" }, StringSplitOptions.None);

                if (splitted[0] == "NZ")
                {
                    arbitraryData = new RtmpNullArbitraryData(splitted[1]);

                    parameters.RemoveAt(0);
                }
                else if (splitted[0] == "Z")
                {
                    arbitraryData = new RtmpNullArbitraryData();

                    parameters.RemoveAt(0);
                }
            }

            return arbitraryData;
        }

        #endregion
    }
}
