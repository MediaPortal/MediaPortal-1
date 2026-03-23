using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using MediaPortal.Video.Database;

using NUnit.Framework;

namespace MediaPortal.Tests.Databases.Video
{
  public enum TitleLanguage
  {
    En,
    Es,
    De,
    Fr
  }

  /// <summary>
  /// Integration tests for .csscript movie info grabbers.
  /// These tests compile all csscript files in the MovieInfo directory at runtime
  /// and call the real TMDB/IMDB APIs, so they require internet access and a valid
  /// API key in each script.
  /// </summary>
  [TestFixture]
  [Category("ScriptGrabber")]
  public class ScriptGrabberTest
  {
    private static readonly (string Name, bool HasTT, TitleLanguage Language)[] Scripts =
    {
      ("IMDB_MP117x", true, TitleLanguage.En),
      ("IMDB_MP13x", true, TitleLanguage.En),
      ("IMDB", true, TitleLanguage.En),
      ("TMDB", true, TitleLanguage.En),
      ("TI_MDB", true, TitleLanguage.En),
      ("Allocine_fr", true, TitleLanguage.Fr),
      ("APIFilmAffinityIMDbMP1", true, TitleLanguage.Es),
      ("FilmAffinity_es", false, TitleLanguage.Es),
      ("imdb_de_ofdb_MP13x", false, TitleLanguage.De),
      ("TMDB_de", true, TitleLanguage.De),
      ("TMDB_fr_MP13x", false, TitleLanguage.Fr),
    };

    public static IEnumerable<string> ScriptNames
    {
      get
      {
        foreach (var script in Scripts)
        {
          yield return script.Name;
        }
      }
    }

    [OneTimeSetUp]
    public void FixtureSetUp()
    {
      // Register NoLog so that Log.Info/Debug/Error calls in scripts and Core don't throw
      if (!GlobalServiceProvider.IsRegistered<ILog>())
      {
        GlobalServiceProvider.Add<ILog>(new NoLog());
      }
      else
      {
        GlobalServiceProvider.Replace<ILog>(new NoLog());
      }
    }

    private static IIMDBScriptGrabber LoadGrabber(string scriptName)
    {
      string scriptsDir = CSScriptLoader.FindScriptsDirectory("MovieInfo");
      string scriptPath = Path.Combine(scriptsDir, scriptName + ".csscript");
      Assembly assembly = CSScriptLoader.LoadScript(scriptPath);
      return (IIMDBScriptGrabber)CSScriptLoader.CreateObject(assembly, "Grabber");
    }

    [Test]
    public void Scripts_MatchesFilesOnDisk()
    {
      string scriptsDir = CSScriptLoader.FindScriptsDirectory("MovieInfo");

      foreach (string file in Directory.GetFiles(scriptsDir, "*.csscript"))
      {
        string name = Path.GetFileNameWithoutExtension(file);
        Assert.IsTrue(ScriptNames.Contains(name),
          "Script '{0}' found on disk but not in Scripts list", name);
      }
    }

    private static string ShawshankTitle(TitleLanguage language)
    {
      switch (language) // C# 7.3 doesn't support switch expressions yet
      {
        case TitleLanguage.Fr: return "Les Évadés";
        case TitleLanguage.De: return "Die Verurteilten";
        case TitleLanguage.Es: return "Cadena Perpetua";
        default: return "The Shawshank Redemption";
      }
    }

    public static IEnumerable<TestCaseData> FindFilmCases
    {
      get
      {
        foreach (var script in Scripts)
        {
          string title = ShawshankTitle(script.Language);
          string expectedTitle = title + " (1994)";

          yield return new TestCaseData(script.Name, title, expectedTitle);

          if (script.HasTT)
          {
            yield return new TestCaseData(script.Name, title + " (1994) [tt0111161]", expectedTitle);
            yield return new TestCaseData(script.Name, "TSR [tt0111161]", expectedTitle);
            yield return new TestCaseData(script.Name, "TSR tt0111161", expectedTitle);
          }
        }
      }
    }

    [TestCaseSource(nameof(FindFilmCases))]
    public void FindFilm_ReturnsResults(string scriptName, string searchString, string expectedTitle)
    {
      bool hasTT = searchString.Contains("tt0111161");

      IIMDBScriptGrabber grabber = LoadGrabber(scriptName);
      ArrayList elements = new ArrayList();

      grabber.FindFilm(searchString, 10, elements);

      if (hasTT)
      {
        Assert.AreEqual(1, elements.Count,
          "FindFilm('{0}') with script {1} should return exactly one result but returned {2}",
          searchString, scriptName, elements.Count);
      }
      else
      {
        Assert.IsTrue(elements.Count >= 1,
          "FindFilm('{0}') with script {1} should return at least one result but returned {2}",
          searchString, scriptName, elements.Count);
      }

      IMDB.IMDBUrl result = (IMDB.IMDBUrl)elements[0];
      Assert.IsTrue(
        result.Title == expectedTitle || result.Title == "tt0111161",
        "Title should be '{0}' or 'tt0111161' for script {1} with search '{2}', but was '{3}'",
        expectedTitle, scriptName, searchString, result.Title);
    }

    public static IEnumerable<TestCaseData> GetDetailsCases
    {
      get
      {
        foreach (var script in Scripts)
        {
          string title = ShawshankTitle(script.Language);
          string searchString = title + " (1994)" + (script.HasTT ? " tt0111161" : "");
          yield return new TestCaseData(script.Name, searchString);
        }
      }
    }

    /// <summary>
    /// This test gives each script title data in the most favourable format, and checks that GetDetails returns correct data for the movie.
    /// </summary>
    [TestCaseSource(nameof(GetDetailsCases))]
    public void GetDetails_ReturnsCorrectData(string scriptName, string searchString)
    {

      IIMDBScriptGrabber grabber = LoadGrabber(scriptName);
      ArrayList elements = new ArrayList();

      grabber.FindFilm(searchString, 10, elements);

      Assert.IsTrue(elements.Count > 0,
        "FindFilm('{0}') should return at least one result for script {1}", searchString, scriptName);

      IMDB.IMDBUrl url = (IMDB.IMDBUrl)elements[0];
      IMDBMovie movieDetails = new IMDBMovie();

      bool success = grabber.GetDetails(url, ref movieDetails);

      Assert.IsTrue(success, "GetDetails should return true for script {0}", scriptName);
      Assert.AreEqual("Frank Darabont", movieDetails.Director,
        "Director should be Frank Darabont for script {0}", scriptName);
      Assert.IsNotEmpty(movieDetails.Title,
        "Title should not be empty for script {0}", scriptName);
      Assert.IsNotEmpty(movieDetails.IMDBNumber,
        "IMDB number should not be empty for script {0}", scriptName);
      Assert.IsTrue(movieDetails.Year > 0,
        "Year should be set for script {0}", scriptName);
      Assert.IsNotEmpty(movieDetails.Plot,
        "Plot should not be empty for script {0}", scriptName);
      Assert.IsNotEmpty(movieDetails.Genre,
        "Genre should not be empty for script {0}", scriptName);
      Assert.IsTrue(movieDetails.Rating > 0,
        "Rating should be greater than 0 for script {0}", scriptName);
      Assert.IsNotEmpty(movieDetails.Cast,
        "Cast should not be empty for script {0}", scriptName);
    }
  }
}
