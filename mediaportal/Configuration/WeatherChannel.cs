using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

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
			string searchURI = String.Format("http://xoap.weather.com/search/search?where={0}", searchString);

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

			return result;
		}
	}
}
