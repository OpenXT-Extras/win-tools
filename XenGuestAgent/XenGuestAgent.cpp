/*
 * Copyright (c) 2014 Citrix Systems, Inc.
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

// XenGuestAgent.cpp : Implementation of WinMain


#include "stdafx.h"
#include "resource.h"
#include "xgamsg.h"
#include "XenGuestAgent.h"
#include "XenGuestAgent_i.h"
#include "XenStoreWrapper.h"
#include "XenSecurityHelper.h"
#include "XenVmInfo.h"
#include "XenHttp.h"
#include "json\json.h"

#include <stdio.h>
#include <assert.h>
#include <string>
#include <sstream>
#include <iomanip>

#define XGA_INSTALL_REGKEY  _T("Software\\Citrix\\XenGuestPlugin")
#define XC_INSTALL_REGKEY  _T("Software\\Citrix\\XenTools")
#define XGA_INSTALL_INSTVAL _T("Install_Dir")
#define XGA_SERVICE_INFO_SIZE 256
#define XGA_RUN_REGKEY		_T("Software\\Microsoft\\Windows\\CurrentVersion\\Run")
#define XGA_RUN_INSTVAL		_T("XciPlugin")
#define XGA_RUN_BLANK		_T("")

extern "C" {
	int poweropts();
	int __stdcall wlanprof(DWORD *WlanStatus);
}

#ifdef _DEBUG
#define _CAN_CONSOLE
#endif // _DEBUG

class CXenGuestAgentModule : public CAtlServiceModuleT< CXenGuestAgentModule, IDS_SERVICENAME >
{
public :
	HINSTANCE m_hInstance;
	TCHAR m_tszDisplayName[XGA_SERVICE_INFO_SIZE];
	TCHAR m_tszServiceDesc[XGA_SERVICE_INFO_SIZE];
	TCHAR m_tszParamsKey[XGA_SERVICE_INFO_SIZE];

	DECLARE_LIBID(LIBID_XenGuestAgentLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_XENGUESTAGENT, APPID_XenGuestAgent)
	
	HRESULT InitializeSecurity() throw()
	{
		CXenSecurityHelper clXsh(&_XenGuestAgent);

		// If not a consolable build, check not running remotely
#		ifndef _CAN_CONSOLE
		// If the remote access check fails, fail to start the service.
		if (!clXsh.CheckDenyRemoteAccess())
			return E_ACCESSDENIED;
#		else //!_CAN_CONSOLE
		//CoInitializeSecurity(NULL // pSecDesc
		//	,0 // cAuthSvc
		//	,NULL // asAuthSvc
		//	,NULL // pReserved1
		//	,0 // dwAuthnLevel
		//	,0 // dwImplLevel
		//	,NULL // pAuthList
		//	,0 // dwCapabilities
		//	,NULL // pReserved3
		//	);
#		endif //_CAN_CONSOLE

		// Calling CoInitializeSecurity at this point will override the values
		// setup for the AppID in the registry. These values explicitly set the
		// security access control to deny all remote access and to allow only
		// the interactive user access and launch permissions (aside from the
		// administrator). The default authentication values are fine for this.

		// NOTE: One oddity was that a NULL DACL was being passed to CoInitializeSecurity
		// earlier but the values in the AppID were still being used. Perhaps 
		// there is more to the overriding of AppID registry values than simply
		// ignoring them in some cases.

		return S_OK;
	}

	void LoadStrings(HINSTANCE hInstance)
	{
		m_hInstance = hInstance;
		
		::LoadString(m_hInstance, IDS_DISPLAYNAME, m_tszDisplayName, sizeof(m_tszDisplayName)/sizeof(TCHAR));
		::LoadString(m_hInstance, IDS_SERVICEDESC, m_tszServiceDesc, sizeof(m_tszServiceDesc)/sizeof(TCHAR));

		_sntprintf_s(m_tszParamsKey,
					 XGA_SERVICE_INFO_SIZE,
					 _TRUNCATE,
					 _T("SYSTEM\\CurrentControlSet\\Services\\%s\\Parameters"),
					 m_szServiceName);
	}

	void SetupParameterKey()
	{
		LONG lRet;
		CRegKey keyParams;

		lRet = keyParams.Open(HKEY_LOCAL_MACHINE, m_tszParamsKey);
		if (lRet != ERROR_SUCCESS)
		{
			// Attempt to create the key
			lRet = keyParams.Create(HKEY_LOCAL_MACHINE, m_tszParamsKey);
			if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_SETUPPARAMETERKEY_COULD_NOT_CREA_XENGUESTAGENT_106)
				,EVENTLOG_ERROR_TYPE
				,EVMSG_START_FAILURE
				,lRet))
			{
				return;
			}
		}
		lRet = keyParams.SetDWORDValue(_T("LogCommunicationErrors"), 0);
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_SETUPPARAMETERKEY_COULD_NOT_SET__XENGUESTAGENT_115)
				,EVENTLOG_ERROR_TYPE
				,EVMSG_START_FAILURE
				,lRet))
		{
			return;
		}
		lRet = keyParams.SetDWORDValue(_T("LogOperationErrors"), 0);
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_SETUPPARAMETERKEY_COULD_NOT_SET__XENGUESTAGENT_123)
				,EVENTLOG_ERROR_TYPE
				,EVMSG_START_FAILURE
				,lRet))
		{
			return;
		}
	}

	void RegisterEventSource()
	{
		LONG lRet;
		CRegKey keyAppLog;
		CRegKey keyNewApp;
		TCHAR tszImageName[_MAX_PATH + 1];
		
		::ZeroMemory(tszImageName, sizeof(TCHAR)*(_MAX_PATH + 1));
		::GetModuleFileName(NULL, tszImageName, _MAX_PATH);

		// Open the app log key
		lRet = keyAppLog.Open(HKEY_LOCAL_MACHINE, 
							  _T("SYSTEM\\CurrentControlSet\\Services\\EventLog\\Application"));
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_REGISTEREVENTSOURCE_COULD_NOT_OP_XENGUESTAGENT_145)
			,EVENTLOG_ERROR_TYPE
			,EVMSG_START_FAILURE
			,lRet))
		{
			return;
		}

		// Create a new key for event logging for this service
		lRet = keyNewApp.Create(keyAppLog, m_szServiceName);
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_REGISTEREVENTSOURCE_COULD_NOT_CR_XENGUESTAGENT_155)
				,EVENTLOG_ERROR_TYPE
				,EVMSG_START_FAILURE
				,lRet))
		{
			return;
		}
		
		// Set the value of the message code base
		lRet = keyNewApp.SetStringValue(_T("EventMessageFile"), tszImageName);
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_REGISTEREVENTSOURCE_COULD_NOT_SE_XENGUESTAGENT_165)
				,EVENTLOG_ERROR_TYPE
				,EVMSG_START_FAILURE
				,lRet))
		{
			return;
		}

		// Set the event types allowed
		DWORD dwData = EVENTLOG_ERROR_TYPE | EVENTLOG_WARNING_TYPE | EVENTLOG_INFORMATION_TYPE;
		lRet = keyNewApp.SetDWORDValue(_T("TypesSupported"), dwData);
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_REGISTEREVENTSOURCE_COULD_NOT_SE_XENGUESTAGENT_176)
				,EVENTLOG_ERROR_TYPE
				,EVMSG_START_FAILURE
				,lRet))
		{
			return;
		}
	}

	void UnregisterEventSource()
	{
		LONG lRet;
		CRegKey keyAppLog;

		// Open the app log key
		lRet = keyAppLog.Open(HKEY_LOCAL_MACHINE, 
							  _T("SYSTEM\\CurrentControlSet\\Services\\EventLog\\Application"));
		if (_XenGuestAgent.LogEventTypeIdLastRegistryError(ctxLS(IDS_UNREGISTEREVENTSOURCE_COULD_NOT__XENGUESTAGENT_193)
			,EVENTLOG_ERROR_TYPE
			,EVMSG_START_FAILURE
			,lRet))
		{
			return;
		}

		// Delete this service's logging key
		keyAppLog.DeleteSubKey(m_szServiceName);	
	}

	void DenyRemoteAccess()
	{
		CXenSecurityHelper clXsh(&_XenGuestAgent);
		clXsh.DenyRemoteAccess();
		clXsh.DenyRemoteLaunchAndActivate();
	}

	void CheckRemoteAccess()
	{
		CXenSecurityHelper clXsh(&_XenGuestAgent);
		
		if (clXsh.CheckDenyRemoteAccess())
			::MessageBox(NULL, _T("CheckRemoteAccess: Remote access is denied."), m_szServiceName, MB_OK|MB_ICONINFORMATION);
		else
			::MessageBox(NULL, _T("CheckRemoteAccess: WARNING remote access is not denied!"), m_szServiceName, MB_OK|MB_ICONEXCLAMATION);
	}

	void UpdateServiceRegistry()
	{
		SC_HANDLE hSCM = NULL;
		SC_HANDLE hService = NULL;
		BOOL rc;
		SERVICE_DESCRIPTION stDesc;

		do {
			hSCM = ::OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
			if (hSCM == NULL)
			{
				_XenGuestAgent.LogEventTypeId(ctxLS(IDS_UPDATESERVICEREGISTRY_COULD_NOT__XENGUESTAGENT_233),
											  EVENTLOG_ERROR_TYPE,
											  EVMSG_START_FAILURE,
											  ::GetLastError());		
				break;
			}

			hService = ::OpenService(hSCM, m_szServiceName, SERVICE_CHANGE_CONFIG);
			if (hSCM == NULL)
			{
				::CloseServiceHandle(hSCM);
				_XenGuestAgent.LogEventTypeId(ctxLS(IDS_UPDATESERVICEREGISTRY_COULD_NOT__XENGUESTAGENT_244),
											  EVENTLOG_ERROR_TYPE,
											  EVMSG_START_FAILURE,
											  ::GetLastError());
				break;
			}

			rc = ::ChangeServiceConfig(hService,
									   SERVICE_NO_CHANGE,
									   SERVICE_AUTO_START,
									   SERVICE_NO_CHANGE,
									   NULL,
									   NULL,
									   NULL,
									   NULL,
									   NULL,
									   NULL,
									   m_tszDisplayName);
			if (!rc)
			{
				_XenGuestAgent.LogEventTypeId(ctxLS(IDS_UPDATESERVICEREGISTRY_CHANGE_SER_XENGUESTAGENT_264),
											  EVENTLOG_ERROR_TYPE,
											  EVMSG_START_FAILURE,
											  ::GetLastError());		
				break;
			}

			stDesc.lpDescription = m_tszServiceDesc;
			rc = ::ChangeServiceConfig2(hService, SERVICE_CONFIG_DESCRIPTION, (LPVOID)&stDesc);
			if (!rc)
			{
				_XenGuestAgent.LogEventTypeId(ctxLS(IDS_UPDATESERVICEREGISTRY_CHANGE_SER_XENGUESTAGENT_275),
											  EVENTLOG_ERROR_TYPE,
											  EVMSG_START_FAILURE,
											  ::GetLastError());
			}
		} while (false);

		if (hService != NULL)
			::CloseServiceHandle(hService);

		if (hSCM != NULL)
			::CloseServiceHandle(hSCM);		
	}

	bool PreStartTasks()
	{
		LPTSTR lpCmdLine = GetCommandLine();
		TCHAR tszTokens[] = _T("-/");
		bool rc = false;

		LPCTSTR lpszToken = FindOneOf(lpCmdLine, tszTokens);
		while (lpszToken != NULL)
		{
			if (WordCmpI(lpszToken, _T("RegServer")) == 0)
			{
				RegisterEventSource();
				break;
			}

			if (WordCmpI(lpszToken, _T("Service")) == 0)
			{
				RegisterEventSource();
				SetupParameterKey();
				break;
			}

			if (WordCmpI(lpszToken, _T("UnregServer")) == 0)
			{
				UnregisterEventSource();
				break;
			}

			if (WordCmpI(lpszToken, _T("DenyRemoteAccess")) == 0)
			{
				DenyRemoteAccess();
				rc = true;
				break;
			}

			if (WordCmpI(lpszToken, _T("CheckRemoteAccess")) == 0)
			{
				CheckRemoteAccess();
				rc = true;
				break;
			}

			lpszToken = FindOneOf(lpszToken, tszTokens);
		}

		return rc;
	}

	void PostStartTasks()
	{
		LPTSTR lpCmdLine = GetCommandLine();
		TCHAR tszTokens[] = _T("-/");

		LPCTSTR lpszToken = FindOneOf(lpCmdLine, tszTokens);
		while (lpszToken != NULL)
		{
			if (WordCmpI(lpszToken, _T("Service"))==0)
			{
				UpdateServiceRegistry();
				break;
			}

			lpszToken = FindOneOf(lpszToken, tszTokens);
		}
	}

	//! @brief Try to run the program as a console executable.
	//! Useful for debugging.
	//! @param dwArgc	Number of command line params.
	//! @param lpszArgv	Command line params.
	void ConsoleMain(DWORD dwArgc, LPTSTR* lpszArgv) throw()
	{
		lpszArgv;
		dwArgc;
		m_status.dwWin32ExitCode = S_OK;
		m_status.dwCheckPoint = 0;
		m_status.dwWaitHint = 0;

#ifndef _ATL_NO_COM_SUPPORT

		HRESULT hr = E_FAIL;
		hr = CXenGuestAgentModule::InitializeCom();
		if (FAILED(hr))
		{
			// Ignore RPC_E_CHANGED_MODE if CLR is loaded. Error is due to CLR initializing
			// COM and InitializeCOM trying to initialize COM with different flags.
			if (hr != RPC_E_CHANGED_MODE || GetModuleHandle(_T("Mscoree.dll")) == NULL)
			{
				return;
			}
		}
		else
		{
			m_bComInitialized = true;
		}

		m_bDelayShutdown = false;
#endif //_ATL_NO_COM_SUPPORT
		// When the Run function returns, the service has stopped.
		m_status.dwWin32ExitCode = this->Run(SW_HIDE);
		
		// Ok, I give up. How do we remote debug with the correct credentials ?
#		ifdef _CAN_CONSOLE
		if (m_status.dwWin32ExitCode == CO_E_WRONG_SERVER_IDENTITY)
		{
			this->RunMessageLoop();
			this->PostMessageLoop();
		}
#		endif // _CAN_CONSOLE

#ifndef _ATL_NO_COM_SUPPORT
		if (m_bService && m_bComInitialized)
			CXenGuestAgentModule::UninitializeCom();
#endif

		SetServiceStatus(SERVICE_STOPPED);
		LogEvent(_T("Service running as Console stopped"));
	}

	HRESULT Start(int nShowCmd) throw()
	{
		// Explicitly load the xs2.dll here for use in the service.
		if (!CXenStoreWrapper::XS2Initialize())
		{
			_XenGuestAgent.LogEventTypeId(ctxLS(IDS_FAILED_TO_LOAD_XS2_LIBRARY___ERR_XENGUESTAGENT_413),
										  EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
			return E_FAIL;
		}
		
		// We are overriding start with our own. Since we are always a service then
		// can ditch the registry checks. First, start the non-COM related tasks.
		if (!_XenGuestAgent.Start())
			return E_UNEXPECTED;

		// Now start the COM service
		m_bService = TRUE;

		SERVICE_TABLE_ENTRY st[] =
		{
			{ m_szServiceName, _ServiceMain },
			{ NULL, NULL }
		};
		if (::StartServiceCtrlDispatcher(st) == 0)
		{
			DWORD const dwLastError = GetLastError();
			if (dwLastError == ERROR_FAILED_SERVICE_CONTROLLER_CONNECT) // If failed to connect
			{
#				ifndef _CAN_CONSOLE
				m_status.dwWin32ExitCode = dwLastError;
#				endif // _CAN_CONSOLE
				_XenGuestAgent.LogEventTypeIdLastError(ctxLS(IDS_FAILED_TO_START__SERVICE_SHOULD__XENGUESTAGENT_439)
					,EVENTLOG_ERROR_TYPE
					,EVMSG_START_FAILURE
				);
				// Allow debug build to attempt to run as console program.
				// Currently doesn't always work that well since there's a problem with
				// "CoRegisterClassObject()" failing with CO_E_WRONG_SERVER_IDENTITY causing an assert.
#				ifdef _CAN_CONSOLE
#					ifdef UNICODE
					LPTSTR *lpCmdLine = __wargv;
#					else //!UNICODE
					LPTSTR *lpCmdLine = __argv;
#					endif //!UNICODE
				ConsoleMain(__argc, lpCmdLine);
#				endif _CAN_CONSOLE

			} // Ends if failed to connect
			else // Else unknown error
			{
				m_status.dwWin32ExitCode = dwLastError;
				_XenGuestAgent.LogEventTypeIdLastError(ctxLS(IDS_FAILED_TO_START_SERVICE_XENGUESTAGENT_459)
					,EVENTLOG_ERROR_TYPE
					,EVMSG_START_FAILURE
				);
			} // Ends else unknown error
		}

		return m_status.dwWin32ExitCode;
	}

	void OnStop() throw()
	{
		// Override the OnStop to get control of our shutdown event first
		_XenGuestAgent.SetShutdownEvent();
		// Call up to parent class to shut the service down
		CAtlServiceModuleT<CXenGuestAgentModule, IDS_SERVICENAME>::OnStop();
	}
};

CXenGuestAgentModule _AtlModule;

namespace JsonExtras
{
//! @brief Write a Json::Value to a string.
//! This really shouldn't be that hard but the Json library adds extra unnecessary fluff.
//! There doesn't seem to be a way to grab the equivalent token from a value.
//! Asking for a numeric type as a string causes an access violation due to direct pointer conversion.
//! Using a "FastWriter" to write the value almost does a straight evaluation, but then adds a newline for good measure.
//! "FastWriter" makes its implementation private, so the only way to re-use it is via the public interface,
//! with any sorting out done externally, which is what happens here.
//! @param rjValue[in]	Value to convert.
//! @param rstrOut[out]	Value as string.
//! @return	Converted string.
std::string & valueToString(Json::Value const &rjValue, std::string &rstrOut)
{
	if (rjValue.isString()) // If it's already a string
	{
		rstrOut = rjValue.asCString();

	} // Ends if it's already a string
	else // Else not just a string
	{
		Json::FastWriter jWriter;

		jWriter.write(rjValue).swap(rstrOut);

		if (!rstrOut.empty() && *(--rstrOut.end()) == '\n') // If got something ending in a newline
		{
			// Lose the newline.
			rstrOut.resize(rstrOut.length() - 1);

		} // Ends if got something ending in a newline
	} // Ends else not just a string

	return rstrOut;
}

std::string valueToString(Json::Value const &rjValue)
{
	std::string strValue;
	valueToString(rjValue, strValue);
	return strValue;
}
} // Ends namespace JsonExtras

bool CXenGuestAgent::Initialize()
{
#define XGA_MAX_MESSAGE 512
	LONG lRet;
	DWORD dwVal;
	CRegKey clInstKey;
	CRegKey keyParams;
	TCHAR tszInstPath[_MAX_PATH + 1];
	DWORD dwLen;

	m_osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
	if (!::GetVersionEx((OSVERSIONINFO*)&m_osvi))
	{		
		LogEventTypeId(ctxLS(IDS_GETVERSIONEX_FAILED_____ERROR__z_XENGUESTAGENT_537), // SNO!
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return false;
	}

	m_hShutdownEvent = ::CreateEvent(NULL, TRUE, FALSE, NULL);
	if (m_hShutdownEvent == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_CREATE_SHUTDOWN_EVENT__XENGUESTAGENT_545),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return false;
	}

    m_hVmsEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL);
	if (m_hVmsEvent == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_CREATE_VMS_EVENT___ERR_XENGUESTAGENT_553),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return false;
	}

	m_hVmsLock = ::CreateMutex(NULL, FALSE, NULL);
	if (m_hVmsLock == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_CREATE_VMS_VECTOR_LOCK_XENGUESTAGENT_561),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return false;
	}

	m_hAlertsLock = ::CreateMutex(NULL, FALSE, NULL);
	if (m_hAlertsLock == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_CREATE_ALERTS_VECTOR_L_XENGUESTAGENT_569),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return false;
	}

	m_bstrInstallDir = L"C:\\Program Files\\Citrix\\XenGuestPlugin\\";
	lRet = clInstKey.Open(HKEY_LOCAL_MACHINE, XGA_INSTALL_REGKEY, KEY_READ);
	if (lRet == ERROR_SUCCESS)
	{
		dwLen = _MAX_PATH;
		lRet = clInstKey.QueryStringValue(XGA_INSTALL_INSTVAL, tszInstPath, &dwLen);
		if ((lRet == ERROR_SUCCESS)&&(dwLen > 0))
		{
			tszInstPath[dwLen] = _T('\0');
			m_bstrInstallDir = CComBSTR(CT2OLE(tszInstPath));
		}
	}

	_tcsncpy_s (m_bstrXcInstallDir, MAX_PATH, _T("C:\\Program Files\\Citrix\\XenTools"), _TRUNCATE);
	lRet = clInstKey.Open(HKEY_LOCAL_MACHINE, XC_INSTALL_REGKEY, KEY_READ);
	if (lRet == ERROR_SUCCESS)
	{
		dwLen = _MAX_PATH;
		lRet = clInstKey.QueryStringValue(XGA_INSTALL_INSTVAL, tszInstPath, &dwLen);
		if ((lRet == ERROR_SUCCESS)&&(dwLen > 0))
		{
			tszInstPath[dwLen] = _T('\0');
			_tcsncpy_s (m_bstrXcInstallDir, MAX_PATH, tszInstPath, _TRUNCATE);
		}
	}

	// Try reading the logging flags if they are there.
	lRet = keyParams.Open(HKEY_LOCAL_MACHINE, _AtlModule.m_tszParamsKey);
	if (lRet == ERROR_SUCCESS)
	{
		lRet = keyParams.QueryDWORDValue(_T("LogCommunicationErrors"), dwVal);
		if (lRet == ERROR_SUCCESS)
			m_bLogCommunicationErrors = (dwVal == 0) ? false : true;

		lRet = keyParams.QueryDWORDValue(_T("LogOperationErrors"), dwVal);
		if (lRet == ERROR_SUCCESS)
			m_bLogOperationErrors = (dwVal == 0) ? false : true;
	}

	return true;
}

void CXenGuestAgent::Uninitialize()
{
#define XGA_THREAD_WAIT 2000
	DWORD dwStatus;

	if (m_hAlertsThread != NULL)
	{
		dwStatus = ::WaitForSingleObject(m_hAlertsThread, XGA_THREAD_WAIT);
		if (dwStatus == WAIT_TIMEOUT)
			::TerminateThread(m_hAlertsThread, (ULONG)-2);

		::CloseHandle(m_hAlertsThread);
		m_hAlertsThread = NULL;
		m_uiAlertsThreadId = 0;
	}

	if (m_hVmsThread != NULL)
	{
		dwStatus = ::WaitForSingleObject(m_hVmsThread, XGA_THREAD_WAIT);
		if (dwStatus == WAIT_TIMEOUT)
			::TerminateThread(m_hVmsThread, (ULONG)-2);

		::CloseHandle(m_hVmsThread);
		m_hVmsThread = NULL;
		m_uiVmsThreadId = 0;
	}

	if (m_hTaskThread != NULL)
	{
		dwStatus = ::WaitForSingleObject(m_hTaskThread, XGA_THREAD_WAIT);
		if (dwStatus == WAIT_TIMEOUT)
			::TerminateThread(m_hTaskThread, (ULONG)-2);

		::CloseHandle(m_hTaskThread);
		m_hTaskThread = NULL;
		m_uiTaskThreadId = 0;
	}

	if (m_hAlertsLock != NULL)
	{
		::CloseHandle(m_hAlertsLock);
		m_hAlertsLock = NULL;
	}

	if (m_hVmsLock != NULL)
	{
		::CloseHandle(m_hVmsLock);
		m_hVmsLock = NULL;
	}

	if (m_hVmsEvent != NULL)
	{
		::CloseHandle(m_hVmsEvent);
		m_hVmsEvent = NULL;
	}

	if (m_hShutdownEvent != NULL)
	{
		::CloseHandle(m_hShutdownEvent);
		m_hShutdownEvent = NULL;
	}	
}

bool CXenGuestAgent::Start()
{
	//Find and set VM UUID
	CXenStoreWrapper clXs;
	DWORD dwCount = 0;
	LPSTR pszVal;
	char szPath[_MAX_PATH + 1];

	if (!clXs.XS2Open())
	{
		LogEventTypeId(ctxLS(IDS_XS2OPEN_FAILED_FOR_START__NOT_RU_XENGUESTAGENT_689),
					   EVENTLOG_WARNING_TYPE, EVMSG_WARNING, ::GetLastError());
		return true;
	}

	pszVal = (LPSTR)clXs.XS2Read("vm", NULL);
	if (pszVal == NULL)
	{
		LogEventTypeId(ctxLS(IDS_XS2READ_FAILED_FOR_VM___ERROR__z_XENGUESTAGENT_697),
					   EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
		return false;
	}

	// Form the absolute path with our UUID
	_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "%s/uuid", pszVal);
	clXs.XS2Free(pszVal);
	pszVal = (LPSTR)clXs.XS2Read(szPath, &dwCount);	
	if ((pszVal == NULL)||(dwCount < 36))
	{
		if (pszVal != NULL)
			clXs.XS2Free(pszVal);
		return false;
	}
	// This is our UUID
	m_bstrUuid = CComBSTR(CA2W(pszVal,CP_UTF8));
	//DebugBreak();
	//__asm int 3;
	//Sleep(10000);
	// Work out whether on not to show the switcher bar.
	// This is to support "native vm" experiences.
	//BOOL bDbusSaysShowSwitcherBar;

	//if (GetDbusShowSwitcherBarProperty(pszVal, &bDbusSaysShowSwitcherBar, EVMSG_START_FAILURE) != FALSE) // If got the setting ok
	//{
		// TODO - at the moment accept UIVM settings as gospel, making the user's selection pointless.
		// Need to support change in state of show-switcher from dbus without overriding user's selection.
		// E.g. bung previous show-switcher state in the registry.

		// Adding/removing autorun in the XenGuestAgent service doesn't seem to happen soon enough when turning off native vm and going back to multi-vm
		// (i.e. switcher bar doesn't appear unless you reboot again).
		// Additionally, on install it seems like the installer writes the autorun entry after the service has tried to fix it,
		// overwriting what we're trying to achieve here.
		// So although this is useful for sorting out whether the switcher bar should run in general, it's not useful in the
		// edge cases of changing the settings in the UIVM, or first install.
		// That has to be handled by some other mechanism.
		//ConfigurePluginAutorun(bDbusSaysShowSwitcherBar);

	//} // Ends if got the setting ok

	//Get our dom ID
	_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", pszVal, "domid");		
	clXs.XS2Free(pszVal);

	pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
	if (pszVal == NULL)
	{
		LogEventTypeId(ctxLS(IDS_XS2READ_FAILED_FOR_VM___ERROR__z_XENGUESTAGENT_720),
			EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
	}
	m_bstrDomId = CComBSTR(CA2W(pszVal,CP_UTF8));

	clXs.XS2Free(pszVal);

	// Start the XenStore notify thread
	m_hTaskThread = (HANDLE)_beginthreadex(NULL, 0, CXenGuestAgent::_TaskThread, this, 0, (UINT*)&m_uiTaskThreadId);
	if (m_hTaskThread == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_START_TASK_THREAD___ER_XENGUESTAGENT_730),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, Checked::get_errno());
		return false;
	}

	// Start the VMS notify thread
	m_hVmsThread = (HANDLE)_beginthreadex(NULL, 0, CXenGuestAgent::_VmsThread, this, 0, (UINT*)&m_uiVmsThreadId);
	if (m_hVmsThread == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_START_VMS_WATCH_THREAD_XENGUESTAGENT_739),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, Checked::get_errno());
		return false;
	}

	// Start the Alerts notify thread
	m_hAlertsThread = (HANDLE)_beginthreadex(NULL, 0, CXenGuestAgent::_AlertsThread, this, 0, (UINT*)&m_uiAlertsThreadId);
	if (m_hAlertsThread == NULL)
	{
		LogEventTypeId(ctxLS(IDS_FAILED_TO_START_ALERTS_WATCH_THR_XENGUESTAGENT_748),
					   EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, Checked::get_errno());
		return false;
	}

	return true;
}

BOOL IsPluginAutorunOn()
{
	CRegKey clRunKey;
	LONG lRet;
	TCHAR tszRunPath[_MAX_PATH + 1];
	DWORD dwLen;
	BOOL bPluginAutorun = TRUE;
	lRet = clRunKey.Open(HKEY_LOCAL_MACHINE, XGA_RUN_REGKEY, KEY_READ);

	if (lRet == ERROR_SUCCESS)
	{
		dwLen = _MAX_PATH;
		lRet = clRunKey.QueryStringValue(XGA_RUN_INSTVAL, tszRunPath, &dwLen);
		if ((lRet != ERROR_SUCCESS)||(dwLen <= 1)) //If entry in registry not populated
			bPluginAutorun = FALSE;
	}

	return bPluginAutorun;
}

BOOL CXenGuestAgent::ConfigurePluginAutorun(BOOL bEnable)
{
	CRegKey clRunKey;
	CRegKey keyParams;
	LONG lRet;
	TCHAR tszRunPath[_MAX_PATH + 1];
	DWORD dwLen;

	lRet = clRunKey.Open(HKEY_LOCAL_MACHINE, XGA_RUN_REGKEY, KEY_ALL_ACCESS);
	BOOL bSuccess = lRet == ERROR_SUCCESS;

	if (bSuccess)
	{
		bSuccess = FALSE;
		dwLen = _MAX_PATH;
		lRet = clRunKey.QueryStringValue(XGA_RUN_INSTVAL, tszRunPath, &dwLen);
		bool const bEntryPopulated = ((lRet == ERROR_SUCCESS)&&(dwLen > 1));

		if (!bEnable) // If turning off
		{
			if (bEntryPopulated) // If entry in registry populated
			{
				// Set to blank (turn off).
				lRet = clRunKey.SetStringValue(XGA_RUN_INSTVAL, XGA_RUN_BLANK);
				if (lRet == ERROR_SUCCESS) // If turned off ok
					bSuccess = TRUE;
				else // Else failed to turn off
				{
					this->LogEventTypeId(ctxLS(IDS_DELETING_XCIPLUGIN_RUN_VALUE_FAI_XENGUESTSERVICES_640),
									 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
				}
			}
			else
			{
				bSuccess = TRUE;
			}
		}
		else // Else turning off
		{
			if (!bEntryPopulated)// If entry in registry not populated
			{
				CComBSTR bstrXGP;
				bstrXGP = this->GetInstallDir();
				bstrXGP += L"XenGuestPlugin.exe";

				for (unsigned int posSpace = 0; posSpace < bstrXGP.Length(); ++posSpace)
				{
					if (bstrXGP[posSpace] == L' ')
					{
						CComBSTR bstrQuote(L"\"");
						CComBSTR bstrTemp(bstrQuote);
						bstrTemp.AppendBSTR(bstrXGP);
						bstrTemp.AppendBSTR(bstrQuote);
						bstrXGP = bstrTemp;
						break;
					}
				}

				// Set to plugin path (turn on).
				lRet = clRunKey.SetStringValue(XGA_RUN_INSTVAL, COLE2T(bstrXGP));
				if (lRet == ERROR_SUCCESS) // If set to plugin path ok
					bSuccess = TRUE;
				else // Else failed to set to plugin path
				{
					this->LogEventTypeId(ctxLS(IDS_CREATING_XCIPLUGIN_RUN_VALUE_FAI_XENGUESTSERVICES_652),
									 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
				}
			}
			else
			{
				bSuccess = TRUE;
			}
		}
	}

	return bSuccess;
}

STDMETHODIMP CXenGuestAgent::TogglePluginAutorun(BOOL bToggle, BOOL* pvbAutorun)
{
	if (!bToggle) // If not toggling
	{
		*pvbAutorun = IsPluginAutorunOn();
	}
	else // Else toggling
	{
		BOOL const bPluginAutorunEnabled = IsPluginAutorunOn();
		BOOL const bConfigurePluginAutorun = this->ConfigurePluginAutorun(!bPluginAutorunEnabled);
		if (!bConfigurePluginAutorun) // If failed to configure
		{
			// No toggling occurred.
			*pvbAutorun = bPluginAutorunEnabled;
		}
		else // Else configured
		{
			// Toggled successfully.
			*pvbAutorun = !bPluginAutorunEnabled;

		}
	}
	return S_OK;
}


void CXenGuestAgent::LogEventTypeId(LPCTSTR tszFormat, WORD wType, DWORD dwEventId, va_list args)
{
	TCHAR   tszMsg[XGA_MAX_MESSAGE + 1];

	// Check log flags
	if ((dwEventId == EVMSG_COMMUNICATION_FAILURE)&&(!m_bLogCommunicationErrors))
		return;
	if ((dwEventId == EVMSG_OPERATION_FAILURE)&&(!m_bLogOperationErrors))
		return;

	if (tszFormat != NULL)
	{
		_vsntprintf_s(tszMsg, XGA_MAX_MESSAGE, _TRUNCATE, tszFormat, args);

		_AtlModule.LogEventEx(dwEventId, tszMsg, wType);
	}
	else
		_AtlModule.LogEventEx(dwEventId, NULL, wType);
}

void CXenGuestAgent::LogEventTypeId(ULONG ulFormat, WORD wType, DWORD dwEventId, va_list args)
{
	if (ulFormat != 0)
	{
		TCHAR tszFormat[_MAX_PATH];

		_stprintf_s(tszFormat, _MAX_PATH, _T("%d"), ulFormat);
		LoadString(NULL, ulFormat, tszFormat, _MAX_PATH);
		LogEventTypeId(tszFormat, wType, dwEventId, args);
	}
	else
		LogEventTypeId((LPCTSTR)NULL, wType, dwEventId, args);
}

void CXenGuestAgent::LogEventTypeId(ULONG ulFormat, WORD wType, DWORD dwEventId, ...)
{
	va_list pArg;
	va_start(pArg, dwEventId);
	LogEventTypeId(ulFormat, wType, dwEventId, pArg);
	va_end(pArg);
}

//! @brief Log event on error, using "GetLastError()".
//! If there is a text description of the error available from Windows, include it in the event log output.
//! @param tszFormat	Format string.
//! @param wType	Event type.
//! @param dwEventId	Event Id.
//! @param lRet	Return value from registry call.
//! @param args	ellipsis arguments.
//! @return	true if error occurred, otherwise false.
bool CXenGuestAgent::LogEventTypeIdLastError(ULONG ulFormat, WORD wType, DWORD dwEventId, va_list args)
{
#	ifdef UNICODE
	typedef std::wstring tstring;
	typedef std::wostringstream tostringstream;
#	else // !UNICODE
	typedef std::string tstring;
	typedef std::ostringstream tostringstream;
#	endif // !UNICODE
	static size_t const HexWidth = 2;
	DWORD const dwLastError = ::GetLastError();
	bool const bError = dwLastError != 0;
	TCHAR tszFormat[_MAX_PATH];

	_stprintf_s(tszFormat, _MAX_PATH, _T("%d"), ulFormat);
	LoadString(NULL, ulFormat, tszFormat, _MAX_PATH);

	if (bError) // If error occurred
	{
		LPVOID lpMsgBuf = NULL;
		tostringstream strstrFormat;
		DWORD dwFormatMessageResult;

		strstrFormat << tszFormat <<  _T(" - ERROR");

		dwFormatMessageResult = FormatMessage(
			 FORMAT_MESSAGE_ALLOCATE_BUFFER | 
			 FORMAT_MESSAGE_FROM_SYSTEM |
			 FORMAT_MESSAGE_IGNORE_INSERTS
			,NULL // source
			,dwLastError // Message ID
			,MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT) // Language ID
			,(LPTSTR) &lpMsgBuf // Buffer
			,0 // Size
			,NULL // Arguments
		);

		bool const bGotDescription = dwFormatMessageResult != 0 && lpMsgBuf != NULL;
		if (bGotDescription) // If got error description
		{
			strstrFormat << _T("(");

		} // Ends if got error description
		else // Else no error description
		{
			strstrFormat << _T(": ");

		} // Ends else no error description

		// Output the error code.
		strstrFormat << dwLastError;

		if (bGotDescription) // If got error description
		{
			strstrFormat << _T("): ") << reinterpret_cast<LPCTSTR>(lpMsgBuf);

		} // Ends else got error description

		if (lpMsgBuf != NULL) // If got message
		{
			LocalFree(lpMsgBuf);

		} // Ends if got message

		LogEventTypeId(strstrFormat.str().c_str()
			,EVENTLOG_ERROR_TYPE
			,EVMSG_START_FAILURE
			,args
		);
	} // Ends if error occurred

	return bError;
}

bool CXenGuestAgent::LogEventTypeIdLastError(ULONG ulFormat, WORD wType, DWORD dwEventId, ...)
{
	va_list pArg;
	va_start(pArg, dwEventId);
	bool const bResult = LogEventTypeIdLastError(ulFormat, wType, dwEventId, pArg);
	va_end(pArg);
	return bResult;
}

//! @brief Log event on registry error, using "GetLastError()" if possible, otherwise last registry error argument.
//! If there is a text description of the error available from Windows, include it in the event log output.
//! @param tszFormat	Format string.
//! @param wType	Event type.
//! @param dwEventId	Event Id.
//! @param lRet	Return value from registry call.
//! @param args	ellipsis arguments.
//! @return	true if registry error occurred, otherwise false.
bool CXenGuestAgent::LogEventTypeIdLastRegistryError(ULONG ulFormat, WORD wType, DWORD dwEventId, LONG lRet, va_list args)
{
#	ifdef UNICODE
	typedef std::wstring tstring;
	typedef std::wostringstream tostringstream;
#	else // !UNICODE
	typedef std::string tstring;
	typedef std::ostringstream tostringstream;
#	endif // !UNICODE
	static size_t const HexWidth = 2;
	bool const bError = lRet != ERROR_SUCCESS;
	TCHAR tszFormat[_MAX_PATH];

	_stprintf_s(tszFormat, _MAX_PATH, _T("%d"), ulFormat);
	LoadString(NULL, ulFormat, tszFormat, _MAX_PATH);
	if (bError) // If error occurred
	{
		LPVOID lpMsgBuf = NULL;
		DWORD const dwLastError = ::GetLastError();
		tostringstream strstrFormat;
		DWORD dwFormatMessageResult;

		strstrFormat << tszFormat <<  _T(" - error ");

		if (dwLastError != 0) // If there is a last error
		{
			dwFormatMessageResult = FormatMessage(
				 FORMAT_MESSAGE_ALLOCATE_BUFFER | 
				 FORMAT_MESSAGE_FROM_SYSTEM |
				 FORMAT_MESSAGE_IGNORE_INSERTS
				,NULL // source
				,dwLastError // Message ID
				,MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL) // Language ID
				,(LPTSTR) &lpMsgBuf // Buffer
				,0 // Size
				,NULL // Arguments
			);
		} // Ends if there is a last error
		else // Else no last error
		{
			// Work out what went wrong with the registry key.
			// Retrieve the system error message for the last-error code.
			dwFormatMessageResult = FormatMessage(
				 FORMAT_MESSAGE_ALLOCATE_BUFFER | 
				 FORMAT_MESSAGE_FROM_SYSTEM |
				 FORMAT_MESSAGE_IGNORE_INSERTS
				,NULL // source
				,lRet // Message ID
				,MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL) // Language ID
				,(LPTSTR) &lpMsgBuf // Buffer
				,0 // Size
				,NULL // Arguments
			);
		} // Ends else no last error

		bool const bGotDescription = dwFormatMessageResult != 0 && lpMsgBuf != NULL;
		if (bGotDescription) // If got error description
		{
			strstrFormat << _T("(");
		}

		// Output the error code in hex.
		strstrFormat << _T("0x") << std::hex << std::setfill(_T('0')) << std::setw(HexWidth);

		if (dwLastError != 0) // If last error
		{
			strstrFormat << dwLastError;

		} // Ends if last error
		else // Else no last error
		{
			strstrFormat << lRet;

		} // Ends else no last error

		strstrFormat << std::resetiosflags(strstrFormat.flags());

		if (bGotDescription) // If got error description
		{
			strstrFormat << _T("): ") << reinterpret_cast<LPCTSTR>(lpMsgBuf);

		} // Ends else got error description

		if (lpMsgBuf != NULL) // If got message
		{
			LocalFree(lpMsgBuf);

		} // Ends if got message

		LogEventTypeId(strstrFormat.str().c_str()
			,EVENTLOG_ERROR_TYPE
			,EVMSG_START_FAILURE
			,args
		);
	} // Ends if error occurred

	return bError;
}

bool CXenGuestAgent::LogEventTypeIdLastRegistryError(ULONG ulFormat, WORD wType, DWORD dwEventId, LONG lRet, ...)
{
	va_list args;
	va_start(args, lRet);
	bool const bResult = LogEventTypeIdLastRegistryError(ulFormat, wType, dwEventId, lRet, args);
	va_end(args);
	return bResult;
}

//! @brief Log event on error, using "GetLastError()". If no error, log output anyway.
//! If there is a text description of the error available from Windows, include it in the event log output.
//! @param tszFormat	Format string.
//! @param wType	Event type.
//! @param dwEventId	Event Id.
//! @param lRet	Return value from registry call.
//! @param args	ellipsis arguments.
//! @return	true if error occurred, otherwise false.
bool CXenGuestAgent::LogEventTypeIdLastErrorAlways(ULONG ulFormat, WORD wType, DWORD dwEventId, va_list args)
{
	bool const bError = LogEventTypeIdLastError(ulFormat, wType, dwEventId, args);
	if (!bError)
	{
		LogEventTypeId(ulFormat, wType, dwEventId);
	}
	return bError;
}

bool CXenGuestAgent::LogEventTypeIdLastErrorAlways(ULONG ulFormat, WORD wType, DWORD dwEventId, ...)
{
	va_list args;
	va_start(args, dwEventId);
	bool const bResult = LogEventTypeIdLastErrorAlways(ulFormat, wType, dwEventId, args);
	va_end(args);
	return bResult;
}

bool CXenGuestAgent::RegisterXgs()
{
	LONG lRet;

	lRet = ::InterlockedIncrement(&m_ulXgsCount);

	if (lRet > XGS_MAX_INSTANCES)
	{
		::InterlockedDecrement(&m_ulXgsCount);
		return false;
	}
	return true;
}

void CXenGuestAgent::UnregisterXgs()
{
	::InterlockedDecrement(&m_ulXgsCount);
	assert(m_ulXgsCount >= 0);
}

void CXenGuestAgent::AddVmsShared(XGS_VMS_SHARED *pVmsShared)
{
	::WaitForSingleObject(m_hVmsLock, INFINITE);
	m_vVmsShared.push_back(pVmsShared);
	::ReleaseMutex(m_hVmsLock);
}

void CXenGuestAgent::RemoveVmsShared(XGS_VMS_SHARED *pVmsShared)
{
	TVmsVector::iterator iter;

	::WaitForSingleObject(m_hVmsLock, INFINITE);
	for (iter = m_vVmsShared.begin(); iter != m_vVmsShared.end(); ++iter)
	{
		if (*iter == pVmsShared)
		{
			m_vVmsShared.erase(iter);
			break;
		}
	}
	::ReleaseMutex(m_hVmsLock);
}

void CXenGuestAgent::AddAlertsShared(XGS_ALERTS_SHARED *pAlertsShared)
{
	::WaitForSingleObject(m_hAlertsLock, INFINITE);
	m_vAlertsShared.push_back(pAlertsShared);
	::ReleaseMutex(m_hAlertsLock);
}

void CXenGuestAgent::RemoveAlertsShared(XGS_ALERTS_SHARED *pAlertsShared)
{
	TAlertsVector::iterator iter;

	::WaitForSingleObject(m_hAlertsLock, INFINITE);
	for (iter = m_vAlertsShared.begin(); iter != m_vAlertsShared.end(); ++iter)
	{
		if (*iter == pAlertsShared)
		{
			m_vAlertsShared.erase(iter);
			break;
		}
	}
	::ReleaseMutex(m_hAlertsLock);
}

CXGSVM** CXenGuestAgent::LoadVms(DWORD *pdwCount)
{
	DWORD dwCount = 0, i;
	CXenStoreWrapper clXs;
	LPSTR* ppszList;
	LPSTR  pszVal;
	CXGSVM **ppVms = NULL;
	char szPath[_MAX_PATH + 1];

	*pdwCount = 0;

	if (!clXs.XS2Open())
	{
		LogEventTypeId(ctxLS(IDS_XS2OPEN_FAILED_FOR_LOADVMS___ERR_XENGUESTAGENT_1090),
					   EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
		return NULL;
	}

	ppszList = clXs.XS2Directory("/xenmgr/vms", &dwCount);
	// Should at least be one, i.e. us
	if ((ppszList == NULL)||(dwCount == 0)) 
	{
		if (ppszList != NULL)
			clXs.XS2FreeDirectory(ppszList, 0);
		LogEventTypeId(ctxLS(IDS_XS2DIRECTORY_NO_VMS_RETURNED____XENGUESTAGENT_1101),
					   EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE);
		return NULL;
	}

	// Allocate storage for them
	try
	{
		ppVms = new CXGSVM*[dwCount];
		if (ppVms == NULL)
			return NULL;
	}
	catch (std::bad_alloc & /*ba*/)
	{
		return NULL;
	}

	for (i = 0; i < dwCount; i++)
	{
		try
		{
			ppVms[i] = new CXGSVM();
			if (ppVms[i] == NULL)
			{
				FreeVms(ppVms, i + 1);
				return NULL;
			}
		}
		catch(std::bad_alloc & /*ba*/)
		{
			FreeVms(ppVms, i + 1);
			return NULL;
		}
	}

	// Loop and load info for each VM 
	for (i = 0; i < dwCount; i++)
	{
		ppVms[i]->m_bstrUuid = CComBSTR(ppszList[i]);
		
		_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", ppszList[i], "name");
		pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
		if (pszVal != NULL)
		{
			ppVms[i]->m_bstrName = CComBSTR(CA2W(pszVal,CP_UTF8));
			clXs.XS2Free(pszVal);
		}
		else
			ppVms[i]->m_bstrName = L"";
		
		_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", ppszList[i], "image");
		pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
		if (pszVal != NULL)
		{
			ppVms[i]->m_bstrImage = CComBSTR(CA2W(pszVal,CP_UTF8));
			clXs.XS2Free(pszVal);
		}
		else
			ppVms[i]->m_bstrImage = L"";

		_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", ppszList[i], "domid");		
		pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
		if (pszVal != NULL)
		{
			ppVms[i]->m_usDomId = (USHORT)strtol(pszVal, NULL, 10);
			clXs.XS2Free(pszVal);
		}
		else
			ppVms[i]->m_usDomId = XEN_DOMID_INVALID;

		_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", ppszList[i], "slot");		
		pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
		if (pszVal != NULL)
		{
			long const lSlot = strtol(pszVal, NULL, 10);
			clXs.XS2Free(pszVal);

			if (lSlot == XEN_SLOT_INVALID)
			{
				ppVms[i]->m_ulSlot = XEN_SLOT_INVALID;
			}
			else
			{
				ppVms[i]->m_ulSlot = (ULONG)lSlot;

				// this is fugly
				if (ppVms[i]->m_ulSlot < 0 || ppVms[i]->m_ulSlot > 100) {
					FreeVms(ppVms, dwCount);
					return NULL;
				}
			}
		}
		else
			ppVms[i]->m_ulSlot = XEN_SLOT_INVALID;

		_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", ppszList[i], "hidden");
		pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
		if (pszVal != NULL)
		{
			ppVms[i]->m_bHidden = ((ULONG)strtol(pszVal, NULL, 10) == 0) ? false : true;
			clXs.XS2Free(pszVal);
		}
		else
			ppVms[i]->m_bHidden = false;

		_snprintf_s(szPath, _MAX_PATH, _TRUNCATE, "/xenmgr/vms/%s/%s", ppszList[i], "uivm");		
		pszVal = (LPSTR)clXs.XS2Read(szPath, NULL);
		if (pszVal != NULL)
		{
			ppVms[i]->m_bUivm = ((ULONG)strtol(pszVal, NULL, 10) == 0) ? false : true;
			clXs.XS2Free(pszVal);
		}
		else
			ppVms[i]->m_bUivm = false;
		
		if ((ppVms[i]->m_bstrName.Length() == 0)&&(ppVms[i]->m_ulSlot == XEN_SLOT_INVALID))
			ppVms[i]->m_bInvalid = true;
		else
			ppVms[i]->m_bInvalid = false;

		// Dbus magic happens.
		GetDbusShowSwitcherBarProperty(ppszList[i], &ppVms[i]->m_bShowSwitcher);

	} // Ends loop over VMs

	*pdwCount = dwCount;
	return ppVms;
}

//! @brief Get the show-switcher property using HTTP over v4v to interrogate dbus.
//! @param pszVmUID       - [in] VM UID who's show-switcher property we're interested in.
//! @param pbShowSwitcher - [out] show-switcher property if successfully retrieved, otherwise unaltered.
//! @param dwEventId      - [in,optional] Event id to log errors with.
//! @result TRUE if successfully retrieved property, otherwise FALSE.
BOOL CXenGuestAgent::GetDbusShowSwitcherBarProperty(CHAR const * const pszVmUID, BOOL * const pbShowSwitcher, DWORD const dwEventId)
{
	BOOL bGotShowSwitcherBar = FALSE;

	if (pbShowSwitcher != NULL)
	{
		CXenHttp clXh(this);
		char szVMPath[_MAX_PATH + 1];

		_snprintf_s(szVMPath, _MAX_PATH, _TRUNCATE, "/vm/%s", pszVmUID);

		Json::FastWriter xcWriter;
		Json::Value xcShowSwitcherBarRoot(Json::objectValue);
		Json::Value xcParams(Json::objectValue);
		Json::Value xcArgs(Json::arrayValue);

		DWORD argsIter = 0;
		xcArgs[argsIter++] = "com.citrix.xenclient.xenmgr.vm"; // Dbus interface to get.
		xcArgs[argsIter++] = "show-switcher"; // Dbus property to get.

		xcParams["args"] = xcArgs;
		xcParams["method"] = Json::Value("Get"); // Dbus method to call (from org.freedesktop.DBus.Properties)
		xcParams["destination"] = Json::Value("com.citrix.xenclient.xenmgr");
		xcParams["interface"] = Json::Value("org.freedesktop.DBus.Properties");
		xcParams["path"] = Json::Value(szVMPath);

		xcShowSwitcherBarRoot["params"] = xcParams;
		xcShowSwitcherBarRoot["method"] = Json::Value("dbus");
		xcShowSwitcherBarRoot["id"] = Json::Value("1"); // Hard-coded Id because it has to be present...

		clXh.Reset();
		std::string buffer = xcWriter.write(xcShowSwitcherBarRoot);

		DWORD const dwStatus = clXh.DoPOST(
			"/dbus/", // szResource
			"", // szParams
			(BYTE *)(buffer.c_str()), // pbReqData
			buffer.length(), // ulSize
			this->m_hShutdownEvent); // hWait1

		if (dwStatus == ERROR_SUCCESS && clXh.GetResponse().m_iHttpCode == 200) // If HTTP POST ok
		{
			// Process response (JSON).
			ULONG const ulContentLength = clXh.GetResponse().m_ulContentLength;
			char * const bufSignal = new char[ulContentLength + 1];
			Json::Features jfFeatures;
			Json::Value jvRoot;
			memcpy(bufSignal, clXh.GetDataBuffer(), ulContentLength);
			bufSignal[ulContentLength] = '\0';

			Json::Reader jrReader(jfFeatures);

			bool const bParsing = jrReader.parse(bufSignal, jvRoot);
			delete[] bufSignal;

			if (!bParsing) // If failed to parse response
			{
				this->LogEventTypeId(ctxLS(IDS_FAILED_TO_PARSE_JSON_PROPERTY__ER_XENGUESTAGENT),
					EVENTLOG_ERROR_TYPE, dwEventId,
					CA2T(jrReader.getFormatedErrorMessages().c_str()),
					CA2T("show-switcher"));

			} // Ends if failed to parse response
			else // Else parsed response ok
			{
				Json::Value jvResponseArgs = jvRoot["result"];
				if (jvResponseArgs.isArray() == false || jvResponseArgs.empty()) // If no results array
				{
					TCHAR szArray[128] = {'a', 'r', 'r', 'a', 'y'}; // Holds string for the localised word "array". Any language which spells array in more than 128 characters will have issues.
					::LoadString(NULL, IDS_ARRAY, szArray, sizeof(szArray)/sizeof(*szArray));
					this->LogEventTypeId(ctxLS(IDS_UNEXPECTED_JSON_VALUE__ER_XENGUESTAGENT),
						EVENTLOG_ERROR_TYPE, dwEventId,
						szArray);

				} // Ends if no results array
				else // Else results array
				{
					Json::Value jvShowSwitcher = jvResponseArgs[(unsigned int)0];

					if (jvShowSwitcher.isBool() == false) // If not a flag
					{
						TCHAR szBool[128] = {'b', 'o', 'o', 'l'}; // Holds string for the localised word "array". Any language which spells array in more than 128 characters will have issues.
						::LoadString(NULL, IDS_BOOL, szBool, sizeof(szBool)/sizeof(*szBool));
						this->LogEventTypeId(ctxLS(IDS_UNEXPECTED_JSON_VALUE__ER_XENGUESTAGENT),
							EVENTLOG_ERROR_TYPE, dwEventId,
							szBool);

					} // Ends if not a flag
					else // Else flag
					{
						bGotShowSwitcherBar = TRUE;
						bool const bShowSwitcherBarProperty = jvShowSwitcher.asBool();
						*pbShowSwitcher = bShowSwitcherBarProperty ? TRUE : FALSE;

					} // Ends else flag
				}

			} // Ends else parsed response ok
		} // Ends if HTTP POST ok
		else // Else HTTP POST failed
		{
			/* HTTP has been blocked, comment out this error until the service can be replaced
			this->LogEventTypeId(ctxLS(IDS_DOPOST___OPERATION_FAILED___ERRO_XENGUESTAGENT_1596),
				EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE,
				dwStatus, clXh.GetResponse().m_iHttpCode);
				*/

		} // Ends else HTTP POST failed
	} // Ends if got the dbus format UUID

	return bGotShowSwitcherBar;
}

VOID CXenGuestAgent::FreeVms(CXGSVM **ppVms, DWORD dwCount)
{
	if (ppVms == NULL)
		return;
	for (DWORD i = 0; i < dwCount; i++)
		delete ppVms[i];
	delete [] ppVms;
}

UINT WINAPI CXenGuestAgent::_TaskThread(void *pv)
{
	CXenGuestAgent *pxga = (CXenGuestAgent*)pv;
	LONG lRet;
	CRegKey keyWlanProf;
    TCHAR szSysPath[_MAX_PATH + 1];
    char szPowerCfgPath[_MAX_PATH + 1];
    char szPowerCfgCmd[_MAX_PATH * 2 + 1];
    STARTUPINFOA stStartInfo;
    PROCESS_INFORMATION stProcessInfo;
    BOOL rc;

	// At some point it might be useful to had a more generic
	// way to have dynamic tasks queued to the task thread.

	if (::WaitForSingleObject(pxga->m_hShutdownEvent, 0) == WAIT_OBJECT_0)
		return 0;

    // Enable Hibernation on XP
    if (pxga->m_osvi.dwMajorVersion < 6)
	{
        if (!GetSystemDirectory(szSysPath, _MAX_PATH + 1))
            pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_RETURN_SYSTEM_DIRECTOR_XENGUESTAGENT_1248),EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE);
	    _snprintf_s(szPowerCfgPath, _MAX_PATH, _TRUNCATE, "%S\\powercfg.exe", szSysPath);
	    _snprintf_s(szPowerCfgCmd, _MAX_PATH * 2, _TRUNCATE, "\"%s\" -H ON", szPowerCfgPath);

	    ::ZeroMemory(&stStartInfo, sizeof(STARTUPINFO));
	    ::ZeroMemory(&stProcessInfo, sizeof(PROCESS_INFORMATION));
	    stStartInfo.cb = sizeof(STARTUPINFO);

	    rc = ::CreateProcessA(szPowerCfgPath, szPowerCfgCmd, NULL, NULL, FALSE,
		    0, NULL, NULL, &stStartInfo, &stProcessInfo);

	    if (!rc)
	    {
		    pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_LAUNCH_POWERCFG_TOOL___XENGUESTAGENT_1261),
			    EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE,
			    ::GetLastError());
	    }
	    else
	    {
		    // Wait until child process exits.
		    WaitForSingleObject (stProcessInfo.hProcess, INFINITE);

		    // Close process and thread handles. 
		    CloseHandle (stProcessInfo.hProcess);
		    CloseHandle (stProcessInfo.hThread);
	    }
    }

	// Disable Hybrid Sleep on Vista+
	if (pxga->m_osvi.dwMajorVersion >= 6)
	{
		int n;
		if (n = poweropts() != 0)
		{
			pxga->LogEventTypeId(ctxLS(IDS_FAILURE_RETURNED_FROM_POWEROPTS__XENGUESTAGENT_1282),
								 EVENTLOG_WARNING_TYPE, EVMSG_WARNING, n);
		}
	}

	// Set XenClient Wireless autoconnect profile
	//
	// Calling wlanprof() will block until the Citrix PV device is found. Therefore,
	// be sure to call this routine last . This is OK since we are in a dedicated
	// thread anyway and no other functions will be called.
	//
	lRet = keyWlanProf.Open(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Citrix\\XenClient\\WlanProf"));
	if (lRet == ERROR_SUCCESS)
	{
		int n;
		DWORD WlanStatus;
		if (n = wlanprof(&WlanStatus) != 0)
		{
			switch (n) {
				case -1:
					pxga->LogEventTypeId(ctxLS(IDS_WLANPROF__WLANAPI_DLL_NOT_FOUND_XENGUESTAGENT_1302),
						EVENTLOG_WARNING_TYPE, EVMSG_WARNING);
					break;
				case -2:
					pxga->LogEventTypeId(ctxLS(IDS_WLANPROF__ERROR_LOADING_WIFI_API_XENGUESTAGENT_1306),
						EVENTLOG_WARNING_TYPE, EVMSG_WARNING);
					break;
				case -3:
					pxga->LogEventTypeId(ctxLS(IDS_WLANPROF__ERROR_GETTING_LIST_OF__XENGUESTAGENT_1310),
						EVENTLOG_WARNING_TYPE, EVMSG_WARNING, WlanStatus);
					break;
				case -4:
					pxga->LogEventTypeId(ctxLS(IDS_WLANPROF__ERROR_ADDING_AP__WLAN__XENGUESTAGENT_1314),
						EVENTLOG_WARNING_TYPE, EVMSG_WARNING, WlanStatus);
					break;
				default:
					pxga->LogEventTypeId(ctxLS(IDS_WLANPROF__UNKNOWN_FAILURE_ENCOUN_XENGUESTAGENT_1318),
						EVENTLOG_WARNING_TYPE, EVMSG_WARNING, n);
					break;
			}
		}
	}

	return 0;
}

UINT WINAPI CXenGuestAgent::_VmsThread(void *pv)
{
	CXenGuestAgent *pxga = (CXenGuestAgent*)pv;
	HANDLE harr[2] = {pxga->m_hShutdownEvent, pxga->m_hVmsEvent};
	DWORD dwStatus;
	UINT uiRet = 0;
	CXenStoreWrapper clXs;
	LPVOID varr[2] = {0, 0};

	if (!clXs.XS2Open())
	{
		pxga->LogEventTypeId(ctxLS(IDS_XS2OPEN_FAILED_FOR_WATCHES___ERR_XENGUESTAGENT_1339),
							 EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return (UINT)-1;
	}

	// Watching the VMS path - all of this will change later to use HTTP/V4V
	varr[0] = clXs.XS2Watch("/xenmgr/vms", pxga->m_hVmsEvent);
	if (varr[0] == NULL)
	{
		clXs.XS2Close();
		pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_OPEN_XENSTORE_VMS_WATC_XENGUESTAGENT_1349),
							 EVENTLOG_ERROR_TYPE, EVMSG_START_FAILURE, ::GetLastError());
		return (UINT)-2;
	}

	do {
		dwStatus = ::WaitForMultipleObjects(2, harr, FALSE, INFINITE);		
		if (dwStatus == WAIT_OBJECT_0)
		{
			// Shutdown signalled, just end
			pxga->LogEventTypeId(ctxLS(IDS_XS_WATCH_THREAD_SHUTTING_DOWN__XENGUESTAGENT_1359),
								 EVENTLOG_INFORMATION_TYPE, EVMSG_GENERAL);
			break;
		}
		else if (dwStatus == WAIT_OBJECT_0 + 1)
		{
			// VMS event, signal all registered XenGuestService object events
			pxga->SignalAllVmsEvents();
		}
		else if (dwStatus == WAIT_FAILED)
		{
			pxga->LogEventTypeId(ctxLS(IDS_WAIT_FAILED_DURING_XS_WATCH_WAIT_XENGUESTAGENT_1370),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
			uiRet = (UINT)-10;
			break;
		}
		else
		{
			// SNO
			pxga->LogEventTypeId(ctxLS(IDS_UNKNOWN_FAILURE_DURING_XS_WATCH__XENGUESTAGENT_1378),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
			uiRet = (UINT)-20;
			break;
		}
	} while (true);

	clXs.XS2Close();

	return uiRet;
}

UINT WINAPI CXenGuestAgent::_RunXcDiag(void *pv, UINT uiSigId)
{
	CXenGuestAgent *pxga = (CXenGuestAgent*)pv;
	std::string strUuid = CW2A(pxga->GetUUID());
	CXenHttp clXh(pxga);
	char szXcDiagPath[_MAX_PATH + 1];
	char szXcDiagOut[_MAX_PATH + 1];
	char szXcDiagCmd[_MAX_PATH * 2 + 1];
	STARTUPINFOA stStartInfo;
	PROCESS_INFORMATION stProcessInfo;
	BOOL rc;
	UINT uiRet = 0;
	static UINT XcDiagRequestNum = 0;
	char szXcDiagRequestNum[16];

	//
	// A status-report request has been received...this can be simulated with:
	// dbus-send --system --type=signal /com/citrix/xenclient/xenmgr com.citrix.xenclient.xenmgr.diag.gather_request
	//

	_snprintf_s(szXcDiagPath, _MAX_PATH, _TRUNCATE, "%S\\xcdiag.exe", pxga->m_bstrXcInstallDir);
	_snprintf_s(szXcDiagOut, _MAX_PATH, _TRUNCATE, "%S\\xcdiag.zip", pxga->m_bstrXcInstallDir);
	_snprintf_s(szXcDiagCmd, _MAX_PATH * 2, _TRUNCATE, "\"%s\" -batch -quick -log -out \"%s\"", szXcDiagPath, szXcDiagOut);

	pxga->LogEventTypeId(ctxLS(IDS_RECEIVED_XCDIAG_REQUEST_XENGUESTAGENT_1414),
		EVENTLOG_INFORMATION_TYPE, EVMSG_GENERAL);

	::ZeroMemory(&stStartInfo, sizeof(STARTUPINFO));
	::ZeroMemory(&stProcessInfo, sizeof(PROCESS_INFORMATION));
	stStartInfo.cb = sizeof(STARTUPINFO);

	//
	// Run the xcdiag tool. The command line arg specifies where
	// to place the final zip file.
	//
	rc = ::CreateProcessA(szXcDiagPath, szXcDiagCmd, NULL, NULL, FALSE,
		0, NULL, NULL, &stStartInfo, &stProcessInfo);

	if (!rc)
	{
		pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_LAUNCH_XCDIAG_TOOL___E_XENGUESTAGENT_1430),
			EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE,
			::GetLastError());
	}
	else
	{
		DWORD filesize;
		HANDLE hFile;
		HANDLE hMapFile;
		unsigned char * lpMapAddress;
		DWORD dwStatus;

		lpMapAddress = NULL;
		// Wait until child process exits.
		WaitForSingleObject (stProcessInfo.hProcess, INFINITE);

		// Close process and thread handles. 
		CloseHandle (stProcessInfo.hProcess);
		CloseHandle (stProcessInfo.hThread);

		// Now post the resultant zip archive to the web server
		_snprintf_s(szXcDiagPath, _MAX_PATH, _TRUNCATE, "xcdiag-%s-%u.zip",
			strUuid.c_str(), uiSigId);

		// Create a memory mapped view of the zip file
		hFile = CreateFileA(szXcDiagOut, 
			GENERIC_READ | GENERIC_WRITE,
			0, 
			NULL,
			OPEN_EXISTING, 
			FILE_ATTRIBUTE_NORMAL, 
			NULL);

		if (hFile == INVALID_HANDLE_VALUE)
		{
			pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_OPEN_XCDIAG_OUTPUT_FIL_XENGUESTAGENT_1465),
				EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE);
			uiRet = (UINT)-5;
		}
		else
		{
			filesize = GetFileSize (hFile, NULL);

			// Create a file mapping object for the file 
			if (filesize == 0) // If file empty
			{
				pxga->LogEventTypeId(ctxLS(IDS_XCDIAG_OUTPUT_FILE_APPEARS_EMPTY_XENGUESTAGENT_1476),
					EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE);
				uiRet = (UINT)-6;

			} // Ends if file empty
			else // Else file not empty
			{
				char *tmp= (char *)malloc (filesize*2+1);
				if (tmp == NULL)
				{
					pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_ALLOCATE_MEMORY_FOR_XC_XENGUESTAGENT_1486),
						EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE);
					uiRet = (UINT)-7;
				}
				else
				{
					hMapFile = CreateFileMapping(
						hFile,          // current file handle
						NULL,           // default security
						PAGE_READWRITE, // read/write permission
						0,              // size of mapping object, high
						filesize,       // size of mapping object, low
						NULL);          // name of mapping object

					if (hMapFile == NULL)
					{
						pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_ACCESS_XCDIAG_FILE_XENGUESTAGENT_1502),
							EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE);
						uiRet = (UINT)-8;

					}
					else
					{
						// Map the view
						lpMapAddress = (unsigned char *)MapViewOfFile(
							hMapFile,			 // handle to mapping object
							FILE_MAP_ALL_ACCESS, // read/write 
							0,                   // high-order 32 bits of file offset
							0,                   // low-order 32 bits of file offset
							filesize);           // number of bytes to map
					}

					if (lpMapAddress == NULL)
					{
						pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_VIEW_XCDIAG_OUTPUT_XENGUESTAGENT_1520),
							EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE);
						uiRet = (UINT)-9;
						if (hMapFile != NULL)
						{
							::CloseHandle(hMapFile);
						}
					}
					else
					{
						// Build the JSON message as follows:
						//
						// {"jsonrpc": "2.0",
						//     "id": 20,
						//     "method": "dbus",
						//     "params": {"destination": "com.citrix.xenclient.xenmgr",
						//         "path": "/com/citrix/xenclient/xenmgr",
						//         "interface": "com.citrix.xenclient.xenmgr.diag",
						//         "method": "gather",
						//         "args": [<filename>, "<textual representation of zip file contents>"]
						//     }
						// }

						Json::FastWriter xcWriter;
						Json::Value xcDiagRoot(Json::objectValue);
						Json::Value xcParams(Json::objectValue);
						Json::Value xcArgs(Json::arrayValue);
						Json::Value xcArray(Json::stringValue);
						DWORD x,y;
						y = 0;
						for (x=0; x < filesize; x++)
						{
							_snprintf_s(&tmp[y], 3, _TRUNCATE, "%02x", lpMapAddress[x]);
							y += 2;
						}

						xcArray = Json::Value(tmp);
						::free (tmp);
						tmp = NULL;

						xcArgs.resize(2);
						x = 0;
						xcArgs[x] = Json::Value(szXcDiagPath);
						x = 1;
						xcArgs[x] = xcArray;
						xcParams["args"] = xcArgs;
						xcParams["method"] = Json::Value("gather");
						xcParams["destination"] = Json::Value("com.citrix.xenclient.xenmgr");
						xcParams["interface"] = Json::Value("com.citrix.xenclient.xenmgr.diag");
						xcParams["path"] = Json::Value("/");
						xcDiagRoot["params"] = xcParams;
						xcDiagRoot["method"] = Json::Value("dbus");
						XcDiagRequestNum++;
						_ltoa_s (XcDiagRequestNum, szXcDiagRequestNum, sizeof(szXcDiagRequestNum), 10);
						xcDiagRoot["id"] = Json::Value(szXcDiagRequestNum);
						xcDiagRoot["jsonrpc"] = Json::Value("2.0");

						std::string buffer = xcWriter.write (xcDiagRoot);

						// Send the request to push the file to the backend via V4V
						clXh.Reset();
						dwStatus = clXh.DoPOST(
							"/dbus/",
							szXcDiagPath, (BYTE *)(buffer.c_str()), buffer.length(),
							pxga->m_hShutdownEvent);

						UnmapViewOfFile(lpMapAddress);
						CloseHandle(hMapFile);

						if (dwStatus == ERROR_SUCCESS && clXh.GetResponse().m_iHttpCode == 200)
						{
							pxga->LogEventTypeId(ctxLS(IDS_XCDIAG_ZIP_FILE_SENT__XENGUESTAGENT_1591),
								EVENTLOG_INFORMATION_TYPE, EVMSG_GENERAL);
						}
						else
						{
							/* HTTP has been blocked, comment out this error until the service can be replaced
							pxga->LogEventTypeId(ctxLS(IDS_DOPOST___OPERATION_FAILED___ERRO_XENGUESTAGENT_1596),
								EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE,
								dwStatus, clXh.GetResponse().m_iHttpCode);
								*/
							uiRet = (UINT)-4;
						}

						CloseHandle(hFile);
					}

					if (tmp != NULL)
					{
						::free (tmp);

					}

				}
			} // Ends else file not empty
		}
	}
	return uiRet;
}

//! @brief Register for a D-Bus signal.
//! @param pxga	GuestAgent instance.
//! @param rclXh	XenHTTP for communicating with D-Bus.
//! @param szRegPath	D-Bus destination path buffer.
//! @param sizeRegPath	Length of destination path buffer.
//! @param strDomId	D-Bus Dom ID.
//! @param pszInterface	D-Bus interface.
//! @param ruiRet[out]	Error code.
//! @return	true if successfully registered, otherwise false.
bool CXenGuestAgent::_registerForSignal(CXenGuestAgent *pxga, CXenHttp &rclXh, char *szRegPath, size_t sizeRegPath
	,std::string const &rstrDomId, char const *pszInterface, UINT &ruiRet)
{
	bool bRegister = false;
	HANDLE hShutdownEvent = pxga->m_hShutdownEvent;
	DWORD dwStatus;
	int iter = 0;

	//
	// The purpose of this first loop is to keep retrying for up to 1 minute
	// to register the notification. Sometimes if we register too quickly after
	// initial startup it fails.
	//
	do
	{
		rclXh.Reset();
		_snprintf_s(szRegPath, sizeRegPath, _TRUNCATE, "/signal-register/%s/%s",
			rstrDomId.c_str(), pszInterface);

		dwStatus = rclXh.DoGET(szRegPath, NULL, hShutdownEvent);

		if (dwStatus != ERROR_SUCCESS || rclXh.GetResponse().m_iHttpCode != 200)
		{
			if (iter >= 20)
			{
				pxga->LogEventTypeId(ctxLS(IDS_DOGET___FAILED_TO_REGISTER_FOR_A_XENGUESTAGENT_1652)
					,EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE
					,dwStatus, rclXh.GetResponse().m_iHttpCode, pszInterface
					);
				ruiRet = (UINT)-2;
			}
		}
		else
		{
			bRegister = true;
		}

		iter++;
		Sleep (3000);
	}
	while ((dwStatus != ERROR_SUCCESS) && (iter < 20));

	return bRegister;
}

//! @brief Registers for alerts
//! @param pxga	GuestAgent instance.
//! @param rclXh	XenHTTP for communicating with D-Bus.
//! @param szRegPath	D-Bus destination path buffer.
//! @param sizeRegPath	Length of destination path buffer.
//! @param strDomId	D-Bus Dom ID.
//! @param pszInterface	D-Bus interface.
//! @param ruiRet[out]	Error code.
//! @return	true if all successfully registered, otherwise false.
bool CXenGuestAgent::_registerForSignals(CXenGuestAgent *pxga, CXenHttp &rclXh, std::string const &rstrDomId, UINT &ruiRet)
{
	bool bRegister = true;
	char szRegPath[_MAX_PATH + 1];

	// Register for xenmgr.diag notifications (from running status-report in dom0)
	if (!_registerForSignal(pxga, rclXh, szRegPath, _MAX_PATH, rstrDomId, "com.citrix.xenclient.xenmgr.diag", ruiRet))
		bRegister = false;

	// Register for general alerts sent to VMs.
	if (!_registerForSignal(pxga, rclXh, szRegPath, _MAX_PATH, rstrDomId, "com.citrix.xenclient.xenmgr.host", ruiRet))
		bRegister = false;

	// Register for USB alerts sent to VMs.
	if (!_registerForSignal(pxga, rclXh, szRegPath, _MAX_PATH, rstrDomId, "com.citrix.xenclient.usbdaemon", ruiRet))
		bRegister = false;

	return bRegister;
}

UINT WINAPI CXenGuestAgent::_AlertsThread(void *pv)
{
	CXenGuestAgent *pxga = (CXenGuestAgent*)pv;
	HANDLE hShutdownEvent;
	DWORD dwStatus;
	UINT uiRet = 0;
	CXenHttp clXh(pxga);
	std::string strDomId = CW2A(pxga->GetDomID());
	UINT uiSigId = 0;

	hShutdownEvent = pxga->m_hShutdownEvent;
	clXh.SetRingSize(4096*2);
	clXh.SetTimeout(INFINITE);

	// Explicitly attempt to open V4V connection to determine if it's successful
	// If unsuccessful the V4V device may not have finished installing
	// Retry until successful

	if (clXh.OpenTransport() != ERROR_SUCCESS)
	{
		pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_OPEN_V4V_CHANNEL_ON_FI_XENGUESTAGENT_1721), 
			EVENTLOG_WARNING_TYPE, EVMSG_CREATION_FAILURE, E_FAIL);
		do
		{
			Sleep(30000);
		}
		while (clXh.OpenTransport() != ERROR_SUCCESS);
	}

	// Register for notifications
	if (!_registerForSignals(pxga, clXh, strDomId, uiRet))
		return uiRet;

	do
	{
		char szNextPath[_MAX_PATH + 1];

		clXh.Reset();

		_snprintf_s(szNextPath, _MAX_PATH, _TRUNCATE, "/signal/%s/%u", strDomId.c_str(), uiSigId);	

		dwStatus = clXh.DoGET(szNextPath, NULL, hShutdownEvent);

		if (dwStatus == ERROR_SUCCESS && clXh.GetResponse().m_iHttpCode == 200)
		{
			ULONG const ulContentLength = clXh.GetResponse().m_ulContentLength;
			char *bufSignal = new char[ulContentLength + 1];
			Json::Features jfFeatures;
			Json::Value jvRoot;
			memcpy(bufSignal, clXh.GetDataBuffer(), ulContentLength);
			bufSignal[ulContentLength] = '\0';

			Json::Reader jrReader(jfFeatures);

			bool const bParsing = jrReader.parse(bufSignal, jvRoot);
			delete[] bufSignal;
			if (!bParsing)
			{
				pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_PARSE_JSON_SIGNAL___ER_XENGUESTAGENT_1759),
					EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE,
					CA2T(jrReader.getFormatedErrorMessages().c_str()));
			}
			else
			{
				int iNumSignals = jvRoot["num_signals"].asInt();
				int iQueueId = jvRoot["queue_id"].asInt();
				Json::Value jvSigArr = jvRoot["signals"];

				for (int i=0; i<iNumSignals; i++)
				{
					Json::Value jvSigObj = jvSigArr[i];
					Json::Value jvParamsArr = jvSigObj["params"];		
					if (jvSigObj["id"].asUInt() > uiSigId)
						uiSigId = jvSigObj["id"].asUInt();

					// verify member == "gather_request" to run xcdiag
					if (strcmp(jvSigObj["member"].asCString(), "gather_request") == 0)
					{
						_RunXcDiag(pv, uiSigId);
					}
					else 
					{
						SAFEARRAY *psaParams = NULL;
						SAFEARRAYBOUND stDim[1];
						BSTR *pbstrPtr;
						HRESULT hr;

						stDim[0].lLbound = 0;
						stDim[0].cElements = 2 + (UINT)jvParamsArr.size();  //interface, member, and params
						psaParams = ::SafeArrayCreate(VT_BSTR, 1, &stDim[0]);
						if (psaParams == NULL)
						{
							uiRet = (UINT)-20;
							return uiRet;
						}
						hr = ::SafeArrayAccessData(psaParams, (LPVOID*)&pbstrPtr);
						if (FAILED(hr))
						{
							::SafeArrayDestroy(psaParams);
							pxga->LogEventTypeId(ctxLS(IDS_SAFEARRAYACCESSDATA___FAILED___H_XENGUESTAGENT_1800),
								EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, hr);
							continue;
						}

						pbstrPtr[0] = CComBSTR(jvSigObj["interface"].asCString()).Detach();
						pbstrPtr[1] = CComBSTR(jvSigObj["member"].asCString()).Detach();

						for (UINT j = 0; j < (UINT)jvParamsArr.size(); j++)
							pbstrPtr[j+2] = CComBSTR(JsonExtras::valueToString(jvParamsArr[(UINT)j]).c_str()).Detach();

						::SafeArrayUnaccessData(psaParams);

						pxga->SignalAllAlertsEvents(psaParams, XenAlertNotify, pxga);
						::SafeArrayDestroy(psaParams);
					}
				}
			}

			uiSigId++; //Bump final good signal id by 1

		}
		else if (dwStatus == ERROR_WAIT_1)
		{
			//hShutdownEvent was signalled
			pxga->LogEventTypeId(ctxLS(IDS_ALERTS_THREAD_SHUTTING_DOWN__XENGUESTAGENT_1825),
				EVENTLOG_INFORMATION_TYPE, EVMSG_GENERAL);
			break;
		}
		else
		{
			pxga->LogEventTypeId(ctxLS(IDS_DOGET___FAILED_TO_RECEIVE_SIGNAL_XENGUESTAGENT_1831),
				EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE,
				dwStatus, clXh.GetResponse().m_iHttpCode);
			uiRet = (UINT)-3;

			// Reset notification queue by re-registering
			if (!_registerForSignals(pxga, clXh, strDomId, uiRet))
				pxga->LogEventTypeId(ctxLS(IDS_FAILED_TO_RESET_SIGNAL_REGISTRAT_XENGUESTAGENT_1838),
				EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE);
			else
				uiSigId = 0;
		}

	} while (true);

	return uiRet;
}

void CXenGuestAgent::SignalAllVmsEvents()
{
	TVmsVector::iterator iter;

	::WaitForSingleObject(m_hVmsLock, INFINITE);
	for (iter = m_vVmsShared.begin(); iter != m_vVmsShared.end(); ++iter)
	{
		::SetEvent((*iter)->hEvent);
	}
	::ReleaseMutex(m_hVmsLock);
}

void CXenGuestAgent::SignalAllAlertsEvents(SAFEARRAY* psa, XenAlertType exat, CXenGuestAgent *pxga)
{
	TAlertsVector::iterator iter;

	::WaitForSingleObject(m_hAlertsLock, INFINITE);
	for (iter = m_vAlertsShared.begin(); iter != m_vAlertsShared.end(); ++iter)
	{
		HRESULT hr;
		TAlertsQueue::value_type xgsAlert;
		::ZeroMemory(&xgsAlert, sizeof(TAlertsQueue::value_type));
		hr = ::SafeArrayCopy(psa, &xgsAlert.psa);

		if (FAILED(hr))
		{
			pxga->LogEventTypeId(ctxLS(IDS_SAFEARRAYCOPY___FAILED___HRESULT_XENGUESTAGENT_1875),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, hr);
			break;
		}
		if (xgsAlert.psa == NULL)
		{
			pxga->LogEventTypeId(ctxLS(IDS_E_OUTOFMEMORY___HRESULT__0XzX_XENGUESTAGENT_1881),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, E_OUTOFMEMORY);
			break;
		}

		xgsAlert.xaType = exat;
		
		(*iter)->qAlertQueue.push(xgsAlert);
		::SetEvent((*iter)->hEvent);
	}
	::ReleaseMutex(m_hAlertsLock);
}

CXenGuestAgent _XenGuestAgent;

extern "C" int WINAPI _tWinMain(HINSTANCE hInstance, HINSTANCE /*hPrevInstance*/, 
                                LPTSTR /*lpCmdLine*/, int nShowCmd)
{
	int iRet;

	_AtlModule.LoadStrings(hInstance);

	if (!_XenGuestAgent.Initialize())
	{
		_XenGuestAgent.SetShutdownEvent();
		_XenGuestAgent.Uninitialize();
		return -2;
	}

	// Up front tasks like registering the event log message binary. For
	// tasks like updating local only access, the app will quite following these.
	if (_AtlModule.PreStartTasks())
		return 0;

    iRet = _AtlModule.WinMain(nShowCmd);

	// Tasks after server/service registration
	_AtlModule.PostStartTasks();

	_XenGuestAgent.SetShutdownEvent();
	_XenGuestAgent.Uninitialize();

	// Free the xs2.dll library as late as possible after all threads are done. If
	// it was not loaded, the routine will handle that.
	CXenStoreWrapper::XS2Uninitialize();

	return iRet;
}

