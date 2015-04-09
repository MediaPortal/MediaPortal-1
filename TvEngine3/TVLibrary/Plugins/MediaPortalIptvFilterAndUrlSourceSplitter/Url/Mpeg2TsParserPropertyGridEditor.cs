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

                        foreach (var filterProgramMapPID in url.Mpeg2TsParser.FilterProgramMapPIDs)
                        {
                            FilterProgramMapPID filterPID = new FilterProgramMapPID();

                            filterPID.AllowFilteringProgramElements = true;
                            filterPID.ProgramMapPID = filterProgramMapPID.ProgramMapPID;

                            foreach (var leaveProgramElement in filterProgramMapPID.LeaveProgramElements)
                            {
                                filterPID.LeaveProgramElements.Add(new ProgramElement() { ProgramElementPID = leaveProgramElement.ProgramElementPID });
                            }

                            parserEditor.FilterProgramMapPIDs.Add(filterPID);
                        }

                        if (parserEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            url.Mpeg2TsParser.AlignToMpeg2TSPacket = parserEditor.AlignToMpeg2TSPacket;
                            url.Mpeg2TsParser.DetectDiscontinuity = parserEditor.DetectDiscontinuity;
                            url.Mpeg2TsParser.SetNotScrambled = parserEditor.SetNotScrambled;

                            url.Mpeg2TsParser.TransportStreamID = parserEditor.TransportStreamID;
                            url.Mpeg2TsParser.ProgramNumber = parserEditor.ProgramNumber;
                            url.Mpeg2TsParser.ProgramMapPID = parserEditor.ProgramMapPID;

                            url.Mpeg2TsParser.FilterProgramMapPIDs.Clear();

                            foreach (var filterProgramMapPID in parserEditor.FilterProgramMapPIDs)
                            {
                                if (filterProgramMapPID.AllowFilteringProgramElements)
                                {
                                    FilterProgramMapPID filterPID = new FilterProgramMapPID();

                                    filterPID.AllowFilteringProgramElements = true;
                                    filterPID.ProgramMapPID = filterProgramMapPID.ProgramMapPID;

                                    foreach (var leaveProgramElement in filterProgramMapPID.LeaveProgramElements)
                                    {
                                        filterPID.LeaveProgramElements.Add(new ProgramElement() { ProgramElementPID = leaveProgramElement.ProgramElementPID });
                                    }

                                    url.Mpeg2TsParser.FilterProgramMapPIDs.Add(filterPID);
                                }
                            }
                            //System.Diagnostics.Debugger.Launch();
                            //value = url.Mpeg2TsParser;
                        }
                    }
                }
            }

            return value;
        }
    }
}
