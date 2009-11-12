using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
    public partial class ParamEdit : Form
    {
        private SectionParam SelectedItem = null;
        private SectionParamCollection Params;
        private Dictionary<ValueTypeEnum, IParamEdit> Panels = new Dictionary<ValueTypeEnum, IParamEdit>();

        public ParamEdit()
        {
            InitializeComponent();
            Panels.Add(ValueTypeEnum.File,new ParamEditFile());
            Panels.Add(ValueTypeEnum.String, new ParamEditString());
            Panels.Add(ValueTypeEnum.Template, new ParamEditTemplate());
            Panels.Add(ValueTypeEnum.Bool, new ParamEditBool());
            Panels.Add(ValueTypeEnum.Script, new ParamEditScript());
        }


        public void Set(SectionParamCollection paramCollection)
        {
            Params = paramCollection;
            foreach(SectionParam param in Params.Items)
            {
                cmb_params.Items.Add(param);
            }
            cmb_params.SelectedIndex = 0;
        }

        private void cmb_params_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedItem = cmb_params.SelectedItem as SectionParam;
            panel1.Controls.Clear();
            Panels[SelectedItem.ValueType].Set(SelectedItem);
            panel1.Controls.Add((Control)Panels[SelectedItem.ValueType]);
            label_desc.Text = SelectedItem.Description;
        }

    }
}
