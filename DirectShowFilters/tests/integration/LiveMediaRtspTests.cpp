// LiveMedia555 (wrappers around live555): the RTSP server of the stream server and the RTSP client of the IPTV source,
// talking to each other over the loopback interface in one process. No filter is involved.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <chrono>
#include <string>
#include "BasicUsageEnvironment.hh"
#include "MPRTSPClient.h"
#include "MPRTSPServer.h"
#include "MPTaskScheduler.h"

// MPRTSPServer logs through the host program
void LogDebug(const wchar_t*, ...) {}

namespace
{
  // A port that is free now (bind to port 0, read it, close)
  unsigned short FreePort()
  {
    WSADATA wsa;
    WSAStartup(MAKEWORD(2, 2), &wsa);
    SOCKET s = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in a = {};
    a.sin_family = AF_INET;
    a.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
    ::bind(s, reinterpret_cast<sockaddr*>(&a), sizeof(a));   // ::, MPRTSPServer.h has "using namespace std"
    int len = sizeof(a);
    ::getsockname(s, reinterpret_cast<sockaddr*>(&a), &len);
    ::closesocket(s);
    return ntohs(a.sin_port);
  }

  struct Answer
  {
    bool received = false;
    int code = 0;
    std::string text;
  };

  void OnAnswer(RTSPClient* client, int code, char* text)
  {
    Answer* answer = static_cast<Answer*>(static_cast<MPRTSPClient*>(client)->Context());
    answer->received = true;
    answer->code = code;
    if (text)
      answer->text = text;
    delete[] text;
  }

  class LiveMediaRtsp : public testing::Test
  {
  protected:
    MPTaskScheduler* scheduler = nullptr;
    UsageEnvironment* env = nullptr;
    unsigned short port = 0;

    void SetUp() override
    {
      scheduler = MPTaskScheduler::createNew();
      env = BasicUsageEnvironment::createNew(*scheduler);
      port = FreePort();
    }

    void TearDown() override
    {
      env->reclaim();
      delete scheduler;
    }

    // Runs the event loop until the answer is there (5 s at the most)
    bool Wait(const Answer& answer)
    {
      const auto end = std::chrono::steady_clock::now() + std::chrono::seconds(5);
      while (!answer.received && std::chrono::steady_clock::now() < end)
        static_cast<TaskScheduler*>(scheduler)->doEventLoop();   // one step, waits up to 1 s for an event
      return answer.received;
    }

    std::string Url(const char* stream) const { return "rtsp://127.0.0.1:" + std::to_string(port) + "/" + stream; }
  };
}

TEST_F(LiveMediaRtsp, ServerStartsWithoutSessions)
{
  MPRTSPServer* server = MPRTSPServer::createNew(*env, Port(port));
  ASSERT_NE(server, nullptr);
  EXPECT_EQ(server->GetSessionCount(), 0);
  EXPECT_EQ(server->GetSessionByIndex(0), nullptr);
  EXPECT_FALSE(server->RemoveSessionById(12345));
  Medium::close(server);
}

TEST_F(LiveMediaRtsp, ServerAnswersOptions)
{
  MPRTSPServer* server = MPRTSPServer::createNew(*env, Port(port));
  ASSERT_NE(server, nullptr);

  Answer answer;
  const std::string url = Url("none");
  MPRTSPClient* client = MPRTSPClient::createNew(&answer, *env, url.c_str(), 0, "mp-test");
  ASSERT_NE(client, nullptr);
  EXPECT_EQ(client->Context(), &answer);
  client->sendOptionsCommand(OnAnswer);
  ASSERT_TRUE(Wait(answer));
  EXPECT_EQ(answer.code, 0) << answer.text;
  EXPECT_NE(answer.text.find("DESCRIBE"), std::string::npos) << answer.text;

  Medium::close(client);
  Medium::close(server);
}

TEST_F(LiveMediaRtsp, UnknownStreamIsNotFound)
{
  MPRTSPServer* server = MPRTSPServer::createNew(*env, Port(port));
  ASSERT_NE(server, nullptr);

  Answer answer;
  const std::string url = Url("no-such-stream");
  MPRTSPClient* client = MPRTSPClient::createNew(&answer, *env, url.c_str(), 0, "mp-test");
  ASSERT_NE(client, nullptr);
  client->sendDescribeCommand(OnAnswer);
  ASSERT_TRUE(Wait(answer));
  EXPECT_EQ(answer.code, 404) << answer.text;
  EXPECT_EQ(server->GetSessionCount(), 0) << "a refused request must not leave a session";

  Medium::close(client);
  Medium::close(server);
}

TEST_F(LiveMediaRtsp, ServerListensOnIPv4Only)
{
  // the TV Server binds the stream server to an IPv4 address; there must be no listener on ::1
  MPRTSPServer* server = MPRTSPServer::createNew(*env, Port(port));
  ASSERT_NE(server, nullptr);

  SOCKET v6 = socket(AF_INET6, SOCK_STREAM, 0);
  if (v6 == INVALID_SOCKET)
  {
    Medium::close(server);
    GTEST_SKIP() << "no IPv6 on this machine";
  }
  sockaddr_in6 a = {};
  a.sin6_family = AF_INET6;
  a.sin6_addr = in6addr_loopback;
  a.sin6_port = htons(port);
  const int result = connect(v6, reinterpret_cast<sockaddr*>(&a), sizeof(a));
  closesocket(v6);
  EXPECT_NE(result, 0);
  Medium::close(server);
}

TEST_F(LiveMediaRtsp, PortInUseGivesNoServer)
{
  MPRTSPServer* first = MPRTSPServer::createNew(*env, Port(port));
  ASSERT_NE(first, nullptr);
  MPRTSPServer* second = MPRTSPServer::createNew(*env, Port(port));
  EXPECT_EQ(second, nullptr);
  if (second)
    Medium::close(second);
  Medium::close(first);
}
