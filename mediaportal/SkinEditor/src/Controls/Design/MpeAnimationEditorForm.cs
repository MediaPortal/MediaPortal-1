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
    public MpeAnimationEditorForm(MpeAnimationType currentValue, MpeParser parser, IWindowsFormsEditorService editorService)
    {
      InitializeComponent();
    }

    public MpeAnimationType SelectedValue
    {
      get { return MpeAnimationType.None ; }
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
