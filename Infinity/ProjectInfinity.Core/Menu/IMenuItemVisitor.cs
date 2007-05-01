using System;

namespace ProjectInfinity.Menu
{
  public interface IMenuItemVisitor
  {
    void Visit(IMenu menu);
    void Visit(IPluginItem plugin);
    void Visit(IMessageItem message);
    void Visit(ICommandItem command);

    [Obsolete("This overload should not be used. Use one of the specific overloads instead")]
    void Visit(IMenuItem item);
  }
}