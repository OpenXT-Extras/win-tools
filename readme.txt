Building
--------

XenGuestPlugin
It is possible to work on XenGuestPlugin for GUI purposes in the debugger, without building the remaining code (i.e. no dependencies required).
Either define an environment variable "CITRIX_GUI_DEBUGGING" to a non-zero value,
or create a file named "IAmACitrixGUIDeveloperAndILikeToTest.txt" in the current working directory when running the XenGuestPlugin in the Visual Studio debugger.

XenGuestAgent
To build XenGuestAgent, headers in xc-windows.git (git://git.xci-test.com/xenclient/xc-windows.git) needs to be available.
The easiest way to achieve this is to "git clone git://git.xci-test.com/xenclient/xc-windows.git" into the same directory as that containing win-tools.

Troubleshooting
---------------
XenGuestAgent
Q. Failing to find header files.
A. Try rebasing local copy of xc-windows.

