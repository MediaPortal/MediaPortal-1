// JScript File
function DontRecord(id, entireSchedule)
{
    WebGuide.WebGuideService.DontRecord(id,entireSchedule,OnRecordProgramResult); 
}

function GetProgramInfo(id)
{
    WebGuide.WebGuideService.GetProgramInfo(id,OnGotProgramInfo);   
}

function RecordProgram(id, recordingType)
{
    WebGuide.WebGuideService.RecordProgram(id,recordingType,OnRecordProgramResult);   
}

function OnRecordProgramResult(result)
{
  updateTime( (new Date()).getTime() );
}

function OnGotProgramInfo(result)
{
  document.getElementById("divInfoBox").style.visibility="visible";
  document.getElementById("labelTitle").innerHTML=result.Title;
  document.getElementById("labelDescription").innerHTML=result.description;
  document.getElementById("labelStartEnd").innerHTML=result.startTime +'-'+result.endTime;
  document.getElementById("labelChannel").innerHTML=result.channel;
  document.getElementById("labelGenre").innerHTML=result.genre;
  document.getElementById("imgLogo").src=result.logo;
  if (result.recordingType<0)
  {
    document.getElementById("buttonDontRecord").style.visibility="hidden";
    document.getElementById("buttonRecordOnce").style.visibility="inherit";
    document.getElementById("buttonRecordDaily").style.visibility="inherit";
    document.getElementById("buttonRecordWeekly").style.visibility="inherit";
    document.getElementById("buttonRecordMonFri").style.visibility="inherit";
    document.getElementById("buttonRecordWeekends").style.visibility="inherit";
    document.getElementById("buttonRecordThis").style.visibility="inherit";
    document.getElementById("buttonRecordAll").style.visibility="inherit";
  }
  else
  {
    document.getElementById("buttonDontRecord").style.visibility="inherit";
    document.getElementById("buttonRecordOnce").style.visibility="hidden";
    document.getElementById("buttonRecordDaily").style.visibility="hidden";
    document.getElementById("buttonRecordWeekly").style.visibility="hidden";
    document.getElementById("buttonRecordMonFri").style.visibility="hidden";
    document.getElementById("buttonRecordWeekends").style.visibility="hidden";
    document.getElementById("buttonRecordThis").style.visibility="hidden";
    document.getElementById("buttonRecordAll").style.visibility="hidden";
  }
}