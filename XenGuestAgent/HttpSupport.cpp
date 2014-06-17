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
#include "HttpSupport.h"
#include <time.h>

#define HTTP_MAX_HEADER_SIZE 4096  // default Apache uses

typedef struct _HTTP_CODE_MAP
{
	int iCode;
	const char *szReturnText;
} HTTP_CODE_MAP;

static const HTTP_CODE_MAP g_arrHttpCodes[] = 
{
	{100,	"Continue"},
	{101,	"Switching Protocols"},
	{200,	"OK"},
	{201,	"Created"},
	{202,	"Accepted"},
	{203,	"Non-Authoritative Information"},
	{204,	"No Content"},
	{205,	"Reset Content"},
	{206,	"Partial Content"},
	{300,	"Multiple Choices"},
	{301,	"Moved Permanently"},
	{302,	"Moved Temporarily"},
	{303,	"See Other"},
	{304,	"Not Modified"},
	{305,	"Use Proxy"},
	{400,	"Bad Request"},
	{401,	"Unauthorized"},
	{402,	"Payment Required"},
	{403,	"Forbidden"},
	{404,	"Not Found"},
	{405,	"Method Not Allowed"},
	{406,	"Not Acceptable"},
	{407,	"Proxy Authentication Required"},
	{408,	"Request Time-out"},
	{409,	"Conflict"},
	{410,	"Gone"},
	{411,	"Length Required"},
	{412,	"Precondition Failed"},
	{413,	"Request Entity Too Large"},
	{414,	"Request-URI Too Large"},
	{415,	"Unsupported Media Type"},
	{416,	"Requested Range Not Satisfiable"},
	{500,	"Internal Server Error"},
	{501,	"Not Implemented"},
	{502,	"Bad Gateway"},
	{503,	"Service Unavailable"},
	{504,	"Gateway Time-out"},
	{505,	"HTTP Version not supported"},
	{0,		NULL}
};

static const HTTP_CODE_MAP g_arrReturnCodes[] = 
{
	{HTTP_NO_ERROR,           "No error"},
	{HTTP_E_FAILED,           "General failure"},
	{HTTP_E_BAD_PARAM,        "Bad parameter"},
	{HTTP_E_NO_MEMORY,        "Out of memory"},
	{HTTP_E_URL_DECODE,       "URL decode failure"},
	{HTTP_E_PARSE_VERSION,    "Version parsing failure"},
	{HTTP_E_PROCESS_REQUEST,  "Failed to process an HTTP request header"},
	{HTTP_E_PROCESS_RESPONSE, "Failed to process an HTTP response header"},
	{HTTP_E_BAD_HEADERS,      "Bad or incomplete HTTP header"},
	{0,		                  NULL}
};

static const UINT g_uiMonthSumDays[12] = { 
	0, 31, 31+28, 31+28+31, 31+28+31+30, 31+28+31+30+31, 31+28+31+30+31+30,
	31+28+31+30+31+30+31, 31+28+31+30+31+30+31+31, 31+28+31+30+31+30+31+31+30,
	31+28+31+30+31+30+31+31+30+31, 31+28+31+30+31+30+31+31+30+31+30
};

void CHttpHeader::Reset()
{
	m_vFieldsIn.clear();
	m_mFieldsOut.clear();
	m_ulHeaderLength = 0;
	m_ulContentLength = 0;
	if (m_szHeaders != NULL)
	{
		delete [] m_szHeaders;
		m_szHeaders = NULL;
	}
	m_ulHeadersLength = 0;
	m_szVersion = NULL;
	m_iMajorVersion = 0;
	m_iMinorVersion = 0;
}

const char *CHttpHeader::HttpCodeToText(int iHttpCode)
{
	ULONG i;

	for (i = 0; g_arrHttpCodes[i].szReturnText == NULL; i++)
	{
		if (g_arrHttpCodes[i].iCode == iHttpCode)
			return g_arrHttpCodes[i].szReturnText;
	}
	return "Unknown Code";
}

const char *CHttpHeader::HttpReturnToText(int iRetCode)
{
	ULONG i;

	for (i = 0; g_arrReturnCodes[i].szReturnText == NULL; i++)
	{
		if (g_arrReturnCodes[i].iCode == iRetCode)
			return g_arrReturnCodes[i].szReturnText;
	}
	return "Unknown return value";
}

std::string CHttpHeader::HttpGetHeaderTime(time_t ht)
{
#define TIME_BUF_SIZE 128
	char szBuf[TIME_BUF_SIZE + 1];
	struct tm stm;
	const ULARGE_INTEGER ullEpoch =  { 3577643008, 27111902 };
	ULARGE_INTEGER ullTime;
	FILETIME ftFileTime;
	SYSTEMTIME tmSysTime;

	ullTime.QuadPart = ht; // time_t (seconds past epoch)
	ullTime.QuadPart *= 10000000; // convert to 100ns ticks
	ullTime.QuadPart += ullEpoch.QuadPart; // add constant epoch bias

	ftFileTime.dwHighDateTime = ullTime.HighPart;
	ftFileTime.dwLowDateTime  = ullTime.LowPart;

	if (!FileTimeToSystemTime(&ftFileTime, &tmSysTime))
		return "?"; // SNO

	stm.tm_year = tmSysTime.wYear - 1900;
	stm.tm_mon  = tmSysTime.wMonth - 1;
	stm.tm_mday = tmSysTime.wDay;
	stm.tm_hour = tmSysTime.wHour;
	stm.tm_min  = tmSysTime.wMinute;
	stm.tm_sec  = tmSysTime.wSecond;
	stm.tm_wday = tmSysTime.wDayOfWeek;
	stm.tm_isdst = 0;

	// Figure out the day of year (Julian date). Note that this depends on 
	// Gregorian calendar math and so only works for the last 500 years or so.
	// Leap years are divisible by 400 or are divisible by 4 but not 100. I.e.,
	// 2004, 1600, 2000, 2400 *are* leap years
	// 2003, 1700, 1800, 1900, 2100, 2200, 2300, 2500, 2600 are not leap years
#define ISLEAPYEAR(y) (((y)%400 == 0) || (((y)%100 != 0) && ((y)%4 == 0)))
#define JDAY(y,m,d) ((d) + g_uiMonthSumDays[(m)-1] + (ISLEAPYEAR(y)?1:0) - 1)
	stm.tm_yday = JDAY(tmSysTime.wYear, tmSysTime.wMonth, tmSysTime.wDay);
#undef ISLEAPYEAR
#undef JDAY

	strftime(szBuf, TIME_BUF_SIZE, "%a, %d %b %Y %H:%M:%S GMT", &stm);
	szBuf[TIME_BUF_SIZE] = '\0'; // sanity check
	return szBuf;
}

int CHttpHeader::UrlEncodeData(const char *szData,
							   ULONG ulLength,
							   char *szEncoded,
							   ULONG *pulEncodedLength)
{
	char c = 0x00;
	ULONG ulEncodedLength = 0;

	if ((szData == NULL)||(ulLength < 1)||
		(szEncoded == NULL)||(pulEncodedLength == NULL)||
		(*pulEncodedLength < (3*ulLength + 1)))
		return HTTP_E_BAD_PARAM;

	memset(szEncoded, 0, *pulEncodedLength);
	*pulEncodedLength = 0;
	
	for (ULONG i = 0; i < ulLength; i++)
	{
		c = szData[i];

		if (isalnum(c & 0xff) == 0)
		{
			switch (c)
			{
			// Basically it is OK to encode everything (simple, see RFC 1738)
			case ' ':
				szEncoded[ulEncodedLength++] = '+';
				break;
			default:
				// Encode everything else
				szEncoded[ulEncodedLength++] = '%';
				szEncoded[ulEncodedLength++] = CharToHex((char)(((UCHAR)c >> 4) & 0x0f));
				szEncoded[ulEncodedLength++] = CharToHex((char)((UCHAR)c & 0x0f));
				break;
			}
		}
		else
		{
			// Just store alnums
			szEncoded[ulEncodedLength++] = c;
		}
	}

	*pulEncodedLength = ulEncodedLength;
	return HTTP_NO_ERROR;
}

int CHttpHeader::UrlDecodeData(const char *szData,
							   ULONG ulLength,
							   char *szDecoded,
							   ULONG *pulDecodedLength)
{
	ULONG ulDecodedLength = 0;
	char t = '\0';
	enum State {Normal, Code1, Code2} enState = Normal;

	if ((szData == NULL)||(ulLength < 1)||
		(szDecoded == NULL)||(pulDecodedLength == NULL))
		return HTTP_E_BAD_PARAM;
	
	for (ULONG i = 0; i < ulLength; i++)
	{
		char c = szData[i];

		switch (enState)
		{
		case Normal:
			if (c == '%')
			{
				enState = Code1;
				continue;
			}
			else if (c == '+')
			{
				// These are spaces at this point
				c = ' ';
			}
			break;
		case Code1:
			if ((isascii(c))&&(isxdigit(c)))
			{
				t = CharToDec(c);
				enState = Code2;
				continue;
			}
			else
			{
				// Echo this char and the %
				szDecoded[ulDecodedLength++] = '%';
				if (ulDecodedLength > *pulDecodedLength)
					return HTTP_E_URL_DECODE;

				enState = Normal;
			}
			break;
		case Code2:
			if ((isascii(c))&&(isxdigit(c)))
			{
				t <<= 4;
				t |= CharToDec(c);
				c = t;
			}
			else
			{
				// Echo this char, the last digit (which we know is there) since we are in
				// this state, and the % - this is a hosed encoding.
				szDecoded[ulDecodedLength++] = '%';
				if (ulDecodedLength > *pulDecodedLength)
					return HTTP_E_URL_DECODE;
				szDecoded[ulDecodedLength++] = szData[i - 1];
				if (ulDecodedLength > *pulDecodedLength)
					return HTTP_E_URL_DECODE;
			}
			enState = Normal;
		default:
			break;
		}

		szDecoded[ulDecodedLength++] = c;
		if (ulDecodedLength > *pulDecodedLength)
			return HTTP_E_URL_DECODE;
	}

	*pulDecodedLength = ulDecodedLength;
	return HTTP_NO_ERROR;
}

int CHttpHeader::HttpParseHeaders()
{
#define HEADER_TOKENS   "\r\n"
	char *szPtr = NULL;
	char *szContext = NULL;
	bool bFound = false;
	bool bFoundLength = false;
	HEADER_FIELD stField = {NULL, NULL};

	szPtr = strtok_s(m_szHeaders, HEADER_TOKENS, &szContext);
	while ((szPtr != NULL)&&(szPtr < (m_szHeaders + m_ulHeadersLength)))
	{
		bFound = false;
		stField.szField = szPtr;
		if ((*szPtr == ' ')||(*szPtr == '\t'))
		{
			for (int j = 0; j < 2; j++)
			{
				szPtr--;
				if (szPtr >= m_szHeaders)
				{
					switch (*szPtr)
					{
					case '\0':
					case '\r':
					case '\n':
						(j == 0) ? *szPtr = '\n' : *szPtr = '\r'; // put back CRLF
						break;
					default:
						continue;
					}
				}
			}
			szPtr = strtok_s(NULL, HEADER_TOKENS, &szContext);
			continue;
		}
		while (*szPtr != '\0')
		{
			if (!bFound)
			{
				if (*szPtr == ':')
				{
					*szPtr = '\0';
					bFound = true;
					szPtr++;
					continue;
				}
			}
			else
			{
				if ((*szPtr != '\t')&&(*szPtr != ' '))
				{
					stField.szValue = szPtr;
					break;
				}
			}
			szPtr++;
		}
		if ((!bFoundLength)&&(_stricmp(stField.szField, HTTP_CONTENT_LENGTH) == 0))
		{
			m_ulContentLength = atol(stField.szValue);
			bFoundLength = true;
		}
		// Do not add incomplete or empty header entries - ie missing :
		if ((stField.szField != NULL)&&(bFound))
		{
			// Allow empty values
			if (stField.szValue == NULL)
				stField.szValue = "";

			m_vFieldsIn.push_back(stField);
		}
		szPtr = strtok_s(NULL, HEADER_TOKENS, &szContext);
	}

	// For HTTP/1.0 assume no Content-Length field means no content.
	if (!bFoundLength) 
		m_ulContentLength = 0;

	return HTTP_NO_ERROR;
}

int CHttpHeader::HttpParseVersion()
{
	char *szPtr = NULL;
	std::string strTemp;
	std::string::size_type pos;

	// No version is an old HTTP/0.9 simple request
	if (m_szVersion == NULL)
	{
		m_iMajorVersion = 0;
		m_iMinorVersion = 9;
		m_szVersion = HTTP_VERSION_09;
		return HTTP_NO_ERROR;
	}

	// Get the vesion numbers
	szPtr = m_szVersion;
	if (strlen(szPtr) <= 5)
		return HTTP_NO_ERROR; // not a value version field

	strTemp = szPtr + 5;
	pos = strTemp.find(".");
	if (pos != std::string::npos)
	{
		m_iMajorVersion = (int)atol(strTemp.substr(0, pos).c_str());
		m_iMinorVersion = (int)atol(strTemp.substr(pos + 1, strTemp.length() - 1).c_str());
		return HTTP_NO_ERROR;
	}
	return HTTP_E_PARSE_VERSION;
}

const char* CHttpHeader::HttpGetHeaderField(const char* szName, int iOccurance)
{
	int c = 0;
	TVecFieldsIn::const_iterator it = m_vFieldsIn.begin();

	if (szName == NULL)
		return NULL;

	while (it != m_vFieldsIn.end())
	{
		if (_stricmp(it->szField, szName) == 0)
		{
			c++;
			if (c == iOccurance)
				return it->szValue;
		}
		it++;
	}

	return NULL;
}

void CHttpHeader::HttpSetHeaderField(const std::string strField, const std::string strValue)
{	
	TMapFieldsOut::iterator it = m_mFieldsOut.find(strField);

	if (m_mFieldsOut.end() != it)
	{
		// Append to existing per RFC 1945 section 4.2
		(it->second) += ","; 
		(it->second) += strValue.c_str();
	}
	else
		m_mFieldsOut.insert(TMapFieldsOut::value_type(strField, strValue));
}

void CHttpHeader::HttpRemoveHeaderField(const std::string strField)
{
	m_mFieldsOut.erase(strField);
}

void CHttpRequest::Reset()
{
	CHttpHeader::Reset();

	if (m_szRequestLine != NULL)
	{
		delete [] m_szRequestLine;
		m_szRequestLine = NULL;
	}
	m_ulRequestLineLength = 0;
	m_szMethod = NULL;
	m_szResource = NULL;
	m_szParams = NULL;
	if (m_szResourceDecoded != NULL)
	{
		delete [] m_szResourceDecoded;
		m_szResourceDecoded = NULL;
	}
	if (m_szParamsDecoded != NULL)
	{
		delete [] m_szParamsDecoded;
		m_szParamsDecoded = NULL;
	}
	m_strRequest = "";
	m_mFieldsOut.clear();
}

int CHttpRequest::ProcessRequestLine()
{
#define REQUEST_TOKENS " \t\r\n"
	char *szContext = NULL;
	char *szPtr = NULL;
	
	// The method is the first item after any delimiter tokens that get eaten
	m_szMethod = strtok_s(m_szRequestLine, REQUEST_TOKENS, &szContext);
	m_szResource = strtok_s(NULL, REQUEST_TOKENS, &szContext);
	m_szVersion = strtok_s(NULL, REQUEST_TOKENS, &szContext);
	
	if ((m_szMethod == NULL)||(m_szResource == NULL))
		return HTTP_E_PROCESS_REQUEST;

	// Locate the paramters/arguments that may be passed in the resource.
	m_szParams = "";
	szPtr = m_szResource;
	while (*szPtr != '\0')
	{
		if (*szPtr == '?')
		{
			*szPtr = '\0';
			m_szParams = ++szPtr;
			break;
		}
		szPtr++;
	}

	// Last step, parse the HTTP version
	return HttpParseVersion();
}

int CHttpRequest::ProcessRequestHeader(const char *szRequest, ULONG ulLength)
{
	ULONG i, j, ulLF = 0;
	bool bInReq = false, bInReqLine = false, bStop = false;
	ULONG ulResourceLength, ulResourceDecoded;
	ULONG ulParamsLength, ulParamsDecoded;
	int rc;

	for (i = 0; i < ulLength; i++)
	{
		if (bStop)
			break;

		switch (szRequest[i])
		{
		case '\n':
			if (bInReq)
			{
				ulLF++;
				if (bInReqLine)
				{
					bInReqLine = false; // leaving request line
					m_ulRequestLineLength = i + 1;

					// Copy and cleanup here
					m_szRequestLine = new char[m_ulRequestLineLength + 1];
					if (m_szRequestLine == NULL)
						return HTTP_E_NO_MEMORY;
					memcpy(m_szRequestLine, szRequest, m_ulRequestLineLength);
					m_szRequestLine[m_ulRequestLineLength] = '\0';

					// Clean up the end of the request line
					for (j = m_ulRequestLineLength - 1; j > 0; j--)
					{
						switch (m_szRequestLine[j])
						{
						case '\r':
						case '\n':
							m_szRequestLine[j] = '\0';
							break;
						default:
							j = 1;
						};
					}
					
					// Process the request line at this point
					rc = ProcessRequestLine();
					if (rc != HTTP_NO_ERROR)
						return rc;

					if ((m_iMajorVersion == 0)&&(m_iMinorVersion == 9))
						bStop = true;
				}
			}
			break;
		case '\r':
			break;
		default:
			if (!bInReq)
			{
				// Set once on first non-CRLF
				bInReq = true;
				bInReq = true;
			}
			ulLF = 0; // reset
		}
		if (ulLF == 2)
		{
			// NOTE: m_ulHeaderLength is the length the request and all the 
			// headers - the offset to the payload if any.
			m_ulHeaderLength = i + 1;
			break;
		}

		// Do not allow endless headers to be passed in
		if (i > HTTP_MAX_HEADER_SIZE)
			break;
	}

	if (m_ulHeaderLength == 0) // incomplete headers
		return HTTP_E_BAD_HEADERS;

	// Decode the resource/URL and any params. During a decode, the result
	// will be <= the original.
	ulResourceLength = strlen(m_szResource);
	m_szResourceDecoded = new char[ulResourceLength + 2];
	if (m_szResourceDecoded == NULL)
		return HTTP_E_NO_MEMORY;
	ulResourceDecoded = ulResourceLength + 1;

	rc = UrlDecodeData(m_szResource, ulResourceLength, m_szResourceDecoded, &ulResourceDecoded);
	if (rc != HTTP_NO_ERROR)
		return rc;

	ulParamsLength = strlen(m_szParams);
	m_szParamsDecoded = new char[ulParamsLength + 2];
	if (m_szParamsDecoded == NULL)
		return HTTP_E_NO_MEMORY;

	if (ulParamsLength != 0)
	{
		ulParamsDecoded = ulParamsLength + 1;

		rc = UrlDecodeData(m_szParams, ulParamsLength, m_szParamsDecoded, &ulParamsDecoded);
		if (rc != HTTP_NO_ERROR)
			return rc;
	}
	else
		m_szParamsDecoded[0] = '\0';

	// Copy in the headers block and parse
	m_ulHeadersLength = (m_ulHeaderLength - m_ulRequestLineLength);
	m_szHeaders = new char[m_ulHeadersLength + 1];
	if (m_szHeaders == NULL)
		return HTTP_E_NO_MEMORY;
	memcpy(m_szHeaders, (szRequest + m_ulRequestLineLength), m_ulHeadersLength);
	m_szHeaders[m_ulHeadersLength] = '\0';

	return HttpParseHeaders();
}

const char* CHttpRequest::GenerateRequestHeader(const std::string strMethod,
												const std::string strResource,
												const std::string strParams)
{
	TMapFieldsOut::const_iterator it = m_mFieldsOut.begin();
	std::string strHeaders = "";

	m_strRequest = "";
	
	while (m_mFieldsOut.end() != it)
	{
		strHeaders += (it->first).c_str();
		strHeaders += ": ";
		strHeaders += (it->second).c_str();
		strHeaders += "\r\n";
		it++;
	}

	m_strRequest = strMethod.c_str();
	m_strRequest += " ";
	m_strRequest += strResource.c_str();
	if (strParams.length() > 0)
	{
		m_strRequest += '?';
		m_strRequest += strParams.c_str();
	}
	m_strRequest += " ";
	m_strRequest += HTTP_VERSION_11;
	m_strRequest += "\r\n";
	m_strRequest += strHeaders.c_str();
	m_strRequest += "\r\n";

	return m_strRequest.c_str();
}

void CHttpResponse::Reset()
{
	CHttpHeader::Reset();

	if (m_szResponseLine != NULL)
	{
		delete [] m_szResponseLine;
		m_szResponseLine = NULL;
	}
	m_ulResponseLineLength = 0;
	m_szCode = NULL;
	m_szReason = NULL;
	m_iHttpCode = 0;
	m_strResponse = "";
	m_mFieldsOut.clear();
}

int CHttpResponse::ProcessResponseLine()
{
#define REQUEST_TOKENS " \t\r\n"
	char *szContext = NULL;
	char *szPtr = NULL;
	int rc;

	// The version is the first item after any delimiter tokens that get eaten
	m_szVersion = strtok_s(m_szResponseLine, REQUEST_TOKENS, &szContext);
	m_szCode = strtok_s(NULL, REQUEST_TOKENS, &szContext);
	m_szReason = strtok_s(NULL, REQUEST_TOKENS, &szContext);

	if ((m_szVersion == NULL)||(m_szCode == NULL)||(m_szReason == NULL))
		return HTTP_E_PROCESS_RESPONSE;

	m_iHttpCode = (short)atol(m_szCode);

	// Last step, parse the HTTP version
	rc = HttpParseVersion();
	if ((rc == HTTP_NO_ERROR)&&(m_iMajorVersion == 9))
		return HTTP_E_PARSE_VERSION;

	return rc;
}

int CHttpResponse::ProcessResponseHeader(const char *szResponse, ULONG ulLength)
{
	ULONG i, j, ulLF = 0;
	bool bInRes = false, bInResLine = false, bStop = false;
	int rc;

	for (i = 0; i < ulLength; i++)
	{
		if (bStop)
			break;

		switch (szResponse[i])
		{
		case '\n': 
			if (bInRes)
			{
				ulLF++;
				if (bInResLine)
				{
					bInResLine = false;  // leaving response line
					m_ulResponseLineLength = i + 1;

					// Copy and cleanup here
					m_szResponseLine = new char[m_ulResponseLineLength + 1];
					if (m_szResponseLine == NULL)
						return HTTP_E_NO_MEMORY;
					memcpy(m_szResponseLine, szResponse, m_ulResponseLineLength);
					m_szResponseLine[m_ulResponseLineLength] = '\0';

					// Clean up the end of the response line
					for (j = m_ulResponseLineLength - 1; j > 0; j--)
					{
						switch (m_szResponseLine[j])
						{
						case '\r':
						case '\n':
							m_szResponseLine[j] = '\0';
							break;
						default:
							j = 1;
						};
					}

					// Process the response line at this point
					rc = ProcessResponseLine();
					if (rc != HTTP_NO_ERROR)
						return rc;
				}
			}
			break;
		case '\r':
			break;
		default:
			if (!bInRes)
			{
				// Set once on first non-CRLF
				bInRes = true;
				bInResLine = true;
			}
			ulLF = 0; // reset
		}
		if (ulLF == 2)
		{
			// NOTE: m_ulHeaderLength is the length the response and all the 
			// headers - the offset to the payload if any.
			m_ulHeaderLength = i + 1;
			break;
		}

		// Do not allow endless headers to be passed in
		if (i > HTTP_MAX_HEADER_SIZE)
			break;
	}

	if (m_ulHeaderLength == 0) // incomplete headers
		return HTTP_E_BAD_HEADERS;

	// Copy in the headers block and parse
	m_ulHeadersLength = (m_ulHeaderLength - m_ulResponseLineLength);
	m_szHeaders = new char[m_ulHeadersLength + 1];
	if (m_szHeaders == NULL)
		return HTTP_E_NO_MEMORY;
	memcpy(m_szHeaders, (szResponse + m_ulResponseLineLength), m_ulHeadersLength);
	m_szHeaders[m_ulHeadersLength] = '\0';

	return HttpParseHeaders();
}

const char* CHttpResponse::GenerateResponseHeader(int iHttpCode)
{
#define STATUS_LINE "HTTP/1.0 %3d %s\r\n"
#define STATUS_BUF_SIZE 256
	TMapFieldsOut::const_iterator it = m_mFieldsOut.begin();
	std::string strHeaders = "";
	char szStatus[STATUS_BUF_SIZE + 1];

	m_strResponse = "";
	
	while (m_mFieldsOut.end() != it)
	{
		strHeaders += (it->first).c_str();
		strHeaders += ": ";
		strHeaders += (it->second).c_str();
		strHeaders += "\r\n";
		it++;
	}

	_snprintf_s(szStatus, STATUS_BUF_SIZE, _TRUNCATE, STATUS_LINE, iHttpCode, HttpCodeToText(iHttpCode));
	szStatus[STATUS_BUF_SIZE] = '\0';

	m_strResponse = szStatus;
	m_strResponse += strHeaders.c_str();
	strHeaders += "\r\n";

	return m_strResponse.c_str();
}
