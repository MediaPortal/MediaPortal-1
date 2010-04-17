using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls.Design
{
  public partial class MpeAnimationEditorForm : UserControl
  {
    protected MpeAnimationType _anim;
    public MpeAnimationEditorForm(MpeAnimationType currentValue, MpeParser parser, IWindowsFormsEditorService editorService)
    {
      InitializeComponent();
      MpeAnimationBaseType mp = new MpeAnimationBaseType();
      _anim = currentValue;
      comboBox1.SelectedIndex = 0;
      propertyGrid1.SelectedObject = _anim.Animation[comboBox1.SelectedIndex];
      if (_anim.Animation[comboBox1.SelectedIndex].Enabled)
      {
        checkBox1.Checked = true;
        propertyGrid1.Enabled = true;
      }
      else
      {
        checkBox1.Checked = false;
        propertyGrid1.Enabled = false;
      }


    }

    public MpeAnimationType SelectedValue
    {
      get { return _anim ; }
    }

    private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
    {
      if (checkBox1.Checked)
      {
        propertyGrid1.Enabled = true;
        _anim.Animation[comboBox1.SelectedIndex].Enabled = true;
      }
      else
      {
        propertyGrid1.Enabled = false;
        _anim.Animation[comboBox1.SelectedIndex].Enabled = false; ;
      }
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      propertyGrid1.SelectedObject = _anim.Animation[comboBox1.SelectedIndex];
      if (_anim.Animation[comboBox1.SelectedIndex].Enabled)
        checkBox1.Checked = true;
      else
        checkBox1.Checked = false;
    }
  }

  #region MpeAnimationEditor

  public class MpeAnimationEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      if (context.Instance is MpeControl)
      {
        try
        {
          MpeControl mpc = (MpeControl)context.Instance;
          IWindowsFormsEditorService editorService =
            (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
          MpeAnimationEditorForm selector = new MpeAnimationEditorForm((MpeAnimationType)value, mpc.Parser, editorService);
          editorService.DropDownControl(selector);
          return selector.SelectedValue;
        }
        catch (Exception ee)
        {
          MpeLog.Debug(ee);
          MpeLog.Error(ee);
        }
      }
      else if (context.Instance is MpeItem)
      {
        try
        {
          IWindowsFormsEditorService editorService =
            (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
          MpeAnimationEditorForm selector =
            new MpeAnimationEditorForm((MpeAnimationType)value, MediaPortalEditor.Global.Parser, editorService);
          editorService.DropDownControl(selector);
          return selector.SelectedValue;
        }
        catch (Exception ee)
        {
          MpeLog.Debug(ee);
          MpeLog.Error(ee);
        }
      }
      return base.EditValue(context, provider, value);
    }
  }

  #endregion
}
