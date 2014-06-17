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

// udbus_smacker.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <stdio.h>
//#include <unistd.h>
#include <io.h>
#include <string.h>

#include "udbus.h"
#include "udbus_v4v.h"
#include <stdlib.h>

template <size_t _Size>
void hexencode(char (&d)[_Size], int v)
{
	char buf[32];
	int i;
	char *p = d;
	char *end = d + _Size;

	sprintf_s(buf, "%d", v);
	for (i = 0; buf[i] && p < end; i++) {
		sprintf_s(p, 3, "%.2x", buf[i]);
		p += 2;
	}
}

int run(int argc, _TCHAR* argv[])
{
#ifdef _FOO
	TransportData transportdata = connect_v4v_socket(NULL);
	char hexencoded_uid[32];
#ifdef _WIN32
	int uid = 0;
	hexencode(hexencoded_uid, uid);
#else // !_WIN32
	int uid;
	uid = getuid();
	
	hexencode(hexencoded_uid, uid);
#endif // _WIN32

	char authline[256];

	dbus_msg *msg, *recv;
	dbus_io dio;

	//fd = dbus_connect_session();
	//if (fd == -1) {
	//	printf("failed connection to session bus\n");
	//	exit(1);
	//}

	dio.io_read = &v4v_io_read;
	dio.io_write = &v4v_io_write;
	dio.priv = (void *) &transportdata;

	sprintf_s(authline, "EXTERNAL %s", hexencoded_uid);

	dbus_auth(&dio, authline);

	msg = dbus_msg_new_method_call("org.freedesktop.DBus", "/org/freedesktop/DBus",
	                               "org.freedesktop.DBus", "Hello");
	if (!msg) {
		exit(1);
	}
	dbus_msg_send(&dio, msg);
	dbus_msg_recv(&dio, &recv);

	msg = dbus_msg_new_method_call("org.freedesktop.DBus", "/org/freedesktop/DBus",
	                               "org.freedesktop.DBus", "ListNames");
	if (!msg) {
		exit(1);
	}
	dbus_msg_send(&dio, msg);
	dbus_msg_recv(&dio, &recv);
	dbus_msg_recv(&dio, &recv);
#endif // _FOO
	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{
	return run(argc, argv);
}
