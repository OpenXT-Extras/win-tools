//
// Copyright (c) 2012 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.Collections.Generic;
using System.Threading;
//using System;
using System.Linq;
//using System.Text;

using NMessageHandle = Udbus.Serialization.NMessageHandle;
using NMessageStruct = Udbus.Serialization.NMessageStruct;

namespace Udbus.Core
{
    public class DbusMessageReceiverPool
    {
        #region Nested interfaces, classes and structs
        private struct MessageHandleData
        {
            public readonly NMessageHandle.UdbusMessageHandle handle;
            public readonly int result;

            public MessageHandleData(NMessageHandle.UdbusMessageHandle handle, int result)
            {
                this.handle = handle;
                this.result = result;
            }

            public static bool operator==(MessageHandleData self, MessageHandleData other)
            {
                bool bResult;
                if (self.handle == null && other.handle == null)
                {
                    bResult = self.result == other.result;
                }
                else
                {
                    bResult = self.handle == other.handle;
                }
                return bResult;
            }
            public static bool operator !=(MessageHandleData self, MessageHandleData other)
            {
                return !(self == other);
            }

            public override int GetHashCode()
            {
                return this.result.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                bool bEqual = obj != null;
                if (bEqual)
                {
                    MessageHandleData other = (MessageHandleData)obj;
                    bEqual = this.handle.Equals(other.handle);
                }
                return bEqual;
            }
            public static bool Equals(MessageHandleData self, MessageHandleData other)
            {
                return NMessageHandle.UdbusMessageHandle.Equals(self.handle, other.handle);
            }
        } // Ends MessageHandleData

        public struct MessageResultData
        {
            public readonly UdbusMessagePair messageData;
            //public readonly NMessageHandle.UdbusMessageHandle handle;
            public readonly int result;

            public MessageResultData(UdbusMessagePair messageData, int result)
            {
                this.messageData = messageData;
                this.result = result;
            }

            public static bool operator ==(MessageResultData self, MessageResultData other)
            {
                bool bResult;
                if (self.messageData.Handle == null && other.messageData.Handle == null)
                {
                    bResult = self.result == other.result;
                }
                else
                {
                    bResult = self.messageData.Handle == other.messageData.Handle;
                }
                return bResult;
            }
            public static bool operator !=(MessageResultData self, MessageResultData other)
            {
                return !(self == other);
            }

            public override int GetHashCode()
            {
                return this.result.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                bool bEqual = obj != null;
                if (bEqual)
                {
                    MessageResultData other = (MessageResultData)obj;
                    bEqual = this.messageData.Handle.Equals(other.messageData.Handle);
                }
                return bEqual;
            }
            public static bool Equals(MessageResultData self, MessageResultData other)
            {
                return NMessageHandle.UdbusMessageHandle.Equals(self.messageData.Handle, other.messageData.Handle);
            }

        } // Ends struct MessageResultData

        private class PoolEntry
        {
            public ManualResetEvent receivedEvent;
            //public int count;
            public int result;
            private List<MessageHandleData> listPending = null; // FIFO

            //private List<NMessageHandle.UdbusMessageHandle> listPending = null; // FIFO

            public PoolEntry(ManualResetEvent receivedEvent)
            {
                this.receivedEvent = receivedEvent;
                this.result = -1; // 0 indicates success.
                //this.count = 0;
            }

            /// <summary>
            /// Add a message to the list of pending messages.
            /// Note: no thread protection.
            /// </summary>
            /// <param name="msgHandle">Message handle to add.</param>
            public void AddPending(NMessageHandle.UdbusMessageHandle msgHandle, int result)
            {
                if (this.listPending == null)
                {
                    this.listPending = new List<MessageHandleData>();
                }
                this.listPending.Add(new MessageHandleData(msgHandle, result));
            }

            public bool QuPending()
            {
                return this.listPending != null && this.listPending.Count > 0;
            }

            /// <summary>
            /// Get a pending message. No checks.
            /// </summary>
            /// <returns></returns>
            public MessageHandleData PopPending()
            {
                MessageHandleData msgDataResult = this.listPending[0];
                this.listPending.RemoveAt(0);
                return msgDataResult;
            }

            public MessageHandleData PushPop(MessageHandleData pushData, int result)
            {
                MessageHandleData popData = pushData;
                if (this.QuPending()) // If got a message pending
                {
                    this.listPending.Add(pushData);
                    popData = this.listPending.ElementAt(0);
                    this.listPending.RemoveAt(0);

                } // Ends if got a message pending

                return popData;
            }

            public MessageHandleData PushPop(NMessageHandle.UdbusMessageHandle push, int result)
            {
                MessageHandleData pushData = new MessageHandleData(push, result);
                return this.PushPop(pushData, result);
            }

            /// <summary>
            /// Get a pending message.
            /// </summary>
            /// <returns>Pending message if available, otherwise null.</returns>
            public MessageHandleData PopPendingSafe()
            {
                MessageHandleData msgDataResult = default(MessageHandleData);
                if (this.QuPending())
                {
                    msgDataResult = this.PopPending();

                }
                return msgDataResult;
            }

            public delegate void WriteLineDelegate(string message);
            public void Dump(uint id, WriteLineDelegate WriteLine)
            {
                if (!this.QuPending())
                {
                    WriteLine(string.Format("Message {0}: Empty", id));
                }
                else
                {
                    WriteLine(string.Format("Message {0}[{1}]:", id, this.listPending.Count));
                    int msgCounter = 0;
                    foreach (var msg in this.listPending)
                    {
                        var s = msg.handle.HandleToStructure();
                        WriteLine(string.Format(" {0} {1}", msgCounter, s.ToString()));
                        ++msgCounter;
                    }
                }
            }
        } // Ends struct PoolEntry
        #endregion // Nested interfaces, classes and structs

        #region Fields
        private Dictionary<uint, PoolEntry> dictReceiptPool = new Dictionary<uint, PoolEntry>();
        //private List<NMessageHandle.UdbusMessageHandle> listPending = new List<NMessageHandle.UdbusMessageHandle>();
        object dictSync = new object();
        //object receiveSync = new object();
        bool bReceiving = false;
        //object receiveFlagSync = new object();
        IHandleDbusSignal handleSignals;
        private System.Threading.EventWaitHandle receiverAvailable = new System.Threading.AutoResetEvent(false); // Releases one at a time, auto-resets to non-signaled.

        Udbus.Core.Logging.ILog log;
        Udbus.Core.Logging.ILog logMessage;
        #endregion // Fields

        #region Properties
        public IHandleDbusSignal HandleSignals { set { this.handleSignals = value; } }
        #endregion // Properties

        #region Helper functions
        internal string GetPoolId() { return this.GetHashCode().ToString(); }
        private Udbus.Core.Logging.ILog CreateLogger()
        {
            return Udbus.Core.Logging.LogCreation.CreateReceiverPoolLogger(this);
//            System.Diagnostics.TraceSource ts = new System.Diagnostics.TraceSource("ReceiverPool");
//            System.Diagnostics.ConsoleTraceListener consoleListener = new System.Diagnostics.ConsoleTraceListener();
//            // ">DateTime: {DateTime}, Timestamp: {Timestamp}, ProcessId {ProcessId}, ThreadId {ThreadId}, Callstack\n{Callstack}< : {Message}
//            Udbus.Core.Logging.Formatter formatter = new Udbus.Core.Logging.Formatter(">{Timestamp}. Thread {ThreadId}< : {Message}"
//                , new KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>[]
//                {
//                    new KeyValuePair<string, Udbus.Core.Logging.Formatter.StashDelegate>("Pool", () => this.GetHashCode().ToString())
//                }
//            );
//            Udbus.Core.Logging.TraceListenerFormat formatConsoleListener = new Udbus.Core.Logging.TraceListenerFormat(consoleListener, formatter);

//            // Remove OutputDebug listener if not debugging.
//#if !DEBUG
////#if !TRACE
//            ts.Listeners.Remove("Default");
////#endif // !TRACE
//#endif // !DEBUG

//#if DEBUG
//            // Add console listener if debugging.
//            ts.Listeners.Add(formatConsoleListener);
//#endif // DEBUG

//            ts.Attributes.Add("autoflush", "true");
//            ts.Attributes.Add("indentsize", "2");

//            // See System.Diagnostics.SourceLevels
//            System.Diagnostics.SourceLevels.All.ToString();
//            System.Diagnostics.SourceSwitch sourceSwitch = new System.Diagnostics.SourceSwitch("ReceiverPool"
//#if !DEBUG
//                , System.Diagnostics.SourceLevels.Verbose.ToString()
//#else // !DEBUG
//                , System.Diagnostics.SourceLevels.Off.ToString()
//#endif // DEBUG
//);
//            ts.Switch = sourceSwitch;

//            return new Udbus.Core.Logging.LogTraceSource(ts);
        }

        private Udbus.Core.Logging.ILog CreateMessageLogger()
        {
            return Udbus.Core.Logging.LogCreation.CreateReceiverPoolMessageLogger(this);
        }

        private static TValue ValueFromPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
        {
            return pair.Value;
        }

        private static T Identity<T>(T t)
        {
            return t;
        }

        private IHandleDbusSignal DefaultHandleSignals()
        {
#if DEBUG
            return new HandleDbusSignalDump(DumpSignal);
#else // !DEBUG
            return null;
#endif // DEBUG
        }
        #endregion // Helper functions

        // .Net 3.5 can't work out delegate instantiation itself. Yeck.
        private readonly static System.Func<KeyValuePair<uint, PoolEntry>, PoolEntry> ValueFromPairDelegate = ValueFromPair;

        #region Debugging
        private void DumpSignal(string text)
        {
            log.Debug("Signal: {0}", text);
        }

        public void DumpPool(System.IO.TextWriter writer)
        {
            Dictionary<uint, PoolEntry>.KeyCollection keys = this.dictReceiptPool.Keys;
            var orderedKeys = this.dictReceiptPool.Keys.OrderBy<uint, uint>(Identity);
            foreach (uint key in orderedKeys)
            {
                PoolEntry entry = this.dictReceiptPool[key];
                entry.Dump(key, writer.WriteLine);
            }
        }

        private static void TraceWriteLine(string write)
        {
            // Because Trace.WriteLine is conditional. Yeck.
            System.Diagnostics.Trace.WriteLine(write);
        }
        public void TracePool()
        {
            Dictionary<uint, PoolEntry>.KeyCollection keys = this.dictReceiptPool.Keys;
            var orderedKeys = this.dictReceiptPool.Keys.OrderBy<uint, uint>(Identity);
            foreach (uint key in orderedKeys)
            {
                PoolEntry entry = this.dictReceiptPool[key];
                entry.Dump(key, TraceWriteLine);
            }
        }
        #endregion // Debugging

        #region Constructors
        public DbusMessageReceiverPool(IHandleDbusSignal handleSignals)
        {
            this.log = CreateLogger();
            this.logMessage = CreateMessageLogger();
            this.handleSignals = handleSignals;
        }

        public DbusMessageReceiverPool()
        {
            this.log = CreateLogger();
            this.handleSignals = DefaultHandleSignals();
        }
        #endregion // Constructors
        private MessageHandleData PushPopMessageIfNecessary(NMessageHandle.UdbusMessageHandle recv, int result, uint id)
        {
            MessageHandleData resultData = new MessageHandleData(recv, result);

            // Search in existing entries.
            PoolEntry entryFind;
            bool bFound = this.dictReceiptPool.TryGetValue(id, out entryFind);
            if (bFound) // If found pending entry
            {
                resultData = entryFind.PushPop(recv, result);

            } // Ends if found pending entry

            return resultData;
        }

        private MessageHandleData FindPendingMessageHandle(uint id, out PoolEntry entryFind, bool bRemove)
        {
            MessageHandleData respDataResult = default(MessageHandleData);

            // Search in existing entries.
            bool bFound = this.dictReceiptPool.TryGetValue(id, out entryFind);
            bFound = bFound && entryFind.QuPending();

            if (bFound) // If found entry
            {
                respDataResult = entryFind.PopPending();

                if (bRemove) // If removing entry
                {
                    if (entryFind.QuPending() == false) // If nothing left in entry
                    {
                        // Clear up entry.
                        if (entryFind.receivedEvent != null) // If it's got an event
                        {
                            // TODO Danger: What if someone else is looking at this entry
                            // Clear up the event.
                            entryFind.receivedEvent.Close();

                        } // Ends if it's got an event

                        this.dictReceiptPool.Remove(id);

                    } // Ends if nothing left in entry
                } // Ends if removing entry
            } // Ends if found entry

            return respDataResult;
        }

        private MessageHandleData FindPendingMessageHandleLocked(uint id, out PoolEntry entryFind, bool bRemove)
        {
            lock (this.dictSync)
            {
                return this.FindPendingMessageHandle(id, out entryFind, bRemove);

            } // Ends lock dictionary
        }

        private bool CheckSetReceiveFlag()
        {
            bool bIAmTheReceiver = !this.bReceiving;
            if (this.bReceiving == false) // If no-one is receiving
            {
                // Stop anyone else receiving.
                this.bReceiving = true;

            } // Ends if no-one is receiving
            return bIAmTheReceiver;
        }

        private bool CheckSetReceiveFlagLocked()
        {
            lock (this.dictSync)
            {
                return this.CheckSetReceiveFlag();

            } // Ends lock receive flag
        }

        /// <summary>
        /// Finds a PoolEntry.
        /// </summary>
        /// <returns>PoolEntry if successful, otherwise null.</returns>
        PoolEntry FindSomeoneElse()
        {
            PoolEntry entry = null;

            if (dictReceiptPool.Count > 0)
            {
                entry = dictReceiptPool.First().Value;
            }

            return entry;
            //return this.dictReceiptPool.Where(kv => kv.Key != id).Select(ValueFromPairDelegate).FirstOrDefault();
        }

        /// <summary>
        /// Finds a PoolEntry corresponding to a different id.
        /// </summary>
        /// <param name="id">Id NOT to find PoolEntry for.</param>
        /// <returns>PoolEntry if successful, otherwise null.</returns>
        PoolEntry FindSomeoneElse(uint id)
        {
            return this.dictReceiptPool.Where(kv => kv.Key != id).Select(ValueFromPairDelegate).FirstOrDefault();
        }

        /// <summary>
        /// Switch receiver to someone else.
        /// </summary>
        /// <param name="id">Id of current receiver.</param>
        private void SwitchToSomeoneElse()
        {
            // Tell someone else to be the receiver.
            PoolEntry someoneElse = this.FindSomeoneElse();
            this.bReceiving = false;
            bool bToldSomeoneElse = false;

            if (someoneElse != null) // If some other entry is pending
            {
                if (someoneElse.receivedEvent != null) // If someone's interested in other result
                {
                    // Wake them up so they can become the receiver.
                    someoneElse.receivedEvent.Set();
                    bToldSomeoneElse = true;

                } // Ends if someone's interested in other result
            } // Ends if some other entry is pending

            if (!bToldSomeoneElse) // If haven't told anyone else
            {
                // Anyone who cares, this position now available.
                this.receiverAvailable.Set();

            } // Ends if haven't told anyone else
        }

        /// <summary>
        /// Switch receiver to someone else with lock obtained.
        /// </summary>
        private void SwitchToSomeoneElseLocked()
        {
            lock (this.dictSync)
            {
                SwitchToSomeoneElse();
            }
        }

        /// <summary>
        /// Switch receiver to someone else.
        /// </summary>
        /// <param name="id">Id of current receiver.</param>
        private void SwitchToSomeoneElse(uint id)
        {
            // Tell someone else to be the receiver.
            PoolEntry someoneElse = this.FindSomeoneElse(id);
            this.bReceiving = false;
            bool bToldSomeoneElse = false;

            if (someoneElse != null) // If some other entry is pending
            {
                if (someoneElse.receivedEvent != null) // If someone's interested in other result
                {
                    // Wake them up so they can become the receiver.
                    someoneElse.receivedEvent.Set();
                    bToldSomeoneElse = true;

                } // Ends if someone's interested in other result
            } // Ends if some other entry is pending

            if (!bToldSomeoneElse) // If haven't told anyone else
            {
                // Anyone who cares, this position now available.
                this.receiverAvailable.Set();

            } // Ends if haven't told anyone else
        }
        //private void SwitchToSomeoneElse(uint id)
        //{
        //    // Tell someone else to be the receiver.
        //    PoolEntry someoneElse = this.FindSomeoneElse(id);
        //    this.bReceiving = false;

        //    if (someoneElse != null) // If some other entry is pending
        //    {
        //        if (someoneElse.receivedEvent != null) // If someone's interested in other result
        //        {
        //            // Wake them up so they can become the receiver.
        //            someoneElse.receivedEvent.Set();

        //        } // Ends if someone's interested in other result
        //    } // Ends if some other entry is pending
        //}

        /// <summary>
        /// Switch receiver to someone else with lock obtained.
        /// </summary>
        /// <param name="id">Id of current receiver.</param>
        private void SwitchToSomeoneElseLocked(uint id)
        {
            lock (this.dictSync)
            {
                SwitchToSomeoneElse(id);
            }
        }

        private MessageHandleData FindPendingMessageHandleAndCheckSetReceiver(uint id
            ,out PoolEntry entryFind
            ,out bool bIAmTheReceiver, bool bRemove)
        {
            MessageHandleData resp = this.FindPendingMessageHandle(id, out entryFind, bRemove);
            bIAmTheReceiver = this.CheckSetReceiveFlag();

            if (resp != default(MessageHandleData)) // If found response
            {
                if (bIAmTheReceiver) // If also going to be receiver
                {
                    this.bReceiving = false;
                    bIAmTheReceiver = false;

                    // Wake up someone else to be the receiver.
                    PoolEntry someoneElse = this.FindSomeoneElse(id);
                    if (someoneElse != null && someoneElse.receivedEvent != null)
                    {
                        someoneElse.receivedEvent.Set();
                    }
                } // Ends if also going to be receiver
            } // Ends if found response
            else if (!bIAmTheReceiver) // Else no response and not the recevier
            {
                System.Diagnostics.Trace.WriteLine(string.Format("That's weird. Message {0} not in store, nor is it becoming receiver, but it got woken up. Snoozing...", id));

            } // Ends else no response and not the recevier

            return resp;
        }

        private MessageHandleData FindPendingMessageHandleAndCheckSetReceiverLocked(uint id
            , out PoolEntry entryFind
            , out bool bIAmTheReceiver, bool bRemove)
        {
            lock (this.dictSync)
            {
                return this.FindPendingMessageHandleAndCheckSetReceiver(id, out entryFind, out bIAmTheReceiver, bRemove);
            }
        }

        private MessageHandleData FindPendingMessageHandleAndCheckSetReceiver(uint id
            , out bool bIAmTheReceiver, bool bRemove)
        {
            PoolEntry entryFind;
            return this.FindPendingMessageHandleAndCheckSetReceiver(id, out entryFind, out bIAmTheReceiver, bRemove);
        }

        private MessageHandleData FindPendingMessageHandleAndCheckSetReceiverLocked(uint id
            , out bool bIAmTheReceiver, bool bRemove)
        {
            lock (this.dictSync)
            {
                return this.FindPendingMessageHandleAndCheckSetReceiver(id, out bIAmTheReceiver, bRemove);

            } // Ends lock dictionary
        }

        private bool HandleSignal(Udbus.Core.UdbusMessagePair messageData)
        {
            bool handleSignal = messageData.Data.typefield.type == Udbus.Types.dbus_msg_type.DBUS_TYPE_SIGNAL;

            if (handleSignal) // If message is actually a signal
            {
                if (this.handleSignals != null) // If got a signal handler
                {
                    this.handleSignals.HandleSignalMessage(messageData);

                } // Ends if got a signal handler

            } // Ends if message is actually a signal

            return handleSignal;
        }

        private void HandleSomeoneElsesMessage(Udbus.Core.UdbusMessagePair messageData, int result)
        {
            PoolEntry entryOther;
            lock (this.dictSync)
            {
                bool bFindEntry = this.dictReceiptPool.TryGetValue(messageData.Data.reply_serial, out entryOther);

                if (!bFindEntry) // If there's nothing in the pool
                {
                    log.Debug("Adding pool message {0}.", messageData.Data.reply_serial);
                    // Don't create event since we're adding the entry which means no-one's looking for it yet.
                    entryOther = new PoolEntry(null);
                    this.dictReceiptPool[messageData.Data.reply_serial] = entryOther;

                } // Ends if there's nothing in the pool

                entryOther.AddPending(messageData.Handle, result);

                if (entryOther.receivedEvent != null) // If someone is listening
                {
                    log.Debug("Letting another thread have a go at {0}.", messageData.Data.reply_serial);
                    // Let the other thread rip.
                    entryOther.receivedEvent.Set();

                } // Ends if someone is listeng
            } // Ends lock pool looking for other message's entry

            // Note: receiver flag hasn't changed, so we'll still do the receiving.
        }

        public void HandleMessage(Udbus.Core.UdbusMessagePair messageData, int result)
        {
            if (!this.HandleSignal(messageData)) // If not a signal
            {
                this.HandleSomeoneElsesMessage(messageData, result);

            } // Ends if not a signal
        }

        public void HandleMessage(Udbus.Core.UdbusMessagePair messageData)
        {
            // Assume message receive result was successful.
            this.HandleMessage(messageData, 0);
        }

        private void ReceiveLoopImpl(Udbus.Serialization.UdbusConnector connector, System.Threading.EventWaitHandle stop)
        {
            // Check whether we can be the receiver straight off.
            bool bIAmTheReceiver = this.CheckSetReceiveFlagLocked();
            log.Debug("Pool loop receiver state: {0}.", bIAmTheReceiver);

            if (bIAmTheReceiver == false)
            {
                WaitHandle[] waitHandles = { this.receiverAvailable, stop };

                while (bIAmTheReceiver == false)
                {
                    log.Debug("Pool loop is not the receiver. Waiting... ");
                    int nWaitHandle = WaitHandle.WaitAny(waitHandles);
                    log.Debug("Pool loop is not the receiver. Finished waiting... ");

                    if (nWaitHandle == 1) // If stopping
                    {
                        break;

                    } // Ends if stopping

                    // Check again (hopefully we'll sneak in there but you never know).
                    bIAmTheReceiver = this.CheckSetReceiveFlagLocked();

                } // Ends loop over handles

            } // Ends if not the receiver

            if (bIAmTheReceiver) // If receiving in this thread
            {
                // Receive from dbus.
                int result = 0;
                    
                while (result == 0 && stop.WaitOne(0) == false)
                {
                    // Blocks. Only way to end loop now is to close the connection.
                    NMessageHandle.UdbusMessageHandle respHandle = connector.ReceiveHandle(out result);

                    if (result == 0) // If got message
                    {
                        UdbusMessagePair messageData = new UdbusMessagePair(respHandle);
                        logMessage.Debug("Pool loop received '{0}'", messageData.Data.ToString());

                        if (!this.HandleSignal(messageData)) // If message is not a signal
                        {
                            this.HandleSomeoneElsesMessage(messageData, result);

                        }
                    } // Ends if got message
                    else // Else failed to get message
                    {
                        // Tell someone else to do the receiving.
                        this.SwitchToSomeoneElseLocked();

                    } // Ends else failed to get message
                } // Ends while haven't received requested message from dbus
            } // Ends if receiving in this thread
        }


        private MessageResultData ReceiveMessageImpl(Udbus.Serialization.UdbusConnector connector, uint id)
        {
            bool bIAmTheReceiver;

            // Look in the cupboard and sort out the receiver. Optimistic check.
            MessageHandleData respHandleResult = FindPendingMessageHandleAndCheckSetReceiverLocked(id,
                out bIAmTheReceiver, true);
            MessageResultData? resultData = null;
            log.Debug("Pool message receiver state: {0}", bIAmTheReceiver);

            while (respHandleResult == default(MessageHandleData))
            {
                if (bIAmTheReceiver == false) // If not receiving in this thread
                {
                    log.Debug("Entering pool non-receiver loop for {0}", id);
                    PoolEntry entry = null;

                    // Need to wait on event specific to this message.
                    lock (this.dictSync)
                    {
                        // Check again while we have the lock.
                        // We allow removal here because this is where we pop last message from entry.
                        respHandleResult = FindPendingMessageHandleAndCheckSetReceiver(id,
                            out entry,
                            out bIAmTheReceiver, true);

                        if (respHandleResult == default(MessageHandleData) && bIAmTheReceiver == false) // If still not done and not the receiver
                        {
                            log.Debug("Adding entry {0} to pool", id);
                            // Nothing's changed since we obtained the lock.
                            // Ensure there's an entry with an event to wait on.
                            bool bFindEntry = entry != default(PoolEntry);

                            if (!bFindEntry) // If there's nothing in the pool
                            {
                                log.Debug("Creating new entry {0} for pool", id);
                                entry = new PoolEntry(new ManualResetEvent(false));
                                this.dictReceiptPool[id] = entry;

                            } // Ends if there's nothing in the pool
                            else // Else something in the pool
                            {
                                log.Debug("Found entry {0} in pool", id);
                                if (entry.receivedEvent == null) // If no event yet
                                {
                                    log.Debug("Added new event to entry {0} in pool", id);
                                    // This means when receiver created entry,
                                    // it didn't bother to create event since no-one was looking for message.
                                    entry.receivedEvent = new ManualResetEvent(false);

                                } // Ends if no event yet

                            } // Ends else something in the pool
                        } // Ends if still not done and not the receiver
                        else
                        {
                            log.Debug("Pool found cached message {0}", id);
                        }
                    } // Ends lock receipt pool

                    if (respHandleResult == default(MessageHandleData)) // If still haven't got result
                    {
                        if (entry != null) // If got entry
                        {
                            // Block on event.
                            log.Debug("Waiting for pool to provide an entry for {0}.", id);
                            entry.receivedEvent.WaitOne();
                            log.Debug("Finished waiting for pool to provide an entry for {0}", id);

                        } // Ends if got entry

                        // Receiver woke us up. Either because our message has arrived, and/or so that we could become receiver.
                        // We allow removal here because this is where we pop last message from entry.
                        respHandleResult = FindPendingMessageHandleAndCheckSetReceiverLocked(id, out bIAmTheReceiver, true);

                    } // Ends if still haven't got result

                } // Ends if not receiving in this thread
                else // Else receiving in this thread
                {
                    while (respHandleResult == default(MessageHandleData))
                    {
                        // Receive from dbus.
                        int result;

                        // Blocks.
                        log.Debug("Blocking to wait for message {0}.", id);
                        NMessageHandle.UdbusMessageHandle respHandle = connector.ReceiveHandle(out result);
                        log.Debug("Unblocking wait for message {0}.", id);

                        if (result == 0) // If got message
                        {
                            UdbusMessagePair messageData = new UdbusMessagePair(respHandle);
                            logMessage.Debug("Pool Received: '{0}'", messageData.Data.ToString());

                            //if (messageData.Data.typefield.type == dbus_msg_type.DBUS_TYPE_SIGNAL) // If message is actually a signal
                            //{
                            //    this.handleSignals.HandleSignalMessage(messageData);

                            //} // Ends if message is actually a signal
                            //else // Else message is not a signal
                            if (!this.HandleSignal(messageData)) // If message is not a signal
                            {
                                if (messageData.Data.reply_serial == id) // If this is our message
                                {
                                    // We're done.
                                    lock (this.dictSync)
                                    {
                                        // Get the earliest message.
                                        respHandleResult = this.PushPopMessageIfNecessary(messageData.Handle, result, id);

                                        if (messageData.Handle == respHandleResult.handle) // If same message handle
                                        {
                                            // This is the result data, already deserialized.
                                            resultData = new MessageResultData(messageData, result);

                                        } // Ends if same message handle

                                        this.SwitchToSomeoneElse(id);

                                    } // Ends lock pool
                                } // Ends if this is our message
                                else // Else this is someone else's message
                                {
                                    this.HandleSomeoneElsesMessage(messageData, result);
                                    //PoolEntry entryOther;
                                    //lock (this.dictSync)
                                    //{
                                    //    bool bFindEntry = this.dictReceiptPool.TryGetValue(messageData.Data.reply_serial, out entryOther);

                                    //    if (!bFindEntry) // If there's nothing in the pool
                                    //    {
                                    //        // Don't create event since we're adding the entry which means no-one's looking for it yet.
                                    //        entryOther = new PoolEntry(null);
                                    //        this.dictReceiptPool[messageData.Data.reply_serial] = entryOther;

                                    //    } // Ends if there's nothing in the pool

                                    //    entryOther.AddPending(messageData.Handle, result);

                                    //    if (entryOther.receivedEvent != null) // If someone is listening
                                    //    {
                                    //        // Let the other thread rip.
                                    //        entryOther.receivedEvent.Set();

                                    //    } // Ends if someone is listeng
                                    //} // Ends lock pool looking for other message's entry

                                    //// Note: receiver flag hasn't changed, so we'll still do the receiving.

                                } // Ends else this is someone else's message
                            } // Ends else message is not a signal
                        } // Ends if got message
                        else // Else failed to get message
                        {
                            // Tell someone else to do the receiving.
                            this.SwitchToSomeoneElseLocked(id);
                            // Hand back the result.
                            respHandleResult = new MessageHandleData(NMessageHandle.UdbusMessageHandle.Initialiser, result);

                        } // Ends else failed to get message
                    } // Ends while haven't received requested message from dbus
                } // Ends else receiving in this thread
            } // Ends loop while haven't got requested message from anywhere

            if (!resultData.HasValue) // If no result yet
            {
                resultData = new MessageResultData(new UdbusMessagePair(respHandleResult.handle), respHandleResult.result);

            } // Ends if no result yet

            return resultData.Value;
        }

        private void HandleOurMessage(uint id, ref MessageHandleData respHandleResult, ref MessageResultData? resultData, int result, ref UdbusMessagePair messageData)
        {
            if (messageData.Data.reply_serial == id) // If this is our message
            {
                // We're done.
                lock (this.dictSync)
                {
                    // Get the earliest message.
                    respHandleResult = this.PushPopMessageIfNecessary(messageData.Handle, result, id);

                    if (messageData.Handle == respHandleResult.handle) // If same message handle
                    {
                        // This is the result data, already deserialized.
                        resultData = new MessageResultData(messageData, result);

                    } // Ends if same message handle

                    this.SwitchToSomeoneElse(id);

                } // Ends lock pool
            } // Ends if this is our message
            else // Else this is someone else's message
            {
                PoolEntry entryOther;
                lock (this.dictSync)
                {
                    bool bFindEntry = this.dictReceiptPool.TryGetValue(messageData.Data.reply_serial, out entryOther);

                    if (!bFindEntry) // If there's nothing in the pool
                    {
                        // Don't create event since we're adding the entry which means no-one's looking for it yet.
                        entryOther = new PoolEntry(null);
                        this.dictReceiptPool[messageData.Data.reply_serial] = entryOther;

                    } // Ends if there's nothing in the pool

                    entryOther.AddPending(messageData.Handle, result);

                    if (entryOther.receivedEvent != null) // If someone is listening
                    {
                        // Let the other thread rip.
                        entryOther.receivedEvent.Set();

                    } // Ends if someone is listeng
                } // Ends lock pool looking for other message's entry

                // Note: receiver flag hasn't changed, so we'll still do the receiving.

            } // Ends else this is someone else's message
        }

        //private MessageHandleData ReceiveMessageImpl(Udbus.Serialization.UdbusConnector connector, uint id)
        //{
        //    bool bIAmTheReceiver;

        //    // Look in the cupboard and sort out the receiver. Optimistic check.
        //    MessageHandleData respHandleResult = FindPendingMessageHandleAndCheckSetReceiverLocked(id,
        //        out bIAmTheReceiver, true);

        //    while (respHandleResult == default(MessageHandleData))
        //    {
        //        if (bIAmTheReceiver == false) // If not receiving in this thread
        //        {
        //            PoolEntry entry = null;

        //            // Need to wait on event specific to this message.
        //            lock (this.dictReceiptPool)
        //            {
        //                // Check again while we have the lock.
        //                // We allow removal here because this is where we pop last message from entry.
        //                respHandleResult = FindPendingMessageHandleAndCheckSetReceiver(id,
        //                    out entry,
        //                    out bIAmTheReceiver, true);

        //                if (respHandleResult == default(MessageHandleData) && bIAmTheReceiver == false) // If still not done and not the receiver
        //                {
        //                    // Nothing's changed since we obtained the lock.
        //                    // Ensure there's an entry with an event to wait on.
        //                    bool bFindEntry = entry != default(PoolEntry);

        //                    if (!bFindEntry) // If there's nothing in the pool
        //                    {
        //                        entry = new PoolEntry(new ManualResetEvent(false));
        //                        this.dictReceiptPool[id] = entry;

        //                    } // Ends if there's nothing in the pool
        //                    else // Else something in the pool
        //                    {
        //                        if (entry.receivedEvent == null) // If no event yet
        //                        {
        //                            // This means when receiver created entry,
        //                            // it didn't bother to create event since no-one was looking for message.
        //                            entry.receivedEvent = new ManualResetEvent(false);

        //                        } // Ends if no event yet

        //                    } // Ends else something in the pool
        //                } // Ends if still not done and not the receiver
        //            } // Ends lock receipt pool

        //            if (respHandleResult == default(MessageHandleData)) // If still haven't got result
        //            {
        //                if (entry != null) // If got entry
        //                {
        //                    // Block on event.
        //                    entry.receivedEvent.WaitOne();

        //                } // Ends if got entry

        //                // Receiver woke us up. Either because out message has arrived, and/or so that we could become receiver.
        //                // We allow removal here because this is where we pop last message from entry.
        //                respHandleResult = FindPendingMessageHandleAndCheckSetReceiverLocked(id, out bIAmTheReceiver, true);

        //            } // Ends if still haven't got result

        //        } // Ends if not receiving in this thread
        //        else // Else receiving in this thread
        //        {
        //            while (respHandleResult == default(MessageHandleData))
        //            {
        //                // Receive from dbus.
        //                int result;

        //                // Blocks.
        //                NMessageHandle.UdbusMessageHandle respHandle = connector.ReceiveHandle(out result);

        //                if (result == 0) // If got message
        //                {
        //                    NMessageStruct.UdbusMessageHandle resp = respHandle.HandleToStructure();
        //                    if (resp.reply_serial == id) // If this is our message
        //                    {
        //                        // We're done.
        //                        lock (this.dictSync)
        //                        {
        //                            // Get the earliest message.
        //                            respHandleResult = this.PushPopMessageIfNecessary(respHandle, result, id);

        //                            this.SwitchToSomeoneElse(id);

        //                        } // Ends lock pool
        //                    } // Ends if this is our message
        //                    else // Else this is someone else's message
        //                    {
        //                        PoolEntry entryOther;
        //                        lock (this.dictSync)
        //                        {
        //                            bool bFindEntry = this.dictReceiptPool.TryGetValue(resp.reply_serial, out entryOther);

        //                            if (!bFindEntry) // If there's nothing in the pool
        //                            {
        //                                // Don't create event since we're adding the entry which means no-one's looking for it yet.
        //                                entryOther = new PoolEntry(null);
        //                                this.dictReceiptPool[resp.reply_serial] = entryOther;

        //                            } // Ends if there's nothing in the pool

        //                            entryOther.AddPending(respHandle, result);

        //                            if (entryOther.receivedEvent != null) // If someone is listening
        //                            {
        //                                // Let the other thread rip.
        //                                entryOther.receivedEvent.Set();

        //                            } // Ends if someone is listeng
        //                        } // Ends lock pool looking for other message's entry

        //                        // Note: receiver flag hasn't changed, so we'll still do the receiving.

        //                    } // Ends else this is someone else's message
        //                } // Ends if got message
        //                else // Else failed to get message
        //                {
        //                    // Tell someone else to do the receiving.
        //                    this.SwitchToSomeoneElseLocked(id);
        //                    // Hand back the result.
        //                    respHandleResult = new MessageHandleData(NMessageHandle.UdbusMessageHandle.Initialiser, result);

        //                } // Ends else failed to get message
        //            } // Ends while haven't received requested message from dbus
        //        } // Ends else receiving in this thread
        //    } // Ends loop while haven't got requested message from anywhere

        //    return respHandleResult;
        //}

        public void LoopMessages(Udbus.Serialization.UdbusConnector connector, System.Threading.EventWaitHandle stop)
        {
            this.ReceiveLoopImpl(connector, stop);
        }
        /// <summary>
        /// Receive a dbus message, where data may already be deserialized.
        /// </summary>
        /// <param name="connector">Connector for DBus communications.</param>
        /// <param name="id">Id of message to receive.</param>
        /// <returns>Received message data.</returns>
        public UdbusMessagePair ReceiveMessageData(Udbus.Serialization.UdbusConnector connector, uint id, out int result)
        {
            MessageResultData resultData = this.ReceiveMessageImpl(connector, id);
            result = resultData.result;
            return resultData.messageData;
        }


        /// <summary>
        /// Receive a dbus message.
        /// </summary>
        /// <param name="connector">Connector for DBus communications.</param>
        /// <param name="id">Id of message to receive.</param>
        /// <returns>Received message.</returns>
        public NMessageHandle.UdbusMessageHandle ReceiveMessageHandle(Udbus.Serialization.UdbusConnector connector, uint id, out int result)
        {
            MessageResultData resultData = this.ReceiveMessageImpl(connector, id);
            result = resultData.result;
            return resultData.messageData.Handle;
        }

        /// <summary>
        /// Receive dbus messages where data may already be deserialized.
        /// </summary>
        /// <param name="connector">Connector for DBus communications.</param>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="messages">Collection of received messages.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>True if received any messages, otherwise false.</returns>
        public int ReceiveMessages(Udbus.Serialization.UdbusConnector connector, uint id, ICollection<UdbusMessagePair> messages, uint count)
        {
            uint i = 0;
            int result = 0;
            for (; i < count; ++i)
            {
                UdbusMessagePair message = this.ReceiveMessageData(connector, id, out result);
                if (result == 0)
                {
                    messages.Add(message);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="connector">Connector for DBus communications.</param>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="messages">Collection of received messages.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>True if received any messages, otherwise false.</returns>
        public int ReceiveMessageHandles(Udbus.Serialization.UdbusConnector connector, uint id, ICollection<NMessageHandle.UdbusMessageHandle> messages, uint count)
        {
            uint i = 0;
            int result = 0;
            for (; i < count; ++i)
            {
                NMessageHandle.UdbusMessageHandle message = this.ReceiveMessageHandle(connector, id, out result);
                if (result == 0)
                {
                    messages.Add(message);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="connector">Connector for DBus communications.</param>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>Collection of received messages.</returns>
        public ICollection<UdbusMessagePair> ReceiveMessages(Udbus.Serialization.UdbusConnector connector, uint id, uint count, out int result)
        {
            List<UdbusMessagePair> messages = new List<UdbusMessagePair>();
            result = this.ReceiveMessages(connector, id, messages, count);
            return messages;
        }

        /// <summary>
        /// Receive dbus messages.
        /// </summary>
        /// <param name="connector">Connector for DBus communications.</param>
        /// <param name="id">Id of message to receive.</param>
        /// <param name="count">Number of messages to receive.</param>
        /// <returns>Collection of received messages.</returns>
        public ICollection<NMessageHandle.UdbusMessageHandle> ReceiveMessageHandles(Udbus.Serialization.UdbusConnector connector, uint id, uint count, out int result)
        {
            List<NMessageHandle.UdbusMessageHandle> messages = new List<NMessageHandle.UdbusMessageHandle>();
            result = this.ReceiveMessageHandles(connector, id, messages, count);
            return messages;
        }

    } // Ends class DbusMessageReceiverPool
} // Ends namespace Udbus.Core
