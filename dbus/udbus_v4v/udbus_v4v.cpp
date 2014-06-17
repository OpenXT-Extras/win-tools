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

// udbus_v4v.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "udbus_v4v.h"

#include "udbus.h" // dbus_io
#include "v4vio.h"  // v4v_io_read(), v4v_io_write()

void _UDBUS_CALLCONV noop_io_debug(void * /*logpriv*/, const char * /*buf*/)
{
}

bool _UDBUS_V4V_CALLCONV populate_dbus_io(dbus_io *pdbus_io, TransportData *ptransport)
{
  pdbus_io->io_read = &v4v_io_read;
  pdbus_io->io_write = &v4v_io_write;
  if (pdbus_io->io_debug == NULL) // If no io_debug set
  {
    // Set it to a no-op function.
    pdbus_io->io_debug = &noop_io_debug;

  } // Ends if no io_debug set
  else // Else io_debug set
  {
    if (pdbus_io->logpriv == NULL) // If no log priv data
    {
      // Give the log function the same data as the other functions.
      pdbus_io->logpriv = ptransport;

    } // Ends if no log priv data
  } // Ends else io_debug set

  pdbus_io->priv = ptransport;
  return true;
}

