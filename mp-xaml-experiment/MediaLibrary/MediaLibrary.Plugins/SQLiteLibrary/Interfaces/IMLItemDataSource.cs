using System;
using MediaLibrary;
using System.Data;
using System.Text;


namespace SQLiteLibrary
{
    //IMLItemDataSource
    public interface IMLItemDataSource
    {

        #region CRUD Replaces

        IMLDataSet GetDataSet();

        IMLView[] GetViews();

        IMLItemList GetAllItems();

        #endregion

        #region Update Replaces

        bool UpdateDataSet(DataSet ds);

        bool UpdateView(string TagName, string TagValue, IMLView View);

        bool UpdateStep(string TagName, string TagValue, IMLViewStep Step);

        bool UpdateItem(IMLItem Item);

        #endregion

        #region Insert Replaces

        bool AddNewView(IMLView View);

        bool AddNewStep(IMLViewStep Step);

        bool AddNewItem(IMLItem Item);

        #endregion

        #region Delete Replaces

        bool DeleteView(IMLView View);

        bool DeleteStep(IMLViewStep Step);

        bool DeleteItem(IMLItem Item);

        bool DeleteAllItems();

        bool DeleteAllViews();

        bool DeleteSteps(IMLView View);

        #endregion

        #region Methods

        bool MoveUp(IMLView View);

        bool MoveUp(IMLViewStep ViewStep);

        bool MoveDown(IMLView View);

        bool MoveDown(IMLViewStep ViewStep);

        bool AddNewTag(string TagName);

        bool RenameTag(string OldTagName, string NewTagName);

        bool DeleteTag(string TagName);

        string[] GetTagNames();

        string[] GetTagValues(string TagName);

        int ItemCount();

        int[] GetAllItemIDs();

        IMLItem FindItem(string Tag, string Value);
 
        IMLItemList Search(string TagName, string SearchString);

        IMLItemList CustomSearch(string Filter, string GroupBy, string GroupFunc, string OrderBy, string OrderType, bool Asc);

        bool BeginUpdate();

        void CancelUpdate();

        void EndUpdate();

        void Refresh();

        void ReloadDataSet(IMLDataSet dataSet);

        #endregion


    }
}