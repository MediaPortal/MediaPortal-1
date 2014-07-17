using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.DIDL;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Tree;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects.Basic;

namespace Mediaportal.TV.Server.TVLibrary.TVEUPnPServer.Server.Objects.Container
{
  class ContainerStorageFolder : BasicItem, IDirectoryContainer
  {
    public ContainerStorageFolder(string id)
      : base(id)
    {
      Restricted = true;
      Searchable = false;
      SearchClass = new List<IDirectorySearchClass>();
      CreateClass = new List<IDirectoryCreateClass>();
    }

    public override string Class
    {
      get { return "object.container.storageFolder"; }
    }

    public override void Initialise()
    {
    }

    public void InitialiseAll()
    {
      Initialise();
      foreach (var treeNode in Children.OfType<BasicContainer>())
      {
        (treeNode).InitialiseAll();
      }
    }

    public virtual IList<IDirectorySearchClass> SearchClass { get; set; }

    public virtual bool Searchable { get; set; }

    public virtual int ChildCount
    {
      get { return Children.Count; }
      set { /*throw new IllegalCallException("Meaningless in this implementation");*/ }
    }

    public virtual IList<IDirectoryCreateClass> CreateClass { get; set; }
  }
}
