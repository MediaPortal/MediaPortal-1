using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Specifies base type for all arbitrary data for RTMP protocol.
    /// </summary>
    internal abstract class RtmpArbitraryData
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpArbitraryData"/> class with specified arbitrary data type.
        /// </summary>
        /// <param name="dataType">The specified arbitrary data type.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpArbitraryData"/> class.
        /// </overloads>
        public RtmpArbitraryData(RtmpArbitraryDataType dataType)
            : this(dataType, RtmpArbitraryData.DefaultName)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpArbitraryData"/> class with specified arbitrary data type and name.
        /// </summary>
        /// <param name="dataType">The specified arbitrary data type.</param>
        /// <param name="name">The arbitrary data name.</param>
        public RtmpArbitraryData(RtmpArbitraryDataType dataType, String name)
        {
            this.DataType = dataType;
            this.Name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets arbitrary data type.
        /// </summary>
        /// <remarks>
        /// Correct arbitrary data type is set in constructor of each arbitrary data type.
        /// </remarks>
        public RtmpArbitraryDataType DataType { get; protected set; }

        /// <summary>
        /// Gets or sets the name of arbitrary data.
        /// </summary>
        public String Name { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the specified arbitrary data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// <para>Always thrown.</para>
        /// </exception>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Constants

        /// <summary>
        /// The default name of arbitrary data.
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="null"/>.
        /// </remarks>
        public static String DefaultName = null;

        #endregion
    }
}
