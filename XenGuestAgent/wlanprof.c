/*
 * Copyright (c) 2010 Citrix Systems, Inc.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#pragma warning(disable : 4201)

#include <stdio.h>
#include <windows.h>
#include "wlanapi.h"
#include <strsafe.h>
#include "resource.h"

#define SIZECHARS(x) (sizeof(x)/sizeof(TCHAR))

typedef
DWORD
(*PWLANOPENHANDLE)(
  DWORD dwClientVersion,
  PVOID pReserved,
  PDWORD pdwNegotiatedVersion,
  PHANDLE phClientHandle
  );

typedef
DWORD
(*PWLANCLOSEHANDLE)(
    HANDLE hClientHandle,
    PVOID pReserved
);

typedef
DWORD
(*PWLANENUMINTERFACES)(
    HANDLE hClientHandle,
    PVOID pReserved,
    PWLAN_INTERFACE_INFO_LIST *ppInterfaceList
);

typedef 
DWORD
(*PWLANSETPROFILE)(
    HANDLE hClientHandle,
    CONST GUID *pInterfaceGuid,
    DWORD dwFlags,
    LPCWSTR strProfileXml,
    LPCWSTR strAllUserProfileSecurity,
    BOOL bOverwrite,
    PVOID pReserved,
    DWORD *pdwReasonCode
);

typedef
DWORD
(*PWLANREASONCODETOSTRING)(
    DWORD dwReasonCode,
    DWORD dwBufferSize,
    PWCHAR pStringBuffer,
    PVOID pReserved
);


PWLANOPENHANDLE pWlanOpenHandle = NULL;
PWLANCLOSEHANDLE pWlanCloseHandle = NULL;
PWLANENUMINTERFACES pWlanEnumInterfaces = NULL;
PWLANSETPROFILE pWlanSetProfile = NULL;
PWLANREASONCODETOSTRING pWlanReasonCodeToString = NULL;

HANDLE hClientHandle = NULL;
PWLAN_INTERFACE_INFO_LIST ppInterfaceList = NULL;


#if 0
char *Guid2String(GUID Guid)
{
	char *sGuid;
	static char csGuid[39];

    sprintf_s (csGuid, sizeof(csGuid), "{%X-%X-%X-%02X%02X-%02X%02X%02X%02X%02X%02X}",
        Guid.Data1, Guid.Data2, Guid.Data3, Guid.Data4[0], Guid.Data4[1], Guid.Data4[2],
        Guid.Data4[3], Guid.Data4[4], Guid.Data4[5], Guid.Data4[6], Guid.Data4[7]);
	sGuid = csGuid;

    return sGuid;
}
#endif

BOOL TestForLaterThanXP()
{
    OSVERSIONINFO osvi;

    ZeroMemory(&osvi, sizeof(OSVERSIONINFO));
    osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);

    GetVersionEx(&osvi);

    if ((osvi.dwMajorVersion > 5) ||
       ((osvi.dwMajorVersion == 5) && (osvi.dwMinorVersion >= 1)))
    {
        return TRUE;
    }

    return FALSE;
}

int __stdcall wlanprof(DWORD *WlanStatus)
{
    HWND hWnd = NULL;
    WCHAR wsProfileXml[1024];
    HINSTANCE hInstance;
    DWORD dwNicCnt;
    DWORD dwServiceVersion;
    HMODULE hModule = NULL;
    TCHAR NewDevPath[MAX_PATH];
    int Found;
    int RetryCount = 0;
    BOOL LaterThanXP;
	int status = 0;

	*WlanStatus = 0;
    LaterThanXP = TestForLaterThanXP();

    //
    // Load the entry points into the WLANAPI DLL to access the WLAN stuff...
    //
    GetSystemDirectory(NewDevPath, SIZECHARS(NewDevPath));
    StringCchCat (NewDevPath, SIZECHARS(NewDevPath), TEXT("\\WLANAPI.DLL"));
    hModule = LoadLibrary(NewDevPath);
    if (!hModule)
    {
//@@@		MessageBox(hWnd, "WLANAPI.DLL not found", "Error", MB_OK);
		return -1;
	}
    //
    // Found the DLL...start fetching the entry points.
    //
    pWlanOpenHandle = (PWLANOPENHANDLE)GetProcAddress(hModule, "WlanOpenHandle");
    pWlanCloseHandle = (PWLANCLOSEHANDLE)GetProcAddress(hModule, "WlanCloseHandle");
    pWlanEnumInterfaces = (PWLANENUMINTERFACES)GetProcAddress(hModule, "WlanEnumInterfaces");
    pWlanSetProfile = (PWLANSETPROFILE)GetProcAddress(hModule, "WlanSetProfile");
    pWlanReasonCodeToString = (PWLANREASONCODETOSTRING)GetProcAddress(hModule, "WlanReasonCodeToString");
    if (!pWlanOpenHandle || !pWlanCloseHandle || !pWlanEnumInterfaces || !pWlanSetProfile || !pWlanReasonCodeToString)
    {
//@@@		MessageBox(hWnd, "Error loading Wifi API procedure(s)", "Error", MB_OK);
		return -2;
	}


    //
    // Load the XML profile data from the embedded
    // string resource. For XP, there is one profile...
    // anything later uses the alternate profile.
    //
    hInstance = GetModuleHandle(NULL);
    LoadStringW (hInstance, IDS_XENWIRELESS, wsProfileXml, 1024);

    Found = 0;

    do
    {
        if (pWlanOpenHandle (1, NULL, &dwServiceVersion, &hClientHandle) != ERROR_SUCCESS)
        {
            // Error opening Wlan Handle
            //
            // Print no error since we are being launched at user login and the WLAN
            // service may not be running yet. So just keep retrying...
            //
            Sleep (5000);
            continue;
        }

        //
        // Fetch the current list of WLAN interfaces
        //
        if (*WlanStatus = 
			pWlanEnumInterfaces (hClientHandle, NULL, &ppInterfaceList) != ERROR_SUCCESS)
        {
            // Error Getting Interfaces
//@@@            MessageBox(hWnd, "Error getting list of device interfaces", "Error", MB_OK);
            status = -3;
			goto done;
        }

        //
        // Check number of Citrix PV wireless adapters found so far
        //
        for (dwNicCnt = 0; dwNicCnt < ppInterfaceList->dwNumberOfItems; dwNicCnt++)
        {
            if (wcsstr(ppInterfaceList->InterfaceInfo[dwNicCnt].strInterfaceDescription, L"Citrix PV"))
                Found++;
        }

        //
        // If none found, sleep for 5s and try again
        //
        if (!Found)
            Sleep (5000);

    } while (!Found);

    MessageBeep ((UINT)-1);

#if 0
    if (ListGuids)
    {
        char entry[512];
	    for (dwNicCnt = 0; dwNicCnt < ppInterfaceList->dwNumberOfItems; dwNicCnt++)
        {
            sprintf_s (entry, sizeof(entry), "%s / %S",
                Guid2String(ppInterfaceList->InterfaceInfo[dwNicCnt].InterfaceGuid),
                ppInterfaceList->InterfaceInfo[dwNicCnt].strInterfaceDescription); 
		    MessageBox(hWnd, entry, "Info", MB_OK);
        }
        return 0;
    }
#endif

    //
    // At this point, we have found at least one Citrix branded wireless
    // adapter. So we can go ahead and set the profile.
    //
	for (dwNicCnt=0; dwNicCnt < ppInterfaceList->dwNumberOfItems; dwNicCnt++)
    {
		DWORD pdwReasonCode = 0;
		DWORD retWlanSetProfile;

		*WlanStatus = 
        retWlanSetProfile = 
            pWlanSetProfile (
                hClientHandle,
                &ppInterfaceList->InterfaceInfo[dwNicCnt].InterfaceGuid,
                0, wsProfileXml,
                NULL, TRUE, NULL,
                &pdwReasonCode);
		if (retWlanSetProfile != ERROR_SUCCESS)
        {
#if 0
            WCHAR swcReason[256];
            char err[256];
			pWlanReasonCodeToString (pdwReasonCode, 256, swcReason, NULL);
            sprintf_s (err, sizeof(err), "[%d] %S", pdwReasonCode, swcReason);
            MessageBox(hWnd, err, "Error adding AP", MB_OK);
#endif
            Sleep (1000);
            dwNicCnt -= 1;
            RetryCount++;
		}
        if (RetryCount > 50)
            break;
	}

    if (RetryCount >= 50)
		status = -4;

done:		// sorry Mattba...

    //FreeLibrary (hModule);

    pWlanCloseHandle(hClientHandle, NULL);

    //
    // We have completed our task, so it is time to remove the
    // registry entry that kicks off this util.
    //
    // HKLM\SOFTWARE\Citrix\XenClient\WlanProf
    //

    RegDeleteKey (HKEY_LOCAL_MACHINE, TEXT("SOFTWARE\\Citrix\\XenClient\\WlanProf"));

	return status;
}
