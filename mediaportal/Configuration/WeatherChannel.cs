using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for Weather.
	/// </summary>
	public class WeatherChannel
	{
		public class City
		{
			public string Name;
			public string Id;

			public City(string name, string id)
			{
				this.Name = name;
				this.Id = id;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public WeatherChannel()
		{
		}

		public ArrayList SearchCity(string searchString)
		{
      ArrayList result = new ArrayList();

      try
      {
        string searchURI = String.Format("http://xoap.weather.com/search/search?where={0}", UrlEncode(searchString));

        //
        // Create the request and fetch the response
        //
        WebRequest request = WebRequest.Create(searchURI);
        WebResponse response = request.GetResponse();

        //
        // Read data from the response stream
        //
        Stream responseStream = response.GetResponseStream();
        Encoding iso8859 = System.Text.Encoding.GetEncoding("iso-8859-1");
        StreamReader streamReader = new StreamReader(responseStream, iso8859);
  			
        //
        // Fetch information from our stream
        //
        string data = streamReader.ReadToEnd();
  			
        XmlDocument document = new XmlDocument();
        document.LoadXml(data);

        XmlNodeList nodes = document.DocumentElement.SelectNodes("/search/loc");

        if (nodes != null)
        {
          //
          // Iterate through our results
          //
          foreach(XmlNode node in nodes)
          {
            string name = node.InnerText;
            string id	= node.Attributes["id"].Value;

            result.Add(new City(name, id));
          }
        }
      }
      catch(Exception)
      {
        //
        // Failed to perform search
        //
        throw new ApplicationException("Failed to perform city search, make sure you are connected to the internet.");
      }

			return result;
		}

    public string UrlEncode(string instring)
    {
      StringReader strRdr = new StringReader(instring);
      StringWriter strWtr = new StringWriter();
      int charValue = strRdr.Read();
      while (charValue != -1)
      {
        if (((charValue >= 48) && (charValue <=57)) // 0-9
          || ((charValue >= 65) && (charValue <=90)) // A-Z
          || ((charValue >= 97) && (charValue <=122))) // a-z
        {
          strWtr.Write((char) charValue);
        }
        else if (charValue == 32)  // Space
        {
          strWtr.Write("+");
        }
        else
        {
          strWtr.Write("%{0:x2}", charValue);
        } 
        
        charValue = strRdr.Read();
      } 
      
      return strWtr.ToString();
    }
	}
}
