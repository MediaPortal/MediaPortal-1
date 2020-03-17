using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents boolean RTMP arbitrary data.
    /// </summary>
    internal class RtmpBooleanArbitraryData : RtmpArbitraryData
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpBoolenArbitraryData"/> class.
        /// </summary>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpBoolenArbitraryData"/> class.
        /// </overloads>
        public RtmpBooleanArbitraryData()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpBoolenArbitraryData"/> class with specified value.
        /// </summary>
        /// <param name="value">The specified boolean value.</param>
        public RtmpBooleanArbitraryData(Boolean value)
            : this(RtmpArbitraryData.DefaultName, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpBoolenArbitraryData"/> class with specified value and name.
        /// </summary>
        /// <param name="name">The name of arbitrary data.</param>
        /// <param name="value">The specified boolean value.</param>
        public RtmpBooleanArbitraryData(String name, Boolean value)
            : base(RtmpArbitraryDataType.Boolean, name)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value of boolean arbitrary data type.
        /// </summary>
        public Boolean Value { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the boolean arbitrary data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data.
        /// </returns>
        public override string ToString()
        {
            if (this.Name != RtmpArbitraryData.DefaultName)
            {
                return String.Format("conn=NB:{0}:{1}", this.Name, this.Value ? "1" : "0");
            }
            else
            {
                return String.Format("conn=B:{0}", this.Value ? "1" : "0");
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
            RtmpBooleanArbitraryData arbitraryData = null;

            if (parameter.StartsWith("conn="))
            {
                parameter = parameter.Substring(5);

                String[] splitted = parameter.Split(new String[] { ":" }, StringSplitOptions.None);

                if (splitted[0] == "NB")
                {
                    arbitraryData = new RtmpBooleanArbitraryData(splitted[1], (splitted[2] == "1"));

                    parameters.RemoveAt(0);
                }
                else if (splitted[0] == "B")
                {
                    arbitraryData = new RtmpBooleanArbitraryData((splitted[1] == "1"));

                    parameters.RemoveAt(0);
                }
            }

            return arbitraryData;
        }

        #endregion
    }
}
