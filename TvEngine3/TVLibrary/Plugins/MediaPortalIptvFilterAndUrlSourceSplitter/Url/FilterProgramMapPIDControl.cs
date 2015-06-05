using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    public partial class FilterProgramMapPIDControl : UserControl
    {
        public FilterProgramMapPIDControl()
        {
            InitializeComponent();
        }

        internal FilterProgramMapPID FilterProgramMapPID { get; set; }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                int pid = int.Parse(this.textBoxProgramElementPID.Text);

                if ((pid < 0) || (pid > ProgramElement.MaximumProgramElementPID))
                {
                    throw new ArgumentOutOfRangeException("Program element PID", pid, "The program element PID must be equal to or greater than zero and lower than 8192.");
                }

                this.errorProvider.SetError(this.textBoxProgramElementPID, String.Empty);

                Boolean found = false;
                foreach (var programElement in this.FilterProgramMapPID.ProgramElements)
                {
                    if (programElement.ProgramElementPID == pid)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    this.FilterProgramMapPID.ProgramElements.Add(new ProgramElement() { ProgramElementPID = pid, LeaveProgramElement = false });
                    this.FilterProgramMapPIDControl_Load(null, null);
                }
            }
            catch (Exception ex)
            {
                this.errorProvider.SetError(this.textBoxProgramElementPID, ex.ToString());
            }
        }

        private void FilterProgramMapPIDControl_Load(object sender, EventArgs e)
        {
            this.checkBoxAllowFilteringProgramElements.Checked = this.FilterProgramMapPID.AllowFilteringProgramElements;
            this.checkedListBoxLeaveProgramElements.Items.Clear();

            foreach (var programElement in this.FilterProgramMapPID.ProgramElements)
            {
                this.checkedListBoxLeaveProgramElements.Items.Add(programElement.ProgramElementPID.ToString(), programElement.LeaveProgramElement ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private void checkedListBoxLeaveProgramElements_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.FilterProgramMapPID.ProgramElements[e.Index].LeaveProgramElement = (e.NewValue == CheckState.Checked);
        }

        private void checkBoxAllowFilteringProgramElements_CheckedChanged(object sender, EventArgs e)
        {
            this.FilterProgramMapPID.AllowFilteringProgramElements = this.checkBoxAllowFilteringProgramElements.Checked;
        }
    }
}
