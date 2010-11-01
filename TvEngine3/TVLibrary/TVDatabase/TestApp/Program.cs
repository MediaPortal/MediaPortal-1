using System;
using System.Collections.Generic;
using System.Text;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

using TvDatabase;

namespace TestApp
{
  class Program
  {
    static void Main(string[] args)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.AddCard("name1", "path1", 1);
      layer.AddCard("name2", "path2", 2);
      layer.AddCard("name3", "path3", 3);
      layer.AddCard("name4", "path4", 4);

      Card card = layer.GetCardByName("name2");
      card = layer.GetCardByDevicePath("path4");

      Channel channel = layer.AddChannel("provider", "name");
      channel = layer.AddChannel("provider", "name");
      

      DatabaseManager.Instance.SaveChanges();

    }
  }
}
