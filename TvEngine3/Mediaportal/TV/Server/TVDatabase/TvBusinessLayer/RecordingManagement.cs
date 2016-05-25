using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class RecordingManagement
  {
    public static IList<Recording> ListAllRecordings()
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetAll<Recording>();
        query = recordingRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<Recording> ListAllRecordingsByMediaType(MediaType mediaType)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetQuery<Recording>(r => r.MediaType == (int)mediaType);
        query = recordingRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static IList<Recording> ListAllActiveRecordingsByMediaType(MediaType mediaType)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetQuery<Recording>(c => c.MediaType == (int)mediaType && c.IsRecording);
        query = recordingRepository.IncludeAllRelations(query);
        return query.ToList();
      }
    }

    public static Recording GetRecording(int idRecording)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetQuery<Recording>(c => c.IdRecording == idRecording);
        query = recordingRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static Recording GetRecordingByFileName(string fileName)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetQuery<Recording>(r => r.FileName == fileName);
        query = recordingRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static Recording GetActiveRecording(int idSchedule)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetQuery<Recording>(r => r.IsRecording && r.IdSchedule == idSchedule);
        query = recordingRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static Recording GetActiveRecordingByTitleAndChannel(string title, int idChannel)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IQueryable<Recording> query = recordingRepository.GetQuery<Recording>(r => r.IsRecording && r.IdChannel == idChannel && r.Title == title);
        query = recordingRepository.IncludeAllRelations(query);
        return query.FirstOrDefault();
      }
    }

    public static Recording SaveRecording(Recording recording)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        recordingRepository.AttachEntityIfChangeTrackingDisabled(recordingRepository.ObjectContext.Recordings, recording);
        recordingRepository.ApplyChanges(recordingRepository.ObjectContext.Recordings, recording);
        recordingRepository.UnitOfWork.SaveChanges();
        recording.AcceptChanges();
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

    public static void ResetActiveRecordings()
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        IList<Recording> activeRecordings = recordingRepository.GetQuery<Recording>(r => r.IsRecording).ToList();
        foreach (Recording rec in activeRecordings)
        {
          rec.IsRecording = false;
        }

        recordingRepository.ApplyChanges(recordingRepository.ObjectContext.Recordings, activeRecordings);
        recordingRepository.UnitOfWork.SaveChanges();
      }
    }

    #region pending deletions

    public static IList<PendingDeletion> ListAllPendingRecordingDeletions()
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        return recordingRepository.GetAll<PendingDeletion>().ToList();
      }
    }

    public static PendingDeletion GetPendingRecordingDeletion(int idPendingDeletion)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        return recordingRepository.FindOne<PendingDeletion>(p => p.IdPendingDeletion == idPendingDeletion);
      }
    }

    public static bool HasRecordingPendingDeletion(string fileName)
    {
      using (IRecordingRepository recordingRepository = new RecordingRepository())
      {
        return recordingRepository.Count<PendingDeletion>(c => c.FileName == fileName) > 0;
      }
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

    #endregion
  }
}