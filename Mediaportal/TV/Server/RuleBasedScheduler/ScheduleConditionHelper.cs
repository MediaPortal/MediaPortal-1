using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Mediaportal.TV.Server.RuleBasedScheduler
{
  public static class ScheduleConditionHelper
  {
    public static string Serialize<T>(IEnumerable<IScheduleCondition> objectToSerialize)
    {
      var ser = new DataContractSerializer(typeof(T));
      using (var writer = new MemoryStream())
      {
        ser.WriteObject(writer, objectToSerialize);
        return Encoding.UTF8.GetString(writer.ToArray());
      }
    }


    public static T Deserialize<T>(string xmlData)
    {
      var encoding = new System.Text.UTF8Encoding();
      byte[] dataAsbytes = encoding.GetBytes(xmlData);
      using (var writer = new MemoryStream(dataAsbytes))
      {
        XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(writer, new XmlDictionaryReaderQuotas());
        var ser = new DataContractSerializer(typeof(T));
        return (T)ser.ReadObject(reader, true);
      }
    }
  }
}
