using ProjectInfinity.Plugins;

namespace ProjectInfinity.Tests.Plugins.Mocks
{
  /// <summary>
  /// Dummy plug-in for testing purposes
  /// </summary>
  [Plugin("DummyPlugin", "Dummy plug-in for testing purposes")]
  public class DummyPlugin : IPlugin
  {
    private bool _isInitialized = false;
    private bool _isDisposed = false;

    public bool IsInitialized
    {
      get { return _isInitialized; }
    }

    public bool IsDisposed
    {
      get { return _isDisposed; }
    }

    public void Reset()
    {
      _isInitialized = false;
      _isDisposed = false;
    }

    #region IPlugin Members

    public void Initialize()
    {
      _isInitialized = true;
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      _isInitialized = false;
      _isDisposed = true;
    }

    #endregion
  }
}