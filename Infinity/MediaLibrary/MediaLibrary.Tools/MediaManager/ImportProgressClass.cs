using System;
using MediaLibrary;
using System.Collections.Generic;
using System.Text;

namespace MediaManager
{
    public class ImportProgressClass : IMLImportProgress
    {
        private ImportProgress ProgressForm;
        public ImportProgressClass(ImportProgress ProgressForm)
        {
            this.ProgressForm = ProgressForm;
        }

        public bool Progress(int PercentComplete, string Text)
        {
            return ProgressForm.ShowProgress(PercentComplete, Text);
        }
    }
}
