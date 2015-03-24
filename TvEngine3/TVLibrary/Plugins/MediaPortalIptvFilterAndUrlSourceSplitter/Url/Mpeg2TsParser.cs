using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for MPEG2 Transport Stream parser.
    /// </summary>
    internal class Mpeg2TsParser : Parser
    {
        #region Private fields

        private int transportStreamID;
        private int programNumber;
        private int programMapPID;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Mpeg2TsParser"/> class.
        /// </summary>
        public Mpeg2TsParser()
            : base()
        {
            this.DetectDiscontinuity = Mpeg2TsParser.DefaultMpeg2TsDetectDiscontinuity;
            this.AlignToMpeg2TSPacket = Mpeg2TsParser.DefaultMpeg2TsAlignToMpeg2TSPacket;
            this.TransportStreamID = Mpeg2TsParser.DefaultMpeg2TsTransportStreamID;
            this.ProgramNumber = Mpeg2TsParser.DefaultMpeg2TsProgramNumber;
            this.ProgramMapPID = Mpeg2TsParser.DefaultMpeg2TsProgramMapPID;
            this.SetNotScrambled = Mpeg2TsParser.DefaultMpeg2TsSetNotScrambled;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Specifies if MPEG2 TS parser have to detect discontinuities in continuity counters.
        /// </summary>
        [Category("MPEG2 Transport Stream parser"), Description("Specifies if MPEG2 TS parser have to detect discontinuities in continuity counters."), DefaultValue(Mpeg2TsParser.DefaultMpeg2TsDetectDiscontinuity)]
        public Boolean DetectDiscontinuity { get; set; }

        /// <summary>
        /// Specifies if MPEG2 TS parser have to align stream to MPEG2 TS packet boundaries.
        /// </summary>
        [Category("MPEG2 Transport Stream parser"), Description("Specifies if MPEG2 TS parser have to align stream to MPEG2 TS packet boundaries. It is strongly recommended to have this option left in default configuration. Without properly aligned MPEG2 packets, other functions in parser will not work."), DefaultValue(Mpeg2TsParser.DefaultMpeg2TsAlignToMpeg2TSPacket)]
        public Boolean AlignToMpeg2TSPacket { get; set; }

        /// <summary>
        /// Specifies the value of transport stream ID in program association section (PAT).
        /// </summary>
        [Category("MPEG2 Transport Stream parser"), Description("The value of transport stream ID in program association section (PAT) or -1 if value don't have to be changed."), DefaultValue(Mpeg2TsParser.DefaultMpeg2TsTransportStreamID)]
        public int TransportStreamID
        {
            get { return this.transportStreamID; }
            set
            {
                if (((value < 0) || (value > 65535)) && (value != Mpeg2TsParser.DefaultMpeg2TsTransportStreamID))
                {
                    throw new ArgumentOutOfRangeException("TransportStreamID", value, "Must be greater than or equal to zero and lower than 65536.");
                }

                this.transportStreamID = value;
            }
        }

        /// <summary>
        /// Specifies the value of program number in program association section (PAT) and transport stream program map section (PMT).
        /// </summary>
        [Category("MPEG2 Transport Stream parser"), Description("The value of program number in program association section (PAT) and transport stream program map section (PMT) or -1 if value don't have to be changed."), DefaultValue(Mpeg2TsParser.DefaultMpeg2TsProgramNumber)]
        public int ProgramNumber
        {
            get { return this.programNumber; }
            set
            {
                if (((value < 0) || (value > 65535)) && (value != Mpeg2TsParser.DefaultMpeg2TsProgramNumber))
                {
                    throw new ArgumentOutOfRangeException("ProgramNumber", value, "Must be greater than or equal to zero and lower than 65536.");
                }

                this.programNumber = value;
            }
        }

        /// <summary>
        /// Specifies the value of program map PID in program association section (PAT) and PID of packet containing transport stream program section (PMT).
        /// </summary>
        [Category("MPEG2 Transport Stream parser"), Description("The value of program map PID in program association section (PAT) and PID of packet containing transport stream program section (PMT) or -1 if value don't have to be changed."), DefaultValue(Mpeg2TsParser.DefaultMpeg2TsProgramMapPID)]
        public int ProgramMapPID
        {
            get { return this.programMapPID; }
            set
            {
                if (((value < 0) || (value > 0x1FFF)) && (value != Mpeg2TsParser.DefaultMpeg2TsProgramMapPID))
                {
                    throw new ArgumentOutOfRangeException("ProgramMapPID", value, "Must be greater than or equal to zero and lower than 8192.");
                }

                this.programMapPID = value;
            }
        }

        /// <summary>
        /// Specifies if MPEG2 TS parser have to set stream as not scrambled.
        /// </summary>
        [Category("MPEG2 Transport Stream parser"), Description("Specifies if MPEG2 TS parser have to set stream as not scrambled."), DefaultValue(Mpeg2TsParser.DefaultMpeg2TsSetNotScrambled)]
        public Boolean SetNotScrambled { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Parses parameters from URL to current instance.
        /// </summary>
        /// <param name="parameters">The parameters from URL.</param>
        public override void Parse(ParameterCollection parameters)
        {
            foreach (var param in parameters)
            {
                if (String.CompareOrdinal(param.Name, Mpeg2TsParser.ParameterMpeg2TsDetectDiscontinuity) == 0)
                {
                    this.DetectDiscontinuity = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, Mpeg2TsParser.ParameterMpeg2TsAlignToMpeg2TSPacket) == 0)
                {
                    this.AlignToMpeg2TSPacket = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, Mpeg2TsParser.ParameterMpeg2TsTransportStreamID) == 0)
                {
                    this.TransportStreamID = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, Mpeg2TsParser.ParameterMpeg2TsProgramNumber) == 0)
                {
                    this.ProgramNumber = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, Mpeg2TsParser.ParameterMpeg2TsProgramMapPID) == 0)
                {
                    this.ProgramMapPID = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, Mpeg2TsParser.ParameterMpeg2TsSetNotScrambled) == 0)
                {
                    this.SetNotScrambled = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }
            }
        }

        /// <summary>
        /// Gets canonical string representation for the specified instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the unescaped canonical representation of the this instance.
        /// </returns>
        public override string ToString()
        {
            ParameterCollection parameters = new ParameterCollection();

            if (this.DetectDiscontinuity != Mpeg2TsParser.DefaultMpeg2TsDetectDiscontinuity)
            {
                parameters.Add(new Parameter(Mpeg2TsParser.ParameterMpeg2TsDetectDiscontinuity, this.DetectDiscontinuity ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }
            if (this.AlignToMpeg2TSPacket != Mpeg2TsParser.DefaultMpeg2TsAlignToMpeg2TSPacket)
            {
                parameters.Add(new Parameter(Mpeg2TsParser.ParameterMpeg2TsAlignToMpeg2TSPacket, this.AlignToMpeg2TSPacket ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }
            if (this.TransportStreamID != Mpeg2TsParser.DefaultMpeg2TsTransportStreamID)
            {
                parameters.Add(new Parameter(Mpeg2TsParser.ParameterMpeg2TsTransportStreamID, this.TransportStreamID.ToString()));
            }
            if (this.ProgramNumber != Mpeg2TsParser.DefaultMpeg2TsProgramNumber)
            {
                parameters.Add(new Parameter(Mpeg2TsParser.ParameterMpeg2TsProgramNumber, this.ProgramNumber.ToString()));
            }
            if (this.ProgramMapPID != Mpeg2TsParser.DefaultMpeg2TsProgramMapPID)
            {
                parameters.Add(new Parameter(Mpeg2TsParser.ParameterMpeg2TsProgramMapPID, this.ProgramMapPID.ToString()));
            }
            if (this.SetNotScrambled != Mpeg2TsParser.DefaultMpeg2TsSetNotScrambled)
            {
                parameters.Add(new Parameter(Mpeg2TsParser.ParameterMpeg2TsSetNotScrambled, this.SetNotScrambled ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }

            return parameters.FilterParameters;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies if MPEG2 TS parser have to detect discontinuities in continuity counters.
        /// </summary>
        protected static readonly String ParameterMpeg2TsDetectDiscontinuity = "Mpeg2TsDetectDiscontinuity";

        /// <summary>
        /// Specifies if MPEG2 TS parser have to align stream to MPEG2 TS packet boundaries.
        /// </summary>
        protected static readonly String ParameterMpeg2TsAlignToMpeg2TSPacket = "Mpeg2TsAlignToMpeg2TSPacket";

        /// <summary>
        /// Specifies the value of transport stream ID in program association section (PAT).
        /// </summary>
        protected static readonly String ParameterMpeg2TsTransportStreamID = "Mpeg2TsTransportStreamID";

        /// <summary>
        /// Specifies the value of program number in program association section (PAT) and transport stream program map section (PMT).
        /// </summary>
        protected static readonly String ParameterMpeg2TsProgramNumber = "Mpeg2TsProgramNumber";

        /// <summary>
        /// Specifies the value of program map PID in program association section (PAT) and PID of packet containing transport stream program section (PMT).
        /// </summary>
        protected static readonly String ParameterMpeg2TsProgramMapPID = "Mpeg2TsProgramMapPID";

        /// <summary>
        /// Specifies if MPEG2 TS parser have to set stream as not scrambled.
        /// </summary>
        protected static readonly String ParameterMpeg2TsSetNotScrambled = "Mpeg2TsSetNotScrambled";

        /// <summary>
        /// Default value for <see cref="ParameterMpeg2TsDetectDiscontinuity"/> parameter.
        /// </summary>
        public const Boolean DefaultMpeg2TsDetectDiscontinuity = true;

        /// <summary>
        /// Default value for <see cref="ParameterMpeg2TsAlignToMpeg2TSPacket"/> parameter.
        /// </summary>
        public const Boolean DefaultMpeg2TsAlignToMpeg2TSPacket = true;

        /// <summary>
        /// Default value for <see cref="ParameterMpeg2TsTransportStreamID"/> parameter.
        /// </summary>
        public const int DefaultMpeg2TsTransportStreamID = -1;

        /// <summary>
        /// Default value for <see cref="ParameterMpeg2TsProgramNumber"/> parameter.
        /// </summary>
        public const int DefaultMpeg2TsProgramNumber = -1;

        /// <summary>
        /// Default value for <see cref="ParameterMpeg2TsProgramMapPID"/> parameter.
        /// </summary>
        public const int DefaultMpeg2TsProgramMapPID = -1;

        /// <summary>
        /// Default value for <see cref="ParameterMpeg2TsSetNotScrambled"/> parameter.
        /// </summary>
        public const Boolean DefaultMpeg2TsSetNotScrambled = false;

        #endregion
    }
}
