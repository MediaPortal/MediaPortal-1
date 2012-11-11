using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class CloneHelper
  {

    public static T DeepCopy<T>(T obj)
    {
      byte[] serialized = Serialize<T>(obj);
      T deserialized = Deserialize<T>(serialized);
      return deserialized;
    }

    private static byte[] Serialize<T>(T objectToSerialize)
    {
      var ser = new DataContractSerializer(typeof(T));
      using (var writer = new MemoryStream())
      {
        ser.WriteObject(writer, objectToSerialize);
        return writer.ToArray();
      }
    }


    private static T Deserialize<T>(byte[] dataAsbytes)
    {
      using (var writer = new MemoryStream(dataAsbytes))
      {
        XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(writer, new XmlDictionaryReaderQuotas());
        var ser = new DataContractSerializer(typeof(T));
        return (T)ser.ReadObject(reader, true);
      }
    }

    /*public static T DeepCopy<T>(T obj)
    {
      using (var ms = new MemoryStream())
      {
        var formatter = new BinaryFormatter();
        formatter.Serialize(ms, obj);
        ms.Position = 0;
        return (T)formatter.Deserialize(ms);
      }
    }
     */
    /*

    public static T DeepCopy<T>(T obj) where T : class
    {
      if (obj == null)
        throw new ArgumentNullException("Object cannot be null");
      return (T)Process(obj);
    }

    private static object Process(object obj)
    {
      if (obj == null)
      {
        return null;
      }
      Type type = obj.GetType();
      if (type.IsValueType || type == typeof(string))
      {
        return obj;
      }
      if (type.IsArray)
      {
        if (type.FullName != null)
        {
          Type elementType = Type.GetType(
            type.FullName.Replace("[]", string.Empty));
          var array = obj as Array;
          if (elementType != null)
          {
            if (array != null)
            {
              Array copied = Array.CreateInstance(elementType, array.Length);
              for (int i = 0; i < array.Length; i++)
              {
                copied.SetValue(Process(array.GetValue(i)), i);
              }
              return Convert.ChangeType(copied, obj.GetType());
            }
          }
        }
      }
      if (type.IsClass)
      {
        ConstructorInfo constructor = typeof(T).GetConstructors().OrderBy(c => c.GetParameters().Length).FirstOrDefault();
        if (constructor != null)
        {
          var instance = constructor.Invoke(new object[constructor.GetParameters().Length]);

          object toret = Activator.CreateInstance(obj.GetType());
          FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                                              BindingFlags.NonPublic | BindingFlags.Instance);
          foreach (FieldInfo field in fields)
          {
            object fieldValue = field.GetValue(obj);
            if (fieldValue == null)
            {
              continue;
            }
            field.SetValue(toret, Process(fieldValue));
          }
          return toret;
        }                          
      }
      throw new ArgumentException("Unknown type");
    }*/
  }
}