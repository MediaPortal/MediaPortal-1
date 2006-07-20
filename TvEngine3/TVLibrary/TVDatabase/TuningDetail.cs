using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase {
  [Serializable]
  public sealed class TuningDetail : TuningDetailDataRow {
  
  #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private TuningDetail() : this(null) {}

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public TuningDetail(DataRowBuilder pRowBuilder) 
      : base(pRowBuilder) {
    }

  #endregion
    
  #region Suggested Customizations
  
//    // Use this factory method template to create new instances of this class
//    public static TuningDetail Create(PersistenceManager pManager,
//      ... your creation parameters here ...) { 
//
//      TuningDetail aTuningDetail = pManager.CreateEntity<TuningDetail>();
//
//      // if this object type requires a unique id and you have implemented
//      // the IIdGenerator interface implement the following line
//      pManager.GenerateId(aTuningDetail, // add id column here //);
//
//      // Add custom code here
//
//      aTuningDetail.AddToManager();
//      return aTuningDetail;
//    }

//    // Implement this method if you want your object to sort in a specific order
//    public override int CompareTo(Object pObject) {
//    }

//    // Implement this method to customize the null object version of this class
//    protected override void UpdateNullEntity() {
//    }

  #endregion
    
    // Add additional logic to your business object here...

    public static TuningDetail Create()
    {
      TuningDetail card = (TuningDetail)DatabaseManager.Instance.CreateEntity(typeof(TuningDetail));
      DatabaseManager.Instance.GenerateId(card, TuningDetail.IdTuningEntityColumn);
      DatabaseManager.Instance.AddEntity(card);
      return card;
    }
  }
  
}
