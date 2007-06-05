namespace ProjectInfinity.Services
{
  public delegate T ServiceCreatorCallback<T>(ServiceScope scope);
}