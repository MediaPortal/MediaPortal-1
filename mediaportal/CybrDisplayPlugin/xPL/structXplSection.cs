namespace xPL
{
    using MediaPortal.GUI.Library;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct structXplSection
    {
        public string Section;
        public List<structXPLMsg> Details;
        public structXplSection(string SectionName)
        {
            this.Section = SectionName;
            try
            {
                this.Details = new List<structXPLMsg>();
                this.Details.Clear();
            }
            catch
            {
                this.Details = null;
                Log.Info("xPL.structXplSection(constructor): caught exception", new object[0]);
            }
        }
    }
}

