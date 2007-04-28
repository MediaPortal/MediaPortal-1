namespace ProjectInfinity.Menu
{
  public interface IMenuItemVisitor
  {
    void Visit(IMenu menu);
    void Visit(IPluginItem plugin);
    void Visit(IMessageItem message);
  }
}