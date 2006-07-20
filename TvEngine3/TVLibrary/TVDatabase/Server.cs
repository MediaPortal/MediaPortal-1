using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase {
  [Serializable]
  public sealed class Server : ServerDataRow {
  
  #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private Server() : this(null) {}

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public Server(DataRowBuilder pRowBuilder) 
      : base(pRowBuilder) {
    }

  #endregion
    
  #region Suggested Customizations
  
//    // Use this factory method template to create new instances of this class
//    public static Server Create(PersistenceManager pManager,
//      ... your creation parameters here ...) { 
//
//      Server aServer = pManager.CreateEntity<Server>();
//
//      // if this object type requires a unique id and you have implemented
//      // the IIdGenerator interface implement the following line
//      pManager.GenerateId(aServer, // add id column here //);
//
//      // Add custom code here
//
//      aServer.AddToManager();
//      return aServer;
//    }

//    // Implement this method if you want your object to sort in a specific order
//    public override int CompareTo(Object pObject) {
//    }

//    // Implement this method to customize the null object version of this class
//    protected override void UpdateNullEntity() {
//    }

  #endregion
    
    // Add additional logic to your business object here...

    public static Server Create()
    {
      Server server = (Server)DatabaseManager.Instance.CreateEntity(typeof(Server));
      DatabaseManager.Instance.GenerateId(server, Server.IdServerEntityColumn);
      DatabaseManager.Instance.AddEntity(server);
      return server;
    }
    public void DeleteAll()
    {
      while (Cards.Count > 0) Cards[0].DeleteAll();
      while (Recordings.Count > 0) Recordings[0].Delete();
      Delete();
    }
  }
  
}
