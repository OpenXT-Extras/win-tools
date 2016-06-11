-------------------------------------------------------------------------------
win-tools.git
-------------------------------------------------------------------------------

This repository is not obsolete and no longer used in OpenXT. It contained a
number of tools related to features that are no longer required or desired in
OpenXT. The removal of these tools also removed the OpenXT dependency on .NET
which greatly simplifies things.

The following is a brief lising in case anyone ever wants to resurrect any of
this work.

XenGuestPlugin:

This is what was called the switcher bar. It was a small dropdown window that
allowed switching back to the UIVM. Long ago it did other things but
unltimately it got reduced to simply doing this which is not terribly useful
for all the .NET bits it brings in.

XenGuestAgent:

This is a COM service that gives other processes like the XenGuestPlugin
access to needed services like XenStore and V4V. This service is superseded
by a much simpler version called OxtService that cuts out all the old and
unused functionality.

XenClientDisplayResolutionAgent:

This was a small .NET Windows application that is used to write the current
guest resolution into XenStore. It is superseded by a new agent called
the OxtUserAgent that performs the same duties without pulling in .NET. This
new service uses OxtService mentioned above.

XenClientGuestService/dbus:

This service and the dbus Windows bindings supported a feature for dbus routing
to Windows guest that was never a feature of XenClient XT but rather its peer
project XenClient. It is not used in OpenXT either and is not gone.

xcdiag:

This project creates a utility for gathering diagnostics from within a guest.
It had been broken prior to the open sourcing of OpenXT. The actual open
sourcing effort further battered it because it relied on numerous proprietary
bits and pieces that could not be open sourced due to license issues. It might
be worth creating something like this in the future, possibly based on what is
left of it here.

-------------------------------------------------------------------------------
Old contents, out of date
-------------------------------------------------------------------------------

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

