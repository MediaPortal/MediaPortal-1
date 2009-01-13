namespace WindowPlugins.VideoEditor
{
  internal enum EditType
  {
    Join,
    Cut,
    Convert,
    Compress,
  }

  internal class EditSettings
  {
    private string fileName;
    private object settings;
    private bool deleteAfter;
    private EditType type;

    public EditSettings(object setting)
    {
      this.settings = setting;
    }

    public object Settings
    {
      get { return settings; }
    }

    public string FileName
    {
      get { return fileName; }
      set { fileName = value; }
    }

    public bool DeleteAfter
    {
      get { return deleteAfter; }
      set { deleteAfter = value; }
    }

    public EditType Type
    {
      get { return type; }
      set { type = value; }
    }
  }
}