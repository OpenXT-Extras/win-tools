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

// XenGuestServices.cpp : Implementation of CXenGuestServices

#include "stdafx.h"
#include <sys/stat.h>
#include "xgamsg.h"
#include "XenGuestServices.h"
#include "XenSecurityHelper.h"
#include "XenHttp.h"
#include "HttpSupport.h"
#include <json/json.h>

HRESULT CXenGuestServices::FinalConstruct()
{
	HRESULT hr = S_OK;

	do {
		m_hLock = ::CreateMutex(NULL, FALSE, NULL);
		if (m_hLock == NULL)
		{
			hr = LogCreateFailure((IDS_FAILED_TO_CREATE_OBJECT_LOCK___H_XENGUESTSERVICES_24),
								  E_OUTOFMEMORY);
			break;
		}

		// Open an XS instance for use by this object
		if (!m_clXs.XS2Open())
		{
			hr = LogCreateFailure((IDS_FAILED_TO_OPEN_XENSTORE___HRESUL_XENGUESTSERVICES_32),
								  E_FAIL);
			break;
		}

		m_clXh.SetRingSize(4096*6);

		// Open a HTTP/V4V connection to be used by this object
		// Causes plugin to crash when V4V is not present (immediately after install)
		/*if (m_clXh.OpenTransport() != ERROR_SUCCESS)
		{
			hr = LogCreateFailure(_T("Failed to open V4V channel - HRESULT: 0x%x"),
								  E_FAIL);
			break;
		}	*/	

		// Initialize unchanging VM values
		hr = InitializeVmValues();
		if (FAILED(hr))
		{
			// Can't continue without these basic values.
			LogCreateFailure((IDS_FAILED_TO_INITIALIZE_VM_VALUES___XENGUESTSERVICES_53), hr);
			break;
		}

		// Register this object - if it fails the global count is exceeded
		if (!m_pclXga->RegisterXgs())
		{
			hr = E_ACCESSDENIED;
			LogCreateFailure((IDS_MAXIMUM_INSTANCE_COUNT_REACHED___XENGUESTSERVICES_61), hr);
		}
	} while (false);

	if (FAILED(hr))
		FreeResources();

	return hr;
}

void CXenGuestServices::FinalRelease()
{
	// Unregister from any event shared lists
	m_pclXga->RemoveVmsShared(&m_stVmsShared);
	m_pclXga->RemoveAlertsShared(&m_stAlertsShared);

	// Drop out of the main XenGuestAgent list
	m_pclXga->UnregisterXgs();

	m_clXh.CloseTransport();
	m_clXs.XS2Close();

	FreeVmList();

	FreeResources();
}

HRESULT CXenGuestServices::LogCreateFailure(ULONG ulMsg, HRESULT hr)
{
	m_pclXga->LogEventTypeId(ulMsg, EVENTLOG_ERROR_TYPE, EVMSG_CREATION_FAILURE, hr);
	return hr;
}

HRESULT CXenGuestServices::InitializeVmValues()
{
	CXGSVM **ppVms = NULL;
	DWORD dwCount = 0, i;
	bool found = false;

	// This is our UUID
	m_bstrUuid = m_pclXga->GetUUID();

	// Load a list of VMs. Regardless of whether the list happens
	// to change while it is being read, this VM's information should
	// always be present in the read part. E.g. if other VMs started
	// while reading, they will be enumerated at the end.
	dwCount = 0;
	ppVms = m_pclXga->LoadVms(&dwCount);
	if (ppVms == NULL)
		return E_FAIL;

	for (i = 0; i < dwCount; i++)
	{
		if ((ppVms[i]->m_bstrUuid == m_bstrUuid)&&
			(!ppVms[i]->m_bInvalid))
		{
			// Ours, save some values
			m_usDomId = ppVms[i]->m_usDomId;
			m_bShowSwitcher = ppVms[i]->m_bShowSwitcher;
			found = true;
			break;
		}
	}

	m_pclXga->FreeVms(ppVms, dwCount);
	if (!found)
		return E_FAIL;

	return S_OK;
}

bool CXenGuestServices::CreateVmList(ULONG ulCount)
{
	if (ulCount == 0)
		return false;

	m_ppiVmList = (IXenVmInfo**)::CoTaskMemAlloc(ulCount*sizeof(IXenVmInfo*));
	if (m_ppiVmList == NULL)
		return false;

	ZeroMemory(m_ppiVmList, ulCount*sizeof(IXenVmInfo*));
	m_ulVmCount = ulCount;

	return true;
}

VOID CXenGuestServices::FreeVmList()
{
	ULONG i;

	for (i = 0; i < m_ulVmCount; i++)
	{
		if (m_ppiVmList[i] != NULL)
			m_ppiVmList[i]->Release();
	}

	m_ulVmCount = 0;
	::CoTaskMemFree((LPVOID)m_ppiVmList);
	m_ppiVmList = NULL;
}

VOID CXenGuestServices::FreeResources()
{
	if (m_hLock != NULL)
	{
		::CloseHandle(m_hLock);
		m_hLock = NULL;
	}

	if (m_hAlertsEvent != NULL)
	{
		::CloseHandle(m_hAlertsEvent);
		m_hAlertsEvent = NULL;
	}

	if (m_hVmsEvent != NULL)
	{
		::CloseHandle(m_hVmsEvent);
		m_hVmsEvent = NULL;
	}
}

bool CXenGuestServices::CheckVmFocus()
{
	LPSTR pszVal;
	bool rc;

	pszVal = (LPSTR)m_clXs.XS2Read("switcher/have_focus", NULL);
	if (pszVal == NULL)
		return true;

	if ((USHORT)strtol(pszVal, NULL, 10) != 0)
		rc = true;
	else
		rc = false;

	m_clXs.XS2Free(pszVal);

	return rc;
}

HRESULT CXenGuestServices::LoadImageFile(IXenVmInitialize *piVmInit,
										 const CComBSTR &bstrImage)
{
	SAFEARRAY *psaImage = NULL;
	SAFEARRAYBOUND stDim[1];
	HRESULT hr = S_OK;
	FILE *fh = NULL;
	errno_t err;
	struct _stat finfo;
	BYTE *pbData = NULL;

	// NOTE: This is the old routine for reading the images from files
	// and we are currently not using it.
	err = _wstat((LPCWSTR)bstrImage, &finfo);
	if (err != 0)
		return E_INVALIDARG;

	stDim[0].lLbound = 0;
	stDim[0].cElements = finfo.st_size;
	psaImage = ::SafeArrayCreate(VT_UI1, 1, &stDim[0]);
	if (psaImage == NULL)
		return E_OUTOFMEMORY;

	hr = ::SafeArrayAccessData(psaImage, (LPVOID*)&pbData);
	if (FAILED(hr))
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_SAFEARRAYACCESSDATA___FAILED___H_XENGUESTSERVICES_230),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, hr);
		::SafeArrayDestroy(psaImage);
		return hr;
	}
	if (pbData == NULL)
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_SAFEARRAYACCESSDATA___RETRIEVED__XENGUESTSERVICES_237),
								 EVENTLOG_WARNING_TYPE, EVMSG_OPERATION_FAILURE);
		::SafeArrayDestroy(psaImage);
		hr = S_FALSE;
		return hr;
	}

	do {
		err = _wfopen_s(&fh, (LPCWSTR)bstrImage, L"rb");
		if (err != 0)
		{
			hr = E_INVALIDARG;
			break;
		}
		if (fh == NULL)
		{
			hr = E_INVALIDARG;
			break;
		}

		err = fread(pbData, finfo.st_size, 1, fh);
		if (err < 1)
		{
			hr = E_FAIL;
			break;
		}		
	} while (false);

	if (fh != NULL)
		fclose(fh);
	::SafeArrayUnaccessData(psaImage);

	// Detach underlying array and let the VM info object own it (or ditch it if things failed
	if (SUCCEEDED(hr))
		piVmInit->InitializeVmImage(psaImage);
	else
		::SafeArrayDestroy(psaImage);

	return hr;
}

HRESULT CXenGuestServices::LoadImageUid(IXenVmInitialize *piVmInit,
										 const CComBSTR &bstrImage)
{
	errno_t err;
	CComBSTR bstrFile;

	// No image, not a problem
	if (bstrImage.Length() == 0)
		return S_OK;

    std::wstring strFile(bstrImage);

    if (strFile[0] != '/')
	    bstrFile += L"/";
	bstrFile += bstrImage;

	err = m_clXh.DoGET(CW2A(bstrFile), NULL, m_pclXga->GetShutdownEvent());
	if (err == ERROR_SUCCESS && m_clXh.GetResponse().m_iHttpCode == 200)
		piVmInit->InitializeVmImage(m_clXh.GetDataArray(true));

	else if (err == ERROR_WAIT_1)
	{
		//hShutdownEvent was signalled
		m_pclXga->LogEventTypeId(ctxLS(IDS_XS_WATCH_THREAD_SHUTTING_DOWN__XENGUESTSERVICES_301),
			EVENTLOG_INFORMATION_TYPE, EVMSG_GENERAL);
	}
	else
	{
		// Allow client to continue with default images even though this was a 
		// communication error that will get logged.
		m_pclXga->LogEventTypeId(ctxLS(IDS_DOGET___FAILED___ERROR__zD_HTTP__XENGUESTSERVICES_308),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE,
								 err, m_clXh.GetResponse().m_iHttpCode);
	}

	m_clXh.Reset();

	return S_OK;
}

STDMETHODIMP CXenGuestServices::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IXenGuestServices
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

STDMETHODIMP CXenGuestServices::GetUuid(BSTR* pbstrUuid)
{
	// Effectively readonly after initialization
	*pbstrUuid = m_bstrUuid.Copy();
	return S_OK;
}

STDMETHODIMP CXenGuestServices::GetDomId(USHORT* pusDomId)
{
	// Effectively readonly after initialization
	*pusDomId = m_usDomId;
	return S_OK;
}

STDMETHODIMP CXenGuestServices::QueryVms(ULONG* pulCount)
{
	HRESULT hr = S_OK;
	CXGSVM **ppVms = NULL;
	DWORD dwCount = 0, i;
	CComPtr<IXenVmInfo> spiVmInfo;
	IXenVmInitialize *piVmInit = NULL;

	*pulCount = 0;
	ppVms = m_pclXga->LoadVms(&dwCount);
	if (ppVms == NULL)
		return E_FAIL;

	::WaitForSingleObject(m_hLock, INFINITE);

	// Dump existing list
	FreeVmList();

	if (!CreateVmList(dwCount))
	{
		::ReleaseMutex(m_hLock);
		m_pclXga->FreeVms(ppVms, dwCount);
		return E_OUTOFMEMORY;
	}

	// Got at least one back - there has to always be at least on - i.e. us.
	for (i = 0; i < dwCount; i++)
	{
		hr = spiVmInfo.CoCreateInstance(CLSID_XenVmInfo); // 1
		if (FAILED(hr))
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_COCREATEINSTANCE_CLSID_XENVMINFO_XENGUESTSERVICES_378),
									 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, hr);
			break;
		}
		hr = spiVmInfo.QueryInterface<IXenVmInitialize>(&piVmInit); // 2
		if (FAILED(hr))
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_QUERYINTERFACE_IXENVMINITIALIZE__XENGUESTSERVICES_385),
									 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, hr);
			break;
		}
		if (piVmInit == NULL)
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_QUERYINTERFACE_IXENVMINITIALIZE__XENGUESTSERVICES_391),
									 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, hr);
			break;
		}

		piVmInit->InitializeVmInfo(ppVms[i]->m_bstrUuid,
								   ppVms[i]->m_usDomId,
								   ppVms[i]->m_bstrName,
								   ppVms[i]->m_ulSlot,
								   (ppVms[i]->m_bHidden ? VARIANT_TRUE : VARIANT_FALSE),
								   (ppVms[i]->m_bUivm ? VARIANT_TRUE : VARIANT_FALSE),
								   (ppVms[i]->m_bShowSwitcher ? VARIANT_TRUE : VARIANT_FALSE));

		hr = LoadImageUid(piVmInit, ppVms[i]->m_bstrImage);
		if (FAILED(hr))
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_LOADIMAGEUID_FAILED__IMAGE__zS___XENGUESTSERVICES_406),
									 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE,
									 COLE2T((BSTR)ppVms[i]->m_bstrImage), hr);
			break;
		}
		
		piVmInit->Release(); // 1
		m_ppiVmList[i] = spiVmInfo.Detach(); // 1
	}

	if (SUCCEEDED(hr))
		*pulCount = m_ulVmCount;
	else
		FreeVmList();

	::ReleaseMutex(m_hLock);

	m_pclXga->FreeVms(ppVms, dwCount);
	
	return S_OK;
}

STDMETHODIMP CXenGuestServices::GetVmObject(ULONG ulIndex, IXenVmInfo ** ppiVmInfo)
{
	if (ulIndex >= m_ulVmCount)
		return E_INVALIDARG;

	::WaitForSingleObject(m_hLock, INFINITE);
	*ppiVmInfo = m_ppiVmList[ulIndex];

	(*ppiVmInfo)->AddRef();
	::ReleaseMutex(m_hLock);

	return S_OK;
}

STDMETHODIMP CXenGuestServices::SwitchToVm(ULONG ulSlot)
{
#define XGS_SWITCH_LEN 32
	char szData[XGS_SWITCH_LEN + 1];

	// Don't allow this VM to switch if it does not have focus
	if (!CheckVmFocus())
		return S_FALSE;

	_snprintf_s(szData, XGS_SWITCH_LEN, _TRUNCATE, "switch slot %d", ulSlot);
	if (!m_clXs.XS2Write("switcher/command", szData))
		return E_FAIL;

	return S_OK;
}


STDMETHODIMP CXenGuestServices::VmHasFocus(VARIANT_BOOL* pvbFocus)
{
	*pvbFocus = (CheckVmFocus() ? VARIANT_TRUE : VARIANT_FALSE);
	return S_OK;
}

STDMETHODIMP CXenGuestServices::RegisterVmsEvent(BSTR* pbstrEventName)
{
	HRESULT hr;
	CXenSecurityHelper clXsh(m_pclXga);
	SECURITY_ATTRIBUTES *psa = NULL;

	if (m_bVmsRegistered)
		return E_ABORT;

	hr = ::CoCreateGuid(&m_gVmsEvent);
	if (FAILED(hr))
	{
		LogCreateFailure((IDS_FAILED_TO_CREATE_VMS_EVENT_GUID__XENGUESTSERVICES_474), hr);
		return hr;
	}

	// Make string out of the GUID
	m_bstrVmsEvent = L"Global\\";
	m_bstrVmsEvent += CComBSTR(m_gVmsEvent);

	// Get security attributes for allowing interactive user access to events.
	psa = clXsh.CreateXgaSecurityAttributes();
	if (psa == NULL)
		return E_UNEXPECTED; // errrors logged in call

	// Create named events for VMS
	m_hVmsEvent = ::CreateEventW(psa, FALSE, FALSE, COLE2T((BSTR)m_bstrVmsEvent));
	clXsh.FreeXgaSecurityAttributes(psa); // done with this
	if (m_hVmsEvent == NULL)
	{
		hr = LogCreateFailure((IDS_FAILED_TO_CREATE_VMS_EVENT___HRE_XENGUESTSERVICES_492),
							  E_OUTOFMEMORY);			
		return E_OUTOFMEMORY;
	}

	// Final step, register with the main XenGuestAgent instance.
	m_stVmsShared.hEvent = m_hVmsEvent;
	m_pclXga->AddVmsShared(&m_stVmsShared);
	*pbstrEventName = m_bstrVmsEvent.Copy();
	m_bVmsRegistered = true;

	return S_OK;
}

STDMETHODIMP CXenGuestServices::UnregisterVmsEvent()
{
	m_pclXga->RemoveVmsShared(&m_stVmsShared);

	if (m_hVmsEvent != NULL)
	{
		::CloseHandle(m_hVmsEvent);
		m_hVmsEvent = NULL;
	}

	m_bVmsRegistered = false;
	return S_OK;
}

STDMETHODIMP CXenGuestServices::RegisterAlertsEvent(BSTR* pbstrEventName)
{
	HRESULT hr;
	CXenSecurityHelper clXsh(m_pclXga);
	SECURITY_ATTRIBUTES *psa = NULL;

	if (m_bAlertsRegistered)
		return E_ABORT;

	hr = ::CoCreateGuid(&m_gAlertsEvent);
	if (FAILED(hr))
	{
		LogCreateFailure((IDS_FAILED_TO_CREATE_ALERTS_EVENT_GU_XENGUESTSERVICES_532), hr);
		return hr;
	}

	// Make string out of the GUID
	m_bstrAlertsEvent = L"Global\\";
	m_bstrAlertsEvent += CComBSTR(m_gAlertsEvent);

	// Get security attributes for allowing interactive user access to events.
	psa = clXsh.CreateXgaSecurityAttributes();
	if (psa == NULL)
		return E_UNEXPECTED; // errrors logged in call

	// Create named events for VMS
	m_hAlertsEvent = ::CreateEventW(psa, FALSE, FALSE, COLE2T((BSTR)m_bstrAlertsEvent));
	clXsh.FreeXgaSecurityAttributes(psa); // done with this
	if (m_hAlertsEvent == NULL)
	{
		hr = LogCreateFailure((IDS_FAILED_TO_CREATE_ALERTS_EVENT____XENGUESTSERVICES_550),
							  E_OUTOFMEMORY);			
		return E_OUTOFMEMORY;
	}

	// Final step, register with the main XenGuestAgent instance.
	m_stAlertsShared.hEvent = m_hAlertsEvent;
	// TODO other pieces of alerts shared struct
	m_hAlertsLock = m_pclXga->GetAlertsLock();
	m_pclXga->AddAlertsShared(&m_stAlertsShared);
	*pbstrEventName = m_bstrAlertsEvent.Copy();
	m_bAlertsRegistered = true;

	return S_OK;
}

STDMETHODIMP CXenGuestServices::UnregisterAlertsEvent()
{
	m_pclXga->RemoveAlertsShared(&m_stAlertsShared);

	if (m_hAlertsEvent != NULL)
	{
		::CloseHandle(m_hAlertsEvent);
		m_hAlertsEvent = NULL;
	}

	m_bVmsRegistered = false;
	return S_OK;
}

STDMETHODIMP CXenGuestServices::GetNextAlert(SAFEARRAY** ppsaParams, XenAlertType* penAlertType)
{
	::WaitForSingleObject(m_hAlertsLock, INFINITE);
	if (m_stAlertsShared.qAlertQueue.empty())
	{
		*ppsaParams = NULL;
		*penAlertType = XenAlertEmpty;
	}
	else
	{
		XGS_ALERT &xa = m_stAlertsShared.qAlertQueue.front();
		*ppsaParams = xa.psa;
		*penAlertType = xa.xaType;
		m_stAlertsShared.qAlertQueue.pop();
	}
	::ReleaseMutex(m_hAlertsLock);
	return S_OK;
}
STDMETHODIMP CXenGuestServices::TogglePluginAutorun(VARIANT_BOOL vbToggle, VARIANT_BOOL* pvbAutorun)
{
	BOOL const bToggle = vbToggle == VARIANT_FALSE ? FALSE : TRUE;
	BOOL bAutorun = TRUE;

	if (pvbAutorun != NULL)
	{
		bAutorun = *pvbAutorun == VARIANT_FALSE ? FALSE : TRUE;
	}

	HRESULT res = m_pclXga->TogglePluginAutorun(bToggle, &bAutorun);

	if (pvbAutorun != NULL)
	{
		*pvbAutorun = (bAutorun == FALSE ? VARIANT_FALSE : VARIANT_TRUE);
	}

	return res;
}


STDMETHODIMP CXenGuestServices::SetAcceleration(ULONG ulAcceleration)
{
    HANDLE hXenInp;
    DWORD dwOut;
    XENINP_ACCELERATION     xaMouAccel;

    hXenInp = CreateFileW(XENINP_USER_FILE_NAME, GENERIC_READ|GENERIC_WRITE,
                     FILE_SHARE_READ|FILE_SHARE_WRITE, NULL, OPEN_EXISTING,
                     FILE_ATTRIBUTE_NORMAL, NULL);
    if (hXenInp == INVALID_HANDLE_VALUE)
    {
        m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_OPEN_XENINP_FILE_HANDL_XENGUESTSERVICES_674),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
    }
    else
    {
        xaMouAccel.Acceleration = ulAcceleration;

        if (!DeviceIoControl(hXenInp, XENINP_IOCTL_ACCELERATION, &xaMouAccel, 
            sizeof(XENINP_ACCELERATION), NULL, 0, &dwOut, NULL))
            m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_SEND_XENINP_IOCTL_ACCE_XENGUESTSERVICES_683),
								 EVENTLOG_ERROR_TYPE, EVMSG_OPERATION_FAILURE, ::GetLastError());
        CloseHandle(hXenInp);
    }

    return S_OK;
}

STDMETHODIMP CXenGuestServices::GetShowSwitcher(VARIANT_BOOL* pbShowSwitcher)
{
	*pbShowSwitcher = m_bShowSwitcher == FALSE ? VARIANT_FALSE : VARIANT_TRUE;
	return S_OK;
}
