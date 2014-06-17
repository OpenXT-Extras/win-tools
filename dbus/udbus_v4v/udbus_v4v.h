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

//@ file: udbus_v4v.h
//@ read/write functions for udbus implemented over v4v.

//struct dbus_io_s;
//typedef struct dbus_io_s dbus_io_2;
typedef struct _dbus_io dbus_io;



#ifdef _UDBUS_USE_XEN_TYPES
#	include <types.h>
#else // !_UDBUS_USE_XEN_TYPES
#	include <stdint.h>
#endif // _UDBUS_USE_XEN_TYPES
#include <vector>

typedef struct _V4V_CONTEXT V4V_CONTEXT;

#ifndef _WIN32

#	define _UDBUS_V4V_CALLCONV

#else // !_WIN32

#	define _UDBUS_V4V_CALLCONV __stdcall

// Compile as static lib or DLL.
// External code using static lib must define _UDBUS_V4V_STATICLIB.
// Otherwise expect to see errors such as:
//  somefile.obj : error LNK2001: unresolved external symbol __imp__dbus_msg_recv@8

#ifdef _UDBUS_V4V_EXPORTS
#	define _UDBUS_V4V_API __declspec(dllexport)
#else // !_UDBUS_V4V_EXPORTS
#	ifdef _UDBUS_V4V_STATICLIB
#		define _UDBUS_V4V_API
# else // !_UDBUS_V4V_STATICLIB
#		define _UDBUS_V4V_API __declspec(dllimport)
#	endif // _UDBUS_V4V_STATICLIB
#endif // _UDBUS_V4V_EXPORTS

#endif // _WIN32

#ifdef __cplusplus
extern "C"
#endif // __cplusplus
{
	struct TransportData;
	_UDBUS_V4V_API bool _UDBUS_V4V_CALLCONV populate_dbus_io(dbus_io *pdbus_io, TransportData *ptransport);
#ifdef __cplusplus
} // Ends extern "C"
#endif // __cplusplus

#ifdef _YESYESYES
#ifdef __cplusplus
extern "C"
#endif // __cplusplus
{
	struct TransportData;
	//_UDBUS_V4V_API TransportData * _UDBUS_V4V_CALLCONV connect_v4v_socket(void (_UDBUS_V4V_CALLCONV *io_debug)(void *logpriv, const char *buf)=NULL);
	//_UDBUS_V4V_API bool _UDBUS_V4V_CALLCONV disconnect_v4v_socket(TransportData *ptransportdata);
	void testV4V();
#ifdef __cplusplus
} // Ends extern "C"
#endif // __cplusplus

// dbus io_read/io_write implementations.
#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	//_UDBUS_V4V_API int _UDBUS_V4V_CALLCONV v4v_io_read(void *priv, void *buf, uint32_t count);
	//_UDBUS_V4V_API int _UDBUS_V4V_CALLCONV v4v_io_write(void *priv, const void *buf, uint32_t count);

#ifdef __cplusplus
} // Ends extern "C"
#endif // __cplusplus

#ifdef _WIN32
DWORD waitOnHandles(TransportData *ptransportdata, LPCTSTR pszErrorMessage);
#endif // _WIN32

#endif // _YESYESYES
