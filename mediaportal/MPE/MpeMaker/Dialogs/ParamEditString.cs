using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using MpeCore.Classes;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
    public partial class ParamEditString : UserControl,IParamEdit
    {
        private SectionParam Param = new SectionParam();
        public ParamEditString()
        {
            InitializeComponent();
        }

        #region IParamEdit Members

        public void Set(SectionParam param)
        {
            Param = param;
            textBox1.Text = param.Value;
        }

        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Param.Value = textBox1.Text;
        }
    }
}
