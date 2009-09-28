using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using MpeCore.Classes;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
    public partial class ParamEditFile : UserControl, IParamEdit
    {
        private SectionParam Param;

        public ParamEditFile()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txt_file.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txt_file.Text = openFileDialog1.FileName;
            }
        }

        #region IParamEdit Members

        public void Set(SectionParam param)
        {
            Param = param;
            txt_file.Text = param.Value;
        }

        #endregion

        private void txt_file_TextChanged(object sender, EventArgs e)
        {
            Param.Value = txt_file.Text;
        }
    }
}
