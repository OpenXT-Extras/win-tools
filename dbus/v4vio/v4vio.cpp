/*
 * Copyright (c) 2013 Citrix Systems, Inc.
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

// v4vio.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "v4vio.h"

// v4vio.cpp : Defines the exported functions for the DLL application.
//

#include <WinIOCtl.h>
#ifdef _WIN32
#	ifdef _UDBUS_USE_XEN_TYPES
#		include <types.h>
#	else // !_UDBUS_USE_XEN_TYPES
#		define _XEN_WIN_TYPES_H_	// We don't want the inc/types.h to be used.
#		include <stdint.h>			// This will pull in the appropriate types on windows. If the types don't match up, we have a serious issue.
#	endif // _UDBUS_USE_XEN_TYPES
#pragma warning (push)
#pragma warning (disable: 4127 4505)
//  warning C4127: conditional expression is constant
//  warning C4505: unreferenced local function has been removed
#endif // _WIN32

#include "v4vapi.h"

#ifdef _WIN32
#pragma warning (pop)
#endif // _WIN32

#include <vector>
#include "v4vio.h"
#ifdef _WIN32
#include <iterator> // stdext::checked_array_iterator

//#define STD_COPY(_macrobegin, _macroend, _macrotarget, _macrosize) std::copy(_macrobegin, _macroend, _macrotarget)stdext::checked_array_iterator i;
#endif // _WIN32

namespace
{
  static ULONG const XHT_DEFAULT_RING_SIZE = 131072; //4096;
  static ULONG const XHT_MAX_WRITE_SIZE = XHT_DEFAULT_RING_SIZE / 2;
  static ULONG const XHT_DEFAULT_TIMEOUT   = 20000; // ms
  static ULONG const XHT_NUMBER_BUF_SIZE   = 64;

  static v4v_ring_id_t const ringIdEmpty = { 0 };
  static v4v_addr_t const addrEmpty = { 0 };

  using std::copy;

  class v4v_exception_with_message : public std::exception
  {
  public:
    v4v_exception_with_message(char const * pwhat)
      : std::exception(pwhat)
    {}

    v4v_exception_with_message()
      : std::exception()
    {}

    v4v_exception_with_message(v4v_exception_with_message const &other)
      : std::exception(other)
    {
    }

    virtual ~v4v_exception_with_message()
    {
    }

    v4v_exception_with_message & operator=(v4v_exception_with_message &other)
    {
      std::exception::operator=(other);
      return *this;
    }

  }; // Ends v4v_exception_with_message

#ifndef _WIN32
  // Copy implementation that avoids warning C4996
  // 4996 -  Function call with parameters that may be unsafe
  template<class _InIt,
  class _OutIt> inline
  _OutIt *copy(_InIt _First, _InIt _Last,
    _OutIt *_Dest, )
  {	// copy [_First, _Last) to [_Dest, ...)
    // Use Microsoft extension's checked_array_iterator
    return std::copy(_First, _Last
      ,stdext::checked_array_iterator<_OutIt *>(_Dest, _Last - _First + 1)).operator->();
  }

#else // _WIN32
  // Copy with size.
  // In standard case, ignore size since iterator will be bounded.
  template<class _InIt,
  class _OutIt> inline
  _OutIt sizedcopy(_InIt _First, _InIt _Last,
    _OutIt _Dest,
    size_t /*_Size*/)
  {
    return std::copy(_First, _Last, _Dest);
  }

  // Specialise for pointers.
  template<class _InIt,
  class _OutIt> inline
  _OutIt *sizedcopy(_InIt _First, _InIt _Last,
    _OutIt *_Dest,
    size_t _Size)
  {	// copy [_First, _Last) to [_Dest, ...)
    // Use Microsoft extension's checked_array_iterator.
    // Avoids warning C4996.
    // 4996 -  Function call with parameters that may be unsafe
    stdext::checked_array_iterator <_OutIt *>checkedIter (_Dest, _Size);
    return std::copy(_First, _Last
      ,stdext::checked_array_iterator <_OutIt *>(_Dest, _Size)).operator->();
  }

#endif // _WIN32

// Nicked and altered from XenHTTP.cpp in win-tools.git\XenGuestAgent
class CV4VTransport
{
private:
  typedef BOOL (WINAPI* CancelIoEx_t)(__in HANDLE hFile, __in_opt LPOVERLAPPED lpOverlapped);
  static OVERLAPPED const initOverlapped;

  V4V_CONTEXT    *m_pcontext;
  ULONG           m_ulRingSize;
  v4v_ring_id_t   m_ringId;
  v4v_addr_t      m_addr;
  bool            m_bOpen;
  bool            m_bOwnsContext;
  bool            m_bCancelled;
  HMODULE         m_k32;
  CancelIoEx_t    m_CancelIoExFn;
  bool            m_bOverlapped;
  OVERLAPPED      m_overlapped;
  HANDLE          m_heventOverlappedRead;
  HANDLE          m_heventOverlappedWrite;
  HANDLE          m_heventOverlappedStop;

public:
  inline OVERLAPPED * getOverlapped() { return m_pcontext != NULL && ((m_pcontext->flags & V4V_FLAG_OVERLAPPED) == V4V_FLAG_OVERLAPPED) ? &m_overlapped : NULL; }
  inline HANDLE getOverlappedReadEventHandle() { return m_heventOverlappedRead; }
  inline HANDLE getOverlappedWriteEventHandle() { return m_heventOverlappedWrite; }
  inline HANDLE getOverlappedStopEventHandle() { return m_heventOverlappedStop; }

  static bool checkAsyncOverlappedResult(V4V_CONTEXT const * const pcontext, OVERLAPPED * const poverlapped)
  {
    bool bContinue = true;
    if (poverlapped != NULL && poverlapped->hEvent != NULL) // If using overlapped io
    {
      DWORD status = WaitForSingleObject(poverlapped->hEvent, INFINITE);
      bContinue = status == WAIT_OBJECT_0;

      if (bContinue) // If waiting 
      {
        DWORD bytes = 0;
        bContinue = GetOverlappedResult(pcontext->v4vHandle, poverlapped, &bytes, FALSE) != FALSE;

      } // Ends if waiting
    } // Ends if using overlapped io

    return bContinue;
  }

public:
  CV4VTransport()
    : m_pcontext(NULL)
    , m_ulRingSize(0)
    , m_ringId(ringIdEmpty)
    , m_addr(addrEmpty)
    , m_bOpen(false)
    , m_bOwnsContext(false)
    , m_bCancelled(false)
    , m_k32(NULL)
    , m_CancelIoExFn(NULL)
    , m_bOverlapped(false)
    , m_overlapped(initOverlapped)
    , m_heventOverlappedRead(NULL)
    , m_heventOverlappedWrite(NULL)
    , m_heventOverlappedStop(NULL)
  {
  }

  CV4VTransport(ULONG ulAddr,
                ULONG ulPort,
                ULONG ulRingSize,
                bool bOverlapped=false,
                V4V_CONTEXT *pcontext=NULL)
    : m_pcontext(NULL)
    , m_ulRingSize(ulRingSize)
    , m_bOpen(false)
    , m_bOwnsContext(pcontext == NULL)
    , m_bCancelled(false)
    , m_k32(NULL)
    , m_CancelIoExFn(NULL)
    , m_bOverlapped(bOverlapped)
    , m_overlapped(initOverlapped)
    , m_heventOverlappedRead(NULL)
    , m_heventOverlappedWrite(NULL)
    , m_heventOverlappedStop(NULL)
  {
    if (pcontext == NULL)
    {
      pcontext = new V4V_CONTEXT;
    }
    ::ZeroMemory(pcontext, sizeof(*pcontext));
    pcontext->flags = V4V_FLAG_NONE;

    if (bOverlapped)
    {
      pcontext->flags |= V4V_FLAG_OVERLAPPED;
      HANDLE heventOverlapped = CreateEvent(NULL, FALSE, FALSE, NULL);
      if (heventOverlapped == NULL)
      {
        throw v4v_exception_with_message("Unable to create overlapped event");
      }

      HANDLE heventOverlappedRead = CreateEvent(NULL, FALSE, FALSE, NULL);
      if (heventOverlappedRead == NULL)
      {
        throw v4v_exception_with_message("Unable to create overlapped read event");
      }

      HANDLE heventOverlappedWrite = CreateEvent(NULL, FALSE, FALSE, NULL);
      if (heventOverlappedWrite == NULL)
      {
        throw v4v_exception_with_message("Unable to create overlapped write event");
      }

      HANDLE heventOverlappedStop = CreateEvent(NULL, FALSE, FALSE, NULL);
      if (heventOverlappedStop == NULL)
      {
        throw v4v_exception_with_message("Unable to create overlapped stop event");
      }
        
      m_overlapped.hEvent = heventOverlapped;
      m_heventOverlappedRead = heventOverlappedRead;
      m_heventOverlappedWrite = heventOverlappedWrite;
      m_heventOverlappedStop = heventOverlappedStop;
    }

    m_ringId.partner = (domid_t)ulAddr;
    m_ringId.addr.domain = V4V_DOMID_NONE;
    m_ringId.addr.port = V4V_PORT_NONE;
    m_addr.domain = (domid_t)ulAddr;
    m_addr.port = ulPort;
    m_pcontext = pcontext;
  }

private:
  explicit CV4VTransport(CV4VTransport &src);
  CV4VTransport & operator=(CV4VTransport &src);
public:
  ~CV4VTransport()
  {
    V4V_CONTEXT *pcontext = NULL;
    HMODULE k32 = m_k32;
    HANDLE heventOverlapped = m_overlapped.hEvent;
    HANDLE heventOverlappedRead = m_heventOverlappedRead;
    HANDLE heventOverlappedWrite = m_heventOverlappedWrite;
    HANDLE heventOverlappedStop = m_heventOverlappedStop;

    if (m_bOwnsContext)
    {
      pcontext = m_pcontext;
    }

    this->Close();

    m_pcontext = NULL;
    m_bOwnsContext = false;
    m_k32 = NULL;
    m_overlapped.hEvent = NULL;
    m_heventOverlappedRead = NULL;
    m_heventOverlappedWrite = NULL;
    m_heventOverlappedStop = NULL;

    if (pcontext != NULL)
    {
      delete pcontext;
    }

    if (heventOverlapped != NULL)
    {
      ::CloseHandle(heventOverlapped);
    }

    if (heventOverlappedRead != NULL)
    {
      ::CloseHandle(heventOverlappedRead);
    }

    if (heventOverlappedWrite != NULL)
    {
      ::CloseHandle(heventOverlappedWrite);
    }

    if (heventOverlappedStop != NULL)
    {
      ::CloseHandle(heventOverlappedStop);
    }

    if (k32 != NULL)
    {
      ::FreeLibrary(k32);
    }
  }

  void swap(CV4VTransport &other)
  {
    std::swap(m_pcontext, other.m_pcontext);
    std::swap(m_ulRingSize, other.m_ulRingSize);
    std::swap(m_ringId, other.m_ringId);
    std::swap(m_addr, other.m_addr);
    std::swap(m_bOpen, other.m_bOpen);
    std::swap(m_bOwnsContext, other.m_bOwnsContext);
    std::swap(m_k32, other.m_k32);
    std::swap(m_CancelIoExFn, other.m_CancelIoExFn);
    std::swap(m_bOverlapped, other.m_bOverlapped);
    std::swap(m_overlapped, other.m_overlapped);
    std::swap(m_heventOverlappedRead, other.m_heventOverlappedRead);
    std::swap(m_heventOverlappedWrite, other.m_heventOverlappedWrite);
    std::swap(m_heventOverlappedStop, other.m_heventOverlappedStop);
  }

  V4V_CONTEXT *ReleaseContext()
  {
    V4V_CONTEXT *pcontext = m_pcontext;
    m_pcontext = NULL;
    m_bOpen = false;
    m_bOwnsContext = false;
    return pcontext;
  }

  BOOL Connect()
  {
    bool bOpen = m_bOpen;
    if (bOpen == false) // If not open yet
    {
      OVERLAPPED * const poverlapped = getOverlapped();

      if (V4vOpen(m_pcontext, m_ulRingSize, poverlapped) != FALSE)
      {
        bool bContinue = checkAsyncOverlappedResult(m_pcontext, poverlapped);

        if (bContinue && V4vBind(m_pcontext, &m_ringId, poverlapped) != FALSE)
        {
          bContinue = checkAsyncOverlappedResult(m_pcontext, poverlapped);
          if (bContinue && V4vConnect(m_pcontext, &m_addr, poverlapped) != FALSE)
          {
            bOpen = true;
          }
        }
      }
    } // Ends if not open yet

    m_bOpen = bOpen;
    return bOpen ? TRUE : FALSE;
  }

  inline const V4V_CONTEXT *GetContext()
  {
    return m_pcontext;
  }

  BOOL CancelIoEx (__in HANDLE hFile, __in_opt LPOVERLAPPED lpOverlapped)
  {
    DWORD const dwLastError = GetLastError();

    if (this->m_k32 == NULL)
    {
      this->m_k32 = LoadLibraryA("kernel32.dll");
    }

    if (this->m_k32 != NULL && this->m_CancelIoExFn == NULL)
    {
      this->m_CancelIoExFn = (CancelIoEx_t)GetProcAddress(this->m_k32, "CancelIoEx");
    }

    BOOL result = FALSE;

    if (this->m_CancelIoExFn != NULL)
    {
      result = this->m_CancelIoExFn(hFile, lpOverlapped);
    }
    else if (dwLastError == ERROR_SUCCESS)
    {
      ::SetLastError(ERROR_CALL_NOT_IMPLEMENTED);
    }
    return result;
  }

  bool Cancel()
  {
    bool result = true;
    if (m_bOpen) // If open
    {
      m_bCancelled = true;
      DWORD const dwLastError = GetLastError();

      // Cancel any current IO.
      result = CancelIoEx(m_pcontext->v4vHandle, NULL) != FALSE;

      // Tell overlapped IO to stop.
      if (m_heventOverlappedStop != NULL)
      {
        SetEvent(m_heventOverlappedStop);
      }

      if (result == FALSE) // If failed to cancel
      {
        if (GetLastError() == ERROR_NOT_FOUND) // If no pending io
        {
          // Restore previous error.
          ::SetLastError(dwLastError);
          result = true;

        } // Ends if no pending io
      } // Ends if failed to cancel
      else // Else cancel succeeded
      {
        // Restore previous error.
        ::SetLastError(dwLastError);

      } // Ends else cancel succeeded

      // Ensure that any current reads stop blocking.
      ::SetEvent(m_pcontext->recvEvent);

    } // Ends if open

    return result;
  }

  void Close()
  {
    if (m_bOpen) // If open
    {
      m_bOpen = false;
      V4vClose(m_pcontext);
      ::SetLastError(ERROR_SUCCESS);

    } // Ends if open
  }

  inline bool Cancelled() { return this->m_bCancelled; }

}; // Ends class CV4VTransport

OVERLAPPED const CV4VTransport::initOverlapped = { 0 };

#define HAVE_V4V_SOCKET

namespace std_virtual_alloc
{
  class virtual_bad_alloc : public std::bad_alloc
  {
    char const * pwhat;

  public:
    virtual_bad_alloc(char const * pwhat)
      : std::bad_alloc()
      , pwhat(pwhat)
    {
    }

    virtual_bad_alloc()
      : std::bad_alloc()
      , pwhat(0)
    {
    }

    virtual_bad_alloc(virtual_bad_alloc const &other)
      : std::bad_alloc(other)
      , pwhat(other.pwhat)
    {
    }
    virtual const char *what() const { return this->pwhat != 0 ? this->pwhat : std::bad_alloc::what(); }

    virtual_bad_alloc & operator= (virtual_bad_alloc const &other)
    {
      std::bad_alloc::operator=(other);
      this->pwhat = other.pwhat;
      return *this;
    }

    virtual ~virtual_bad_alloc()
    {}

  }; // Ends v4v_exception_with_message

  // Stolen from <xmemory>
  // TEMPLATE FUNCTION _Allocate
template<class _Ty> inline
  _Ty *_Allocate(size_t _Count, _Ty *)
  {  // allocate storage for _Count elements of type _Ty
  void *_Ptr = 0;

  if (_Count <= 0)
    _Count = 0;
  else if (((size_t)(-1) / sizeof (_Ty) < _Count)
    || (_Ptr = ::VirtualAlloc(NULL,
                           _Count * sizeof (_Ty),
                            MEM_COMMIT,
                            PAGE_READWRITE)) == 0)
    throw virtual_bad_alloc();  // report no memory

  return ((_Ty *)_Ptr);
 }

#define _VIRTUALALLOCALLOCATOR allocator // Class name will actually be allocator, but that makes code harder to read.

// Allocator which uses VirtualAlloc/VirtualFree
// ripped straight out of VS2010 std::allocator
template<class _Ty>
class _VIRTUALALLOCALLOCATOR
  : protected std::allocator<_Ty>
{ 
public:
  // Hijack existing implementation.
  using std::allocator<_Ty>::pointer;
  using std::allocator<_Ty>::const_pointer;
  using std::allocator<_Ty>::reference;
  using std::allocator<_Ty>::const_reference;
  using std::allocator<_Ty>::size_type;
  using std::allocator<_Ty>::difference_type;
  using std::allocator<_Ty>::value_type;
  using std::allocator<_Ty>::max_size;
  using std::allocator<_Ty>::construct;
  using std::allocator<_Ty>::destroy;

public:
  template<class _Other1>
  struct rebind
  {  // convert this type to an _ALLOCATOR<_Other>
    typedef _VIRTUALALLOCALLOCATOR<_Other1> other;
  };

  // generic allocator for objects of class _Ty
  _VIRTUALALLOCALLOCATOR() _THROW0()
  {  // construct default allocator (do nothing)
  }

  _VIRTUALALLOCALLOCATOR(const _VIRTUALALLOCALLOCATOR<_Ty>& other) _THROW0()
    : std::allocator<_Ty>(other)
  {  // construct by copying (do nothing)
  }

  template<class _Other>
    _VIRTUALALLOCALLOCATOR(const _VIRTUALALLOCALLOCATOR<_Other>& /*other*/) _THROW0()
  {  // construct from a related allocator (do nothing)
  }

  template<class _Other>
  _VIRTUALALLOCALLOCATOR<_Ty>& operator=(const _VIRTUALALLOCALLOCATOR<_Other>& other)
  {  // assign from a related allocator (do nothing)
    return std::allocator<_Ty>::operator=<_Other>(other);
  }

  pointer allocate(size_type _Count)
  {  // allocate array of _Count elements
    return (std_virtual_alloc::_Allocate(_Count, (pointer)0));
  }

  pointer allocate(size_type _Count, const void *)
  {  // allocate array of _Count elements, ignore hint
    return (allocate(_Count));
  }

  void deallocate(pointer _Ptr, size_type)
  {  // deallocate object at _Ptr, ignore size
    //::operator delete(_Ptr);
    if (::VirtualFree(_Ptr, 0, MEM_RELEASE) == FALSE)
    {
      throw virtual_bad_alloc("bad deallocation");
    }
  }
}; // Ends _VIRTUALALLOCALLOCATOR
template <class _Ty1, class _Ty2>
  bool operator== (const _VIRTUALALLOCALLOCATOR<_Ty1> &, const _VIRTUALALLOCALLOCATOR<_Ty2> &) throw() { return true; }
template <class _Ty1, class _Ty2>
  bool operator!= (const _VIRTUALALLOCALLOCATOR<_Ty1> &, const _VIRTUALALLOCALLOCATOR<_Ty2> &) throw() { return false; }

#undef _VIRTUALALLOCALLOCATOR

} // Ends namespace std_virtual_alloc

//template <typename T>
class TransportBuffer
{
  typedef char T;
  // If using OVERLAPPED IO, need VirtualAlloc.
  //typedef std::vector<T>::allocator_type allocator_type;
  typedef std_virtual_alloc::allocator<T> allocator_type;
public:
  typedef std::vector<T, allocator_type> buffer_type;
  typedef buffer_type::size_type size_type;
  typedef buffer_type::value_type value_type;
  typedef buffer_type::iterator iterator;
  typedef buffer_type::const_iterator const_iterator;
private:
  buffer_type buffer;
  size_type posRead;
  size_type posWrite;
  bool bWrap;  ///< Only true if posRead==posWrite and buffer is wrapped.
  char _unused[3];
public:
  //class BufferSpan
  //{
  //	TransportBuffer &transportbuffer;
  //	TransportBuffer::buffer_type &buffer;
  //	TransportBuffer::size_type * TransportBuffer::* ppos;
  //	operator TransportBuffer::value_type * () { return &buffer[*(transportbuffer.*ppos)]; }

  //};
public:
  TransportBuffer(ULONG ulBufferSize=0)
    : buffer(ulBufferSize)
    , posRead(0)
    , posWrite(0)
    , bWrap(false)
  {
    if (ulBufferSize > 0)
    {
      buffer[0] = 0;
    }
  }

  void reset()
  {
    posRead = posWrite = 0;
    bWrap = false;
    if (buffer.empty() == false)
    {
      buffer[0] = 0;
    }
  }

  inline size_type writesize()
  {
    if (posRead == posWrite && !bWrap)
    {
      reset();
    }
    return (posRead <= posWrite ? buffer.size() - (posWrite - posRead) : posRead - posWrite);
  }

  // @return  Number of contiguous write values left.
  inline size_type nowrap_writesize()
  {
    if (posRead == posWrite && !bWrap)
    {
      reset();
    }

    size_type end = buffer.size();
    if (posRead > posWrite)
    {
      end = posRead;
    }
    return end - posWrite;
  }

  inline size_type readsize()
  {
    if (posRead == posWrite && !bWrap)
    {
      reset();
    }
    return (posRead == posWrite && !bWrap
      ? 0
      : (posRead >= posWrite ? (buffer.size() - posRead) + posWrite : posWrite - posRead)
    );
  }

private:
  size_type _writecheck(size_type count)
  {
    size_type const sizeBuffer = buffer.size();
    if (count > sizeBuffer)
    {
      throw std::out_of_range("TransportBuffer<T> write count parameter too large for buffer.");
    }
    size_type const sizeDifference = writesize();

    if (count > sizeDifference) // If writing more than there is space for
    {
      throw std::out_of_range("TransportBuffer<T> write count parameter larger than available write space.");

    } // Ends if writing more than there is space for

    return sizeDifference;
  }

  size_type _readcheck(size_type count)
  {
    size_type const sizeBuffer = buffer.size();
    if (count > sizeBuffer)
    {
      throw std::out_of_range("TransportBuffer<T> read count parameter too large for buffer.");
    }
    size_type const sizeDifference = readsize();

    if (count > sizeDifference) // If reading more than there is space for
    {
      throw std::out_of_range("TransportBuffer<T> read count parameter larger than available read space.");

    } // Ends if reading more than there is space for

    return sizeDifference;
  }

public:
  //!@brief Get access to writeable contiguous buffer.
  //!@param count - Number of continguous values required.
  //!@param sizeWriteBuffer[out] - The size of the current write buffer.
  //!@param bReset[optional]     - If true, and buffer isn't big enough, reset buffer and try again.
  //!@return  Write buffer pointer.
  value_type * writebuffer(size_type count, size_type &sizeWriteBuffer, bool bReset=false)
  {
    value_type *result = NULL;
    if (count > 0) // If writing data
    {
      try
      {
        // Can we write count into buffer as it stands ?
        sizeWriteBuffer = _writecheck(count);
      }
      catch (std::out_of_range &/*exOutOfRange*/)
      {
        if (bReset)
        {
          reset();

          // Can we write count into buffer once we've reset ?
          sizeWriteBuffer = _writecheck(count);
        }
        else
        {
          throw;
        }
      }

      result = &buffer[posWrite];
      posWrite += count;

      bWrap = posWrite == posRead;

    } // Ends if writing data

    return result;
  }

  bool truncate_write(size_type count)
  {
    bool const bTruncate = posWrite - count >= posRead;
  
    if (bTruncate)
    {
      posWrite -= count;
    }
    return bTruncate;
  }

  size_type write(value_type const * in, size_type count)
  {
    size_type sizeWrite = 0;

    if (count > 0) // If data to write
    {
      size_type const sizeBuffer = buffer.size();
      /*size_type const sizeDifference = */_writecheck(count);

      // Writing n bytes.
      // If write past end of buffer, continue at beginning, up to point we're at now.
      size_type remainder = sizeBuffer - posWrite;
      iterator begin = buffer.begin() + posWrite;

      if (count > remainder) // If more to write than what's in remainder
      {
        sizeWrite = sizedcopy(in, in + remainder, begin, count + 1) - begin;

        // Wrap.
        in += remainder;
        begin = buffer.begin();
        posWrite = 0;
        count -= remainder;

      } // Ends if more to write than what's in remainder

      size_type const sizeCopy = sizedcopy(in, in + count, begin, count + 1) - begin;
      sizeWrite += sizeCopy;
      posWrite += sizeCopy;

      this->bWrap = (posWrite == posRead);

    } // Ends if data to write

    return sizeWrite;
  }

  size_type read(value_type *out, size_type count)
  {
    size_type sizeRead = 0;

    if (count > 0) // If data to read
    {
      size_type const sizeBuffer = buffer.size();
      /*size_type const sizeDifference = */_readcheck(count);

      size_type remainder = sizeBuffer - posRead;
      const_iterator begin = buffer.begin() + posRead;

      if (bWrap)
      {
        // If we're reading data and were wrapped, we can't be wrapped anymore.
        bWrap = false;
      }

      // Read n bytes.
      // If reading past end of buffer, continue at beginning up to point we're at now.
      const_iterator end;

      if (count > remainder) // If more to read than what's in remainder
      {
        end = buffer.end();
        sizeRead = sizedcopy(begin, end, out, count + 1) - out;

        // Wrap.
        out += remainder;
        begin = buffer.begin();
        posRead = 0;
        count -= remainder;

      } // Ends if more to write than what's in remainder

      end = begin + count;
      size_type const sizeCopy = sizedcopy(begin, end, out, count + 1) - out;
      sizeRead += sizeCopy;
      posRead += sizeCopy;

    } // Ends if data to read

    return sizeRead;
  }

  void swap(TransportBuffer &other)
  {
    buffer.swap(other.buffer);
    std::swap(posRead, other.posRead);
    std::swap(posWrite, other.posWrite);
  }
}; // Ends class TransportBuffer

void testTransportBuffer()
{
  TransportBuffer b(16);
  printf ("Readsize: %d\n", b.readsize());
  char const chtest1[] = "aaaaaaaa"; // 8
  char const chtest2[] = "1234567890"; // 10
  char const chtest3[] = "bbbbbb"; // 6
  b.write(chtest1, sizeof(chtest1) - 1); // write 8
  char buffer[17] = {0};
  buffer[sizeof(chtest1) - 1] = 'x'; // This shouldn't change
  b.read(buffer, sizeof(chtest1) - 1); // read 8
  b.write(chtest2, sizeof(chtest2) - 1); // write 10
  b.write(chtest3, sizeof(chtest3) - 1); // write 6
  printf ("Readsize with full buffer: %d\n", b.readsize());
  memset(buffer, 0, sizeof(buffer) - 1);
  b.read(buffer, sizeof(buffer) - 1); // read 16
  printf("Result: %s\n", buffer);
  TransportBuffer::size_type sizeWrite;
  b.writebuffer(10, sizeWrite); // Allocate 10 for write
  try
  {
    /*TransportBuffer::value_type *pwritebuffer = */b.writebuffer(10, sizeWrite);
  }
  catch (std::out_of_range &)
  {
    printf("Caught out of bounds for writing bigger than buffer size as expected\n");
  }
  TransportBuffer::value_type *pwritebuffer = b.writebuffer(3, sizeWrite);
  pwritebuffer[0] = 'x';
  pwritebuffer[1] = 'y';
  b.truncate_write(2);
}

void testWrappedTransportBuffer()
{
  TransportBuffer b(30);
  char const chtest1[] = "1234567890"; // 10
  char buffer[6] = {0};
  // Write 40, Read 20.
  for (int i = 0; i < 4; ++i)
  {
    b.write(chtest1, sizeof(chtest1) - 1); // Write 10.
    b.read(buffer, sizeof(buffer)-1); // Read 5.
  }
  // Buffer write pointer now wrapped past length (30) to 40 i.e. at 10.
  // Buffer read pointer now 20 in i.e. 10 to go.

  // Read 10.
  b.read(buffer, sizeof(buffer)-1); // Read 5
  // Should still be wrapped.
  b.read(buffer, sizeof(buffer)-1); // Read 5
  // Should no longer be wrapped.
  b.read(buffer, sizeof(buffer)-1); // Read 5
  // Should be read==write.
  b.read(buffer, sizeof(buffer)-1); // Read 5
  try
  {
    b.read(buffer, sizeof(buffer)-1); // Read 5
  }
  catch (std::out_of_range &)
  {
    printf("Caught out of bounds reading more than was in buffer as expected\n");
  }
}

#ifdef _WIN32
void showError(TransportData *ptransportdata, DWORD dwLastError, LPCTSTR pszMessage);
void showError(TransportData *ptransportdata, LPCTSTR pszMessage);
#else _WIN32
void showError(TransportData *ptransportdata, unsigned int error, char *pszMessage=NULL);
void showError(TransportData *ptransportdata, char *pszMessage);
#endif // _WIN32
} // Ends anonymous namespace

extern "C"
{
struct TransportData
{
  TransportBuffer transportbuffer;
  CV4VTransport transport;
  std::vector<HANDLE> handles;
  void (_V4VIO_CALLCONV *io_debug)(void *logpriv, const char *buf);
  DWORD dwWaitResult;

public:
  inline TransportData() : dwWaitResult(WAIT_FAILED) {}
private:
  TransportData(TransportData const &);
  TransportData & operator= (TransportData const &);
}; // Ends struct TransportData
} // Ends extern "C"

void _V4VIO_CALLCONV default_v4v_io_debug(void * /*logpriv*/, const char *buf)
{
  printf(buf);
  printf("\n");
}

TransportData * _V4VIO_CALLCONV connect_v4v_socket(void (_V4VIO_CALLCONV *io_debug)(void *logpriv, const char *buf))
{
#ifdef HAVE_V4V_SOCKET
  static ULONG const ulAddr = 0;
  static ULONG const ulPort = 5556;	// RPC port
  static ULONG const ulRingSize = XHT_DEFAULT_RING_SIZE;

  TransportData *ptransportdata = new TransportData;
  {
    // Initialise with temporaries.
    CV4VTransport transport(ulAddr
      ,ulPort
      ,ulRingSize
      ,false//true // Overlapped
      ,NULL // pcontext
    ); 
    ptransportdata->transport.swap(transport);

    TransportBuffer transportbuffer(ulRingSize);
    ptransportdata->transportbuffer.swap(transportbuffer);
  }
  ptransportdata->io_debug = io_debug;

  int logpriv = 1;
  if (io_debug != NULL)
  {
    io_debug(&logpriv, "About to connect to V4V\n");
  }
  if (ptransportdata->transport.Connect() == FALSE)
  {
    showError(ptransportdata, _T("Connecting V4V"));
    TransportData *ptransportdataTemp = ptransportdata;
    ptransportdata = NULL;
    disconnect_v4v_socket(ptransportdataTemp);
  }
  else
  {
    //ptransportdata->handles.push_back(ptransportdata->transport.GetContext()->v4vHandle);
    ptransportdata->handles.push_back(ptransportdata->transport.GetContext()->recvEvent);
    if (io_debug != NULL)
    {
      io_debug(&logpriv, "Connected to V4V\n");
    }
  }
  return ptransportdata;
#else // !HAVE_V4V_SOCKET
  return NULL;
#endif // HAVE_V4V_SOCKET
}


bool _V4VIO_CALLCONV cancel_v4v_socket_io(TransportData *ptransportdata)
{
  void (_V4VIO_CALLCONV *io_debug)(void *logpriv, const char *buf) = ptransportdata->io_debug;
  if (io_debug == NULL)
  {
    io_debug = default_v4v_io_debug;
  }
  int logpriv = 1;

  io_debug(&logpriv, "Cancelling v4v");

  bool const bCancel = ptransportdata->transport.Cancel();
  if (bCancel == false)
  {
    showError(ptransportdata, _T("Cancel failed"));

  }

  return bCancel;
}

bool _V4VIO_CALLCONV disconnect_v4v_socket(TransportData *ptransportdata)
{
  void (_V4VIO_CALLCONV *io_debug)(void *logpriv, const char *buf) = ptransportdata->io_debug;
  if (io_debug == NULL)
  {
    io_debug = default_v4v_io_debug;
  }
  int logpriv = 1;
  io_debug(&logpriv, "Disconnecting v4v\n");
  delete ptransportdata;
  return true;
}

DWORD _V4VIO_CALLCONV wait_for_v4v_read_handles_timeout(DWORD dwMilliseconds, TransportData *ptransportdata, LPCTSTR pszErrorMessage)
{
  DWORD dwWait = WAIT_OBJECT_0;

  // It's up to the caller to work out which handles should be waited on.
  if (ptransportdata->dwWaitResult == WAIT_FAILED || ptransportdata->dwWaitResult == WAIT_ABANDONED) // If haven't already waited
  {
    if (ptransportdata->handles.empty() == false) // If there are handles to wait on
    {
      dwWait = ::WaitForMultipleObjects(ptransportdata->handles.size(), &ptransportdata->handles[0], FALSE, dwMilliseconds);

      if (dwWait == WAIT_FAILED)
      {
        showError(ptransportdata, pszErrorMessage);

      }
      else if (dwWait == WAIT_ABANDONED)
      {
        // Do nothing. Not an error, but not going to do anything either.
      }
      else
      {
        if (ptransportdata->transport.Cancelled()) // If cancelled io
        {
          // Treat as a failed wait.
          dwWait = WAIT_FAILED;

        } // Ends if cancelled io
        else if (dwWait >= WAIT_OBJECT_0 && dwWait < WAIT_OBJECT_0 + ptransportdata->handles.size())
        {
          // We don't really care which object was signalled.
          dwWait = WAIT_OBJECT_0;

        }
        else if (dwWait >= WAIT_ABANDONED_0 && dwWait < WAIT_ABANDONED_0 + ptransportdata->handles.size())
        {
          // We don't really care which object was abandoned.
          dwWait = WAIT_ABANDONED_0;

        }
      }
    } // Ends if there are handles to wait on

    ptransportdata->dwWaitResult = dwWait;

  } // Ends if haven't already waited
  else // Else already waited
  {
    dwWait = ptransportdata->dwWaitResult;

  } // Ends else already waited

  return dwWait;
}

DWORD _V4VIO_CALLCONV wait_for_v4v_read_handles(TransportData *ptransportdata, LPCTSTR pszErrorMessage)
{
  return wait_for_v4v_read_handles_timeout(XHT_DEFAULT_TIMEOUT, ptransportdata, pszErrorMessage);
}

void _V4VIO_CALLCONV testV4V()
{
  testTransportBuffer();
  testWrappedTransportBuffer();
}

static void _adjust_read_data(TransportBuffer &transportbuffer, void *&buf, uint32_t count, TransportBuffer::size_type &sizeWriteBufferPre,
    TransportBuffer::size_type sizeWriteBuffer, TransportBuffer::value_type *&pwritebuffer,
    DWORD &dwRead, DWORD &dwOffset)
{
  if (dwRead > 0 && dwOffset >= count) // If filled the buffer
  {
    // Read from the buffer to the output.
    transportbuffer.read((TransportBuffer::value_type *)buf, count);
    // Move the output pointer.
    buf = (TransportBuffer::value_type *)buf + count;
    // Reset the contiguous buffer size (presumably to the entire buffer size since just emptied the buffer).
    sizeWriteBufferPre = transportbuffer.nowrap_writesize();
    // Get pointer to the transport buffer.
    pwritebuffer = transportbuffer.writebuffer(sizeWriteBufferPre, sizeWriteBuffer);
  } // Ends if filled the buffer
  else // Else buffer not full
  {
    dwOffset += dwRead;

  } // Ends else buffer not full
}

#ifdef __cplusplus
extern "C"
#endif __cplusplus
int _V4VIO_CALLCONV v4v_io_read(void *priv, void *buf, uint32_t count)
{
  TransportData *ptransportdata  = (TransportData *)priv;
  V4V_CONTEXT const *pcontext = ptransportdata->transport.GetContext();
  TransportBuffer &transportbuffer = ptransportdata->transportbuffer;
  size_t transportRead = transportbuffer.readsize();
  bool bReadFile = count != 0;
  int result = bReadFile ? -1 : 0;

  // No counting beyond MAXINT because we have to squeeze return value into an int.
  // Marvellous.
  if (count > MAXINT) // If count too big for result value
  {
    // We will just read as many bytes as can squeeze into an int.
    count = MAXINT;

  } // Ends if count too big for result value

  uint32_t countReadFromBuffer = 0;

  if (bReadFile && transportRead > 0) // If reading data and got data in buffer
  {
    if (transportRead > count)
    {
      transportRead = count;
    }

    countReadFromBuffer = transportbuffer.read((TransportBuffer::value_type *)buf, transportRead);
    count -= countReadFromBuffer;
    buf = (TransportBuffer::value_type *)buf + countReadFromBuffer;

    if (count == 0) // If read everything from buffer
    {
      result = transportRead;
      bReadFile = false;

    } // Ends if read everything from buffer
  } // Ends if reading data and got data in buffer

  //if (count != 0 && pcontext != NULL) // If got data to read and context
  if (bReadFile && pcontext != NULL) // If got data to read and context
  {
    if (ptransportdata->transport.Cancelled()) // If cancelled
    {
      result = -2;

    } // Ends if cancelled
    else // Else not cancelled
    {
      // Read from v4v in ring buffer sized chunks.
      // May not wait if waiting has already occurred.
      DWORD const dwWait = wait_for_v4v_read_handles(ptransportdata, _T("Failed read wait"));

      if (dwWait != WAIT_FAILED && dwWait != WAIT_TIMEOUT) // If wait succeeded
      {
        // We're about to do some reading, so setup wait result flag so that
        // next wait call realises it needs to actually wait again.
        // Unfortunate internal state wrangling to decouple waiting for read from read itself.
        ptransportdata->dwWaitResult = WAIT_FAILED;

        DWORD        dwRead      = 0;
        DWORD        dwOffset    = 0;
        TransportBuffer::size_type sizeWriteBuffer;
        TransportBuffer::size_type sizeWriteBufferPre = transportbuffer.nowrap_writesize();
        TransportBuffer::value_type *pwritebuffer = transportbuffer.writebuffer(sizeWriteBufferPre, sizeWriteBuffer);
        OVERLAPPED * const poverlapped = ptransportdata->transport.getOverlapped();
        bool const bAsync = poverlapped != NULL;

        do
        {
          BOOL const bRead = ::ReadFile(pcontext->v4vHandle
            ,pwritebuffer + dwOffset          // Buffer to read to.
            ,sizeWriteBuffer - dwOffset       // Number of bytes to read.
            ,&dwRead                          // Number of bytes read.
            ,poverlapped                      // Overlapped.
          );

          if (bRead == FALSE) // If read failed
          {
            if (bAsync) // If using overlapped io
            {
              DWORD const dwLastError = GetLastError();

              if (ERROR_IO_PENDING == dwLastError) // If IO pending
              {
                HANDLE heventWaiters[2] = 
                {
                  poverlapped->hEvent
                  , ptransportdata->transport.getOverlappedStopEventHandle()
                };
                DWORD const status = ::WaitForMultipleObjects(2, heventWaiters, FALSE, INFINITE);

                if (WAIT_FAILED == status) // If something bad happened
                {
                  showError(ptransportdata, _T("Wait after reading"));
                  break;

                } // Ends if something bad happened
                else if (WAIT_OBJECT_0 == status) // Else read completed
                {
                  if (HasOverlappedIoCompleted(poverlapped) == FALSE) // If overlapped IO has not completed
                  {
                      showError(ptransportdata, _T("async read completions signaled but HasOverlappedIoCompleted reports otherwise"));
                      break;

                  } // Ends if overlapped IO has not completed
                  else // Else overlapped IO has completed
                  {
                    if (GetOverlappedResult(pcontext->v4vHandle, poverlapped, &dwRead, FALSE) == FALSE) // If failed to get overlapped result
                    {
                        showError(ptransportdata, _T("GetOverlappedResult() for read failed"));
                        break;

                    } // Ends if failed to get overlapped result
                    else // Else got overlapped result
                    {
                      _adjust_read_data(transportbuffer, buf, count, sizeWriteBufferPre, sizeWriteBuffer, pwritebuffer, dwRead, dwOffset);

                    } // Ends else got overlapped result

                  } // Ends overlapped IO has completed
                } // Ends else read completed
              } // Ends if IO pending
              else // Else IO not pending
              {
                showError(ptransportdata, _T("Reading (async)"));
                break;

              } // Ends else IO not pending
            } // Ends if using overlapped io
            else // Else not using overlapped io
            {
              showError(ptransportdata, _T("Reading"));
              break;

            } // Ends else not using overlapped io

          } // Ends if read failed
          else // Else read succeeded
          {
            _adjust_read_data(transportbuffer, buf, count, sizeWriteBufferPre, sizeWriteBuffer, pwritebuffer, dwRead, dwOffset);

          } // Ends else read succeeded

        } while (dwRead > 0 && dwOffset < count);

        // Note we could truncate by count in case of error.
        transportbuffer.truncate_write(sizeWriteBufferPre - dwOffset); // This is how many bytes we didn't write to buffer.

        // Now copy from transport buffer to output buffer.
        if (dwOffset >= count)
        {
          transportbuffer.read((TransportBuffer::value_type *)buf, count);
          result = count + countReadFromBuffer;
        }

      } // Ends if wait succeeded
    } // Ends else not cancelled
  } // Ends if got data to read and context

  return result;
}

#ifdef __cplusplus
extern "C"
#endif __cplusplus
int _V4VIO_CALLCONV v4v_io_write(void *priv, const void *buf, uint32_t count)
{
  TransportData *ptransportdata = (TransportData *)priv;
  V4V_CONTEXT const *pcontext    = ptransportdata->transport.GetContext();
  int result = -1;

  if (pcontext != NULL)
  {
    //DWORD const dwWait = wait_for_v4v_read_handles(ptransportdata, _T("Failed write wait"));

    //if (dwWait != WAIT_FAILED) // If wait succeeded
    {
      DWORD dwWritten = 0;
      DWORD dwSize = count;

      DWORD dwToWrite = count;
      char const * pchbuf = (char const *)buf;

      OVERLAPPED * const poverlapped = ptransportdata->transport.getOverlapped();
      bool const bAsync = poverlapped != NULL;

      while (dwSize > 0)
      {
        if (dwSize > XHT_MAX_WRITE_SIZE)
        {
          // Apparently there's a bug in the V4V windows driver
          // which means we have to send at most "half ring sized" buffers.
          dwToWrite = XHT_MAX_WRITE_SIZE;
        }
        else
        {
          dwToWrite = dwSize;
        }

        dwWritten = 0;

        BOOL const bWrite = ::WriteFile(pcontext->v4vHandle
          ,pchbuf      // Buffer.
          ,dwToWrite   // Number of bytes to write.
          ,&dwWritten  // Number of bytes written.
          ,poverlapped // Overlapped.
        );

        if (bWrite == FALSE || dwWritten == 0) // If failed to write
        {
          if (bAsync) // If using overlapped io
          {
            DWORD const dwLastError = GetLastError();

            if (ERROR_IO_PENDING == dwLastError) // If IO pending
            {
              HANDLE heventWaiters[2] = 
              {
                poverlapped->hEvent
                , ptransportdata->transport.getOverlappedStopEventHandle()
              };
              DWORD const status = ::WaitForMultipleObjects(2, heventWaiters, FALSE, INFINITE);

              if (WAIT_FAILED == status) // If something bad happened
              {
                showError(ptransportdata, _T("Wait after writing"));
                break;

              } // Ends if something bad happened
              else if (WAIT_OBJECT_0 == status) // Else write completed
              {
                if (HasOverlappedIoCompleted(poverlapped) == FALSE) // If overlapped IO has not completed
                {
                    showError(ptransportdata, _T("async write completions signaled but HasOverlappedIoCompleted reports otherwise"));
                    break;

                } // Ends if overlapped IO has not completed
                else // Else overlapped IO has completed
                {
                  if (GetOverlappedResult(pcontext->v4vHandle, poverlapped, &dwWritten, FALSE) == FALSE) // If failed to get overlapped result
                  {
                      showError(ptransportdata, _T("GetOverlappedResult() for write failed"));
                      break;

                  } // Ends if failed to get overlapped result
                  else // Else got overlapped result
                  {
                    dwSize -= dwWritten;
                    pchbuf += dwWritten;

                  } // Ends else got overlapped result

                } // Ends overlapped IO has completed
              } // Ends else write completed
            } // Ends if IO pending
            else // Else IO not pending
            {
              showError(ptransportdata, _T("Writing (async)"));
              break;

            } // Ends else IO not pending
          } // Ends if using overlapped io
          else // Else not using overlapped io
          {
            showError(ptransportdata, _T("Writing"));
            break;

          } // Ends else not using overlapped io

        } // Ends if failed to write
        else // Else wrote to file
        {
          dwSize -= dwWritten;
          pchbuf += dwWritten;

        } // Ends else wrote to file
      } // Ends while haven't written all data

      result = dwSize == 0 ? 0 : -1;

    } // Ends if wait succeeded

  } // Ends if got context

  return result;
}

// Error handling
namespace 
{
#ifdef _WIN32
void showError(TransportData *ptransportdata, DWORD dwLastError, LPCTSTR pszMessage)
{
  LPVOID lpMsgBuf = NULL;
  DWORD dwFormatMessageResult = FormatMessage(
      FORMAT_MESSAGE_ALLOCATE_BUFFER | 
      FORMAT_MESSAGE_FROM_SYSTEM |
      FORMAT_MESSAGE_IGNORE_INSERTS
    ,NULL // source
    ,dwLastError // Message ID
    ,MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT) // Language ID
    ,(LPTSTR) &lpMsgBuf // Buffer
    ,0 // Size
    ,NULL // Arguments
  );
  if (lpMsgBuf != NULL)
  {
    if (dwFormatMessageResult != 0)
    {
      TCHAR *ptszError = NULL;
      size_t sizeError = 0;
      if (pszMessage != NULL)
      {
        sizeError = _tcslen(static_cast<TCHAR *>(lpMsgBuf)) + _tcslen(pszMessage) + 12 + 32;
        ptszError = new TCHAR[sizeError];
        _stprintf_s(ptszError, sizeError, _T("Error[%d]. %s: %s\n"), dwLastError, pszMessage, lpMsgBuf);
        //_tprintf(_T("Error[%d]. %s: %s\n"), dwLastError, pszMessage, lpMsgBuf);
      }
      else
      {
        sizeError = _tcslen(static_cast<TCHAR *>(lpMsgBuf)) + 12 + 32;
        ptszError = new TCHAR[sizeError];
        _stprintf_s(ptszError, sizeError, _T("Error[%d]: %s\n"), dwLastError, lpMsgBuf);
        //_tprintf(_T("Error[%d]: %s\n"), dwLastError, lpMsgBuf);
      }
      
      // Handle UNICODE conversion.
#			ifndef _UNICODE
      char *pszError = ptszError;
#			else // _UNICODE
      size_t converted;
      char *pszError = new char[sizeError];
      //WideCharToMultiByte(
      wcstombs_s(&converted, pszError, sizeError, ptszError, sizeError);
#			endif // !_UNICODE

      // Debug output.
      int debugflag = 1; // We use non null pointer to indicate special case debugging. Hack.
      ptransportdata->io_debug(&debugflag, pszError);

      // Cleanup UNICODE conversion.
#			ifdef _UNICODE
      delete[] pszError;
#			endif // !_UNICODE
      delete[] ptszError;
    }
    LocalFree(lpMsgBuf);
  }
}

void showError(TransportData *ptransportdata, LPCTSTR pszMessage)
{
  showError(ptransportdata, ::GetLastError(), pszMessage);
}

#else // !_WIN32
void showError(TransportData *ptransportdata, unsigned int error)
{
}
#endif // _WIN32
} // Ends anonymous namespace

