namespace MediaPortal.MPInstaller
{
  public interface IMPIInternalPlugin
  {
    /// <summary>
    /// Called when the installation start.
    /// </summary>
    /// <param name="pk">The loaded package class</param>
    /// <returns></returns>
    bool OnStartInstall(ref MPpackageStruct pk);

    /// <summary>
    /// Called when the installation end.
    /// </summary>
    /// <param name="pk">The loaded package class</param>
    /// <returns></returns>
    bool OnEndInstall(ref MPpackageStruct pk);

    /// <summary>
    /// Called when the unistallation start.
    /// </summary>
    /// <param name="pk">The loaded package class</param>
    /// <returns></returns>
    bool OnStartUnInstall(ref MPpackageStruct pk);

    /// <summary>
    /// Called when the unistallation end.
    /// </summary>
    /// <param name="pk">The loaded package class</param>
    /// <returns></returns>
    bool OnEndUnInstall(ref MPpackageStruct pk);
  }
}