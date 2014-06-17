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

#ifndef UDBUS_H
#define UDBUS_H

// v4v pulls in inc/types.h which defines uint8_t et al.
// udbus doesn't want to rely on that by default, but will support them if available.
#ifdef _UDBUS_USE_XEN_TYPES
#	include <types.h>
#else // !_UDBUS_USE_XEN_TYPES
#	include <stdint.h>
#endif // _UDBUS_USE_XEN_TYPES

#ifndef _WIN32
#	#define _UDBUS_CALLCONV

#	define _UDBUS_API

#	include <stdbool.h>

#else //_WIN32
// Default C# interop binding is __stdcall.
#	define _UDBUS_CALLCONV __stdcall

// Compile as static lib or DLL.
// External code using static lib must define _UDBUS_STATICLIB.
// Otherwise expect to see errors such as:
//  somefile.obj : error LNK2001: unresolved external symbol __imp__dbus_msg_recv@8

// Default to static library.
#if !defined _UDBUS_EXPORTS && !defined _UDBUS_DLL && !defined _UDBUS_STATICLIB
#	define _UDBUS_STATICLIB
#endif // !defined _UDBUS_EXPORTS && !defined _UDBUS_DLL && !defined _UDBUS_STATICLIB

// _UDBUS_EXPORTS ? DLL export
// _UDBUS_DLL ? DLL import
// _UDBUS_STATICLIB ? STATICLIB
#ifdef _UDBUS_EXPORTS
#	define _UDBUS_API __declspec(dllexport)
#else // !_UDBUS_EXPORTS
#	ifdef _UDBUS_STATICLIB
#		define _UDBUS_API
#	elif _UDBUS_DLL // !_UDBUS_STATICLIB
#		define _UDBUS_API __declspec(dllimport)
# else // !_UDBUS_DLL
// Should never happen.
#		error Udbus library linker type not defined. Define _UDBUS_EXPORTS (DLL export), _UDBUS_DLL (DLL import), or _UDBUS_STATICLIB (Static library).
#	endif // _UDBUS_DLL
#endif // _UDBUS_EXPORTS

#	ifndef __cplusplus
	typedef int bool;
#	endif // __cplusplus
#endif // _WIN32

#	ifdef __cplusplus
extern "C" {
#	endif // __cplusplus

#define UDBUS_VERSION_MAJOR 0
#define UDBUS_VERSION_MINOR 9

typedef enum {
	DBUS_SIGNATURE,
	DBUS_OBJECTPATH,
	DBUS_BOOLEAN,
	DBUS_BYTE,
	DBUS_STRING,
	DBUS_INT16,
	DBUS_UINT16,
	DBUS_INT32,
	DBUS_UINT32,
	DBUS_INT64,
	DBUS_UINT64,
	DBUS_DOUBLE,
	DBUS_ARRAY,
	DBUS_VARIANT,
	DBUS_STRUCT_BEGIN,
	DBUS_STRUCT_END,
	DBUS_DICT_BEGIN,
	DBUS_DICT_END,
	DBUS_INVALID = -1,
} dbus_type;

typedef struct {
	dbus_type a[256];
} dbus_sig;

struct dbus_reader {
	uint8_t *data;
	uint32_t align_offset;
	uint32_t offset;
	uint32_t length;
	int endianness; /* 0 = little endian, 1 = big endian */
};

struct dbus_writer {
	uint8_t *buffer;
	uint32_t offset;
	uint32_t length;
	int endianness; /* 0 = little endian, 1 = big endian */
};

struct dbus_header {
	uint8_t endianness; /* 0 = little endian, 1 = big endian */
	uint8_t messagetype;
	uint8_t flags;
	uint8_t ver;
	uint32_t bodylen;
	uint32_t serial;
	uint32_t fieldslen;
};

typedef struct { uint32_t *ptr; uint32_t offset; } dbus_array_writer;
typedef struct { uint32_t length; uint32_t offset; } dbus_array_reader;

typedef enum {
	DBUS_FIELD_INVALID = 0,
	DBUS_FIELD_PATH = 1,
	DBUS_FIELD_INTERFACE = 2,
	DBUS_FIELD_MEMBER = 3,
	DBUS_FIELD_ERROR_NAME = 4,
	DBUS_FIELD_REPLY_SERIAL = 5,
	DBUS_FIELD_DESTINATION = 6,
	DBUS_FIELD_SENDER = 7,
	DBUS_FIELD_SIGNATURE = 8,
	DBUS_FIELD_UNIX_FDS = 9,
} dbus_field_type;

typedef enum {
	DBUS_TYPE_INVALID = 0,
	DBUS_TYPE_METHOD_CALL = 1,
	DBUS_TYPE_METHOD_RETURN = 2,
	DBUS_TYPE_ERROR = 3,
	DBUS_TYPE_SIGNAL = 4,
} dbus_msg_type;

typedef struct {
	union {
		uint8_t type;
		int _dummy;	 // For padding.
	} typefield;
	uint32_t serial;
	char *destination;
	char *path;
	char *interface;
	char *method;
	char *error_name;
	char *sender;
	dbus_sig signature;
	uint32_t reply_serial;
	int w;
	uint8_t *body;
	union {
		struct dbus_writer writer; /* writing body */
		struct dbus_reader reader; /* reading body */
	} comms;
} dbus_msg;

typedef struct {
	int state;
	/* s1 */
	uint8_t headerdata[16];
	int headeroff;
	/* s2 */
	struct dbus_header header;
	/* s3 */
	uint8_t *fielddata; /* alloc'ed */
	int fieldoff;
	/* s4 */
	dbus_msg *msg;
	uint32_t bodyoff;
} dbus_eventpart;

/* vectorise IO operations so that user can decide about buffering
 * and how to read/write to whatever is providing the data (channel, handle, etc)
 */
typedef struct _dbus_io {
	/* return 0 if succeed to write count bytes, non-0 otherwise */
	int (_UDBUS_CALLCONV *io_write)(void *priv, const void *buf, uint32_t count);
	/* return number of bytes read negative on error */
	int (_UDBUS_CALLCONV *io_read)(void *priv, void *buf, uint32_t count);
	/* debugging logging */
	void (_UDBUS_CALLCONV *io_debug)(void *logpriv, const char *buf);
	/* retrieve fd for select operation */
	int (_UDBUS_CALLCONV *io_get_fd)(void *priv);
	/* private pointer passed to write/read (eg. handle, channel, buffer, etc) */
	void *priv;
	/* private pointer passed for debugging */
	void *logpriv;
	/* next serial */
	int next_serial;
} dbus_io;

_UDBUS_API void _UDBUS_CALLCONV      dbus_msg_free(dbus_msg *msg);
_UDBUS_API dbus_msg * _UDBUS_CALLCONV dbus_msg_new(uint32_t serial);
_UDBUS_API dbus_msg * _UDBUS_CALLCONV dbus_msg_new_method_call(uint32_t serial, const char *destination, const char *path,
                                                               const char *interface, const char *method);
_UDBUS_API dbus_msg * _UDBUS_CALLCONV dbus_msg_new_signal(uint32_t serial, const char *path, const char *interface, const char *name);

_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_destination(dbus_msg *msg, const char *destination);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_path(dbus_msg *msg, const char *path);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_method(dbus_msg *msg, const char *method);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_error_name(dbus_msg *msg, const char *error_name);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_sender(dbus_msg *msg, const char *sender);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_interface(dbus_msg *msg, const char *interface);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_set_signature(dbus_msg *msg, dbus_sig *signature);

/* create a body buffer of specified length.
 BEWARE: need to be called before adding any elements to the body */
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add(dbus_msg *msg, uint32_t length);

/* method to add differents types of elements to the body. */
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_byte       (dbus_msg *msg, uint8_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_boolean    (dbus_msg *msg, bool val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_int16      (dbus_msg *msg, int16_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_uint16     (dbus_msg *msg, uint16_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_int32      (dbus_msg *msg, int32_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_uint32     (dbus_msg *msg, uint32_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_int64      (dbus_msg *msg, int64_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_uint64     (dbus_msg *msg, uint64_t val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_double     (dbus_msg *msg, double val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_string     (dbus_msg *msg, const char *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_objectpath (dbus_msg *msg, const char *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_array_begin(dbus_msg *msg, dbus_type element, dbus_array_writer *ptr);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_array_end  (dbus_msg *msg, dbus_array_writer *ptr);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_structure  (dbus_msg *msg);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_add_variant    (dbus_msg *msg, dbus_sig *signature);

/* methods to introspect a received message body for all different types */
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_byte        (dbus_msg *msg, uint8_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_boolean     (dbus_msg *msg, bool *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_int16       (dbus_msg *msg, int16_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_uint16      (dbus_msg *msg, uint16_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_int32       (dbus_msg *msg, int32_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_uint32      (dbus_msg *msg, uint32_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_int64       (dbus_msg *msg, int64_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_uint64      (dbus_msg *msg, uint64_t *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_double      (dbus_msg *msg, double *val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_string      (dbus_msg *msg, char **val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_object_path (dbus_msg *msg, char **val);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_array       (dbus_msg *msg, dbus_type element, dbus_array_reader *ptr);
_UDBUS_API uint32_t _UDBUS_CALLCONV dbus_msg_body_get_array_left  (dbus_msg *msg, dbus_array_reader *ptr);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_structure   (dbus_msg *msg);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_body_get_variant     (dbus_msg *msg, dbus_sig *signature);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_body_free_string     (char *val);
_UDBUS_API void _UDBUS_CALLCONV dbus_msg_body_free_object_path(char *val);

_UDBUS_API int _UDBUS_CALLCONV dbus_msg_send(dbus_io *dio, dbus_msg *msg);
_UDBUS_API int _UDBUS_CALLCONV dbus_msg_recv(dbus_io *dio, dbus_msg **msg);

/* event based recv */
_UDBUS_API int _UDBUS_CALLCONV dbus_event_init(dbus_eventpart *evpart);
_UDBUS_API int _UDBUS_CALLCONV dbus_event_has_message(dbus_eventpart *evpart);
_UDBUS_API int _UDBUS_CALLCONV dbus_event_recv(dbus_io *dio, dbus_eventpart *evpart, dbus_msg **msg);
_UDBUS_API int _UDBUS_CALLCONV dbus_event_get_fd(dbus_io *dio);

/* connection method */
_UDBUS_API int _UDBUS_CALLCONV dbus_connect_session(void);
_UDBUS_API int _UDBUS_CALLCONV dbus_connect_system (void);

/* auth handshake */
_UDBUS_API int _UDBUS_CALLCONV dbus_auth(dbus_io *dio, char *auth);

#	ifdef __cplusplus
} // Ends extern "C"
#	endif // __cplusplus
#endif
