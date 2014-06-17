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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XenGuestPlugin
{
    /// <summary>
    /// This is a dictionary which fires events when it changes.
    /// Also, the Values property will remain sorted if the appropriate
    /// parameter is set.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ChangeableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public event CollectionChangeEventHandler CollectionChanged;
        public event EventHandler BatchCollectionChanged;

        /// <summary>
        /// Defaults to not sorted
        /// </summary>
        public ChangeableDictionary()
        {
        }

        public void Add(TKey key, TValue value)
        {
            // will throw if aleady exists
            dictionary.Add(key, value);

            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, value));
        }

        /// <summary>
        /// If this dictionary already contains a value for the given key, will fire a Remove event and then an Add event.
        /// Otherwise will just fire an Add event.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
            set
            {
                TValue v;

                if (dictionary.TryGetValue(key, out v))
                {
                    // Remove old value and fire event
                    dictionary.Remove(key);
                    OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, v));
                }

                dictionary[key] = value;

                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, value));
            }
        }

        public bool Remove(TKey key)
        {
            TValue v;
            if (!dictionary.TryGetValue(key, out v))
                return false;

            dictionary.Remove(key);

            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, v));

            return true;
        }

        public void OnBatchCollectionChanged()
        {
            if (BatchCollectionChanged != null)
            {
                try
                {
                    BatchCollectionChanged(this, null);
                }
                catch (Exception e)
                {
                    log.Debug("Exception firing batchCollectionChanged cache.", e);
                    log.Debug(e, e);
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                        throw;
#endif
                }
            }
        }

        private void OnCollectionChanged(CollectionChangeEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        #region IDictionary<TKey,TValue> Members

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get 
            { 
                return dictionary.Values;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Caller beware: you won't get any CollectionChangedEvents from calling this method (e.g. a 'Remove' event for each element).
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDictionary Members

        void IDictionary.Add(object key, object value)
        {
            Add((TKey) key, (TValue) value);
        }

        bool IDictionary.Contains(object key)
        {
            return Contains((KeyValuePair<TKey,TValue>) key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (IDictionaryEnumerator)GetEnumerator();
        }

        bool IDictionary.IsFixedSize
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        ICollection IDictionary.Keys
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        ICollection IDictionary.Values
        {
            get
            {
                return dictionary.Values;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        int ICollection.Count
        {
            get { return dictionary.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        object ICollection.SyncRoot
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }
}
