using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes;

namespace MpeCore.Interfaces
{
  public interface IPathProvider
  {
    string Name { get; }
    Dictionary<string, string> Paths { get; }
    string Expand(string filenameTemplate);
    string Colapse(string fileName);
  }
}