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

// XenVmInfo.cpp : Implementation of CXenVmInfo

#include "stdafx.h"
#include "XenVmInfo.h"


// CXenVmInfo

STDMETHODIMP CXenVmInfo::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IXenVmInfo
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

STDMETHODIMP CXenVmInfo::InitializeVmInfo(BSTR bstrUuid,
										  USHORT usDomId,
										  BSTR bstrName,
										  ULONG ulSlot, 
										  VARIANT_BOOL vbHidden,
										  VARIANT_BOOL vbUivm,
										  VARIANT_BOOL vbSwitcherShown)
{
	CComVariant hidden(vbHidden);
	CComVariant svm(vbHidden);

	m_bstrUuid = bstrUuid;
	m_usDomId = usDomId;
	m_bstrName = bstrName;
	m_ulSlot = ulSlot;
	m_vbHidden = vbHidden;
	m_vbUivm = vbUivm;
	m_vbSwitcherShown = vbSwitcherShown;

	return S_OK;
}

STDMETHODIMP CXenVmInfo::InitializeVmImage(SAFEARRAY *psa)
{
	if (psa != NULL)
	{
		m_psaImage = psa;
		m_vbHasImage = VARIANT_TRUE;
	}
	return S_OK;
}

STDMETHODIMP CXenVmInfo::GetUuid(BSTR* pbstrUuid)
{
	*pbstrUuid = m_bstrUuid.Copy();
	return S_OK;
}

STDMETHODIMP CXenVmInfo::GetDomId(USHORT* pusDomId)
{
	*pusDomId = m_usDomId;
	return S_OK;
}

STDMETHODIMP CXenVmInfo::GetName(BSTR* pbstrName)
{
	*pbstrName = m_bstrName.Copy();
	return S_OK;
}

STDMETHODIMP CXenVmInfo::GetSlot(ULONG* pulSlot)
{
	*pulSlot = m_ulSlot;
	return S_OK;
}

STDMETHODIMP CXenVmInfo::IsHidden(VARIANT_BOOL* pvbHidden)
{
	*pvbHidden = m_vbHidden.boolVal;
	return S_OK;
}

STDMETHODIMP CXenVmInfo::IsUivm(VARIANT_BOOL* pvbUivm)
{
	*pvbUivm = m_vbUivm.boolVal;
	return S_OK;
}

STDMETHODIMP CXenVmInfo::GetImage(SAFEARRAY** ppsaImage)
{
	SAFEARRAY *psaImage = NULL;
	HRESULT hr;

	if (ppsaImage == NULL)
		return E_INVALIDARG;

	hr = ::SafeArrayCopy(m_psaImage, &psaImage);
	if (FAILED(hr))
		return hr;

	if (psaImage == NULL)
		return E_OUTOFMEMORY;

	*ppsaImage = psaImage;
	return S_OK;
}

STDMETHODIMP CXenVmInfo::HasImage(VARIANT_BOOL* pvbHasImage)
{
	*pvbHasImage = m_vbHasImage.boolVal;
	return S_OK;
}

STDMETHODIMP CXenVmInfo::IsSwitcherShown(VARIANT_BOOL* pvbSwitcherShown)
{
	*pvbSwitcherShown = m_vbSwitcherShown.boolVal;
	return S_OK;
}

