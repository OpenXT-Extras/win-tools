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

//@ file: v4vio.h
//@ read/write functions implemented over v4v.

//#include <types.h>	// uint32_t
#ifdef _UDBUS_USE_XEN_TYPES
#	include <types.h>
#else // !_UDBUS_USE_XEN_TYPES
#	include <stdint.h>
#endif // _UDBUS_USE_XEN_TYPES
#include <vector>

typedef struct _V4V_CONTEXT V4V_CONTEXT;

#ifndef _WIN32

#	define _V4VIO_CALLCONV

#else // _WIN32

#	define _V4VIO_CALLCONV __stdcall

// Compile as static lib or DLL.
// External code using static lib must define _V4VIO_STATICLIB.
// Otherwise expect to see errors such as:
//  somefile.obj : error LNK2001: unresolved external symbol __imp__dbus_msg_recv@8

#ifdef _V4VIO_EXPORTS
#	define _V4VIO_API __declspec(dllexport)
#else // !_V4VIO_EXPORTS
#	ifdef _V4VIO_STATICLIB
#		define _V4VIO_API
# else // !_V4VIO_STATICLIB
#		define _V4VIO_API __declspec(dllimport)
#	endif // _V4VIO_STATICLIB
#endif // _V4VIO_EXPORTS

#endif // !_WIN32

#ifdef __cplusplus
extern "C"
#endif // __cplusplus
{
	struct TransportData;
	_V4VIO_API TransportData * _V4VIO_CALLCONV connect_v4v_socket(void (_V4VIO_CALLCONV *io_debug)(void *logpriv, const char *buf) /*=NULL*/);
	// Cancels IO. OK to call from a separate thread (hopefully), but thread doing IO blocking *must* complete calls before socket can be disconnected.
	_V4VIO_API bool _V4VIO_CALLCONV cancel_v4v_socket_io(TransportData *ptransportdata);
	_V4VIO_API bool _V4VIO_CALLCONV disconnect_v4v_socket(TransportData *ptransportdata);
#	ifdef _WIN32
	_V4VIO_API DWORD _V4VIO_CALLCONV wait_for_v4v_read_handles_timeout(DWORD dwMilliseconds, TransportData *ptransportdata, LPCTSTR pszErrorMessage);
	_V4VIO_API DWORD _V4VIO_CALLCONV wait_for_v4v_read_handles(TransportData *ptransportdata, LPCTSTR pszErrorMessage);
#	endif // _WIN32
	//_V4VIO_API void _V4VIO_CALLCONV testV4V();
#ifdef __cplusplus
} // Ends extern "C"
#endif // __cplusplus

// dbus io_read/io_write implementations.
#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	_V4VIO_API int _V4VIO_CALLCONV v4v_io_read(void *priv, void *buf, uint32_t count);
	_V4VIO_API int _V4VIO_CALLCONV v4v_io_write(void *priv, const void *buf, uint32_t count);

#ifdef __cplusplus
} // Ends extern "C"
#endif // __cplusplus

