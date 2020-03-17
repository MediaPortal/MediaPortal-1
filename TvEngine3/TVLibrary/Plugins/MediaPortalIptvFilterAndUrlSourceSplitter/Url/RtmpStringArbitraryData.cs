using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents string RTMP arbitrary data.
    /// </summary>
    internal class RtmpStringArbitraryData : RtmpArbitraryData
    {
        #region Private fields

        private String value;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpStringArbitraryData"/> class.
        /// </summary>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpStringArbitraryData"/> class.
        /// </overloads>
        public RtmpStringArbitraryData()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpStringArbitraryData"/> class with specified value.
        /// </summary>
        /// <param name="value">The specified string value.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="value"/> is <see langword="null"/>.</para>
        /// </exception>
        public RtmpStringArbitraryData(String value)
            : this(RtmpArbitraryData.DefaultName, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpStringArbitraryData"/> class with specified value and name.
        /// </summary>
        /// <param name="name">The name of arbitrary data.</param>
        /// <param name="value">The specified string value.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="value"/> is <see langword="null"/>.</para>
        /// </exception>
        public RtmpStringArbitraryData(String name, String value)
            : base(RtmpArbitraryDataType.String, name)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value of number arbitrary data type.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="Value"/> is <see langword="null"/>.</para>
        /// </exception>
        public String Value
        {
            get { return this.value; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Value");
                }

                this.value = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Encodes string value to be correct for MediaPortal IPTV Source Filter.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the eencoded string value this arbitrary data.
        /// </returns>
        protected virtual String EncodeValue()
        {
            return this.Value.Replace("\\", "\\5c").Replace(" ", "\\20");
        }

        /// <summary>
        /// Gets canonical string representation for the string arbitrary data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data.
        /// </returns>
        public override string ToString()
        {
            if (this.Name != RtmpArbitraryData.DefaultName)
            {
                return String.Format("conn=NS:{0}:{1}", this.Name, this.EncodeValue());
            }
            else
            {
                return String.Format("conn=S:{0}", this.EncodeValue());
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
            RtmpStringArbitraryData arbitraryData = null;

            if (parameter.StartsWith("conn="))
            {
                parameter = parameter.Substring(5);

                String[] splitted = parameter.Split(new String[] { ":" }, StringSplitOptions.None);

                if (splitted[0] == "NS")
                {
                    arbitraryData = new RtmpStringArbitraryData(splitted[1], splitted[2]);

                    parameters.RemoveAt(0);
                }
                else if (splitted[0] == "S")
                {
                    arbitraryData = new RtmpStringArbitraryData(splitted[1]);

                    parameters.RemoveAt(0);
                }
            }

            return arbitraryData;
        }

        #endregion
    }
}
