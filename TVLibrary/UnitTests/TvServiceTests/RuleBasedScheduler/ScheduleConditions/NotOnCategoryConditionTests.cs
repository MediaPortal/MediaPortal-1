using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class NotOnCategoryConditionTests
  {    
    
    [Test]
    public void NotOnCategoryConditionTest()
    {
      ProgramCategoryDTO catScifi = new ProgramCategoryDTO();
      catScifi.Category = "scifi";
      catScifi.IdCategory = 1;

      ProgramCategoryDTO catAction = new ProgramCategoryDTO();
      catAction.Category = "action";
      catAction.IdCategory = 2;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.ReferencedProgramCategory = catScifi;
      programs.Add(prg);
     

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.ReferencedProgramCategory = catAction;
      programs.Add(prg);

      IList<ProgramCategoryDTO> cats = new ObservableCollection<ProgramCategoryDTO>();
      cats.Add(catScifi);
      var notOnCategoryCondition = new NotOnCategoryCondition(cats);

      IQueryable<ProgramDTO> prgsQuery = notOnCategoryCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(2, prgsQuery.FirstOrDefault().IdProgram);            
    }

    [Test]
    public void NotOnCategoryConditionNothingFoundTest()
    {
      ProgramCategoryDTO catScifi = new ProgramCategoryDTO();
      catScifi.Category = "scifi";
      catScifi.IdCategory = 1;

      ProgramCategoryDTO catAction = new ProgramCategoryDTO();
      catAction.Category = "action";
      catAction.IdCategory = 2;

      ProgramCategoryDTO catComedy = new ProgramCategoryDTO();
      catComedy.Category = "comedy";
      catComedy.IdCategory = 3;

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      prg.ReferencedProgramCategory = catScifi;
      programs.Add(prg);


      prg = new ProgramDTO();
      prg.IdProgram = 2;
      prg.ReferencedProgramCategory = catAction;
      programs.Add(prg);

      IList<ProgramCategoryDTO> cats = new ObservableCollection<ProgramCategoryDTO>();
      cats.Add(catComedy);
      var onCategoryCondition = new NotOnCategoryCondition(cats);

      IQueryable<ProgramDTO> prgsQuery = onCategoryCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(2, prgsQuery.Count());
    }
   }
}
