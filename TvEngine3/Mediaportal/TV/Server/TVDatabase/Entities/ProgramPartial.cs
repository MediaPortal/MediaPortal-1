using System.ComponentModel.DataAnnotations;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  /*[MetadataType (typeof (BookingMetadata))]
public partial class Booking
{
 // This is your custom partial class     
}

public class BookingMetadata
{
 [Required] [StringLength(15)]
 public object ClientName { get; set; }

 [Range(1, 20)]
 public object NumberOfGuests { get; set; }

 [Required] [DataType(DataType.Date)]
 public object ArrivalDate { get; set; }
}
*/

  public class ProgramMetadata
  {
    [ProgramAttribute("Title", 1)]
    public string title { get; set; }

    [ProgramAttribute("Description", 2)]
    public string description { get; set; }

    [ProgramAttribute("SeriesNum", 3)]
    public int seriesNum { get; set; }

    [ProgramAttribute("EpisodeNum", 4)]
    public int episodeNum { get; set; }

    [ProgramAttribute("Classification", 5)]
    public int classification { get; set; }

    [ProgramAttribute("StarRating", 6)]
    public int starRating { get; set; }

    [ProgramAttribute("ParentalRating", 7)]
    public int parentalRating { get; set; }

    [ProgramAttribute("EpisodeName", 8)]
    public string episodeName { get; set; }

    [ProgramAttribute("EpisodePart", 9)]
    public int episodePart { get; set; }    
  }

  [MetadataType(typeof(ProgramMetadata))]
  public partial class Program
  {
    public Program Clone()
    {
      var p = new Program();
      p.idProgram = idProgram;
      p.idChannel = idChannel;
      p.startTime = startTime;
      p.endTime = endTime;
      p.title = title;
      p.description = description;
      p.ProgramCategory = ProgramCategory;
      p.state = state;
      p.originalAirDate = originalAirDate;
      p.seriesNum = seriesNum;
      p.episodeNum = episodeNum;
      p.state = state;
      p.episodeName = episodeName;
      p.episodePart = episodePart;
      p.starRating = starRating;
      p.classification = classification;
      p.classification = classification;
      return p;
    }
   
  }
}
