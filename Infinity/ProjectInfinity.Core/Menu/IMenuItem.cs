using System;

namespace ProjectInfinity.Menu
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