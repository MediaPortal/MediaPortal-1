using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase {
  [Serializable]
  public sealed class GroupMap : GroupMapDataRow {
  
  #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private GroupMap() : this(null) {}

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public GroupMap(DataRowBuilder pRowBuilder) 
      : base(pRowBuilder) {
    }

  #endregion
    
  #region Suggested Customizations
  
//    // Use this factory method template to create new instances of this class
//    public static GroupMap Create(PersistenceManager pManager,
//      ... your creation parameters here ...) { 
//
//      GroupMap aGroupMap = pManager.CreateEntity<GroupMap>();
//
//      // if this object type requires a unique id and you have implemented
//      // the IIdGenerator interface implement the following line
//      pManager.GenerateId(aGroupMap, // add id column here //);
//
//      // Add custom code here
//
//      aGroupMap.AddToManager();
//      return aGroupMap;
//    }

//    // Implement this method if you want your object to sort in a specific order
//    public override int CompareTo(Object pObject) {
//    }

//    // Implement this method to customize the null object version of this class
//    protected override void UpdateNullEntity() {
//    }

  #endregion
    
    // Add additional logic to your business object here...

    public static GroupMap Create()
    {
      GroupMap card = (GroupMap)DatabaseManager.Instance.CreateEntity(typeof(GroupMap));
      DatabaseManager.Instance.GenerateId(card, GroupMap.IdMapEntityColumn);
      DatabaseManager.Instance.AddEntity(card);
      return card;
    }
    public static GroupMap New()
    {
      GroupMap card = (GroupMap)DatabaseManager.Instance.CreateEntity(typeof(GroupMap));
      return card;
    }

    public class Comparer : IComparer<GroupMap>
    {
      #region IComparer Members

      public int Compare(GroupMap left, GroupMap right)
      {
        if (left.Channel.SortOrder == -1) return 6000;
        if (right.Channel.SortOrder == -1) return 0;
        return left.Channel.SortOrder.CompareTo(right.Channel.SortOrder);
      }

      #endregion
    }
  }
  
}
