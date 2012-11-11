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
    [Program("Title", 1)]
    public string title { get; set; }

    [Program("Description", 2)]
    public string description { get; set; }

    [Program("SeriesNum", 3)]
    public int seriesNum { get; set; }

    [Program("EpisodeNum", 4)]
    public int episodeNum { get; set; }

    [Program("Classification", 5)]
    public int classification { get; set; }

    [Program("StarRating", 6)]
    public int starRating { get; set; }

    [Program("ParentalRating", 7)]
    public int parentalRating { get; set; }

    [Program("EpisodeName", 8)]
    public string episodeName { get; set; }

    [Program("EpisodePart", 9)]
    public int episodePart { get; set; }    
  }

  [MetadataType(typeof(ProgramMetadata))]
  public partial class Program
  {
    public Program Clone()
    {
      var p = new Program();
      p.IdProgram = IdProgram;
      p.IdChannel = IdChannel;
      p.StartTime = StartTime;
      p.EndTime = EndTime;
      p.Title = Title;
      p.Description = Description;
      p.ProgramCategory = ProgramCategory;
      p.State = State;
      p.OriginalAirDate = OriginalAirDate;
      p.SeriesNum = SeriesNum;
      p.EpisodeNum = EpisodeNum;
      p.State = State;
      p.EpisodeName = EpisodeName;
      p.EpisodePart = EpisodePart;
      p.StarRating = StarRating;
      p.Classification = Classification;      
      return p;
    }
   
  }
}
