using System;
using System.Data.Objects;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
  public interface IObjectContext : IDisposable 
  { 
    IObjectSet<T> CreateObjectSet<T>() where T : class; 
    void SaveChanges();
  }
}
