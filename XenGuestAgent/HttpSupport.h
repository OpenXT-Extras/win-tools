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
#include "XenGuestAgent.h"

// HTTP 1.0/1.1 Codes
#define HTTP_CONTINUE				        100 // 1.1: Continue
#define HTTP_SWITCHING_PROTOCOLS            101 // 1.1:	Switching Protocols
#define HTTP_OK						        200 // 1.0: OK
#define HTTP_CREATED					    201 // 1.0: Created
#define HTTP_ACCEPTED				        202 // 1.0: Accepted
#define HTTP_NONAUTHORATATIVE_INFORMATION   203 // 1.1: Non-Authoritative Information
#define HTTP_NO_CONTENT				        204 // 1.0: No Content
#define HTTP_RESET_CONTENT                  205 // 1.1: Reset Content
#define HTTP_PARTIAL_CONTENT                206 // 1.1: Partial Content
#define HTTP_MULTIPLE_CHOICES		        300 // 1.0: Multiple Choices
#define HTTP_MOVED_PERMANENTLY		        301 // 1.0: Moved Permanently
#define HTTP_MOVED_TEMPORARILY		        302 // 1.0: Moved Temporarily
#define HTTP_SEE_OTHER                      303 // 1.1: See Other
#define HTTP_NOT_MODIFIED			        304 // 1.0: Not Modified
#define HTTP_USE_PROXY                      305 // 1.1: Use Proxy
#define HTTP_BAD_REQUEST				    400 // 1.0: Bad Request
#define HTTP_UNAUTHORIZED			        401 // 1.0: Unauthorized
#define HTTP_PAYMENT_REQUIRED               402 // 1.1: Payment Required
#define HTTP_FORBIDDEN				        403 // 1.0: Forbidden
#define HTTP_NOT_FOUND				        404 // 1.0: Not Found
#define HTTP_METHOD_NOT_ALLOWED             405 // 1.1: Method Not Allowed
#define HTTP_NOT_ACCEPTABLE                 406 // 1.1: Not Acceptable
#define HTTP_PROXY_AUTHENTICATION_REQUIRED  407 // 1.1: Proxy Authentication Required
#define HTTP_REQUEST_TIMEOUT                408 // 1.1: Request Time-out
#define HTTP_CONFLICT                       409 // 1.1: Conflict
#define HTTP_GONE                           410 // 1.1: Gone
#define HTTP_LENGTH_REQUIRED                411 // 1.1: Length Required
#define HTTP_PRECONDITION_FAILED            412 // 1.1: Precondition Failed
#define HTTP_REQUEST_ENTITY_TOO_LARGE       413 // 1.1: Request Entity Too Large
#define HTTP_REQUEST_URI_TOO_LARGE          414 // 1.1: Request-URI Too Large
#define HTTP_UNSUPPORTED_MEDIA_TYPE         415 // 1.1: Unsupported Media Type
#define HTTP_REQUESTED_RANGE_NOT_SATISFIABL 416 // 1.1: Requested Range Not Satisfiable
#define HTTP_INTERNAL_SERVER_ERROR	        500 // 1.0: Internal Server Error
#define HTTP_NOT_IMPLEMENTED			    501 // 1.0: Not Implemented
#define HTTP_BAD_GATEWAY				    502 // 1.0: Bad Gateway
#define HTTP_SERVICE_UNAVAILABLE		    503 // 1.0: Service Unavailable
#define HTTP_GATEWAY_TIMEOUT                504 // 1.1: Gateway Time-out
#define HTTP_VERSION_NOT_SUPPORTED          505 // 1.1: HTTP Version not supported"},

// HTTP header fields
#define HTTP_ACCEPT_ENCODING "Accept-Encoding"
#define HTTP_ALLOW "Allow"
#define HTTP_AUTHORIZATION "Authorization"
#define HTTP_CONTENT_ENCODING "Content-Encoding"
#define HTTP_CONTENT_LENGTH "Content-Length"
#define HTTP_CONTENT_TYPE "Content-Type"
#define HTTP_CONTENT_DISPOSITION "Content-Disposition"
#define HTTP_HOST "Host"
#define HTTP_DATE "Date"
#define HTTP_EXPIRES "Expires"
#define HTTP_FROM "From"
#define HTTP_IF_MODIFIED_SINCE "If-Modified-Since"
#define HTTP_IF_RANGE "If-Range"
#define HTTP_LAST_MODIFIED "Last-Modified"
#define HTTP_LOCATION "Location"
#define HTTP_PRAGMA "Pragma"
#define HTTP_RANGE "Range"
#define HTTP_REFERER "Referer"
#define HTTP_SERVER "Server"
#define HTTP_USER_AGENT "User-Agent"
#define HTTP_WWW_AUTHENTICATE "WWW-Authenticate"
#define HTTP_DEFLATE "deflate"

#define HTTP_CONNECTION "Connection"
#define HTTP_PX_KEEP_ALIVE "Keep-Alive"

// HTTP versions
#define HTTP_VERSION_09 "HTTP/0.9"
#define HTTP_VERSION_10 "HTTP/1.0"
#define HTTP_VERSION_11 "HTTP/1.1"

// HTTP Methods
#define HTTP_METHOD_GET "GET"
#define HTTP_METHOD_HEAD "HEAD"
#define HTTP_METHOD_POST "POST"
#define HTTP_METHOD_PUT "PUT"

// MIME Types
#define MIME_TEXT_PLAIN "text/plain"
#define MIME_TEXT_HTML "text/html"
#define MIME_TEXT_XML "text/xml"
#define MIME_IMAGE_GIF "image/gif"
#define MIME_TEXT_RTF "text/rtf"
#define MIME_IMAGE_JPEG "image/jpeg"
#define MIME_IMAGE_PNG "image/png"
#define MIME_APPLICATION_OCTET_STREAM "application/octet-stream"
#define MIME_APPLICATION_ZIP "application/zip"
#define MIME_MULTIPART_MIXED "multipart/mixed"
#define MIME_FORM_DATA "application/x-www-form-urlencoded"
#define MIME_MULTIPART_FORM_DATA "multipart/form-data"

// HTTP return codes
#define HTTP_NO_ERROR           0
#define HTTP_E_FAILED           1
#define HTTP_E_BAD_PARAM        2
#define HTTP_E_NO_MEMORY        3
#define HTTP_E_URL_DECODE       4
#define HTTP_E_PARSE_VERSION    5
#define HTTP_E_PROCESS_REQUEST  6
#define HTTP_E_PROCESS_RESPONSE 7
#define HTTP_E_BAD_HEADERS      8

typedef struct _HEADER_FIELD
{
	const char *szField;
	const char *szValue;
} HEADER_FIELD;

typedef std::vector<HEADER_FIELD> TVecFieldsIn;

typedef std::map<std::string, std::string> TMapFieldsOut;

class CHttpHeader
{
protected:
	CXenGuestAgent *m_pclXga;
	TVecFieldsIn    m_vFieldsIn;
	TMapFieldsOut   m_mFieldsOut;
	char           *m_szHeaders;
	ULONG           m_ulHeadersLength;

public:
	ULONG           m_ulHeaderLength;
	ULONG           m_ulContentLength;
	char           *m_szVersion;
	int             m_iMajorVersion;
	int             m_iMinorVersion;

public:
	CHttpHeader(CXenGuestAgent *pclXga) : m_pclXga(pclXga),
										  m_szHeaders(NULL),
										  m_ulHeadersLength(0),
										  m_ulContentLength(0),
										  m_ulHeaderLength(0),
										  m_szVersion(NULL),
										  m_iMajorVersion(0),
										  m_iMinorVersion(0)
	{
	}

	~CHttpHeader()
	{
		m_vFieldsIn.clear();
	}

	void Reset();

	inline UCHAR CharToHex(const char c)
	{
		if ((c >= 0)&&(c <= 9)) 
			return ('0' + c);
		else if ((c >= 10)&&(c <= 15))
			return ('A' + (c - 10));
		return 0;
	}

	inline UCHAR CharToDec(const char c)
	{
		if ('0' <= c && c <= '9')
			return c - '0';
		else if ('A' <= c && c <= 'F')
			return c - 'A' + 10;
		else if ('a' <= c && c <= 'f')
			return c - 'a' + 10;
		return 0;
	}

	const char *HttpCodeToText(int iHttpCode);
	const char *HttpReturnToText(int iRetCode);
	std::string HttpGetHeaderTime(time_t htime);
	int UrlEncodeData(const char *szData,
					  ULONG ulLength,
					  char *szEncoded,
					  ULONG *pulEncodedLength);
	int UrlDecodeData(const char *szData,
					  ULONG ulLength,
					  char *szDecoded,
					  ULONG *pulDecodedLength);

	const char* HttpGetHeaderField(const char* szName, int iOccurance);
	void HttpSetHeaderField(const std::string strField, const std::string strValue);
	void HttpRemoveHeaderField(const std::string strField);

protected:
	int HttpParseHeaders();
	int HttpParseVersion();

};

class CHttpRequest : public CHttpHeader
{
protected:
	char *m_szRequestLine;
	ULONG m_ulRequestLineLength;
	std::string m_strRequest;

public:
	char *m_szMethod;
	char *m_szResource;
	char *m_szParams;
	char *m_szResourceDecoded;
	char *m_szParamsDecoded;
	
public:
	CHttpRequest(CXenGuestAgent *pclXga) : CHttpHeader(pclXga),
										   m_szRequestLine(NULL),
										   m_ulRequestLineLength(0),
										   m_szMethod(NULL),
										   m_szResource(NULL),
										   m_szParams(NULL),
										   m_szResourceDecoded(NULL),
										   m_szParamsDecoded(NULL),
										   m_strRequest("")
	{
	}

	~CHttpRequest()
	{
		Reset();
	}

	void Reset();
	int ProcessRequestHeader(const char *szRequest, ULONG ulLength);
	const char* GenerateRequestHeader(const std::string strMethod,
									  const std::string strResource,
									  const std::string strParams);

protected:
	int ProcessRequestLine();
};

class CHttpResponse : public CHttpHeader
{
protected:
	char *m_szResponseLine;
	ULONG m_ulResponseLineLength;
	std::string m_strResponse;

public:
	char *m_szVersion;
	char *m_szCode;
	char *m_szReason;
	int   m_iHttpCode;

public:
	CHttpResponse(CXenGuestAgent *pclXga) : CHttpHeader(pclXga),
											m_szResponseLine(NULL),
										    m_ulResponseLineLength(0),
										    m_szCode(NULL),										   
										    m_szReason(NULL),
											m_iHttpCode(0)
	{
	}

	~CHttpResponse()
	{
	}

	void Reset();
	int ProcessResponseHeader(const char *szResponse, ULONG ulLength);
	const char* GenerateResponseHeader(int iHttpCode);

protected:
	int ProcessResponseLine();
};
