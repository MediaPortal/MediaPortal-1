#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using TvPlugin;
using MediaPortal.Player;
using TvPluginTests.IPlayerStubs;

#endregion

namespace TvPluginTests
{
  [TestFixture]
  public class TVHomeTest
  {
    #region setup/teardown

    [TestFixtureSetUp]
    public void TVHomeTestFixtureSetup()
    {
    }

    [TearDown]
    public void TVHomeTearDown()
    {
    }

    [TestFixtureTearDown]
    public void TVHomeTestFixtureTearDown()
    {
    }

    [SetUp]
    public void TVHomeSetUp()
    {
    }    

    #endregion

    #region GetPreferedAudioStreamIndex Tests

    #region 1 audiostream available tests

    [Test]
    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1, lang: dan.
    /// pref_lang : empty
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest1()
    {
      g_Player.Player = new PlayerSingleAudioStreamMpeg1Dan();

      List<string> prefLangs = new List<string>();
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);
      
      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");      
    }

    [Test]
    ///<summary>
    /// streams : 1
    /// stream1 : ac3, lang: dan.
    /// pref_lang : empty
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest2()
    {
      g_Player.Player = new PlayerSingleAudioStreamAC3Dan();

      List<string> prefLangs = new List<string>();
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1, lang: dan.
    /// pref_lang : empty
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest3()
    {
      g_Player.Player = new PlayerSingleAudioStreamMpeg1Dan();

      List<string> prefLangs = new List<string>();
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 1
    /// stream1 : ac3, lang: dan.
    /// pref_lang : eng
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest4()
    {
      g_Player.Player = new PlayerSingleAudioStreamAC3Dan();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    #endregion

    #region multiple audiostreams tests

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : empty
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest5()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : dan
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest6()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("dan");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;            
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : dan
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest7()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("dan");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 1, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : eng
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest8()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 2, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : deu
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest9()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 4, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : deu
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : true
    ///</summary>
    public void GetPreferedAudioStreamIndexTest10()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = true;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 1, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : swe, eng
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest11()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("swe");
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = true;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 2, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : swe, no
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest12()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("swe");
      prefLangs.Add("no");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = true;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : swe, no
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest13()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("swe");
      prefLangs.Add("no");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = true;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 1, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : swe, eng
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest14()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("swe");
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = true;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 3, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : deu, eng
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest15()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 3, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : deu, eng
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : true
    ///</summary>
    public void GetPreferedAudioStreamIndexTest16()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = true;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;
      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 4, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    [Test]
    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1, lang: dan.
    /// stream2 : ac3, lang: dan.
    /// stream3 : mpeg1, lang: eng.
    /// stream4 : ac3, lang: eng.
    /// stream5 : mpeg1, lang: deu.
    /// pref_lang : empty
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest27()
    {
      g_Player.Player = new PlayerMultipleAudioStreams();

      List<string> prefLangs = new List<string>();
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 1, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    #endregion

    #region 1 dualmono audiostream available tests

    [Test]
    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1 dualmono, lang: left:dan right:eng
    /// pref_lang : empty
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest17()
    {
      g_Player.Player = new PlayerSingleDualMonoAudioStreamMpeg1DanEng();      

      List<string> prefLangs = new List<string>();
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");      
    }

    [Test]
    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1 dualmono, lang: left:dan right:eng
    /// pref_lang : dan
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest18()
    {
      g_Player.Player = new PlayerSingleDualMonoAudioStreamMpeg1DanEng();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("dan");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;      

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.LEFT_MONO, "dualMonoMode returned should be LEFT_MONO");
    }

    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1 dualmono, lang: left:dan right:eng
    /// pref_lang : eng
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest19()
    {
      g_Player.Player = new PlayerSingleDualMonoAudioStreamMpeg1DanEng();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.RIGHT_MONO, "dualMonoMode returned should be RIGHT_MONO");
    }

    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1 dualmono, lang: left:dan right:eng
    /// pref_lang : deu
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest20()
    {
      g_Player.Player = new PlayerSingleDualMonoAudioStreamMpeg1DanEng();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    ///<summary>
    /// streams : 1
    /// stream1 : mpeg1 dualmono, lang: left:dan right:eng
    /// pref_lang : deu,eng
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest21()
    {
      g_Player.Player = new PlayerSingleDualMonoAudioStreamMpeg1DanEng();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.RIGHT_MONO, "dualMonoMode returned should be RIGHT_MONO");
    }

    #endregion

    #region multiple mixed dualmono audiostream available tests

    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1         , lang: dan
    /// stream2 : mpeg1 dualmono, lang: left: dan right: eng.  
    /// stream3 : mpeg1 dualmono, lang: left: deu right: swe.  
    /// stream4 : ac3           , lang: eng
    /// stream5 : mpeg1         , lang: eng
    /// pref_lang : deu
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest22()
    {
      g_Player.Player = new PlayerMultipleDualMonoAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("deu");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 2, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.LEFT_MONO, "dualMonoMode returned should be UNSUPPORTED");
    }


    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1         , lang: dan
    /// stream2 : mpeg1 dualmono, lang: left: dan right: eng.  
    /// stream3 : mpeg1 dualmono, lang: left: deu right: swe.  
    /// stream4 : ac3           , lang: eng
    /// stream5 : mpeg1         , lang: eng
    /// pref_lang : fra, deu
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest23()
    {
      g_Player.Player = new PlayerMultipleDualMonoAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("fra");
      prefLangs.Add("deu");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 2, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.LEFT_MONO, "dualMonoMode returned should be LEFT_MONO");
    }

    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1         , lang: dan
    /// stream2 : mpeg1 dualmono, lang: left: dan right: eng.  
    /// stream3 : mpeg1 dualmono, lang: left: deu right: swe.  
    /// stream4 : ac3           , lang: eng
    /// stream5 : mpeg1         , lang: eng
    /// pref_lang : eng
    /// PreferAC3 : true
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest24()
    {
      g_Player.Player = new PlayerMultipleDualMonoAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = true;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 3, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }

    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1         , lang: dan
    /// stream2 : mpeg1 dualmono, lang: left: dan right: eng.  
    /// stream3 : mpeg1 dualmono, lang: left: deu right: swe.  
    /// stream4 : ac3           , lang: eng
    /// stream5 : mpeg1         , lang: eng
    /// pref_lang : eng
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest25()
    {
      g_Player.Player = new PlayerMultipleDualMonoAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("eng");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 1, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.RIGHT_MONO, "dualMonoMode returned should be RIGHT_MONO");
    }

    ///<summary>
    /// streams : 5
    /// stream1 : mpeg1         , lang: dan
    /// stream2 : mpeg1 dualmono, lang: left: dan right: eng.  
    /// stream3 : mpeg1 dualmono, lang: left: deu right: swe.  
    /// stream4 : ac3           , lang: eng
    /// stream5 : mpeg1         , lang: eng
    /// pref_lang : dan
    /// PreferAC3 : false
    /// PreferAudioTypeOverLang : false
    ///</summary>
    public void GetPreferedAudioStreamIndexTest26()
    {
      g_Player.Player = new PlayerMultipleDualMonoAudioStreams();

      List<string> prefLangs = new List<string>();
      prefLangs.Add("dan");
      TVHome.PreferredLanguages = prefLangs; //empty
      TVHome.PreferAC3 = false;
      TVHome.PreferAudioTypeOverLang = false;

      eAudioDualMonoMode dualMonoMode = eAudioDualMonoMode.UNSUPPORTED;

      int index = TVHome.GetPreferedAudioStreamIndex(out dualMonoMode);

      Assert.AreEqual(index, 0, "Wrong audio index returned");
      Assert.AreEqual(dualMonoMode, eAudioDualMonoMode.UNSUPPORTED, "dualMonoMode returned should be UNSUPPORTED");
    }  

    #endregion

    #endregion

  }
}
