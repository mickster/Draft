﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// ReSharper disable All
#pragma warning disable 1591

using System;
using System.Collections;

namespace Draft
{
    /// <devdoc>
    ///  <para>
    ///    This is a simple implementation of IDictionary using a singly linked list. This
    ///    will be smaller and faster than a Hashtable if the number of elements is 10 or less.
    ///    This should not be used if performance is important for large numbers of elements.
    ///  </para>
    /// </devdoc>
    internal class ListDictionary : IDictionary
    {

        // ReSharper disable InconsistentNaming
        private const string ArgumentNull_Key = "Key cannot be null.";

        private const string Argument_AddingDuplicate = "An entry with the same key already exists.";

        private const string ArgumentOutOfRange_NeedNonNegNum = "Index is less than zero.";

        private const string Arg_InsufficientSpace = "Insufficient space in the target location to copy the information.";

        private const string InvalidOperation_EnumOpCantHappen = "Enumerator is positioned before the first element or after the last element of the collection.";

        private const string InvalidOperation_EnumFailedVersion = "Collection was modified after the enumerator was instantiated.";
        // ReSharper restore InconsistentNaming


        private DictionaryNode _head;
        private int _version;
        private int _count;
        private readonly IComparer _comparer;
        private Object _syncRoot;

        public ListDictionary()
        {
        }

        public ListDictionary(IComparer comparer)
        {
            _comparer = comparer;
        }

        public object this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", ArgumentNull_Key);
                }
                DictionaryNode node = _head;
                if (_comparer == null)
                {
                    while (node != null)
                    {
                        object oldKey = node.key;
                        if (oldKey != null && oldKey.Equals(key))
                        {
                            return node.value;
                        }
                        node = node.next;
                    }
                }
                else
                {
                    while (node != null)
                    {
                        object oldKey = node.key;
                        if (oldKey != null && _comparer.Compare(oldKey, key) == 0)
                        {
                            return node.value;
                        }
                        node = node.next;
                    }
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", ArgumentNull_Key);
                }
                _version++;
                DictionaryNode last = null;
                DictionaryNode node;
                for (node = _head; node != null; node = node.next)
                {
                    object oldKey = node.key;
                    if ((_comparer == null) ? oldKey.Equals(key) : _comparer.Compare(oldKey, key) == 0)
                    {
                        break;
                    }
                    last = node;
                }
                if (node != null)
                {
                    // Found it
                    node.value = value;
                    return;
                }
                // Not found, so add a new one
                DictionaryNode newNode = new DictionaryNode();
                newNode.key = key;
                newNode.value = value;
                if (last != null)
                {
                    last.next = newNode;
                }
                else
                {
                    _head = newNode;
                }
                _count++;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public ICollection Keys
        {
            get
            {
                return new NodeKeyValueCollection(this, true);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        public ICollection Values
        {
            get
            {
                return new NodeKeyValueCollection(this, false);
            }
        }

        public void Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", ArgumentNull_Key);
            }
            _version++;
            DictionaryNode last = null;
            DictionaryNode node;
            for (node = _head; node != null; node = node.next)
            {
                object oldKey = node.key;
                if ((_comparer == null) ? oldKey.Equals(key) : _comparer.Compare(oldKey, key) == 0)
                {
                    throw new ArgumentException(Argument_AddingDuplicate);
                }
                last = node;
            }
            // Not found, so add a new one
            DictionaryNode newNode = new DictionaryNode();
            newNode.key = key;
            newNode.value = value;
            if (last != null)
            {
                last.next = newNode;
            }
            else
            {
                _head = newNode;
            }
            _count++;
        }

        public void Clear()
        {
            _count = 0;
            _head = null;
            _version++;
        }

        public bool Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", ArgumentNull_Key);
            }
            for (DictionaryNode node = _head; node != null; node = node.next)
            {
                object oldKey = node.key;
                if ((_comparer == null) ? oldKey.Equals(key) : _comparer.Compare(oldKey, key) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", ArgumentOutOfRange_NeedNonNegNum);

            if (array.Length - index < _count)
                throw new ArgumentException(Arg_InsufficientSpace);

            for (DictionaryNode node = _head; node != null; node = node.next)
            {
                array.SetValue(new DictionaryEntry(node.key, node.value), index);
                index++;
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new NodeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NodeEnumerator(this);
        }

        public void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", ArgumentNull_Key);
            }
            _version++;
            DictionaryNode last = null;
            DictionaryNode node;
            for (node = _head; node != null; node = node.next)
            {
                object oldKey = node.key;
                if ((_comparer == null) ? oldKey.Equals(key) : _comparer.Compare(oldKey, key) == 0)
                {
                    break;
                }
                last = node;
            }
            if (node == null)
            {
                return;
            }
            if (node == _head)
            {
                _head = node.next;
            }
            else
            {
                last.next = node.next;
            }
            _count--;
        }

        private class NodeEnumerator : IDictionaryEnumerator
        {
            private ListDictionary _list;
            private DictionaryNode _current;
            private int _version;
            private bool _start;


            public NodeEnumerator(ListDictionary list)
            {
                _list = list;
                _version = list._version;
                _start = true;
                _current = null;
            }

            public object Current
            {
                get
                {
                    return Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException(InvalidOperation_EnumOpCantHappen);
                    }
                    return new DictionaryEntry(_current.key, _current.value);
                }
            }

            public object Key
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException(InvalidOperation_EnumOpCantHappen);
                    }
                    return _current.key;
                }
            }

            public object Value
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException(InvalidOperation_EnumOpCantHappen);
                    }
                    return _current.value;
                }
            }

            public bool MoveNext()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException(InvalidOperation_EnumFailedVersion);
                }
                if (_start)
                {
                    _current = _list._head;
                    _start = false;
                }
                else if (_current != null)
                {
                    _current = _current.next;
                }
                return (_current != null);
            }

            public void Reset()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException(InvalidOperation_EnumFailedVersion);
                }
                _start = true;
                _current = null;
            }
        }


        private class NodeKeyValueCollection : ICollection
        {
            private ListDictionary _list;
            private bool _isKeys;

            public NodeKeyValueCollection(ListDictionary list, bool isKeys)
            {
                _list = list;
                _isKeys = isKeys;
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", ArgumentOutOfRange_NeedNonNegNum);
                for (DictionaryNode node = _list._head; node != null; node = node.next)
                {
                    array.SetValue(_isKeys ? node.key : node.value, index);
                    index++;
                }
            }

            int ICollection.Count
            {
                get
                {
                    int count = 0;
                    for (DictionaryNode node = _list._head; node != null; node = node.next)
                    {
                        count++;
                    }
                    return count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return _list.SyncRoot;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new NodeKeyValueEnumerator(_list, _isKeys);
            }


            private class NodeKeyValueEnumerator : IEnumerator
            {
                private ListDictionary _list;
                private DictionaryNode _current;
                private int _version;
                private bool _isKeys;
                private bool _start;

                public NodeKeyValueEnumerator(ListDictionary list, bool isKeys)
                {
                    _list = list;
                    _isKeys = isKeys;
                    _version = list._version;
                    _start = true;
                    _current = null;
                }

                public object Current
                {
                    get
                    {
                        if (_current == null)
                        {
                            throw new InvalidOperationException(InvalidOperation_EnumOpCantHappen);
                        }
                        return _isKeys ? _current.key : _current.value;
                    }
                }

                public bool MoveNext()
                {
                    if (_version != _list._version)
                    {
                        throw new InvalidOperationException(InvalidOperation_EnumFailedVersion);
                    }
                    if (_start)
                    {
                        _current = _list._head;
                        _start = false;
                    }
                    else if (_current != null)
                    {
                        _current = _current.next;
                    }
                    return (_current != null);
                }

                public void Reset()
                {
                    if (_version != _list._version)
                    {
                        throw new InvalidOperationException(InvalidOperation_EnumFailedVersion);
                    }
                    _start = true;
                    _current = null;
                }
            }
        }

        private class DictionaryNode
        {
            public object key;
            public object value;
            public DictionaryNode next;
        }
    }
}