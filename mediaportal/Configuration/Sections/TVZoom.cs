using System;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class TVZoom : SectionSettings
  {
    private bool _init = false;

    public TVZoom()
      : this("TV Zoom")
    {
    }

    public TVZoom(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;
        //
        // Load all available aspect ratio
        //
        defaultZoomModeComboBox.Items.Clear();
        foreach (Geometry.Type item in Enum.GetValues(typeof(Geometry.Type)))
        {
          defaultZoomModeComboBox.Items.Add(Util.Utils.GetAspectRatio(item));
        }
        //
        // Change aspect ratio labels to the current core proj description
        //
        cbAllowNormal.Text = Util.Utils.GetAspectRatio(Geometry.Type.Normal);
        cbAllowOriginal.Text = Util.Utils.GetAspectRatio(Geometry.Type.Original);
        cbAllowZoom.Text = Util.Utils.GetAspectRatio(Geometry.Type.Zoom);
        cbAllowZoom149.Text = Util.Utils.GetAspectRatio(Geometry.Type.Zoom14to9);
        cbAllowStretch.Text = Util.Utils.GetAspectRatio(Geometry.Type.Stretch);
        cbAllowNonLinearStretch.Text = Util.Utils.GetAspectRatio(Geometry.Type.NonLinearStretch);
        cbAllowLetterbox.Text = Util.Utils.GetAspectRatio(Geometry.Type.LetterBox43);
        LoadSettings();
      }
    }

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        cbAllowNormal.Checked = xmlreader.GetValueAsBool("mytv", "allowarnormal", true);
        cbAllowOriginal.Checked = xmlreader.GetValueAsBool("mytv", "allowaroriginal", true);
        cbAllowZoom.Checked = xmlreader.GetValueAsBool("mytv", "allowarzoom", true);
        cbAllowZoom149.Checked = xmlreader.GetValueAsBool("mytv", "allowarzoom149", true);
        cbAllowStretch.Checked = xmlreader.GetValueAsBool("mytv", "allowarstretch", true);
        cbAllowNonLinearStretch.Checked = xmlreader.GetValueAsBool("mytv", "allowarnonlinear", true);
        cbAllowLetterbox.Checked = xmlreader.GetValueAsBool("mytv", "allowarletterbox", true);
        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("mytv", "defaultar", defaultZoomModeComboBox.Items[0].ToString());
        foreach (Geometry.Type item in Enum.GetValues(typeof(Geometry.Type)))
        {
          string currentAspectRatio = Util.Utils.GetAspectRatio(item);
          if (defaultAspectRatio == currentAspectRatio)
          {
            defaultZoomModeComboBox.SelectedItem = currentAspectRatio;
            break;
          }
        }
      }
    }

    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mytv", "defaultar", defaultZoomModeComboBox.SelectedItem);

        xmlwriter.SetValueAsBool("mytv", "allowarnormal", cbAllowNormal.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowaroriginal", cbAllowOriginal.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarzoom", cbAllowZoom.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarzoom149", cbAllowZoom149.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarstretch", cbAllowStretch.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarnonlinear", cbAllowNonLinearStretch.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarletterbox", cbAllowLetterbox.Checked);
      }
    }
  }
}
