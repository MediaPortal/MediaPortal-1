#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Xml;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;
using UPnP.Infrastructure.CP.SSDP;
using UPnP.Infrastructure.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service
{
  internal class ServiceBase : IDisposable
  {
    protected CpDevice _device = null;
    protected CpService _service = null;
    protected StateVariableChangedDlgt _stateVariableDelegate = null;
    protected EventSubscriptionFailedDlgt _eventSubscriptionDelegate = null;
    protected string _unqualifiedServiceName = string.Empty;

    public ServiceBase(CpDevice device, string serviceName, bool isOptional = false)
    {
      _device = device;
      _unqualifiedServiceName = serviceName.Substring(serviceName.LastIndexOf(":") + 1);
      _service.IsOptional = isOptional;
      if (!device.Services.TryGetValue(serviceName, out _service) && !isOptional)
      {
        throw new NotImplementedException(string.Format("Device does not implement a {0} service.", _unqualifiedServiceName));
      }
    }

    ~ServiceBase()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        UnsubscribeStateVariables();
      }
    }

    public void SubscribeStateVariables(StateVariableChangedDlgt svChangeDlg, EventSubscriptionFailedDlgt esFailDlg = null)
    {
      if (_service == null)
      {
        return;
      }
      UnsubscribeStateVariables();
      if (svChangeDlg != null)
      {
        _stateVariableDelegate = svChangeDlg;
        _service.StateVariableChanged += _stateVariableDelegate;
      }
      if (esFailDlg != null)
      {
        _eventSubscriptionDelegate = esFailDlg;
        _service.EventSubscriptionFailed += _eventSubscriptionDelegate;
      }
      else if (svChangeDlg != null)
      {
        _eventSubscriptionDelegate = SubscribeFailed;
        _service.EventSubscriptionFailed += _eventSubscriptionDelegate;
      }
      _service.SubscribeStateVariables();
    }

    public void UnsubscribeStateVariables()
    {
      if (_service == null)
      {
        return;
      }
      if (_service.IsStateVariablesSubscribed)
      {
        _service.UnsubscribeStateVariables();
      }
      if (_stateVariableDelegate != null)
      {
        _service.StateVariableChanged -= _stateVariableDelegate;
      }
      if (_eventSubscriptionDelegate != null)
      {
        _service.EventSubscriptionFailed -= _eventSubscriptionDelegate;
      }
      _stateVariableDelegate = null;
      _eventSubscriptionDelegate = null;
    }

    private void SubscribeFailed(CpService service, UPnPError error)
    {
      this.LogError("UPnP: failed to subscribe to state variable events for service {0}, code = {1}, description = {2}", _unqualifiedServiceName, error.ErrorCode, error.ErrorDescription);
    }

    public object QueryStateVariable(string stateVariableName)
    {
      CpStateVariable stateVariable;
      if (!_service.StateVariables.TryGetValue(stateVariableName, out stateVariable))
      {
        throw new TvException("Failed to find state variable {0} for service {1}.", stateVariableName, _unqualifiedServiceName);
      }
      ServiceDescriptor serviceDescriptor = null;
      IDictionary<string, ServiceDescriptor> deviceServiceDescriptors;
      if (_service.Connection.RootDescriptor.ServiceDescriptors.TryGetValue(_service.ParentDevice.UUID, out deviceServiceDescriptors))
      {
        deviceServiceDescriptors.TryGetValue(_service.ServiceTypeVersion_URN, out serviceDescriptor);
      }
      if (serviceDescriptor == null)
      {
        throw new TvException("Failed to find service descriptor for service {0}.", _unqualifiedServiceName);
      }

      StringBuilder action = new StringBuilder();
      using (StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(action, UPnPConfiguration.DEFAULT_XML_WRITER_SETTINGS.Encoding))
      {
        using (XmlWriter writer = XmlWriter.Create(stringWriter, UPnPConfiguration.DEFAULT_XML_WRITER_SETTINGS))
        {
          SoapHelper.WriteSoapEnvelopeStart(writer, true);
          writer.WriteStartElement("u", "QueryStateVariable", "urn:schemas-upnp-org:control-1-0");
          writer.WriteStartElement("varName");
          stateVariable.DataType.SoapSerializeValue(stateVariable.Name, true, writer);
          writer.WriteEndElement();
          SoapHelper.WriteSoapEnvelopeEndAndClose(writer);
        }
      }
      LinkData preferredLink = serviceDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink;
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(
        new Uri(preferredLink.DescriptionLocation), serviceDescriptor.ControlURL)
      );
      request.ServicePoint.BindIPEndPointDelegate = (servicePoint, remoteEndPoint, retryCount) =>
      {
        if (retryCount > 0)
        {
          return null;
        }
        return new IPEndPoint(preferredLink.Endpoint.EndPointIPAddress, 0);
      };
      request.Method = "POST";
      request.KeepAlive = false;
      request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
      request.ServicePoint.Expect100Continue = false;
      request.AllowAutoRedirect = true;
      OperatingSystem os = Environment.OSVersion;
      request.UserAgent = os.Platform + "/" + os.Version + " UPnP/1.1 " + UPnPConfiguration.PRODUCT_VERSION;
      request.ContentType = "text/xml; charset=\"utf-8\"";
      request.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:control-1-0#QueryStateVariable\"");
      try
      {
        using (Stream requestStream = request.GetRequestStream())
        {
          using (StreamWriter sw = new StreamWriter(requestStream, UPnPConsts.UTF8_NO_BOM))
          {
            sw.Write(action.ToString());
            sw.Close();
          }
          requestStream.Close();
        }
      }
      catch
      {
        request.Abort();
        throw;
      }

      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      object value = null;
      try
      {
        Encoding contentEncoding;
        string mediaType;
        if (!EncodingUtils.TryParseContentTypeEncoding(response.ContentType, Encoding.UTF8, out mediaType, out contentEncoding) || mediaType != "text/xml")
        {
          throw new TvException("Failed to parse content type header.");
        }
        using (Stream responseStream = response.GetResponseStream())
        {
          using (TextReader textReader = new StreamReader(responseStream, contentEncoding))
          {
            using (XmlReader xmlReader = XmlReader.Create(textReader, UPnPConfiguration.DEFAULT_XML_READER_SETTINGS))
            {
              // Search for the QueryStateVariableResponse element. We don't
              // use ReadToFollowing() or similar because ATI CableCARD tuners
              // have an error in the namespace name.
              // correct = urn:schemas-upnp-org:control-1-0
              // ATI incorrect = urn=:schemas-upnp-org:control-1-0
              bool foundResponse = false;
              while (xmlReader.Read())
              {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name.EndsWith("QueryStateVariableResponse"))
                {
                  foundResponse = true;
                  break;
                }
              }
              if (!foundResponse)
              {
                throw new Exception("Failed to find QueryStateVariableResponse element in QueryStateVariable response.");
              }
              if (!xmlReader.ReadToDescendant("return"))
              {
                throw new TvException("Failed to find return element in QueryStateVariable response.");
              }
              value = stateVariable.DataType.SoapDeserializeValue(xmlReader, true);
              xmlReader.Close();
            }
            textReader.Close();
          }
          responseStream.Close();
        }
        return value;
      }
      finally
      {
        response.Close();
      }
    }
  }
}