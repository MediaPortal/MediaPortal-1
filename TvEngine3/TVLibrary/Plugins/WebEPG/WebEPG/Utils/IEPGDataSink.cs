public interface IEpgDataSink
{
    // Methods
    void Close();
    void EndChannelPrograms(string id, string name);
    void Open();
    bool StartChannelPrograms(string id, string name);
    void WriteChannel(string id, string name);
    void WriteProgram(ProgramData programData, bool merged);
}