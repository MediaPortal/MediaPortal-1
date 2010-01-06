using System;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class MovieZoom : SectionSettings
  {
    private bool _init = false;

    public MovieZoom()
      : this("Video Zoom") {}

    public MovieZoom(string name)
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
        foreach (Geometry.Type item in Enum.GetValues(typeof (Geometry.Type)))
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
        cbAllowNormal.Checked = xmlreader.GetValueAsBool("movies", "allowarnormal", true);
        cbAllowOriginal.Checked = xmlreader.GetValueAsBool("movies", "allowaroriginal", true);
        cbAllowZoom.Checked = xmlreader.GetValueAsBool("movies", "allowarzoom", true);
        cbAllowZoom149.Checked = xmlreader.GetValueAsBool("movies", "allowarzoom149", true);
        cbAllowStretch.Checked = xmlreader.GetValueAsBool("movies", "allowarstretch", true);
        cbAllowNonLinearStretch.Checked = xmlreader.GetValueAsBool("movies", "allowarnonlinear", true);
        cbAllowLetterbox.Checked = xmlreader.GetValueAsBool("movies", "allowarletterbox", true);

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("movieplayer", "defaultar",
                                                               defaultZoomModeComboBox.Items[0].ToString());
        foreach (Geometry.Type item in Enum.GetValues(typeof (Geometry.Type)))
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
        xmlwriter.SetValue("movieplayer", "defaultar", defaultZoomModeComboBox.SelectedItem);

        xmlwriter.SetValueAsBool("movies", "allowarnormal", cbAllowNormal.Checked);
        xmlwriter.SetValueAsBool("movies", "allowaroriginal", cbAllowOriginal.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarzoom", cbAllowZoom.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarzoom149", cbAllowZoom149.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarstretch", cbAllowStretch.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarnonlinear", cbAllowNonLinearStretch.Checked);
        xmlwriter.SetValueAsBool("movies", "allowarletterbox", cbAllowLetterbox.Checked);
      }
    }
  }
}