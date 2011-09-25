using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class ProgramConditionStringTests
  {

    #region string based tests

    #region equals tests
    [Test]
    public void ProgramConditionStringEqualsTest ()
    {      
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual("GuMp", prgsQuery.FirstOrDefault().Title);
    }

    [Test]
    public void ProgramConditionStringEqualsMultipleTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "gumP";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(2, prgsQuery.Count());      
    }

   [Test]
    public void ProgramConditionStringEqualsNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp123";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.Equals);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    
    }

    #endregion

    #region not contains

   [Test]
   public void ProgramConditionStringNotContainsTest()
   {
     IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

     ProgramDTO prg = new ProgramDTO();
     prg.Title = "GuMp123";
     programs.Add(prg);

     prg = new ProgramDTO();
     prg.Title = "stuff";
     programs.Add(prg);

     var titlePrgCond = new ProgramCondition<string>("Title", "abc", ConditionOperator.NotContains);

     IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
     Assert.AreEqual(2, prgsQuery.Count());     
   }

   [Test]
   public void ProgramConditionStringNotContainsMultipleTest()
   {
     IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

     ProgramDTO prg = new ProgramDTO();
     prg.Title = "GuMp123";
     programs.Add(prg);

     prg = new ProgramDTO();
     prg.Title = "stuffguMP";
     programs.Add(prg);

     prg = new ProgramDTO();
     prg.Title = "abc";
     programs.Add(prg);

     var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.NotContains);

     IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
     Assert.AreEqual(1, prgsQuery.Count());
     Assert.AreEqual("abc", prgsQuery.FirstOrDefault().Title);
   }

   [Test]
   public void ProgramConditionStringNotContainsMiddleSectionTest()
   {
     IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

     ProgramDTO prg = new ProgramDTO();
     prg.Title = "GuMp123";
     programs.Add(prg);

     prg = new ProgramDTO();
     prg.Title = "stuff";
     programs.Add(prg);

     var titlePrgCond = new ProgramCondition<string>("Title", "mp12", ConditionOperator.NotContains);

     IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
     Assert.AreEqual(1, prgsQuery.Count());
     Assert.AreEqual("stuff", prgsQuery.FirstOrDefault().Title);
   }

   [Test]
   public void ProgramConditionStringNotContainsNothingFoundTest()
   {
     IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

     ProgramDTO prg = new ProgramDTO();
     prg.Title = "GuMP";
     programs.Add(prg);

     prg = new ProgramDTO();
     prg.Title = "Gump";
     programs.Add(prg);

     var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.NotContains);

     IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
     Assert.AreEqual(0, prgsQuery.Count());
   }

   #endregion

    #region contains

   [Test]
   public void ProgramConditionStringContainsTest()
   {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp123";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.Contains);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual("GuMp123", prgsQuery.FirstOrDefault().Title);
    }

   [Test]
   public void ProgramConditionStringContainsMultipleTest()
   {
     IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

     ProgramDTO prg = new ProgramDTO();
     prg.Title = "GuMp123";
     programs.Add(prg);

     prg = new ProgramDTO();
     prg.Title = "stuffguMP";
     programs.Add(prg);

     var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.Contains);

     IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
     Assert.AreEqual(2, prgsQuery.Count());     
   }

    [Test]
    public void ProgramConditionStringContainsMiddleSectionTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp123";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "mp12", ConditionOperator.Contains);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual("GuMp123", prgsQuery.FirstOrDefault().Title);
    }

    [Test]
    public void ProgramConditionStringContainsNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "yeah";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gump", ConditionOperator.Contains);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }

    #endregion

    #region startswith

    [Test]
    public void ProgramConditionStringStartsWithTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp123";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gu", ConditionOperator.StartsWith);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual("GuMp123", prgsQuery.FirstOrDefault().Title);
    }

    [Test]
    public void ProgramConditionStringStartsWithMultipleTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "GuMp123";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "guMP";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "gu", ConditionOperator.StartsWith);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(2, prgsQuery.Count());
    }


    [Test]
    public void ProgramConditionStringStartsWithNothingFoundTest()
    {
      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();

      ProgramDTO prg = new ProgramDTO();
      prg.Title = "enuff";
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.Title = "stuff";
      programs.Add(prg);

      var titlePrgCond = new ProgramCondition<string>("Title", "uff", ConditionOperator.StartsWith);

      IQueryable<ProgramDTO> prgsQuery = titlePrgCond.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }

    #endregion

    #endregion


  }
}
