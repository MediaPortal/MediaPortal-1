// Runs a piece of code that may call assert() / abort(): in a Debug build a failed assert() writes a message and ends the process,
// which would end the whole test run. The guard turns the abort into a return value.
#pragma once

#include <csetjmp>
#include <fcntl.h>
#include <io.h>
#include <csignal>
#include <cstdlib>

namespace abort_guard
{
  inline std::jmp_buf& Jump() { static std::jmp_buf jump; return jump; }
  inline void OnAbort(int) { std::longjmp(Jump(), 1); }

  // true when the code aborted. The code must not own objects with destructors that matter (a longjmp does not run them).
  template <class F>
  bool Aborts(F code)
  {
    _set_abort_behavior(0, _WRITE_ABORT_MSG | _CALL_REPORTFAULT);
    void (*previous)(int) = std::signal(SIGABRT, OnAbort);
    bool aborted = false;
    // the message of a failed assert() goes to stderr: keep it out of the test output
    const int savedStderr = _dup(2);
    const int quiet = _open("NUL", _O_WRONLY);
    if (quiet >= 0)
      _dup2(quiet, 2);
    if (setjmp(Jump()) == 0)
      code();
    else
      aborted = true;
    if (quiet >= 0)
    {
      _dup2(savedStderr, 2);
      _close(quiet);
    }
    _close(savedStderr);
    std::signal(SIGABRT, previous);
    return aborted;
  }
}
