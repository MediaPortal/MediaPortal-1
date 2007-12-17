using System;

namespace ProjectInfinity.MenuManager
{
  public interface IMenuItem
  {
    string Text { get; }
    string ImagePath { get; }
    void Accept(IMenuItemVisitor visitor);
    [Obsolete]
    void Execute();
  }
}