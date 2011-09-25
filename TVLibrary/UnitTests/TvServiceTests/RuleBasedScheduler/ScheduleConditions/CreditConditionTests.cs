using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using TvLibrary.Interfaces;
using TvService;

namespace TVServiceTests.RuleBasedScheduler.ScheduleConditions
{
  [TestFixture]
  public class CreditConditionTests
  {    
    
    [Test]
    public void CreditConditionTest()
    {
      ProgramCreditDTO creditActor = new ProgramCreditDTO();
      creditActor.Role = "Actor";
      creditActor.Person = "Clint EastWood";

      ProgramCreditDTO creditDir = new ProgramCreditDTO();
      creditDir.Role = "Director";
      creditDir.Person = "Steven Spielberg";

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      IList<ProgramCreditDTO> creditActors = new ObservableCollection<ProgramCreditDTO>();
      creditActors.Add(creditActor);
      prg.ReferencedProgramCredits = creditActors;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      IList<ProgramCreditDTO> creditDirs = new ObservableCollection<ProgramCreditDTO>();
      creditDirs.Add(creditDir);
      prg.ReferencedProgramCredits = creditDirs;
      programs.Add(prg);

      ProgramCreditDTO creditActorLowerCase = new ProgramCreditDTO();
      creditActorLowerCase.Role = "actor";
      creditActorLowerCase.Person = "clint eastWood";
      List<ProgramCreditDTO> programCreditDtos = new List<ProgramCreditDTO>();
      programCreditDtos.Add(creditActorLowerCase);
      var creditCondition = new CreditCondition(programCreditDtos);

      IQueryable<ProgramDTO> prgsQuery = creditCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);            
    }

    [Test]
    public void CreditConditionMultipleCreditsTest()
    {
      ProgramCreditDTO creditActor = new ProgramCreditDTO();
      creditActor.Role = "Actor";
      creditActor.Person = "Clint EastWood";

      ProgramCreditDTO creditDir = new ProgramCreditDTO();
      creditDir.Role = "Director";
      creditDir.Person = "Steven Spielberg";

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      IList<ProgramCreditDTO> creditActors = new ObservableCollection<ProgramCreditDTO>();
      creditActors.Add(creditActor);
      creditActors.Add(creditDir);
      prg.ReferencedProgramCredits = creditActors;
      programs.Add(prg);
      
      prg = new ProgramDTO();
      prg.IdProgram = 2;            
      programs.Add(prg);

      ProgramCreditDTO creditActorLowerCase = new ProgramCreditDTO();
      creditActorLowerCase.Role = "actor";
      creditActorLowerCase.Person = "clint eastWood";
      ProgramCreditDTO creditDirLowerCase = new ProgramCreditDTO();
      creditDirLowerCase.Role = "director";
      creditDirLowerCase.Person = "steven spielberg";
      List<ProgramCreditDTO> programCreditDtos = new List<ProgramCreditDTO>();
      programCreditDtos.Add(creditActorLowerCase);
      programCreditDtos.Add(creditDirLowerCase);
      var creditCondition = new CreditCondition(programCreditDtos);

      IQueryable<ProgramDTO> prgsQuery = creditCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(1, prgsQuery.Count());
      Assert.AreEqual(1, prgsQuery.FirstOrDefault().IdProgram);
    }



    [Test]
    public void CreditConditionNothingFoundTest()
    {
      ProgramCreditDTO creditActor = new ProgramCreditDTO();
      creditActor.Role = "Actor";
      creditActor.Person = "Clint EastWood";

      ProgramCreditDTO creditDir = new ProgramCreditDTO();
      creditDir.Role = "Director";
      creditDir.Person = "Steven Spielberg";

      IList<ProgramDTO> programs = new ObservableCollection<ProgramDTO>();
      ProgramDTO prg = new ProgramDTO();
      prg.IdProgram = 1;
      IList<ProgramCreditDTO> creditActors = new ObservableCollection<ProgramCreditDTO>();
      creditActors.Add(creditActor);
      prg.ReferencedProgramCredits = creditActors;
      programs.Add(prg);

      prg = new ProgramDTO();
      prg.IdProgram = 2;
      IList<ProgramCreditDTO> creditDirs = new ObservableCollection<ProgramCreditDTO>();
      creditDirs.Add(creditDir);
      prg.ReferencedProgramCredits = creditDirs;
      programs.Add(prg);

      ProgramCreditDTO creditActorLowerCase = new ProgramCreditDTO();
      creditActorLowerCase.Role = "waterboy";
      creditActorLowerCase.Person = "clint eastWood";
      List<ProgramCreditDTO> programCreditDtos = new List<ProgramCreditDTO>();
      programCreditDtos.Add(creditActorLowerCase);
      var creditCondition = new CreditCondition(programCreditDtos);

      IQueryable<ProgramDTO> prgsQuery = creditCondition.ApplyCondition(programs.AsQueryable());
      Assert.AreEqual(0, prgsQuery.Count());
    }
   }
}
