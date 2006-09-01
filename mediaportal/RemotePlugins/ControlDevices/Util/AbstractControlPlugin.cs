using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;
using MediaPortal.ControlDevices;

namespace MediaPortal.ControlDevices
{
  public abstract class AbstractControlPlugin
  {
    protected IControlSettings _settings;
    protected string _dllPath = string.Empty;
    protected ILog _log;

    public AbstractControlPlugin()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

  }
}
