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
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Request;
using Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef.Response;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Shef
{
  internal class ShefClient
  {
    private string _ipAddress = null;

    public ShefClient(string ipAddress)
    {
      _ipAddress = ipAddress;
    }

    public bool SendRequest(IShefRequest shefRequest)
    {
      IShefResponse shefResponse;
      return SendRequest(shefRequest, out shefResponse);
    }

    public bool SendRequest(IShefRequest shefRequest, out IShefResponse shefResponse)
    {
      shefResponse = null;

      string uri = string.Format("http://{0}:8080/{1}", _ipAddress, shefRequest.GetQueryUri());

      HttpWebRequest request = null;
      HttpWebResponse response = null;
      try
      {
        request = (HttpWebRequest)HttpWebRequest.Create(uri);
        request.Timeout = 5000;
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DirecTV SHEF: failed to send request, URI = {0}", uri);
        if (request != null)
        {
          request.Abort();
        }
        return false;
      }

      try
      {
        using (Stream responseStream = response.GetResponseStream())
        {
          DataContractJsonSerializer deserialiser = new DataContractJsonSerializer(shefRequest.GetResponseType());
          shefResponse = deserialiser.ReadObject(responseStream) as IShefResponse;
          responseStream.Close();
          ShefResponseStatus status = shefResponse.Status;
          if (status.Code != ShefResponseStatusCode.Ok || status.CommandResult != 0)
          {
            this.LogError("DirecTV SHEF: non-OK response, code = {0}, result = {1}, message = {2}, URI = {3}", status.Code, status.CommandResult, status.Message, uri);
            return false;
          }
          return true;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DirecTV SHEF: failed to handle response, URI = {0}", uri);
        return false;
      }
      finally
      {
        if (response != null)
        {
          response.Close();
        }
      }
    }
  }
}