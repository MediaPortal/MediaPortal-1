using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using MediaPortal.Video.Database;

using NUnit.Framework;

namespace MediaPortal.Tests.Databases.Video
{
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

    public static IEnumerable<string> ScriptNames
    {
      get
      {
        string scriptsDir = CSScriptLoader.FindScriptsDirectory("MovieInfo");

        foreach (string scriptPath in Directory.GetFiles(scriptsDir, "*.csscript"))
        {
          yield return Path.GetFileNameWithoutExtension(scriptPath);
        }
      }
    }

    private static IIMDBScriptGrabber LoadGrabber(string scriptName)
    {
      string scriptsDir = CSScriptLoader.FindScriptsDirectory("MovieInfo");
      string scriptPath = Path.Combine(scriptsDir, scriptName + ".csscript");
      Assembly assembly = CSScriptLoader.LoadScript(scriptPath);
      return (IIMDBScriptGrabber)CSScriptLoader.CreateObject(assembly, "Grabber");
    }

    public static IEnumerable<object[]> SearchStrings
    {
      get
      {
        // filename, hasTT
        yield return new object[] { "The Shawshank Redemption", false };
        yield return new object[] { "The Shawshank Redemption (1994) [tt0111161]", true };
        yield return new object[] { "TSR [tt0111161]", true };
        yield return new object[] { "TSR tt0111161", true };
      }
    }

    [Test]
    public void FindFilm_ReturnsResults(
      [ValueSource(nameof(ScriptNames))] string scriptName,
      [ValueSource(nameof(SearchStrings))] object[] searchCase)
    {
      string searchString = (string)searchCase[0];
      bool hasTT = (bool)searchCase[1];

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
      Assert.AreEqual("The Shawshank Redemption (1994)", result.Title,
        "Title should be 'The Shawshank Redemption (1994)' for script {0} with search '{1}'",
        scriptName, searchString);
    }

    [Test]
    public void GetDetails_ReturnsCorrectData(
      [ValueSource(nameof(ScriptNames))] string scriptName)
    {
      IIMDBScriptGrabber grabber = LoadGrabber(scriptName);
      ArrayList elements = new ArrayList();

      grabber.FindFilm("The Shawshank Redemption (1994) tt0111161", 10, elements);

      Assert.IsTrue(elements.Count > 0,
        "FindFilm should return at least one result for script {0}", scriptName);

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
