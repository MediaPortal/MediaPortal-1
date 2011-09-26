using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class ProgramDTO
  {
    public int IdProgram { get; set; }
    public ChannelDTO ReferencedChannel { get; set; }
    public DateTime StartTime{ get; set; }
    public DateTime EndTime{ get; set; }

    [ProgramAttribute("Title1", 1)]
    public string Title { get; set; }

    [ProgramAttribute("Description", 2)]
    public string Description { get; set; }

    [ProgramAttribute("SeriesNum", 3)]
    public int SeriesNum { get; set; }

    [ProgramAttribute("EpisodeNum", 4)]
    public int EpisodeNum { get; set; }
    public string Genre { get; set; }
    public DateTime OriginalAirDate { get; set; }

    [ProgramAttribute("Classification", 5)]
    public int Classification { get; set; }

    [ProgramAttribute("StarRating", 6)]
    public int StarRating { get; set; }

    [ProgramAttribute("ParentalRating", 7)]
    public int ParentalRating { get; set; }

    [ProgramAttribute("EpisodeName", 8)]
    public string EpisodeName { get; set; }

    [ProgramAttribute("EpisodePart", 9)]
    public int EpisodePart { get; set; }    
    public ProgramCategoryDTO ReferencedProgramCategory { get; set; }
    public IList<ProgramCreditDTO> ReferencedProgramCredits { get; set; }
    public bool PreviouslyShown { get; set; }   
  }  
}
