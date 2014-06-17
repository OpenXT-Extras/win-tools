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

// udbus_example.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <stdio.h>
//#include <unistd.h>
#include <io.h>
#include <string.h>

#include "udbus.h"
#include "v4vio.h"
#include <stdlib.h>
#include <memory>
#include <conio.h>
#include <errno.h>

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

template <size_t _Size>
void hexencode(char (&d)[_Size], SID v)
{
	char buf[32];
	int i;
	char *p = d;
	char *end = d + _Size;

	//typedef struct _SID {
	//	BYTE  Revision;
	//	BYTE  SubAuthorityCount;
	//	SID_IDENTIFIER_AUTHORITY IdentifierAuthority;
	//#ifdef MIDL_PASS
	//	[size_is(SubAuthorityCount)] DWORD SubAuthority[*];
	//#else // MIDL_PASS
	//	DWORD SubAuthority[ANYSIZE_ARRAY];
	//#endif // MIDL_PASS
	//} SID, *PISID;
	// Revision.
	sprintf_s(buf, "%d", v.Revision);
	for (i = 0; buf[i] && p < end; i++) {
		sprintf_s(p, 3, "%.2x", buf[i]);
		p += 2;
	}

	// Identifier Authority.
	for (BYTE byteIdAuthCounter = 0; byteIdAuthCounter < sizeof(v.IdentifierAuthority.Value) / sizeof(*v.IdentifierAuthority.Value); ++byteIdAuthCounter) {
		int val = v.IdentifierAuthority.Value[byteIdAuthCounter];
		sprintf_s(buf, "%d", val);
		for (i = 0; buf[i] && p < end; i++) {
			sprintf_s(p, 3, "%.2x", buf[i]);
			p += 2;
		}
	}

	// Sub-authority.
	for (BYTE byteSubAuthCounter = 0; byteSubAuthCounter < v.SubAuthorityCount; ++byteSubAuthCounter) {
		for (size_t sizeAuthCounter = 0; sizeAuthCounter < sizeof(v.SubAuthority) / sizeof(*v.SubAuthority); ++sizeAuthCounter) {
			int val = v.SubAuthority[byteSubAuthCounter];
			sprintf_s(buf, "%d", val);
			for (i = 0; buf[i] && p < end; i++) {
				sprintf_s(p, 3, "%.2x", buf[i]);
				p += 2;
			}
		}
	}
}

int _V4VIO_CALLCONV file_io_read(void *priv, void *buf, uint32_t count)
{
	int fd = *((int *) priv);
	size_t offset = 0;
	while (offset < count) {
		int r = _read(fd, (char *)buf + offset, count - offset);
		if (r <= 0) return -1;
		offset += r;
	}
	return 0;
}

int _V4VIO_CALLCONV file_io_write(void *priv, const void *buf, uint32_t count)
{
	int fd = *((int *) priv);
	uint32_t offset = 0;
	while (offset < count) {
		int r = _write(fd, (const char *)buf + offset, count - offset);
		if (r <= 0) return -1;
		offset += r;
	}
	return 0;
}

void _V4VIO_CALLCONV io_debug(void *priv, const char *s)
{
	if (priv == NULL)
	{
		fprintf(stderr, "debug: %s\n", s);
	}
	else
	{
		fprintf(stderr, s);
	}
}

template <typename T> void NoOpDeleter(T p) { printf("NoOp 0x%08x\n", p); }


namespace MessageInfo
{
	typedef void (_V4VIO_CALLCONV * pfn_io_debug)(void *priv, const char *s);
	static void dumpStuff(pfn_io_debug io_debug, dbus_msg *pmsg, char const * const ptype)
	{
		if (pmsg == NULL)
		{
			if (io_debug != NULL)
			{
				int nFlag = 1;
				io_debug(&nFlag, "Message is null\n");
			}
			else
			{
				//printf("Message is null\n");
			}
		}
		else
		{
			char *pinterface = "<No Interface>";
			char *pmethod = "<No Method>";
			int serial = -1;
			int reply_serial = -1;

			if (pmsg->interface != NULL)
			{
				pinterface = pmsg->interface;
			}
			if (pmsg->method != NULL)
			{
				pmethod = pmsg->method;
			}
			serial = pmsg->serial;
			reply_serial = pmsg->reply_serial;

			/*
	DBUS_TYPE_INVALID = 0,
	DBUS_TYPE_METHOD_CALL = 1,
	DBUS_TYPE_METHOD_RETURN = 2,
	DBUS_TYPE_ERROR = 3,
	DBUS_TYPE_SIGNAL = 4,
			*/
			static char const * const types[] =
			{
	"DBUS_TYPE_INVALID = 0",
	"DBUS_TYPE_METHOD_CALL = 1",
	"DBUS_TYPE_METHOD_RETURN = 2",
	"DBUS_TYPE_ERROR = 3",
	"DBUS_TYPE_SIGNAL = 4",
			};
			static const size_t sizeTypes = sizeof(types) / sizeof(*types);
			char const * ptype_description = "Unknown type";

			if (pmsg->typefield.type < sizeTypes)
			{
				ptype_description = types[pmsg->typefield.type];
			}

			if (io_debug != NULL)
			{
				char szMessage[512];
				sprintf_s(szMessage, "%s: Type=%s. Serial=%d. Reply Serial=%d. Interface=%s. Method=%s\n", ptype, ptype_description, serial, reply_serial, pinterface, pmethod);
				int nFlag = 1;
				// Pass non-null priv for special handling. Hack.
				io_debug(&nFlag, szMessage);
			}
			else
			{
				//printf("%s: Serial=%d. Interface=%s. Method=%s\n", ptype, pmsg->serial, pinterface, pmethod);
			}
		}
	}

	static void dumpMethodSend(pfn_io_debug io_debug, dbus_msg *pmsg)
	{
		int log = 1;
		io_debug(&log, "\n");
		dumpStuff(io_debug, pmsg, "Sending");
	}

	static void dumpMethodReceive(pfn_io_debug io_debug, dbus_msg *pmsg)
	{
		dumpStuff(io_debug, pmsg, "Received");
	}

} // Ends namespace MessageInfo

class DbusMessageVisitor
{
public:
	typedef MessageInfo::pfn_io_debug pfn_io_debug;
protected:
	pfn_io_debug io_debug;

public:
	DbusMessageVisitor(pfn_io_debug io_debug=&::io_debug)
		: io_debug(io_debug)
	{}

private:
	virtual void onMessage(dbus_msg * /*pmsg*/)
	{
	}

public:
	virtual void onMethod(dbus_msg *pmsg)
	{
		MessageInfo::dumpStuff(io_debug, pmsg, "Method");
		this->onMessage(pmsg);
	}

	virtual void onSignal(dbus_msg *pmsg)
	{
		MessageInfo::dumpStuff(io_debug, pmsg, "Signal");
		this->onMessage(pmsg);
	}

	virtual void onMethodReturn(dbus_msg *pmsg)
	{
		MessageInfo::dumpStuff(io_debug, pmsg, "Method Return");
		this->onMessage(pmsg);
	}

	virtual void onDefault(dbus_msg *pmsg)
	{
		MessageInfo::dumpStuff(io_debug, pmsg, "Dunno");
		this->onMessage(pmsg);
	}

	virtual void onError(int error)
	{
		char szError[512];
		sprintf_s(szError, "Error: %d\n", error);
		int nFlag = 1;
		io_debug(&nFlag, szError);
	}

	virtual bool quContinue()
	{
		return true;
	}

}; // Ends class DbusMessageVisitor

class DbusMessageVisitorSerial : public DbusMessageVisitor
{
private:
	uint32_t serial;
	bool bContinue;

public:
	DbusMessageVisitorSerial(int serial, pfn_io_debug io_debug=&::io_debug)
		: DbusMessageVisitor(io_debug)
		, serial(serial)
		, bContinue(true)
	{
	}

	DbusMessageVisitorSerial(dbus_msg *pmsg, pfn_io_debug io_debug=&::io_debug)
		: DbusMessageVisitor(io_debug)
		, serial (pmsg != NULL ? pmsg->serial : 0)
		, bContinue(true)
	{
	}

	// DbusMessageVisitor overrides
	virtual bool quContinue()
	{
		return this->bContinue;
	}

private:
	virtual void onMessage(dbus_msg *pmsg)
	{
		if (pmsg->reply_serial == this->serial) // If found serial
		{
			this->bContinue = false;
		}
	}

}; // Ends class DbusMessageVisitorSerial

class DbusMessageVisitorXen : public DbusMessageVisitor
{
	virtual void onSignal(dbus_msg *pmsg)
	{
		static char const szStorageSpaceLow[] = "storage_space_low";
		char szExtraInfo[128] = "Signal";

		// Use the IDL files to work out what we should be doing.
		if (strncmp(pmsg->method, szStorageSpaceLow, sizeof(szStorageSpaceLow) / sizeof(*szStorageSpaceLow)) == 0)
		{
			int32_t percentage = 0;
			int r = dbus_msg_body_get_int32(pmsg, &percentage);
			if (r == 0)
			{
				sprintf_s(szExtraInfo, "Signal. Left: %d%s", percentage, "%");
			}

		} // Ends if storage space low
		MessageInfo::dumpStuff(io_debug, pmsg, szExtraInfo);
	}

}; // Ends class DbusMessageVisitorXen

// Loop until visitor says stop.
dbus_msg * loop_find_dbus(dbus_io &dio, DbusMessageVisitor &visitor)
{
	dio.io_debug(dio.logpriv, "Looping to find message...\n");
	int r = 0;
	dbus_msg *pmsg = NULL;
	while (r == 0 && visitor.quContinue())
	{
		if (pmsg)
		{
			dbus_msg_free(pmsg);
			pmsg = NULL;
		}

		r |= dbus_msg_recv(&dio, &pmsg);

		if (r == 0) // If got message ok
		{
			switch (pmsg->typefield.type)
			{
				case DBUS_TYPE_METHOD_CALL:
					visitor.onMethod(pmsg);
					break;
				case DBUS_TYPE_SIGNAL:
					visitor.onSignal(pmsg);
					break;
				case DBUS_TYPE_METHOD_RETURN:
					visitor.onMethodReturn(pmsg);
					break;
				default:
					visitor.onDefault(pmsg);
					break;
			}

		} // Ends if got message ok
		else
		{
			visitor.onError(r);
			char szMessage[64];
			sprintf_s(szMessage, "Houston, we have a problem finding: %d\n", r);
			dio.io_debug(dio.logpriv, szMessage);
		}
	}
	dio.io_debug(dio.logpriv, "Finished looping to find message...\n");

	return pmsg;
}

void loop_dbus(dbus_io &dio, DbusMessageVisitor &visitor)
{
	dio.io_debug(dio.logpriv, "Looping...\n");
	int r = 0;
	dbus_msg *pmsg = NULL;
	while (r == 0 && visitor.quContinue())
	{
		if (pmsg)
		{
			dbus_msg_free(pmsg);
			pmsg = NULL;
		}
		r |= dbus_msg_recv(&dio, &pmsg);
		if (r == 0) // If got message ok
		{
			switch (pmsg->typefield.type)
			{
				case DBUS_TYPE_METHOD_CALL:
					visitor.onMethod(pmsg);
					break;
				case DBUS_TYPE_SIGNAL:
					visitor.onSignal(pmsg);
					break;
				case DBUS_TYPE_METHOD_RETURN:
					visitor.onMethodReturn(pmsg);
					break;
				default:
					visitor.onDefault(pmsg);
					break;
			}

		} // Ends if got message ok
		else
		{
			visitor.onError(r);
			char szMessage[64];
			sprintf_s(szMessage, "Houston, we have a problem: %d\n", r);
			dio.io_debug(dio.logpriv, szMessage);
		}
	}
	dio.io_debug(dio.logpriv, "Finished looping...\n");
}

template <class T, class D=void(_V4VIO_CALLCONV *)(T *)>
class KillIt
{
	T *ptr;
	D del;
private:
	template <class TD>
	static void _V4VIO_CALLCONV Die(TD *ptr)
	{
		delete ptr;
	}
public:
	KillIt()
		: ptr(NULL)
		, del(D())
	{}

	template <class TX>
	explicit KillIt(TX *p)
		: ptr(p)
		, del(&Die<TX *>)
	{
	}

	template <class TX, class DX>
	KillIt(TX *p, DX fptr)
		: ptr(p)
		, del(fptr)
	{
	}

	T * get() { return this->ptr; }
	T * release()
	{
		T *tempptr = this->ptr;
		this->ptr = NULL;
		return tempptr;
	}
	void reset(T *preset)
	{
		D tempdel = this->del;

		this->Destroy();

		this->ptr = preset;
		this->del = tempdel;
	}

private:
	void Destroy()
	{
		if (this->del)
		{
			D tempdel = this->del;
			T *tempptr = this->ptr;
			if (this->ptr != NULL)
			{
				this->ptr = NULL;
			}
			this->del = D();

			if (tempptr != NULL)
			{
				tempdel(tempptr);
			}
		}
		else
		{
			this->ptr = NULL;
		}
	}
public:
	~KillIt()
	{
		this->Destroy();
	}
}; // Ends class KillIt

int run(int /*argc*/, _TCHAR* /*argv*/[])
{
	int r = 0;
	// Sadly, can't deduce deleter type from declaration. Booo.
	//std::tr1::unique_ptr<TransportData, bool (_V4VIO_CALLCONV *)(TransportData *)> ptrtransportdata (connect_v4v_socket(&io_debug), &disconnect_v4v_socket);
	KillIt<TransportData, bool (_V4VIO_CALLCONV *)(TransportData *)> ptrtransportdata (connect_v4v_socket(&io_debug), &disconnect_v4v_socket);
	char hexencoded_uid[32];

	char authline[256];

	dbus_msg *msg = NULL, *recv = NULL;
	dbus_io dio;
	int serial = 22;//1;

#ifdef _WIN32
	int uid = 0;
	hexencode(hexencoded_uid, uid);
#else // !_WIN32
	int uid;
	uid = getuid();
	
	hexencode(hexencoded_uid, uid);

	fd = dbus_connect_session();
	if (fd == -1) {
		printf("failed connection to session bus\n");
		exit(1);
	}
#endif // _WIN32

	int logpriv = 1;
	dio.io_read = &v4v_io_read;
	dio.io_write = &v4v_io_write;
	dio.io_debug = &io_debug;
  dio.io_get_fd = 0;
  dio.next_serial = serial + 1;
	dio.logpriv = &logpriv;
	dio.priv = ptrtransportdata.get();

	sprintf_s(authline, "EXTERNAL %s", hexencoded_uid);

	dbus_auth(&dio, authline);

	msg = dbus_msg_new_method_call(serial++,
	                               "org.freedesktop.DBus", "/org/freedesktop/DBus",
	                               "org.freedesktop.DBus", "Hello");
	if (!msg) {
		exit(1);
	}
	MessageInfo::dumpMethodSend(&io_debug, msg);
	dbus_msg_send(&dio, msg);
	dbus_msg_recv(&dio, &recv);
	MessageInfo::dumpMethodReceive(&io_debug, recv);
	dbus_msg_free(msg);
	dbus_msg_free(recv);

#define _GETVARIANT
#ifdef _GETVARIANT

	//dbus-send --system --print-reply --reply-timeout=120000 --type=method_call
	//--dest=com.citrix.xenclient.xenmgr / org.freedesktop.DBus.Properties.Get
	//string:com.citrix.xenclient.xenmgr.config string:iso-path
	msg = dbus_msg_new_method_call(serial++,
	                               "com.citrix.xenclient.xenmgr", "/",
	                               "org.freedesktop.DBus.Properties", "Get");
	dbus_sig sigGetProperty;
	sigGetProperty.a[0] = DBUS_STRING;
	sigGetProperty.a[1] = DBUS_STRING;
	sigGetProperty.a[2] = DBUS_INVALID;
	dbus_msg_set_signature(msg, &sigGetProperty);
	dbus_msg_body_add(msg, 4096);
	r |= dbus_msg_body_add_string(msg, "com.citrix.xenclient.xenmgr.config");
	r |= dbus_msg_body_add_string(msg, "iso-path");
	r |= dbus_msg_send(&dio, msg);
	DbusMessageVisitorSerial visitProperty(msg);
	dbus_msg_free(msg);

	recv = loop_find_dbus(dio, visitProperty);
	KillIt<dbus_msg> ptrRecv(recv, dbus_msg_free);
	MessageInfo::dumpMethodReceive(&io_debug, recv);

	if (recv != NULL)
	{
		char *isopath;
		dbus_sig sigVariant;
		sigVariant.a[0] = DBUS_INVALID;
		r |= dbus_msg_body_get_variant(recv, &sigVariant);
		r |= dbus_msg_body_get_string(recv, &isopath);

		if (r == 0)
		{
			char szMessage[512];
			sprintf_s(szMessage, "Here is isopath: %s\n", isopath);
			printf("Here is isopath: %s\n", isopath);
			dio.io_debug(dio.logpriv, szMessage);
		}
	}

#endif // _GETVARIANT

#define _LISTNAMES
#ifdef _LISTNAMES
	msg = dbus_msg_new_method_call(serial++,
	                               "org.freedesktop.DBus", "/org/freedesktop/DBus",
	                               "org.freedesktop.DBus", "ListNames");
	if (!msg) {
		exit(1);
	}
	MessageInfo::dumpMethodSend(&io_debug, msg);
	dbus_msg_send(&dio, msg);
	dbus_msg_free(msg);

	dbus_msg_recv(&dio, &recv);
	//std::shared_ptr<dbus_msg> ptrRecv(recv, dbus_msg_free);
	//KillIt<dbus_msg> ptrRecv(recv, dbus_msg_free);
	ptrRecv.reset(recv);
	MessageInfo::dumpMethodReceive(&io_debug, recv);

	if (recv != NULL)
	{
	// Loop over message to see contents.
		dbus_array_reader array_reader = {0};
		r |= dbus_msg_body_get_array(recv, DBUS_STRING, &array_reader);
		int counter = 0;
		char szMessage[512];

		while(dbus_msg_body_get_array_left(recv, &array_reader) > 0)
		{
			char *val = NULL;
			r |= dbus_msg_body_get_string(recv, &val);
			if (r != 0 || val == NULL)
			{
				dio.io_debug(dio.logpriv, "Oops\n");
				break;
			}
			//sprintf_s(szMessage, "Here is string[%d]: %s\n", counter, val);
			//printf("Here is string[%d]: %s\n", counter, val);
			//dio.io_debug(dio.logpriv, szMessage);
		
			++counter;
		}
		sprintf_s(szMessage, "ListNames received %d names\n", counter);
		dio.io_debug(dio.logpriv, szMessage);
	}
#endif // _LISTNAMES

//#define _SWITCHVM
#ifdef _SWITCHVM
	// Send switch vm.
	dio.io_debug(dio.logpriv, "Prepare to switch\n");
	_getch();
	msg = dbus_msg_new_method_call(serial++,
		"com.citrix.xenclient.xenmgr", "/vm/00000000_0000_0000_0000_000000000001",//"/vm/e416b82d_c52a_4dbf_b29c_63c5bfe167a5",
		"com.citrix.xenclient.xenmgr.vm", "switch");
	MessageInfo::dumpMethodSend(&io_debug, msg);
	r |= dbus_msg_send(&dio, msg);
#endif // _SWITCHVM

	DbusMessageVisitorXen visitor;

#define _HANDLENOTIFICATION
#ifdef _HANDLENOTIFICATION
	// Try handling a notification

	r = 0;
	msg = NULL;
	msg = dbus_msg_new_method_call(serial++,
		"org.freedesktop.DBus", "/org/freedesktop/DBus",
		"org.freedesktop.DBus", "AddMatch");
	if (!msg) {
		dio.io_debug(dio.logpriv, "Unable to create method message for AddMatch\n");
		exit(1);
	}
	MessageInfo::dumpMethodSend(&io_debug, msg);
	dbus_sig signature;
	signature.a[0] = DBUS_STRING;
	signature.a[1] = DBUS_INVALID;
	dbus_msg_set_signature(msg, &signature);
	dbus_msg_body_add(msg, 4096);
	//r |= dbus_msg_body_add_string(msg, "type='signal',interface='com.citrix.xenclient.xenmgr.host'");
	//r |= dbus_msg_body_add_string(msg, "type='method_call'");
	r |= dbus_msg_body_add_string(msg, "type='signal',interface='com.citrix.xenclient.xenmgr.host'");
	r |= dbus_msg_send(&dio, msg);
#endif //_HANDLENOTIFICATION

	loop_dbus(dio, visitor);
	// Not really necessary but nice to make it explicit for debugging.
	TransportData *ptransportdata = ptrtransportdata.release();
	disconnect_v4v_socket(ptransportdata);
	dio.io_debug(dio.logpriv, "Hit me\n");
	_getch();
	return 0;
}

void printsizes()
{
	printf("Size of dbus_msg: %d\n", sizeof(dbus_msg));
	printf("Size of dbus_sig: %d\n", sizeof(dbus_sig));
	printf("Size of dbus_reader: %d\n", sizeof(dbus_reader));
	printf("Size of dbus_writer: %d\n", sizeof(dbus_writer));
	printf("Size of dbus_array_writer: %d\n", sizeof(dbus_array_writer));
	printf("Size of : dbus_array_reader%d\n", sizeof(dbus_array_reader));
	dbus_msg foo = {0};
	printf("Size of dbus_msg::typefield: %d\n", sizeof(foo.typefield));
	printf("Size of dbus_msg::comms: %d\n", sizeof(foo.comms));
	//printf("Size of : %d\n", sizeof());
	//printf("Size of : %d\n", sizeof());
}

int _tmain(int argc, _TCHAR* argv[])
{
	//testV4V();
	//return 0;

	//printsizes()
	//return 0;
	return run(argc, argv);
}

