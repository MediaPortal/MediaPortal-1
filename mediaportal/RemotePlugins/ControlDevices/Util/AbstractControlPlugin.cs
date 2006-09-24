using MediaPortal.Services;
using MediaPortal.Utils.Services;
using MediaPortal.ControlDevices;

namespace MediaPortal.ControlDevices
{
  public abstract class AbstractControlPlugin
  {
    private string _libraryName = string.Empty;
    protected IControlSettings _settings;
    protected string _dllPath = string.Empty;
    protected ILog _log;

    public AbstractControlPlugin()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    public string LibraryName 
    { 
      set { _libraryName = value; }
      get { return _libraryName; }
    }

    public void Initialize() 
    {
    }

  }
}
