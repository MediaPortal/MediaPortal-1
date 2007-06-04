using System;
using System.Globalization;

namespace ProjectInfinity
{
  /// <summary>
  /// Occurs when requested service is not found in the current- or one of its parent <see cref="ServiceScope"/>s.
  /// </summary>
  [Serializable]
  public class ServiceNotFoundException : Exception
  {
    private Type serviceType;

    public ServiceNotFoundException()
    {
    }

    public ServiceNotFoundException(string message)
      : base(message)
    {
    }

    public ServiceNotFoundException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ServiceNotFoundException"/> instance, and initializes it with the given type.
    /// </summary>
    /// <param name="serviceType">the type of service that was not found.</param>
    public ServiceNotFoundException(Type serviceType)
      : base(string.Format(CultureInfo.InvariantCulture, "Could not find the {0} service", serviceType))
    {
      this.serviceType = serviceType;
    }

    /// <summary>
    /// Returns the type of the service that was not found.
    /// </summary>
    public Type ServiceType
    {
      get { return serviceType; }
    }
  }
}