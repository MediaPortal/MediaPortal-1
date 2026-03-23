using System;
using System.Collections;
using System.Reflection;

using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using MediaPortal.Video.Database;

using NUnit.Framework;

namespace MediaPortal.Tests.Databases.Video
{
  /// <summary>
  /// Integration tests for .csscript movie info grabbers.
  /// These tests compile the csscript files at runtime and call the real TMDB/IMDB APIs,
  /// so they require internet access and a valid API key in the script.
  /// </summary>
  [TestFixture]
  [Category("ScriptGrabber")]
  public class ScriptGrabberTest
  {
    private Assembly _scriptAssembly;
    private IIMDBScriptGrabber _grabber;

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

      // Locate and compile the TI_MDB.csscript
      string scriptsDir = CSScriptLoader.FindScriptsDirectory("MovieInfo");
      string scriptPath = System.IO.Path.Combine(scriptsDir, "TI_MDB.csscript");

      _scriptAssembly = CSScriptLoader.LoadScript(scriptPath);
      _grabber = (IIMDBScriptGrabber)CSScriptLoader.CreateObject(_scriptAssembly, "Grabber");
    }

    [Test]
    public void FindFilm_WithImdbId_ReturnsResults()
    {
      ArrayList elements = new ArrayList();
      _grabber.FindFilm("Harry Potter and the Philosopher's Stone (2005) [tt0241527]", 10, elements);

      Assert.IsTrue(elements.Count > 0, "FindFilm should return at least one result for tt0241527");

      IMDB.IMDBUrl firstResult = (IMDB.IMDBUrl)elements[0];
      Assert.IsNotNull(firstResult, "First result should not be null");
      Assert.IsNotEmpty(firstResult.URL, "Result URL should not be empty");
      StringAssert.Contains("Harry Potter", firstResult.Title, "Result title should contain 'Harry Potter'");
    }

    [Test]
    public void GetDetails_HarryPotter_DirectorIsChrisColumbus()
    {
      ArrayList elements = new ArrayList();
      _grabber.FindFilm("Harry Potter and the Philosopher's Stone (2005) [tt0241527]", 10, elements);

      Assert.IsTrue(elements.Count > 0, "FindFilm should return at least one result");

      IMDB.IMDBUrl url = (IMDB.IMDBUrl)elements[0];
      IMDBMovie movieDetails = new IMDBMovie();

      bool success = _grabber.GetDetails(url, ref movieDetails);

      Assert.IsTrue(success, "GetDetails should return true");
      Assert.AreEqual("Chris Columbus", movieDetails.Director, "Director should be Chris Columbus");
      Assert.IsNotEmpty(movieDetails.Title, "Title should not be empty");
      Assert.IsNotEmpty(movieDetails.IMDBNumber, "IMDB number should not be empty");
      Assert.IsTrue(movieDetails.Year > 0, "Year should be set");
    }

    [Test]
    public void GetName_ReturnsGrabberName()
    {
      string name = _grabber.GetName();

      Assert.IsNotNull(name, "GetName should not return null");
      Assert.IsNotEmpty(name, "GetName should not return empty");
      StringAssert.Contains("TMDB", name, "Grabber name should contain TMDB");
    }

    [Test]
    public void GetLanguage_ReturnsLanguageCode()
    {
      string language = _grabber.GetLanguage();

      Assert.IsNotNull(language, "GetLanguage should not return null");
      Assert.IsNotEmpty(language, "GetLanguage should not return empty");
      Assert.AreEqual(2, language.Length, "Language should be a two-letter code");
    }
  }
}
