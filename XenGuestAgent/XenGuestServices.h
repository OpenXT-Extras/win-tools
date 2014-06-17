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

#include "XenGuestAgent_i.h"
#include "XenGuestAgent.h"
#include "XenStoreWrapper.h"
#include "XenVmInfo.h"
#include "XenHttp.h"

// CXenGuestServices

class ATL_NO_VTABLE CXenGuestServices :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CXenGuestServices, &CLSID_XenGuestServices>,
	public ISupportErrorInfo,
	public IDispatchImpl<IXenGuestServices, &IID_IXenGuestServices, &LIBID_XenGuestAgentLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
private:
	GUID         m_gVmsEvent;
	GUID         m_gAlertsEvent;
	CComBSTR     m_bstrVmsEvent;
	CComBSTR     m_bstrAlertsEvent;
	HANDLE       m_hVmsEvent;
	HANDLE       m_hAlertsEvent;
	HANDLE       m_hHookFile;
	bool         m_bVmsRegistered;
	bool         m_bAlertsRegistered;
	HANDLE       m_hLock;
	CComBSTR     m_bstrUuid;
	USHORT       m_usDomId;
	BOOL         m_bShowSwitcher;
	IXenVmInfo **m_ppiVmList;
	ULONG        m_ulVmCount;

	CXenGuestAgent    *m_pclXga;
	CXenStoreWrapper   m_clXs;
	CXenHttp           m_clXh;
	XGS_VMS_SHARED     m_stVmsShared;
	XGS_ALERTS_SHARED  m_stAlertsShared;
	HANDLE			   m_hAlertsLock;

public:
	CXenGuestServices() : m_bstrVmsEvent(L""),
						  m_bstrAlertsEvent(L""),
						  m_hVmsEvent(NULL),
						  m_hAlertsEvent(NULL),
						  m_hHookFile(NULL),
						  m_bVmsRegistered(false),
						  m_bAlertsRegistered(false),
						  m_hLock(NULL),
						  m_bstrUuid(L"?"),
						  m_usDomId(XEN_DOMID_INVALID),
						  m_bShowSwitcher(TRUE),
						  m_ppiVmList(NULL),
						  m_ulVmCount(0),
						  m_pclXga(&_XenGuestAgent),
						  m_clXh(&_XenGuestAgent)
	{
		m_gVmsEvent = GUID_NULL;
		m_gAlertsEvent = GUID_NULL;
		::ZeroMemory(&m_stVmsShared, sizeof(XGS_VMS_SHARED));
	}

DECLARE_REGISTRY_RESOURCEID(IDR_XENGUESTSERVICES)


BEGIN_COM_MAP(CXenGuestServices)
	COM_INTERFACE_ENTRY(IXenGuestServices)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	HRESULT LogCreateFailure(ULONG ulMsg, HRESULT hr);
	HRESULT InitializeVmValues();
	bool CreateVmList(ULONG ulCcount);
	VOID FreeVmList();
	VOID FreeResources();
	bool CheckVmFocus();
	HRESULT LoadImageFile(IXenVmInitialize *piVmInit, const CComBSTR &bstrImage);
	HRESULT LoadImageUid(IXenVmInitialize *piVmInit, const CComBSTR &bstrImage);

public:

	STDMETHOD(GetUuid)(BSTR* pbstrUuid);
	STDMETHOD(GetDomId)(USHORT* pusDomId);
	STDMETHOD(QueryVms)(ULONG* pulCount);
	STDMETHOD(GetVmObject)(ULONG ulIndex, IXenVmInfo ** ppiVmInfo);
	STDMETHOD(SwitchToVm)(ULONG ulSlot);
	STDMETHOD(VmHasFocus)(VARIANT_BOOL* pvbFocus);
	STDMETHOD(RegisterVmsEvent)(BSTR* pbstrEventName);
	STDMETHOD(UnregisterVmsEvent)();
	STDMETHOD(RegisterAlertsEvent)(BSTR* pbstrEventName);
	STDMETHOD(UnregisterAlertsEvent)();
	STDMETHOD(GetNextAlert)(SAFEARRAY** ppsaParams, XenAlertType* penAlertType);
	STDMETHOD(TogglePluginAutorun)(VARIANT_BOOL vbToggle, VARIANT_BOOL* pvbAutorun);
	STDMETHOD(SetAcceleration)(ULONG ulAcceleration);
	STDMETHOD(GetShowSwitcher)(VARIANT_BOOL* pbShowSwitcher);
};

OBJECT_ENTRY_AUTO(__uuidof(XenGuestServices), CXenGuestServices)
