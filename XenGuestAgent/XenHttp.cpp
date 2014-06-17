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

#include "stdafx.h"
#include "resource.h"
#include "xgamsg.h"
#include "v4vapi.h"
#include "HttpSupport.h"
#include "XenHttp.h"

#define XHT_USER_AGENT        "XenGuestAgent/1.0"
#define XHT_DEFAULT_RING_SIZE 4096
#define XHT_DEFAULT_TIMEOUT   20000 // ms
#define XHT_NUMBER_BUF_SIZE   64

class CXenTransport
{
private:
	CXenGuestAgent *m_pclXga;
	V4V_CONTEXT     m_stContext;
	ULONG           m_ulRingSize;
	v4v_ring_id_t   m_stRingId;
    v4v_addr_t      m_stAddr;
	
public:
	CXenTransport(CXenGuestAgent *pclXga,
				  ULONG ulAddr,
				  ULONG ulPort,
				  ULONG ulRingSize)
	{
		m_pclXga = pclXga;
		::ZeroMemory(&m_stContext, sizeof(V4V_CONTEXT));
		m_stContext.flags = V4V_FLAG_NONE;
		m_ulRingSize = ulRingSize;
		m_stRingId.partner = (domid_t)ulAddr;
		m_stRingId.addr.domain = V4V_DOMID_NONE;
		m_stRingId.addr.port = V4V_PORT_NONE;
		m_stAddr.domain = (domid_t)ulAddr;
        m_stAddr.port = ulPort;
	}

	~CXenTransport()
	{
		this->Close();
	}

	BOOL Connect()
	{
		if (!V4vOpen(&m_stContext, m_ulRingSize, NULL))
			return FALSE;

		if (!V4vBind(&m_stContext, &m_stRingId, NULL))
			return FALSE;

		if (!V4vConnect(&m_stContext, &m_stAddr, NULL))
			return FALSE;
		
		return TRUE;
	}

	const V4V_CONTEXT *GetContext()
	{
		return &m_stContext;
	}

	void Close()
	{
		if (m_stContext.v4vHandle != NULL)
		{
			V4vClose(&m_stContext);
			::ZeroMemory(&m_stContext, sizeof(V4V_CONTEXT));
		}
		::SetLastError(ERROR_SUCCESS);
	}
};

CXenHttp::CXenHttp(CXenGuestAgent *pclXga) : m_clReq(pclXga), m_clResp(pclXga)
{
	m_pclXga = pclXga;
	m_pclXtrans = NULL; 
	m_psaData = NULL;
	m_szFront = new char[XHT_DEFAULT_RING_SIZE];
	m_bAccessed = false;
	m_bInUse = false;
	m_ulAddr = 0;
	m_ulPort = 80;
	m_ulRingSize = XHT_DEFAULT_RING_SIZE;
	m_ulTimeout = XHT_DEFAULT_TIMEOUT;
}

CXenHttp::~CXenHttp()
{
	CloseTransport();
	this->Cleanup();
}

void CXenHttp::SetDestination(ULONG ulAddr, ULONG ulPort)
{
	m_ulAddr = ulAddr;
	m_ulPort = ulPort;
}

void CXenHttp::SetRingSize(ULONG ulRingSize)
{
	m_ulRingSize = ulRingSize;
	delete [] m_szFront;
	m_szFront = new char[ulRingSize];
}

void CXenHttp::SetTimeout(ULONG ulTimeout)
{
	m_ulTimeout = ulTimeout;
}

DWORD CXenHttp::DoGET(const char *szResource,
					  const char *szParams,
					  HANDLE hWait1)
{
	return DoHttpRequest(HTTP_METHOD_GET,
						 szResource,
						 szParams,
						 NULL,
						 0,
						 hWait1);
}

DWORD CXenHttp::DoPOST(const char *szResource,
					   const char *szParams,
					   const BYTE *pbData,
					   ULONG ulSize,
					   HANDLE hWait1)
{
	return DoHttpRequest(HTTP_METHOD_POST,
						 szResource,
						 szParams,
						 pbData,
						 ulSize,
						 hWait1);
}

const CHttpRequest& CXenHttp::GetRequest()
{
	return m_clReq;
}

const CHttpResponse& CXenHttp::GetResponse()
{
	return m_clResp;
}

SAFEARRAY *CXenHttp::GetDataArray(bool bDetach)
{
	SAFEARRAY *psa = m_psaData;

	if (m_bAccessed)
		return NULL;

	if (bDetach)
		m_psaData = NULL;

	return psa;
}

const BYTE *CXenHttp::GetDataBuffer()
{
	HRESULT hr;
	BYTE *pbData = NULL;

	if (m_psaData == NULL)
	{
		::SetLastError(ERROR_INVALID_DATA);
		return NULL;
	}

	if (!m_bAccessed)
	{
		hr = ::SafeArrayAccessData(m_psaData, (LPVOID*)&pbData);
		if (FAILED(hr))
		{
			::SetLastError(ERROR_ACCESS_DENIED);
			return NULL;
		}
		m_bAccessed = true;
	}

	return pbData;
}

void CXenHttp::ReleaseDataBuffer()
{
	if (m_psaData == NULL)
		return;

	if (m_bAccessed)
	{
		::SafeArrayUnaccessData(m_psaData);
		m_bAccessed = false;
	}
}

DWORD CXenHttp::OpenTransport()
{
	DWORD dwRet;

	if (m_pclXtrans != NULL)
		CloseTransport();

	m_pclXtrans = new CXenTransport(m_pclXga, m_ulAddr, m_ulPort, m_ulRingSize);
	if (m_pclXtrans == NULL)
		return ERROR_OUTOFMEMORY;

	// First time, open the connection
	if (!m_pclXtrans->Connect())
	{
		dwRet = ::GetLastError();
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_CONNECT_TO_BACKEND___E_XENHTTP_213),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE, dwRet);
		return dwRet;
	}

	return ERROR_SUCCESS;
}

void CXenHttp::CloseTransport()
{
	if (m_pclXtrans != NULL)
	{
		m_pclXtrans->Close();
		delete m_pclXtrans;
		m_pclXtrans = NULL;
	}
}

void CXenHttp::Cleanup()
{
	m_clReq.Reset();
	m_clResp.Reset();

	if (m_szFront != NULL)
	{
		delete [] m_szFront;
		m_szFront = NULL;
	}

	ReleaseDataBuffer();

	if (m_psaData != NULL)
	{
		::SafeArrayDestroy(m_psaData);
		m_psaData = NULL;
	}

	m_bInUse = false;
}

void CXenHttp::Reset()
{
	this->Cleanup();
	m_szFront = new char[m_ulRingSize];
}

DWORD CXenHttp::DoHttpRequest(const char *szMethod,
							  const char *szResource,
							  const char *szParams,
							  const BYTE *pbReqData,
							  ULONG ulSize,
							  HANDLE hWait1)
{
	const char     *szRequest;
	DWORD           dwRet, dwStatus, dwToWrite, dwWritten, dwRead, dwRemaining;
	int             iRet;
	BOOL            bRet;
	HRESULT         hr;
	BYTE           *pbRespData = NULL;
	BYTE           *pbPtr;
	SAFEARRAYBOUND  stDim[1];
	HANDLE          harrWait[2];
	ULONG           ulHandles = 1;
	char            szLength[XHT_NUMBER_BUF_SIZE];

	if (m_szFront == NULL)
		return ERROR_OUTOFMEMORY;

	if (szResource == NULL)
		return ERROR_INVALID_PARAMETER;

	if (m_pclXtrans == NULL) {
		dwRet = OpenTransport();
		if (dwRet != ERROR_SUCCESS)
			return dwRet;
	}

	if (m_bInUse)
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_ATTEMPTING_TO_REUSE_CXENHTTP_WIT_XENHTTP_292),
								 EVENTLOG_ERROR_TYPE, EVMSG_CRITICAL_FAILURE);
		return ERROR_ACCESS_DENIED;
	}
	m_bInUse = true;

	// Create a GET or POST request
	m_clReq.HttpSetHeaderField(HTTP_USER_AGENT, XHT_USER_AGENT);
	if ((pbReqData != NULL)&&(ulSize > 0))
	{
		_ltoa_s(ulSize, szLength, XHT_NUMBER_BUF_SIZE - 1, 10);
		m_clReq.HttpSetHeaderField(HTTP_CONTENT_LENGTH, szLength);
	}
	szRequest = m_clReq.GenerateRequestHeader(szMethod,
											  szResource,
											  (szParams != NULL) ? szParams : "");
	if (szRequest == NULL)
		return ERROR_OUTOFMEMORY;

	harrWait[0] = m_pclXtrans->GetContext()->recvEvent;
	if ((hWait1 != NULL)&&(hWait1 != INVALID_HANDLE_VALUE))
	{
		harrWait[1] = hWait1;
		ulHandles++;
	}

	dwToWrite = (DWORD)strlen(szRequest);
	bRet = ::WriteFile(m_pclXtrans->GetContext()->v4vHandle,
					   (void*)szRequest,
					   dwToWrite,
					   &dwWritten,
					   NULL);
	if (!bRet)
	{
		dwRet = ::GetLastError();
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_SEND_zS___ERROR__zD_XENHTTP_327),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod), dwRet);
		return dwRet;
	}
	if (dwToWrite != dwWritten)
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_SEND_zS___NOT_ALL_DATA_XENHTTP_334),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod));
		return ERROR_WRITE_FAULT;
	}

	// If there is any request data to write, send it here
	if (pbReqData != NULL)
	{
		while (ulSize > 0)
		{
			dwToWrite = (ulSize >= m_ulRingSize) ? m_ulRingSize : ulSize;
			bRet = ::WriteFile(m_pclXtrans->GetContext()->v4vHandle,
							   (void*)pbReqData,
							   dwToWrite,
							   &dwWritten,
							   NULL);
			if (!bRet)
			{
				dwRet = ::GetLastError();
				m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_SEND_zS_DATA___ERROR___XENHTTP_354),
										 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
										 CA2T(szMethod), dwRet);
				return dwRet;
			}
			if (dwToWrite != dwWritten)
			{
				m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_SEND_zS_DATA___NOT_ALL_XENHTTP_361),
										 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
										 CA2T(szMethod));
				return ERROR_WRITE_FAULT;
			}

			if (ulSize >= m_ulRingSize)
			{
				ulSize -= m_ulRingSize;
				pbReqData += m_ulRingSize;
			}
			else
				ulSize = 0;
		}
	}

	dwStatus = ::WaitForMultipleObjects(ulHandles, harrWait, FALSE, m_ulTimeout);
	if (dwStatus == (WAIT_OBJECT_0 + 1))
	{
		// This is OK, the passed in handle was signalled - return to caller at this point.
		return ERROR_WAIT_1;
	}
	else if (dwStatus == WAIT_TIMEOUT)
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_zS_HEADER_TIMEOUT_AFTER_zD_MS_XENHTTP_385),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod), m_ulTimeout);
		return ERROR_TIMEOUT;
	}
	else if (dwStatus == WAIT_FAILED)
	{
		dwRet = ::GetLastError();
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILURE_DURING_WAITFORMULTIPLEOB_XENHTTP_393),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod), dwRet);
		return dwRet;
	}

	// Should be data to read
	bRet = ::ReadFile(m_pclXtrans->GetContext()->v4vHandle,
					  m_szFront,
					  m_ulRingSize,
					  &dwRead,
					  NULL);
	if (!bRet)
	{
		dwRet = ::GetLastError();
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_READ_zS_HEADER___ERROR_XENHTTP_408),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod), dwRet);
		return dwRet;
	}
	if (dwRead == 0)
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_READ_zS_HEADER___NO_DA_XENHTTP_415),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod));
		return ERROR_INVALID_DATA;
	}
	
	// Process the response header
	iRet = m_clResp.ProcessResponseHeader(m_szFront, dwRead);
	if (iRet != HTTP_NO_ERROR)
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_PROCESS_zS_RESPONSE____XENHTTP_425),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
								 CA2T(szMethod), iRet);
		return ERROR_INVALID_DATA;
	}

	if (m_clResp.m_ulContentLength == 0) // Done, just return
		return ERROR_SUCCESS;

	if (m_clResp.m_ulContentLength < (dwRead - m_clResp.m_ulHeaderLength))
	{
		// This is bogus
		m_pclXga->LogEventTypeId(ctxLS(IDS_INVALID_CONTENT_LENGTH_SPECIFIED_XENHTTP_437),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE);
		return ERROR_ACCESS_DENIED;
	}

	// More data, allocate a SAFEARRAY to hold it all and start fetching the rest.
	stDim[0].lLbound = 0;
	stDim[0].cElements = m_clResp.m_ulContentLength;
	m_psaData = ::SafeArrayCreate(VT_UI1, 1, &stDim[0]);
	if (m_psaData == NULL)
		return E_OUTOFMEMORY;

	hr = ::SafeArrayAccessData(m_psaData, (LPVOID*)&pbRespData);
	if (FAILED(hr))
	{
		m_pclXga->LogEventTypeId(ctxLS(IDS_SAFEARRAYACCESSDATA___FAILED___H_XENHTTP_452),
								 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE, hr);
		return ERROR_ACCESS_DENIED;
	}

	// Copy in any of the data already fetched
	pbPtr = pbRespData;
	memcpy(pbRespData, (m_szFront + m_clResp.m_ulHeaderLength), (dwRead - m_clResp.m_ulHeaderLength));
	pbPtr += (dwRead - m_clResp.m_ulHeaderLength);
	dwRemaining = m_clResp.m_ulContentLength - (dwRead - m_clResp.m_ulHeaderLength);

	dwRet = ERROR_SUCCESS;

	while (dwRemaining > 0) // may be nothing more to read
	{
		dwStatus = ::WaitForMultipleObjects(ulHandles, harrWait, FALSE, m_ulTimeout);
		if (dwStatus == (WAIT_OBJECT_0 + 1))
		{
			// This is OK, the passed in handle was signalled - return to caller at this point.
			dwRet = ERROR_WAIT_1;
			break;
		}
		else if (dwStatus == WAIT_TIMEOUT)
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_zS_DATA_TIMEOUT_AFTER_zD_MS_XENHTTP_476),
									 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
									 CA2T(szMethod), m_ulTimeout);
			dwRet = ERROR_TIMEOUT;
			break;
		}
		else if (dwStatus == WAIT_FAILED)
		{
			dwRet = ::GetLastError();
			m_pclXga->LogEventTypeId(ctxLS(IDS_FAILURE_DURING_WAITFORMULTIPLEOB_XENHTTP_485),
									 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
									 CA2T(szMethod), dwRet);
			break;
		}

		bRet = ::ReadFile(m_pclXtrans->GetContext()->v4vHandle,
						  pbPtr,
						  dwRemaining,
						  &dwRead,
						  NULL);
		if (!bRet)
		{
			dwRet = ::GetLastError();
			m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_READ_zS_DATA___ERROR___XENHTTP_499),
									 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
									 CA2T(szMethod), dwRet);
			break;
		}
		if (dwRead == 0)
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_FAILED_TO_READ_zS_DATA___NO_DATA_XENHTTP_506),
									 EVENTLOG_ERROR_TYPE, EVMSG_COMMUNICATION_FAILURE,
									 CA2T(szMethod));
			dwRet = ERROR_INVALID_DATA;
			break;
		}
		if (dwRead > dwRemaining)
		{
			m_pclXga->LogEventTypeId(ctxLS(IDS_CRITICAL_FAILURE___zS_VALUE_READ_XENHTTP_514),
									 EVENTLOG_ERROR_TYPE, EVMSG_CRITICAL_FAILURE,
									 CA2T(szMethod));
			dwRet = ERROR_INVALID_DATA;
			break;
		}
		pbPtr += dwRead;
		dwRemaining -= dwRead;
	}

	::SafeArrayUnaccessData(m_psaData);
	return dwRet;
}
