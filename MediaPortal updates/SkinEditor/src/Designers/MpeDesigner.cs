using System;

namespace Mpe.Designers
{
  public interface MpeDesigner
  {
    #region Properties

    /// <summary>
    /// This Property will return a name which identifies the resource being editted.
    /// </summary>
    /// <returns>A string identifying the resource being editted.</returns>
    string ResourceName { get; }

    bool AllowAdditions { get; }
    bool AllowDeletions { get; }

    #endregion

    #region Methods

    void Initialize();
    void Resume();
    void Pause();
    void Save();
    void Cancel();
    void Destroy();

    #endregion
  }

  #region Exception Classes

  public class DesignerException : Exception
  {
    public DesignerException(string msg) : base(msg)
    {
      //
    }

    public DesignerException(string msg, Exception inner) : base(msg, inner)
    {
      //
    }
  }

  #endregion
}