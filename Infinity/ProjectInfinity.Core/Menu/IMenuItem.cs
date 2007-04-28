namespace ProjectInfinity.Menu
{
  public interface IMenuItem
  {
    string Text { get; }
    void Accept(IMenuItemVisitor visitor);
  }
}