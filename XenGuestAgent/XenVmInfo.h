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

// XenVmInfo.h : Declaration of the CXenVmInfo

#pragma once
#include "resource.h"       // main symbols

#include "XenGuestAgent_i.h"

#define XEN_DOMID_INVALID (0x7FFFU)
#define XEN_SLOT_INVALID  (0xFFFFFFFFUL)

// CXenVmInfo

class ATL_NO_VTABLE CXenVmInfo :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CXenVmInfo, &CLSID_XenVmInfo>,
	public ISupportErrorInfo,
	public IXenVmInitialize,
	public IDispatchImpl<IXenVmInfo, &IID_IXenVmInfo, &LIBID_XenGuestAgentLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
private:
	CComBSTR m_bstrUuid;
	USHORT   m_usDomId;
	CComBSTR m_bstrName;
	ULONG    m_ulSlot;

	CComVariant m_vbHidden;
	CComVariant m_vbUivm;
	CComVariant m_vbHasImage;
	CComVariant m_vbSwitcherShown;

	SAFEARRAY *m_psaImage;

public:
	CXenVmInfo() : m_bstrUuid(L"00000000-0000-0000-0000-000000000000"),
				   m_usDomId(XEN_DOMID_INVALID),
				   m_bstrName(L""),
				   m_ulSlot(XEN_SLOT_INVALID),
				   m_psaImage(NULL)
	{
		m_vbHidden = CComVariant(VARIANT_FALSE);
		m_vbUivm = CComVariant(VARIANT_FALSE);
		m_vbHasImage = CComVariant(VARIANT_FALSE);
		m_vbSwitcherShown = CComVariant(VARIANT_TRUE); // Default to show switcher bar.
	}

DECLARE_REGISTRY_RESOURCEID(IDR_XENVMINFO)


BEGIN_COM_MAP(CXenVmInfo)
	COM_INTERFACE_ENTRY(IXenVmInfo)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IXenVmInitialize)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
		if (m_psaImage != NULL)
			::SafeArrayDestroy(m_psaImage);
		m_psaImage = NULL;
	}

	STDMETHOD(InitializeVmInfo)(BSTR bstrUuid,
								USHORT usDomId,
								BSTR bstrName,
								ULONG ulSlot, 
								VARIANT_BOOL vbHidden,
								VARIANT_BOOL vbUivm,
								VARIANT_BOOL vbSwitcherShown);
	STDMETHOD(InitializeVmImage)(SAFEARRAY *psa);

public:

	STDMETHOD(GetUuid)(BSTR* pbstrUuid);
	STDMETHOD(GetDomId)(USHORT* pusDomId);
	STDMETHOD(GetName)(BSTR* pbstrName);
	STDMETHOD(GetSlot)(ULONG* pulSlot);
	STDMETHOD(IsHidden)(VARIANT_BOOL* pvbHidden);
	STDMETHOD(IsUivm)(VARIANT_BOOL* pvbUivm);
	STDMETHOD(GetImage)(SAFEARRAY** ppsaImage);
	STDMETHOD(HasImage)(VARIANT_BOOL* pvbHasImage);
	STDMETHOD(IsSwitcherShown)(VARIANT_BOOL* pvbSwitcherShown);
};

OBJECT_ENTRY_AUTO(__uuidof(XenVmInfo), CXenVmInfo)
