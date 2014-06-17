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


#pragma once
#include "resource.h"       // main symbols
#include "HttpSupport.h"
#include "XenGuestAgent.h"

class CXenTransport;

class CXenHttp
{
private:
	CXenGuestAgent *m_pclXga;
	CXenTransport  *m_pclXtrans;
	SAFEARRAY      *m_psaData;
	char           *m_szFront;
	bool            m_bAccessed;
	bool            m_bInUse;
	ULONG           m_ulAddr;
	ULONG           m_ulPort;
	ULONG           m_ulRingSize;
	ULONG           m_ulTimeout;
	CHttpRequest    m_clReq;
	CHttpResponse   m_clResp;
	
public:
	CXenHttp(CXenGuestAgent *pclXga);
	~CXenHttp();

	void SetDestination(ULONG ulAddr, ULONG ulPort);
	void SetRingSize(ULONG ulRingSize);
	void SetTimeout(ULONG ulTimeout);

	DWORD DoGET(const char *szResource,
				const char *szParams,
				HANDLE hWait1);
	DWORD DoPOST(const char *szResource,
				 const char *szParams,
				 const BYTE *pbData,
				 ULONG ulSize,
				 HANDLE hWait1);

	const CHttpRequest& GetRequest();
	const CHttpResponse& GetResponse();
	SAFEARRAY *GetDataArray(bool bDetach);
	const BYTE *GetDataBuffer();
	DWORD OpenTransport();
	void CloseTransport();
	void ReleaseDataBuffer();
	void Reset();
private: 
	void Cleanup();

protected:

	DWORD DoHttpRequest(const char *szMethod,
						const char *szResource,
						const char *szParams,
						const BYTE *pbReqData,
						ULONG ulSize,
						HANDLE hWait1);
};
