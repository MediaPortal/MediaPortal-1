using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase
{
  [Serializable]
  public sealed class Setting : SettingDataRow
  {

    #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private Setting() : this(null) { }

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public Setting(DataRowBuilder pRowBuilder)
      : base(pRowBuilder)
    {
    }

    #endregion

    #region Suggested Customizations

    //    // Use this factory method template to create new instances of this class
    //    public static Setting Create(PersistenceManager pManager,
    //      ... your creation parameters here ...) { 
    //
    //      Setting aSetting = pManager.CreateEntity<Setting>();
    //
    //      // if this object type requires a unique id and you have implemented
    //      // the IIdGenerator interface implement the following line
    //      pManager.GenerateId(aSetting, // add id column here //);
    //
    //      // Add custom code here
    //
    //      aSetting.AddToManager();
    //      return aSetting;
    //    }

    //    // Implement this method if you want your object to sort in a specific order
    //    public override int CompareTo(Object pObject) {
    //    }

    //    // Implement this method to customize the null object version of this class
    //    protected override void UpdateNullEntity() {
    //    }

    #endregion

    // Add additional logic to your business object here...

    public static Setting Create()
    {
      Setting setting = (Setting)DatabaseManager.Instance.CreateEntity(typeof(Setting));
      setting.Tag = "";
      setting.Value = "";
      DatabaseManager.Instance.GenerateId(setting, Setting.IdSettingEntityColumn);

      DatabaseManager.Instance.AddEntity(setting);
      return setting;
    }
  }

}
