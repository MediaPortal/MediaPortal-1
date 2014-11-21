using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents object RTMP arbitrary data.
    /// </summary>
    internal class RtmpObjectArbitraryData : RtmpArbitraryData
    {
        #region Private fields

        private RtmpArbitraryDataCollection objects;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpObjectArbitraryData"/> class.
        /// </summary>
        /// <param name="value">The specified object value.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpObjectArbitraryData"/> class.
        /// </overloads>
        public RtmpObjectArbitraryData()
            : this(RtmpArbitraryData.DefaultName)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpObjectArbitraryData"/> class with specified name.
        /// </summary>
        /// <param name="name">The name of arbitrary data.</param>
        public RtmpObjectArbitraryData(String name)
            : base(RtmpArbitraryDataType.Object, name)
        {
            this.objects = new RtmpArbitraryDataCollection();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the objects in this object arbitrary data type.
        /// </summary>
        [System.ComponentModel.Editor(typeof(RtmpArbitraryDataCollectionEditor), typeof(UITypeEditor))]
        public RtmpArbitraryDataCollection Objects
        {
            get { return this.objects; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the object arbitrary data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the canonical representation of the this arbitrary data.
        /// </returns>
        public override string ToString()
        {
            if (this.Name != RtmpArbitraryData.DefaultName)
            {
                if (String.IsNullOrEmpty(this.Objects.ToString()))
                {
                    return String.Format("conn=NO:{0}:1 conn=O:{0}:0", this.Name);
                }
                else
                {
                    return String.Format("conn=NO:{0}:1 {1} conn=NO:{0}:0", this.Name, this.Objects.ToString());
                }
            }
            else
            {
                if (String.IsNullOrEmpty(this.Objects.ToString()))
                {
                    return String.Format("conn=O:1 conn=O:0");
                }
                else
                {
                    return String.Format("conn=O:1 {0} conn=O:0", this.Objects.ToString());
                }
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
            RtmpObjectArbitraryData arbitraryData = null;

            if (parameter.StartsWith("conn="))
            {
                parameter = parameter.Substring(5);

                String[] splitted = parameter.Split(new String[] { ":" }, StringSplitOptions.None);

                if (splitted[0] == "NO")
                {
                    if (splitted[2] != "0")
                    {
                        arbitraryData = new RtmpObjectArbitraryData(splitted[1]);
                        parameters.RemoveAt(0);
                    }
                }
                else if (splitted[0] == "O")
                {
                    if (splitted[1] != "0")
                    {
                        arbitraryData = new RtmpObjectArbitraryData();
                        parameters.RemoveAt(0);
                    }
                }

                while ((arbitraryData != null) && (parameters.Count != 0))
                {
                    RtmpArbitraryData objectArbitraryData = RtmpArbitraryDataFactory.CreateArbitraryData(ref parameters);

                    if (objectArbitraryData == null)
                    {
                        break;
                    }

                    arbitraryData.Objects.Add(objectArbitraryData);
                }
            }

            return arbitraryData;
        }

        #endregion
    }
}
