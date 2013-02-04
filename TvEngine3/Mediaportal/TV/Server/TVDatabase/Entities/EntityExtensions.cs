using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

      PropertyInfo[] properties = trackingItem.GetType().GetProperties();
      foreach (PropertyInfo propertyInfo in from oPropertyInfo in properties let type = oPropertyInfo.PropertyType let found = IsAssignableToGenericType(type, typeof(TrackableCollection<>)) where found select oPropertyInfo)
      {        
        var changeTrackers = propertyInfo.GetValue(trackingItem, null) as IList;
        bool changeDetected = false;

        if (changeTrackers != null && changeTrackers.Count > 0)
        {
          var markedForDeletions = new List<object>();
          int i = 0;
          foreach (object changeTracker in changeTrackers)
          {
            var objectWithChangeTracker = changeTracker as IObjectWithChangeTracker;
            if (objectWithChangeTracker != null && objectWithChangeTracker.ChangeTracker.State != ObjectState.Unchanged)
            {
              changeDetected = true;
              UnloadAllUnchangedRelationsForEntity(objectWithChangeTracker); //take care of children too              
            }
            else
            {
              markedForDeletions.Add(changeTracker);
            }
            i++;
          }
          if (markedForDeletions.Count > 0)
          {
            foreach (object changeTracker in markedForDeletions)
            {
              changeTrackers.Remove(changeTracker);
            } 
          }                  
        }                  

        if (!changeDetected)
        {
          propertyInfo.SetValue(trackingItem, null, null); 
        }      
      }

      if (trackingItem.ChangeTracker.State != ObjectState.Added)
      {
        FieldInfo[] fieldInfos = trackingItem.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
          var objectWithChangeTracker = fieldInfo.GetValue(trackingItem) as IObjectWithChangeTracker;

          if (objectWithChangeTracker != null && objectWithChangeTracker.ChangeTracker.State == ObjectState.Unchanged)
          {
            fieldInfo.SetValue(trackingItem, null);
          }
        } 
      }      

      trackingItem.StartTracking();
    }

  }
}
