using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class to edit MPEG2 TS parser.
    /// </summary>
    internal class Mpeg2TsParserPropertyGridEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                return UITypeEditorEditStyle.Modal;
            }

            return base.GetEditStyle(context);
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context != null) && (context.Instance != null) && (provider != null))
            {
                IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

                if (editorService != null)
                {
                    SimpleUrl url = (SimpleUrl)context.Instance;

                    using (Mpeg2TsParserEditor parserEditor = new Mpeg2TsParserEditor())
                    {
                        parserEditor.AlignToMpeg2TSPacket = url.Mpeg2TsParser.AlignToMpeg2TSPacket;
                        parserEditor.DetectDiscontinuity = url.Mpeg2TsParser.DetectDiscontinuity;
                        parserEditor.SetNotScrambled = url.Mpeg2TsParser.SetNotScrambled;

                        parserEditor.TransportStreamID = url.Mpeg2TsParser.TransportStreamID;
                        parserEditor.ProgramNumber = url.Mpeg2TsParser.ProgramNumber;
                        parserEditor.ProgramMapPID = url.Mpeg2TsParser.ProgramMapPID;

                        foreach (var section in url.Mpeg2TsParser.Sections)
                        {
                            parserEditor.Sections.Add(section);
                        }

                        foreach (var filterProgramNumber in url.Mpeg2TsParser.FilterProgramNumbers)
                        {
                            FilterProgramNumber filter = new FilterProgramNumber(filterProgramNumber.ProgramNumber);

                            filter.AllowFilteringProgramElements = true;

                            foreach (var leaveProgramElement in filterProgramNumber.ProgramElements)
                            {
                                filter.ProgramElements.Add(new ProgramElement() { ProgramElementPID = leaveProgramElement.ProgramElementPID, LeaveProgramElement = true });
                            }

                            parserEditor.FilterProgramNumbers.Add(filter);
                        }

                        if (parserEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            Mpeg2TsParser parser = new Mpeg2TsParser();

                            parser.AlignToMpeg2TSPacket = parserEditor.AlignToMpeg2TSPacket;
                            parser.DetectDiscontinuity = parserEditor.DetectDiscontinuity;
                            parser.SetNotScrambled = parserEditor.SetNotScrambled;

                            parser.TransportStreamID = parserEditor.TransportStreamID;
                            parser.ProgramNumber = parserEditor.ProgramNumber;
                            parser.ProgramMapPID = parserEditor.ProgramMapPID;

                            parser.FilterProgramNumbers.Clear();

                            foreach (var filterProgramNumber in parserEditor.FilterProgramNumbers)
                            {
                                if (filterProgramNumber.AllowFilteringProgramElements)
                                {
                                    FilterProgramNumber filter = new FilterProgramNumber(filterProgramNumber.ProgramNumber);

                                    filter.AllowFilteringProgramElements = true;

                                    foreach (var programElement in filterProgramNumber.ProgramElements)
                                    {
                                        if (programElement.LeaveProgramElement)
                                        {
                                            filter.ProgramElements.Add(new ProgramElement() { ProgramElementPID = programElement.ProgramElementPID, LeaveProgramElement = true });
                                        }
                                    }

                                    parser.FilterProgramNumbers.Add(filter);
                                }
                            }

                            value = parser;
                        }
                    }
                }
            }

            return value;
        }
    }
}
