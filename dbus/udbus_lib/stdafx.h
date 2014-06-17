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

// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#ifdef _WIN32

#	pragma once

#ifdef _DEBUG
#define _BIND_TO_CURRENT_VCLIBS_VERSION 1
#endif _DEBUG

#	include "targetver.h"

#	define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#endif // _WIN32

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <sys/types.h>

#ifdef _WIN32
#	pragma warning (push)
#	pragma warning (disable: 4820 4668 4255 4514)
	// 4668: 'XXX' is not defined as a preprocessor macro, replacing with '0' for '#if/#elif'
	// 4820: 'XXX' : 'N' bytes padding added after data member 'YYY'
	// 4255: 'FARPROC' : no function prototype given: converting '()' to '(void)
	// 4514: 'xxx' : unreferenced inline function has been removed
#	include <WinSock2.h>
#	pragma warning (pop)
#	ifndef _INTPTR
#		define _INTPTR 0
#	endif // _INTPTR
#endif // _WIN32
