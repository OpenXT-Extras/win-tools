/*
 * Copyright (c) 2012 Citrix Systems, Inc.
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

// XenGuestServices.h : Declaration of the CXenGuestServices

#pragma once
#include "resource.h"       // main symbols
#include "input.h"
#include "XenGuestAgent_i.h"

#define APPID_XenGuestAgent "{AB21B428-D26C-4471-9A8F-F1C708397D21}"

#define XGS_MAX_INSTANCES 64

class CXenHttp;

// A single instance of this class connects the various bits and pieces of the
// XenGuestAgent service together.

class CXGSVM 
{
public:
	CComBSTR m_bstrUuid;
	CComBSTR m_bstrName;
	CComBSTR m_bstrImage;
	USHORT   m_usDomId;
	ULONG    m_ulSlot;
	BOOL     m_bHidden;
	BOOL     m_bUivm;
	BOOL     m_bInvalid;
	BOOL     m_bShowSwitcher;
};

typedef struct _XGS_VMS_SHARED {
	HANDLE hEvent;
} XGS_VMS_SHARED, *PXGS_VMS_SHARED;

typedef std::vector<PXGS_VMS_SHARED> TVmsVector;

typedef struct _XGS_ALERT {
	SAFEARRAY* psa;
	XenAlertType xaType;
} XGS_ALERT, *PXGS_ALERT;

typedef std::queue<XGS_ALERT> TAlertsQueue;

typedef struct _XGS_ALERTS_SHARED {
	HANDLE hEvent;
	TAlertsQueue qAlertQueue;
} XGS_ALERTS_SHARED, *PXGS_ALERTS_SHARED;

typedef std::vector<PXGS_ALERTS_SHARED> TAlertsVector;

class CXenGuestAgent
{
private:
	OSVERSIONINFOEX  m_osvi;
	volatile LONG    m_ulXgsCount;
	HANDLE           m_hShutdownEvent;
	HANDLE           m_hTaskThread;
	UINT             m_uiTaskThreadId;
	bool             m_bLogCommunicationErrors;
	bool             m_bLogOperationErrors;
	CComBSTR         m_bstrUuid;
	CComBSTR         m_bstrDomId;
	CComBSTR         m_bstrInstallDir;
	TCHAR            m_bstrXcInstallDir[MAX_PATH+1];

	HANDLE           m_hVmsEvent;
	HANDLE           m_hVmsThread;
	UINT             m_uiVmsThreadId;
	HANDLE           m_hVmsLock;
	TVmsVector       m_vVmsShared;

	HANDLE           m_hAlertsEvent;
	HANDLE           m_hAlertsThread;
	UINT             m_uiAlertsThreadId;
	HANDLE           m_hAlertsLock;
	TAlertsVector    m_vAlertsShared;

public:
	CXenGuestAgent() : m_ulXgsCount(0),
	                   m_hShutdownEvent(NULL),
	                   m_hTaskThread(NULL),
	                   m_uiTaskThreadId(0),
	                   m_bLogCommunicationErrors(false),
	                   m_bLogOperationErrors(false),
	                   m_bstrUuid(L""),
	                   m_bstrDomId(L""),
	                   m_bstrInstallDir(L""),
	                   m_hVmsEvent(NULL),
	                   m_hVmsThread(NULL),
	                   m_uiVmsThreadId(0),
	                   m_hVmsLock(NULL),
	                   m_hAlertsThread(NULL),
	                   m_uiAlertsThreadId(0),
	                   m_hAlertsLock(NULL)
	{
		::ZeroMemory(&m_osvi, sizeof(OSVERSIONINFO));
		m_vVmsShared.clear();
		m_vAlertsShared.clear();
	}

	~CXenGuestAgent()
	{
	}

	const CComBSTR& GetInstallDir()
	{
		return m_bstrInstallDir;
	}

	HANDLE GetShutdownEvent()
	{
		return m_hShutdownEvent;
	}

	HANDLE GetAlertsLock()
	{
		return m_hAlertsLock;
	}

	VOID SetShutdownEvent()
	{
		if (m_hShutdownEvent != NULL)
			::SetEvent(m_hShutdownEvent);
	}

	const OSVERSIONINFOEX* GetOsInfo()
	{
		return &m_osvi;
	}

	bool Initialize();
	VOID Uninitialize();
	bool Start();

	BOOL ConfigurePluginAutorun(BOOL bEnable);
	STDMETHOD(TogglePluginAutorun)(BOOL vbToggle, BOOL* pvbAutorun);

	VOID LogEventTypeId(LPCTSTR tszFormat, WORD wType, DWORD dwEventId, va_list args);
	VOID LogEventTypeId(ULONG ulFormat, WORD wType, DWORD dwEventId, va_list args);
	VOID LogEventTypeId(ULONG ulFormat, WORD wType, DWORD dwEventId, ...);
	bool LogEventTypeIdLastError(ULONG ulFormat, WORD wType, DWORD dwEventId, va_list args);
	bool LogEventTypeIdLastError(ULONG ulFormat, WORD wType, DWORD dwEventId, ...);
	bool LogEventTypeIdLastRegistryError(ULONG ulFormat, WORD wType, DWORD dwEventId, LONG lRet, va_list args);
	bool LogEventTypeIdLastRegistryError(ULONG ulFormat, WORD wType, DWORD dwEventId, LONG lRet, ...);
	bool LogEventTypeIdLastErrorAlways(ULONG ulFormat, WORD wType, DWORD dwEventId, va_list args);
	bool LogEventTypeIdLastErrorAlways(ULONG ulFormat, WORD wType, DWORD dwEventId, ...);

	bool RegisterXgs();
	void UnregisterXgs();

	void AddVmsShared(XGS_VMS_SHARED *pVmsShared);
	void RemoveVmsShared(XGS_VMS_SHARED *pVmsShared);

	void AddAlertsShared(XGS_ALERTS_SHARED *pAlertsShared);
	void RemoveAlertsShared(XGS_ALERTS_SHARED *pAlertsShared);

	CXGSVM** LoadVms(DWORD *pdwCount);

	BOOL GetDbusShowSwitcherBarProperty(CHAR const * const pszVmUID, BOOL * const pbShowSwitcher, DWORD const dwEventId=EVMSG_GENERAL);

	void FreeVms(CXGSVM **ppVms, DWORD dwCount);

	const CComBSTR& GetUUID()
	{
		return m_bstrUuid;
	}

	const CComBSTR& GetDomID()
	{
		return m_bstrDomId;
	}

	inline void ConvertUuidToDbusFormat(CComBSTR& bstrUuid)
	{
		for (ULONG i = 0; i < bstrUuid.Length(); i++)
			if (bstrUuid[i] == L'-')
				bstrUuid[i] = L'_';
	}

private:
	static bool _registerForSignal(CXenGuestAgent *pxga, CXenHttp &rclXh, char *szRegPath, size_t sizeRegPath, std::string const &rstrDomId, char const *pszInterface, UINT &ruiRet);
	static bool _registerForSignals(CXenGuestAgent *pxga, CXenHttp &rclXh, std::string const &rstrDomId, UINT &ruiRet);
private:
	static UINT WINAPI _TaskThread(void* pv);
	static UINT WINAPI _VmsThread(void* pv);
	static UINT WINAPI _RunXcDiag(void *pv, UINT uiSigId);
	static UINT WINAPI _AlertsThread(void* pv);

	VOID SignalAllVmsEvents();
	VOID SignalAllAlertsEvents(SAFEARRAY* psa, XenAlertType exat, CXenGuestAgent *pxga);
    VOID UpdateMouseAcceleration(CXenGuestAgent *pxga);
};

extern CXenGuestAgent _XenGuestAgent;
