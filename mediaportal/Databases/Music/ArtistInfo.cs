using System;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class ArtistInfo 
  {
    string m_strArtist="";
    string m_strBorn="";
    string m_strYearsActive="";
    string m_strGenres="";
    string m_strTones="" ;
    string m_strStyles="" ;
    string m_strInstruments="" ;
    string m_strAMGBio="" ;
    string m_strImage ="";
    string m_strAlbums ="";
    string m_strCompilations ="";
    string m_strSingles ="";
    string m_strMisc ="";
		
    public ArtistInfo()
    {
    }
    
    public ArtistInfo Clone()
    {
      ArtistInfo newartist = new ArtistInfo();
      newartist.Artist=Artist;
      newartist.Born=Born;
      newartist.YearsActive=YearsActive;
      newartist.Genres=Genres;
      newartist.Tones=Tones;
      newartist.Styles=Styles;
      newartist.Image=Image;
      newartist.Instruments=Instruments;
      newartist.AMGBio=AMGBio;
      newartist.Albums=Albums;
      newartist.Compilations=Compilations;
      newartist.Singles=Singles;
      newartist.Misc=Misc;
      return newartist;
    }

    public string Born
    {
      get { return m_strBorn;}
      set { m_strBorn=value;}
    }
    public string YearsActive
    {
      get { return m_strYearsActive;}
      set { m_strYearsActive=value;}
    }
    public string Image
    {
      get { return m_strImage;}
      set { m_strImage=value;}
    }
    public string AMGBio
    {
      get { return m_strAMGBio;}
      set { m_strAMGBio=value;}
    }
    public string Styles
    {
      get { return m_strStyles;}
      set { m_strStyles=value;}
    }
    public string Tones
    {
      get { return m_strTones;}
      set { m_strTones=value;}
    }
    public string Genres
    {
      get { return m_strGenres;}
      set { m_strGenres=value;}
    }
    public string Artist
    {
      get { return m_strArtist;}
      set { m_strArtist=value;}
    }

    public string Albums
    {
      get { return m_strAlbums;}
      set { m_strAlbums=value;}
    }
    public string Compilations
    {
      get { return m_strCompilations;}
      set { m_strCompilations=value;}
    }
    public string Singles
    {
      get { return m_strSingles;}
      set { m_strSingles=value;}
    }
    public string Misc
    {
      get { return m_strMisc;}
      set { m_strMisc=value;}
    }
    public string Instruments
    {
      get
      {
        return m_strInstruments;
      }
      set 
      {
        m_strInstruments=value;
      }
    }
  }
}
