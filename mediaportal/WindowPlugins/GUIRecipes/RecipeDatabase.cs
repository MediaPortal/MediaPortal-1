/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System.Collections;
using System.Windows.Forms;
using SQLite.NET;
using MediaPortal.GUI.Library;
using System;

namespace GUIRecipes
{
	/// <summary>
	/// Summary description for RecipeDatabase.
	/// </summary>
	public class RecipeDatabase
	{
		public enum M_Keys {
			VERSION = 1,
			ONLINE = 2,
			O_NAME = 3,
			O_PASS = 4,
			SUB_CAT = 5
		}

		private int m_key=0;
		private string m_content;
		private int m_var1=0;
		private int m_var2=0;

		public int M_Key {
			get { return m_key; }
			set { m_key = value; }
		}

		public string M_Content {
			get { return m_content; }
			set { m_content = value; }
		}

		public int M_Var1 {
			get { return m_var1; }
			set { m_var1 = value; }
		}

		public int M_Var2 {
			get { return m_var2; }
			set { m_var2 = value; }
		}

		private static RecipeDatabase instance=null;
		private SQLiteClient m_db;
		private bool dbExists;

		private RecipeDatabase() {
			try {
				// Open database
				try
				{
					System.IO.Directory.CreateDirectory("database");
				}
				catch(Exception){}
				dbExists = System.IO.File.Exists( @"database\RecipeDatabaseV3.db3" );
				m_db = new SQLiteClient(@"database\RecipeDatabaseV3.db3");

        MediaPortal.Database.DatabaseUtility.SetPragmas(m_db);
				if( !dbExists ){
					CreateTables();
				}
			} catch (SQLiteException ex){
				Console.Write("Recipedatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}
		
		public static RecipeDatabase GetInstance() {
			if( instance == null ) {
				instance = new RecipeDatabase();
			}
			return instance;
		}

		private void  CreateTables(){
			if( m_db == null ){
				return;
			}
			m_db.Execute("CREATE TABLE Recipe ( Recipe_ID integer primary key, TITLE text, YIELD text, DIRECTIONS text, VOTE text, VERSION integer)\n");
			m_db.Execute("CREATE TABLE INGREDIENT ( INGREDIENT_ID integer primary key, Recipe_ID integer, TITLE text, LOT text, UNIT text)\n");
			m_db.Execute("CREATE TABLE CATEGORY ( CATEGORY_ID integer primary key, TITLE text)\n" );
			m_db.Execute("CREATE TABLE Recipe_CATEGORY( Recipe_CATEGORY_ID integer primary key, CATEGORY_ID integer, Recipe_ID integer, TITLE text)\n" );
			m_db.Execute("CREATE TABLE FAVORITE ( CATEGORY_ID integer primary key, TITLE text)\n" );
			m_db.Execute("CREATE TABLE MAIN_CATEGORY ( CATEGORY_ID integer primary key, TITLE text, NUM integer, SUB integer)\n" );
			m_db.Execute("CREATE TABLE MAINTENANCE ( ID integer primary key, KEY integer, CONTENT text, VAR1 integer, VAR2 integer)\n" );
			m_db.Execute("CREATE TABLE DELETE ( ID integer primary key, STAT integer)\n" );
			AddMaintenance((int)M_Keys.ONLINE, "Init", 0,0);
			AddMaintenance((int)M_Keys.SUB_CAT, "Init", 0,0);
			AddMaintenance((int)M_Keys.VERSION, "Version", 0,3);
		}

		private void BuildMaintenance(ArrayList row) {
			if (row!=null) {
				IEnumerator en = row.GetEnumerator();
				en.MoveNext();
				string Id = (string) en.Current;
				en.MoveNext();
				M_Key = (int) en.Current;
				en.MoveNext();
				M_Content=(string)en.Current;
				en.MoveNext();
				M_Var1=(int)en.Current;
				en.MoveNext();
				M_Var2=(int)en.Current;
			}
		}

		public bool CheckKey(int key) {
			// look to exist key
			string rSQL = String.Format("SELECT * FROM MAINTENANCE WHERE KEY = '{0}'",key);
			SQLiteResultSet rs = m_db.Execute( rSQL );
      BuildMaintenance((ArrayList)rs.RowsList[0]);
			if (M_Key==key) return true;
			else return false;
		}

		public void AddMainCat(string content, int num, int sub) {
			string rSQL = String.Format( "INSERT INTO MAIN_CATEGORY VALUES ( null, '{0}', '{1}', '{2}' )", content, num, sub);
			SQLiteResultSet rs = m_db.Execute( rSQL );
		}

		public void AddMaintenance(int key, string content, int var1, int var2) {
			// look to exist key
			M_Key=0;
			string rSQL = String.Format("SELECT * FROM MAINTENANCE WHERE KEY = '{0}'",key.ToString() );
			SQLiteResultSet rs = m_db.Execute( rSQL );
      BuildMaintenance((ArrayList)rs.RowsList[0]);

			rSQL = String.Format( "INSERT INTO MAINTENANCE VALUES ( null, '{0}', '{1}', '{2}', {3} )", key.ToString(),content,var1,var2);
			rs = m_db.Execute( rSQL );
		}

		public void AddRecipe( Recipe r ) // add recipe to database
		{
			if (r.Categories.Count==0) return;

			string rSql = String.Format( "INSERT INTO Recipe VALUES ( null, '{0}', '{1}', '{2}', {3}, {4} )", 
							RemoveInvalidChars( r.Title ), 
							RemoveInvalidChars( r.Yield ), 
							RemoveInvalidChars( r.Directions ),
							"0","1");
			m_db.Execute( "BEGIN" );
			m_db.Execute( rSql );
			int rId = m_db.LastInsertID();
			AddCategories( r, rId );
			for (int i=0; i<r.Ingredients.Count; i++){
				string iSql = String.Format( "INSERT INTO INGREDIENT VALUES ( null, '{0}', '{1}', '{2}', '{3}')", rId, RemoveInvalidChars( (string)r.Ingredients[i] ),RemoveInvalidChars((string)r.Lot[i]),RemoveInvalidChars((string)r.Unit[i]));
				m_db.Execute( iSql );
			}
			m_db.Execute( "END" );
		}
		
		private void  AddCategories(Recipe r, int id) // add category to database
		{
			ArrayList cat = r.Categories;
			foreach( string category in cat ){
				string cSQL = String.Format( "Select * from CATEGORY where title = '{0}'", 
												RemoveInvalidChars( category.Trim() ) );
				SQLiteResultSet rs = m_db.Execute( cSQL );
				string cId = "";

				if( rs.RowsList.Count > 0 ){
					// this category already exists
					cId = rs.GetField( 0, 0 );
				} else {
					// the category doesn't exist, so we need to add it
					string iSQL = String.Format( "INSERT INTO CATEGORY VALUES ( null, '{0}')", 
													RemoveInvalidChars( category.Trim() ));
					m_db.Execute( iSQL );
					cId = m_db.LastInsertID().ToString();
				}

				string crSQL = String.Format( "INSERT INTO Recipe_CATEGORY VALUES( null, {0}, {1}, '{2}' )", cId, id.ToString(),RemoveInvalidChars( r.Title ) );
				m_db.Execute( crSQL );
			}
		}

		public  Recipe GetRecipe( string title ) // get a recipe from database
		{
			string rSQL = String.Format("SELECT * FROM Recipe WHERE TITLE = '{0}'",title );
			SQLiteResultSet rs = m_db.Execute( rSQL );
      return BuildRecipe((ArrayList)rs.RowsList[0]);
		}

		private Recipe BuildRecipe(ArrayList row){
			string stunit = "";

			if (row==null) return null;
			Recipe rec = new Recipe();
			IEnumerator en = row.GetEnumerator();
			en.MoveNext();
			rec.Id = (string) en.Current;
			en.MoveNext();
			rec.Title = (string) en.Current;
			en.MoveNext();

			stunit=(string)en.Current;
			stunit=stunit.Trim()+" ";
			string[] s2 = stunit.Split( ' ' );

			rec.Yield = stunit;
			rec.CYield = Convert.ToInt16(s2[0]);
			en.MoveNext();
			rec.Directions = (string)en.Current;
			rec.Ingredients = BuildIngredients( rec.Id );
			rec.Lot = BuildLot( rec.Id );
			rec.Unit = BuildUnit( rec.Id );
			return rec;
		}

		private Recipe BuildCategorie(ArrayList row){
			if (row==null) return null;
			Recipe rec = new Recipe();
			IEnumerator en = row.GetEnumerator();
			en.MoveNext();
			rec.Id = (string) en.Current;
			en.MoveNext();
			rec.Title = (string) en.Current;
			return rec;
		}

		private ArrayList BuildIngredients(string id){
			string aSQL = String.Format( "SELECT TITLE FROM INGREDIENT WHERE Recipe_ID = {0}", id );
			return m_db.Execute( aSQL ).GetColumn( 0 );
		}

		private ArrayList BuildLot(string id){
			string aSQL = String.Format( "SELECT LOT FROM INGREDIENT WHERE Recipe_ID = {0}", id );
			return m_db.Execute( aSQL ).GetColumn( 0 );
		}
		
		private ArrayList BuildUnit(string id){
			string aSQL = String.Format( "SELECT UNIT FROM INGREDIENT WHERE Recipe_ID = {0}", id );
			return m_db.Execute( aSQL ).GetColumn( 0 );
		}

		public ArrayList GetCategories(){
			string cSQL = "SELECT * FROM CATEGORY ORDER BY TITLE";
			SQLiteResultSet rs = m_db.Execute( cSQL );
			return rs.GetColumn( 1 );
		}

		public ArrayList GetMainCategories(){
			string cSQL = "SELECT * FROM MAIN_CATEGORY WHERE SUB = '0'";
			SQLiteResultSet rs = m_db.Execute( cSQL );
			return rs.GetColumn( 1 );
		}

		public void AddFavorite(string id )	{
			string cSQL = String.Format( "INSERT INTO FAVORITE VALUES ( null, '{0}')",id);
			SQLiteResultSet rs = m_db.Execute( cSQL );
			return;
		}
		
		public void DeleteFavorite(string id )	
		{
			string cSQL = String.Format( "DELETE FROM FAVORITE WHERE TITLE = '{0}'", id );
			SQLiteResultSet rs = m_db.Execute( cSQL );
			return;
		}

		public void DeleteRecipe(string id ) // delete a recipe from database
		{
			string cSQL = String.Format( "SELECT * FROM Recipe WHERE TITLE = '{0}'", id );
			SQLiteResultSet rs = m_db.Execute( cSQL );
			string recipeID = (string) rs.GetField(0,0); 

			cSQL = String.Format( "DELETE FROM Recipe WHERE Recipe_ID = '{0}'", recipeID );
			rs = m_db.Execute( cSQL );
			cSQL = String.Format( "DELETE FROM Recipe_CATEGORY WHERE TITLE = '{0}'", id );
			rs = m_db.Execute( cSQL );
			cSQL = String.Format( "DELETE FROM FAVORITE WHERE TITLE = '{0}'", id );
			rs = m_db.Execute( cSQL );
			cSQL = String.Format( "DELETE FROM INGREDIENT WHERE Recipe_ID = '{0}'", recipeID);
			rs = m_db.Execute( cSQL );
			return;
		}

		public  ArrayList SearchRecipes(string text,byte typ) //search recipe in database
		{
			string stext="%"+RemoveInvalidChars(text)+"%";
			string rSQL="";
			ArrayList recipes = new ArrayList();
			if (typ==1) {// Search in Title
				rSQL = String.Format("SELECT recipe_id,title FROM Recipe WHERE TITLE LIKE '{0}'",stext);
			} 
			else {      // Search in all
				rSQL = String.Format("SELECT recipe_id,title FROM Recipe WHERE TITLE LIKE '{0}' OR DIRECTIONS LIKE '{0}'",stext);
			}
			SQLiteResultSet rs = m_db.Execute( rSQL );	

			foreach( ArrayList row in rs.RowsList ) {
				recipes.Add( BuildCategorie( row  ));
			}
			return recipes;
		}

		public ArrayList GetRecipesForFavorites()
		{
			ArrayList recipes = new ArrayList();
			string sql = String.Format( "Select * from favorite" );
			SQLiteResultSet rs = m_db.Execute( sql );
			foreach( ArrayList row in rs.RowsList )
			{
				recipes.Add( BuildCategorie( row ));
			}
			return recipes;
		}

		public  ArrayList GetSubsForCategory( string category ) {
			ArrayList recipes = new ArrayList();
			string sql = String.Format("SELECT * FROM MAIN_CATEGORY WHERE TITLE = '{0}'",category);
			SQLiteResultSet rs = m_db.Execute( sql );
			string categoryID = (string) rs.GetField(0,2); 
			string sql2 = String.Format( "Select * from main_category where sub = {0}", categoryID );
			rs = m_db.Execute( sql2 );
			foreach( ArrayList row in rs.RowsList ) {
				recipes.Add( BuildCategorie( row ));
			}
			return recipes;
		}

		public  ArrayList GetRecipesForCategory( string category )
		{
			ArrayList recipes = new ArrayList();
			string sql = String.Format( "Select * from category where title = '{0}'", category );
			SQLiteResultSet rs = m_db.Execute( sql );
			
			string categoryID = rs.GetField(0,0).ToString(); 
			string sql2 = String.Format( "Select recipe_id,title from recipe_category where category_id = {0}", categoryID );
			rs = m_db.Execute( sql2 );
			foreach( ArrayList row in rs.RowsList )
			{
				recipes.Add( BuildCategorie( row ));
			}
			return recipes;
		}

		string  RemoveInvalidChars( string strTxt)
		{
			if( strTxt == null ) return "";
			string strReturn="";
			for (int i=0; i < (int)strTxt.Length; ++i)
			{
				char k=strTxt[i];
				int z=(int) k;
				if (z<32 || z>255) k=' ';
				if (k=='^' || k=='~' || k=='#') k=' ';
				if (k=='\'') 
				{
					strReturn += "'";
				}
				strReturn += k;
			}
			if (strReturn=="") 
				strReturn=GUILocalizeStrings.Get(2014);
			strTxt=strReturn.Trim();
			return strTxt;
		}
	}
}
