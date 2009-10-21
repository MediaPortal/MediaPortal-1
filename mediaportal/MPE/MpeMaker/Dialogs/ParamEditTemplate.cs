using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using MpeCore.Classes;
using System.Windows.Forms;

namespace MpeMaker.Dialogs
{
    public partial class ParamEditTemplate : UserControl,IParamEdit
    {
        private SectionParam Param = new SectionParam();
        public ParamEditTemplate()
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

        private void button1_Click(object sender, EventArgs e)
        {
            PathTemplateSelector dlg = new PathTemplateSelector();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dlg.Result ;
            }
        }
    }
}
