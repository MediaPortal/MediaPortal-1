using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents number RTMP arbitrary data.
    /// </summary>
    internal class RtmpNumberArbitraryData : RtmpArbitraryData
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpNumberArbitraryData"/> class.
        /// </summary>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpNumberArbitraryData"/> class.
        /// </overloads>
        public RtmpNumberArbitraryData()
            : this(0.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpNumberArbitraryData"/> class with specified value.
        /// </summary>
        /// <param name="value">The specified number value.</param>
        public RtmpNumberArbitraryData(double value)
            : this(RtmpArbitraryData.DefaultName, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpNumberArbitraryData"/> class with specified value and name.
        /// </summary>
        /// <param name="name">The name of arbitrary data.</param>
        /// <param name="value">The specified number value.</param>
        public RtmpNumberArbitraryData(String name, double value)
            : base(RtmpArbitraryDataType.Number, name)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value of number arbitrary data type.
        /// </summary>
        public double Value { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the number arbitrary data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data.
        /// </returns>
        public override string ToString()
        {
            if (this.Name != RtmpArbitraryData.DefaultName)
            {
                return String.Format("conn=NN:{0}:{1}", this.Name, this.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                return String.Format("conn=N:{0}", this.Value.ToString(CultureInfo.InvariantCulture));
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
            RtmpNumberArbitraryData arbitraryData = null;

            if (parameter.StartsWith("conn="))
            {
                parameter = parameter.Substring(5);

                String[] splitted = parameter.Split(new String[] { ":" }, StringSplitOptions.None);

                if (splitted[0] == "NN")
                {
                    arbitraryData = new RtmpNumberArbitraryData(splitted[1], double.Parse(splitted[2], CultureInfo.InvariantCulture));

                    parameters.RemoveAt(0);
                }
                else if (splitted[0] == "N")
                {
                    arbitraryData = new RtmpNumberArbitraryData(double.Parse(splitted[1], CultureInfo.InvariantCulture));

                    parameters.RemoveAt(0);
                }
            }

            return arbitraryData;
        }

        #endregion
    }
}
