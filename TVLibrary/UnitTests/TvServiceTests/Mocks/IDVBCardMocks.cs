using TvService;

namespace TVServiceTests.Mocks
{
  public interface IDVBCardMocks
  {
    bool Idle { get; set; }
    bool HasCAM { get; set; }
    bool Enabled { get; set; }
    bool Present { get; set; }
    int NumberOfChannelsDecrypting { get; set; }
    bool IsTunedToTransponder { get; set; }
    bool IsOwner { get; set; }
    int NumberOfUsersOnCard { get; set; }
    ITvCardHandler GetMockedCardHandler ();
  }
}