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
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace WebEPG.config.Grabber
{
  public abstract class JsonNode
  {
    protected JsonNode parent;

    public static JsonNode LoadJson(string source)
    {
      JavaScriptSerializer js = new JavaScriptSerializer();
      object nodeO = js.DeserializeObject(source);
      return NodeFactory(nodeO, null);
    }

    protected static JsonNode NodeFactory(object nodeO, JsonNode aParent)
    {
      if (nodeO is object[])
        return new JsonArrayNode((object[])nodeO) { parent = aParent };
      if (nodeO is Dictionary<string, object>)
        return new JsonObjectNode((Dictionary<string, object>)nodeO) { parent = aParent };
      return new JsonSimple(nodeO) { parent = aParent };

    }

    public abstract List<JsonNode> GetNodes(string xpath, string filter);
    protected bool MatchesFilter(string filter)
    {
      if (String.IsNullOrEmpty(filter))
        return true;

      string[] fltr = filter.Split('=');
      List<JsonNode> subs = GetNodes(fltr[0], "");
      foreach (JsonNode sub in subs)
      {
        if (sub is JsonSimple)
          if (((JsonSimple)sub).ValueS == fltr[1])
            return true;
      }
      return false;

    }

    public string GetValue(string name)
    {
      JsonObjectNode jo = this as JsonObjectNode;
      if (jo != null)
        return jo.GetValue(name);
      JsonSimple js = this as JsonSimple;
      if (js != null)
        return js.ToString();
      return null;
    }

  }

  internal class JsonSimple : JsonNode
  {
    private object obj;

    public JsonSimple(object o)
    {
      obj = o;
    }
    public override string ToString()
    {
      if (obj == null)
        return null;
      return obj.ToString();
    }

    public override List<JsonNode> GetNodes(string xpath, string filter)
    {
      List<JsonNode> res = new List<JsonNode>();
      if (MatchesFilter(filter))
        res.Add(this);
      return res;
    }

    public string ValueS
    {
      get
      {
        return obj.ToString();
      }
    }

    public int ValueI
    {
      get
      {
        if (obj is Int32)
          return (Int32)obj;
        throw new InvalidCastException();
      }
    }

    public bool ValueB
    {
      get
      {
        if (obj is bool)
          return (bool)obj;
        throw new InvalidCastException();
      }
    }


  }

  internal class JsonArrayNode : JsonNode, IEnumerable
  {
    private List<JsonNode> list;

    public JsonArrayNode(object[] objs)
    {
      list = new List<JsonNode>();
      foreach (object o in objs)
      {
        JsonNode node = NodeFactory(o, this);
        list.Add(node);
      }
    }

    public override string ToString()
    {
      return "JsonArrayNode, Count=" + list.Count;
    }

    public override List<JsonNode> GetNodes(string xpath, string filter)
    {
      List<JsonNode> res = new List<JsonNode>();
      foreach (JsonNode node in list)
        res.AddRange(node.GetNodes(xpath, filter));
      return res;
    }

    public IEnumerator GetEnumerator()
    {
      return list.GetEnumerator();
    }
  }

  internal class JsonObjectNode : JsonNode, IEnumerable
  {
    private Dictionary<string, JsonNode> dict;

    public JsonObjectNode(Dictionary<string, object> obj)
    {
      dict = new Dictionary<string, JsonNode>();
      foreach (KeyValuePair<string, object> kv in obj)
        dict.Add(kv.Key, NodeFactory(kv.Value, this));
    }

    public override string ToString()
    {
      return "JsonObjectNode, Count=" + dict.Count;
    }

    public override List<JsonNode> GetNodes(string xpath, string filter)
    {
      List<JsonNode> res = new List<JsonNode>();
      if (String.IsNullOrEmpty(xpath))
      {
        if (MatchesFilter(filter))
          res.Add(this);
      }
      else
      {
        string[] xp = xpath.Split('/');
        string rest = String.Join("/", xp, 1, xp.Length - 1);
        if (xp[0] == "..")
          res.AddRange(parent.GetNodes(rest, filter));
        else
          if (dict.ContainsKey(xp[0]))
            res.AddRange(dict[xp[0]].GetNodes(rest, filter));
      }
      return res;
    }

    public new string GetValue(string name)
    {
      if (dict.ContainsKey(name))
      {
        JsonSimple js = dict[name] as JsonSimple;
        if (js != null)
          return js.ValueS;
      }
      return null;
    }
    public IEnumerator GetEnumerator()
    {
      return dict.GetEnumerator();
    }
  }

}
