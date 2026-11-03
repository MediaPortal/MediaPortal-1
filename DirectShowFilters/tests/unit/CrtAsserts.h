// The Debug CRT shows a dialog for _ASSERTE / assert(). TestSupport.cpp turns it into a counter, so a test run is never stopped by
// an assertion of the code under test and the tests can look at how many there were.
#pragma once

int CrtAssertsSeen();
void ResetCrtAsserts();
