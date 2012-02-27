using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public static class EntityExtensions
  {

    private static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
      if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
      {
        return true;
      }

      var interfaceTypes = givenType.GetInterfaces();
      if (interfaceTypes.Where(it => it.IsGenericType).Any(it => it.GetGenericTypeDefinition() == genericType))
      {
        return true;
      }
      Type baseType = givenType.BaseType;
      if (baseType == null)
      {
        return false;
      }
      return baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericType || IsAssignableToGenericType(baseType, genericType);
    }

    public static void UnloadAllUnchangedRelationsForEntity(this IObjectWithChangeTracker trackingItem)
    {
      trackingItem.StopTracking();

      /*FieldInfo[] fields = trackingItem.GetType().GetFields(BindingFlags.NonPublic);
      foreach (FieldInfo field in from fieldInfo in fields let type = fieldInfo.FieldType let found = IsAssignableToGenericType(type, typeof(TrackableCollection<>)) where found select fieldInfo)
      {
        field.SetValue(trackingItem, null);
      }*/

      PropertyInfo[] properties = trackingItem.GetType().GetProperties();
      foreach (PropertyInfo propertyInfo in from oPropertyInfo in properties let type = oPropertyInfo.PropertyType let found = IsAssignableToGenericType(type, typeof(TrackableCollection<>)) where found select oPropertyInfo)
      {
        //(TrackableCollection<T>) Activator.CreateInstance(typeof (TrackableCollection<>).MakeGenericType(typeof (GroupMap);
        //var abc =  
        //Type t = propertyInfo.PropertyType.GetGenericTypeDefinition();
        IList changeTrackers = propertyInfo.GetValue(trackingItem, null) as IList;
        bool changeDetected = false;

        var deleteChangeTrackers = new List<int>();

        //Type d1 = typeof(TrackableCollection<>);
        //Type[] typeArgs = { t };
        //Type constructed = d1.MakeGenericType(typeArgs);
        //object o = Activator.CreateInstance(constructed);

        if (changeTrackers != null)
        {
          int i = 0;
          foreach (object changeTracker in changeTrackers)
          {
            IObjectWithChangeTracker objectWithChangeTracker = changeTracker as IObjectWithChangeTracker;
            if (objectWithChangeTracker != null && objectWithChangeTracker.ChangeTracker.State != ObjectState.Unchanged)
            {
              changeDetected = true;
              UnloadAllUnchangedRelationsForEntity(objectWithChangeTracker); //take care of children too              
            }
            else
            {
              deleteChangeTrackers.Add(i);
            }
            i++;
          }
        }                  

        if (!changeDetected)
        {
          propertyInfo.SetValue(trackingItem, null, null); 
        }
        else
        {

          for (int i = deleteChangeTrackers.Count - 1; i >= 0; i--)
          {
            changeTrackers.RemoveAt(deleteChangeTrackers[i]);
          }
          

          /*foreach (var objectWithChangeTracker in deleteChangeTrackers)
          {
            changeTrackers.Add(objectWithChangeTracker);
          } */         
        }
      }

      trackingItem.StartTracking();
    }

  }
}
