using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents base class for Adobe Flash HTTP Streaming protocol described by manifest file.
    /// </summary>
    internal class AfhsManifestUrl : HttpUrl
    {
        #region Private fields

        private String segmentFragmentUrlExtraParameters;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AfhsManifestUrl"/> class.
        /// </summary>
        /// <param name="url">The URL of manifest to initialize.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="AfhsManifestUrl"/> class.
        /// </overloads>
        public AfhsManifestUrl(String url)
            : this(new Uri(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AfhsManifestUrl"/> class.
        /// </summary>
        /// <param name="uri">The uniform resource identifier with manifest URL.</param>
        /// <exception cref="ArgumentException">
        /// <para>The protocol supplied by <paramref name="uri"/> is not supported.</para>
        /// </exception>
        public AfhsManifestUrl(Uri uri)
            : base(uri)
        {
            this.SegmentFragmentUrlExtraParameters = AfhsManifestUrl.DefaultSegmentFragmentUrlExtraParameters;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets segment and fragment extra parameters attached to each segment and fragment URL. Segment and fragment extra parameters should start with '?'.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="ExtraParameters"/> is <see langword="null"/>.</para>
        /// </exception>
        public String SegmentFragmentUrlExtraParameters
        {
            get { return this.segmentFragmentUrlExtraParameters; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("SegmentFragmentUrlExtraParameters");
                }

                this.segmentFragmentUrlExtraParameters = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the specified instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the unescaped canonical representation of the this instance.
        /// </returns>
        public override string ToString()
        {
            ParameterCollection parameters = new ParameterCollection();

            if (this.SegmentFragmentUrlExtraParameters != AfhsManifestUrl.DefaultSegmentFragmentUrlExtraParameters)
            {
                parameters.Add(new Parameter(AfhsManifestUrl.ParameterSegmentFragmentUrlExtraParameters, this.SegmentFragmentUrlExtraParameters));
            }

            // return formatted connection string
            return base.ToString() + ParameterCollection.ParameterSeparator + parameters.FilterParameters;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies segment and fragment extra parameters added to each segment and fragment for AFHS protocol.
        /// </summary>
        protected static String ParameterSegmentFragmentUrlExtraParameters = "AfhsSegmentFragmentUrlExtraParameters";

        // default values for some parameters

        /// <summary>
        /// Default segment and fragment extra parameters.
        /// </summary>
        /// <remarks>
        /// This value is <see cref="System.String.Empty"/>.
        /// </remarks>
        public static String DefaultSegmentFragmentUrlExtraParameters = String.Empty;

        #endregion
    }
}
