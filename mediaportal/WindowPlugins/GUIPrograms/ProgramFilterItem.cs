using System;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for ProgramFilterItem.
	/// </summary>
	public class ProgramFilterItem
	{
    string mTitle = "";
    string mTitle2 = "";
    string mGenre = "";
    string mCountry = "";
    string mManufacturer = "";
    int mYear = -1;
    int mRating = 5;

		public ProgramFilterItem()
		{
			//
			// TODO: Add constructor logic here
			//
		}
    public string Title
    {
      get
      {
        return mTitle;
      }
      set
      {
        mTitle = value;
      }
    }
    public string Title2
    {
      get
      {
        return mTitle2;
      }
      set
      {
        mTitle2 = value;
      }
    }

    public string Genre
    {
      get
      {
        return mGenre;
      }
      set
      {
        mGenre = value;
      }
    }
    public string Country
    {
      get
      {
        return mCountry;
      }
      set
      {
        mCountry = value;
      }
    }
    public string Manufacturer
    {
      get
      {
        return mManufacturer;
      }
      set
      {
        mManufacturer = value;
      }
    }
    public int Year
    {
      get
      {
        return mYear;
      }
      set
      {
        mYear = value;
      }
    }
    public int Rating
    {
      get
      {
        return mRating;
      }
      set
      {
        mRating = value;
      }
    }

	}
}
