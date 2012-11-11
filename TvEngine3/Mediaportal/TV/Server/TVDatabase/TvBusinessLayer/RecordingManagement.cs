using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class RecordingManagement
  {
    public static Recording GetRecording(int idRecording)
    {
      //lazy loading verified ok
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        Recording recording = recordingRepository.GetRecording(idRecording);
        return recording;
      }
    }

    public static void DeleteRecording(int idRecording)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository(true))
      {
        recordingRepository.Delete<Recording>(s => s.IdRecording == idRecording);
        recordingRepository.UnitOfWork.SaveChanges();
      }
    }

    public static IList<Recording> ListAllRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      //lazy loading verified ok
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        return recordingRepository.ListAllRecordingsByMediaType(mediaType).ToList();
      }
    }

    public static Recording SaveRecording(Recording recording)
    {
      using (var recordingRepository = new RecordingRepository())
      {
        recordingRepository.AttachEntityIfChangeTrackingDisabled(recordingRepository.ObjectContext.Recordings, recording);                        
        recordingRepository.ApplyChanges(recordingRepository.ObjectContext.Recordings, recording);
        recordingRepository.UnitOfWork.SaveChanges();
        recording.AcceptChanges();
        return recording;
      }      
    }

    public static Recording GetRecordingByFileName(string filename)
    {
      //lazy loading verified ok
      using (var recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> recordingsByFileName = recordingRepository.GetQuery<Recording>(r => r.FileName == filename);
        Recording recordingByFileName = recordingRepository.IncludeAllRelations(recordingsByFileName).FirstOrDefault();
        return recordingByFileName;
      }
    }

    public static Recording GetActiveRecording(int idSchedule)
    {
      //lazy loading verified ok
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> recordings =
          recordingRepository.GetQuery<Recording>(r => r.IsRecording && r.IdSchedule == idSchedule);
        Recording activeRecording = recordingRepository.IncludeAllRelations(recordings).FirstOrDefault();
        return activeRecording;
      }
    }

    public static Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel)
    {
      //lazy loading verified ok
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> recordings =
          recordingRepository.GetQuery<Recording>(r => r.IsRecording && r.IdChannel == idChannel && r.Title == title);
        Recording activeRecordingByTitleAndChannel =
          recordingRepository.IncludeAllRelations(recordings).FirstOrDefault();
        return activeRecordingByTitleAndChannel;
      }
    }

    public static IList<Recording> ListAllActiveRecordingsByMediaType(MediaTypeEnum mediaType)
    {
      //lazy loading verified ok
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> allActiveRecordingsByMediaType =
          recordingRepository.GetQuery<Recording>(c => c.MediaType == (int) mediaType && c.IsRecording);
        allActiveRecordingsByMediaType = recordingRepository.IncludeAllRelations(allActiveRecordingsByMediaType);
        return allActiveRecordingsByMediaType.ToList();
      }
    }

    public static bool HasRecordingPendingDeletion(string filename)
    {
      bool hasRecordingPendingDeletion;
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        hasRecordingPendingDeletion =
          recordingRepository.Count<PendingDeletion>(c => c.FileName == filename) > 0;      
      }
      return hasRecordingPendingDeletion;
    }

    public static PendingDeletion SaveRecordingPendingDeletion(PendingDeletion pendingDeletion)
    {
      using (var recordingRepository = new RecordingRepository())
      {
        recordingRepository.AttachEntityIfChangeTrackingDisabled(recordingRepository.ObjectContext.PendingDeletions, pendingDeletion);        
        recordingRepository.ApplyChanges(recordingRepository.ObjectContext.PendingDeletions, pendingDeletion);        
        recordingRepository.UnitOfWork.SaveChanges();
        pendingDeletion.AcceptChanges();
        return pendingDeletion;
      }      
    }

    public static void DeletePendingRecordingDeletion(int idPendingDeletion)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository(true))
      {
        recordingRepository.Delete<PendingDeletion>(s => s.IdPendingDeletion == idPendingDeletion);
        recordingRepository.UnitOfWork.SaveChanges();
      }
    }

    public static PendingDeletion GetPendingRecordingDeletion(int idPendingDeletion)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        PendingDeletion pendingDeletion = recordingRepository.FindOne<PendingDeletion>(p => p.IdPendingDeletion == idPendingDeletion);
        return pendingDeletion;
      }
    }

    public static IList<PendingDeletion> ListAllPendingRecordingDeletions()
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        var listAllPendingRecordingDeletions = recordingRepository.GetAll<PendingDeletion>().ToList();
        return listAllPendingRecordingDeletions;
      }
    }

    public static void ResetActiveRecordings()
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> activeRecordings = recordingRepository.GetAll<Recording>();
        foreach (Recording rec in activeRecordings)
        {
          rec.IsRecording = false;          
        }

        recordingRepository.ApplyChanges(recordingRepository.ObjectContext.Recordings, activeRecordings);
        //recordingRepository.UpdateList(activeRecordings);
        recordingRepository.UnitOfWork.SaveChanges();
      }            
    }
  }
}
