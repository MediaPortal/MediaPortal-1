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
    public string Title { get; set; }
    public string Description { get; set; }
    public int SeriesNum { get; set; }
    public int EpisodeNum { get; set; }
    public string Genre { get; set; }
    public DateTime OriginalAirDate { get; set; }
    public int Classification { get; set; }
    public int StarRating { get; set; }
    public int ParentalRating { get; set; }
    public string EpisodeName { get; set; }
    public int EpisodePart { get; set; }    
    public ProgramCategoryDTO ReferencedProgramCategory { get; set; }
    public IList<ProgramCreditDTO> ReferencedProgramCredits { get; set; }
    public bool PreviouslyShown { get; set; }   
  }  
}
