﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using ConverterDictionary = System.Collections.Generic.IDictionary<System.Type, Mikodev.Network.IPacketConverter>;

namespace Mikodev.Network
{
    /// <summary>
    /// 数据包解析工具. Binary packet reader
    /// </summary>
    public sealed class PacketReader : IDynamicMetaObjectProvider
    {
        internal _Element _spa;
        internal Dictionary<string, PacketReader> _dic = null;
        internal readonly ConverterDictionary _con = null;

        /// <summary>
        /// 创建对象并指定字节数组和转换器. Create reader with byte array and converters
        /// </summary>
        /// <param name="buffer">Binary data packet (Should be readonly)</param>
        /// <param name="converters">Packet converters, use default converters if null</param>
        public PacketReader(byte[] buffer, ConverterDictionary converters = null)
        {
            _spa = new _Element(buffer);
            _con = converters;
        }

        /// <summary>
        /// 创建对象并指定部分字节数组和转换器. Create reader with part of byte array and converters
        /// </summary>
        /// <param name="buffer">Binary data packet (Should be readonly)</param>
        /// <param name="offset">Start index</param>
        /// <param name="length">Packet length</param>
        /// <param name="converters">Packet converters, use default converters if null</param>
        public PacketReader(byte[] buffer, int offset, int length, ConverterDictionary converters = null)
        {
            _spa = new _Element(buffer, offset, length);
            _con = converters;
        }

        /// <summary>
        /// Parse this packet (return false if error)
        /// </summary>
        internal bool _Init()
        {
            if (_dic != null)
                return true;
            if (_spa._idx != _spa._off)
                return false;
            var dic = new Dictionary<string, PacketReader>();
            var len = 0;

            while (_spa._idx < _spa._max)
            {
                if (_spa._buf._Read(_spa._max, ref _spa._idx, out len) == false)
                    return false;
                var key = Encoding.UTF8.GetString(_spa._buf, _spa._idx, len);
                if (dic.ContainsKey(key))
                    return false;
                _spa._idx += len;
                if (_spa._buf._Read(_spa._max, ref _spa._idx, out len) == false)
                    return false;
                dic.Add(key, new PacketReader(_spa._buf, _spa._idx, len, _con));
                _spa._idx += len;
            }

            _dic = dic;
            return true;
        }

        internal PacketReader _GetItem(string key, bool nothrow)
        {
            var res = _Init();
            if (res == true && _dic.TryGetValue(key, out var val))
                return val;
            if (nothrow)
                return null;
            throw new PacketException(res ? PacketError.PathError : PacketError.Overflow);
        }

        internal PacketReader _SetItem(IEnumerable<string> keys, bool nothrow)
        {
            var rdr = this;
            foreach (var i in keys)
                if ((rdr = rdr._GetItem(i, nothrow)) == null)
                    return null;
            return rdr;
        }

        /// <summary>
        /// 子节点个数. Child node count
        /// </summary>
        public int Count => _Init() ? _dic.Count : 0;

        /// <summary>
        /// 字节点标签集合. Child node keys
        /// </summary>
        public IEnumerable<string> Keys => _Init() ? _dic.Keys : Enumerable.Empty<string>();

        /// <summary>
        /// 根据路径获取子节点. Get node by path
        /// </summary>
        /// <param name="path">Node path</param>
        /// <param name="nothrow">return null if not found</param>
        /// <param name="split">Path separators, use default separators if null</param>
        [IndexerName("Node")]
        public PacketReader this[string path, bool nothrow = false, char[] split = null] => _SetItem(path?.Split(split ?? _Extension.s_seps) ?? new[] { string.Empty }, nothrow);

        /// <summary>
        /// 根据标签获取子节点. Get node by key
        /// </summary>
        /// <param name="key">Node key</param>
        /// <param name="nothrow">return null if not found</param>
        public PacketReader Pull(string key, bool nothrow = false) => _GetItem(key ?? string.Empty, nothrow);

        /// <summary>
        /// 根据标签集合依序获取子节点. Get node by key collection
        /// </summary>
        /// <param name="keys">key collection</param>
        /// <param name="nothrow">return null if not found</param>
        public PacketReader Pull(string[] keys, bool nothrow = false)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (keys.Length < 1)
                throw new ArgumentException("Key collection can not be empty!");
            return _SetItem(keys, nothrow);
        }

        /// <summary>
        /// 从当前节点读取目标类型对象. Convert current node to target type object
        /// </summary>
        /// <param name="type">Target type</param>
        public object Pull(Type type) => _Caches.Converter(type, _con, false)._GetValueWrapErr(_spa, true);

        /// <summary>
        /// 从当前节点读取目标类型对象 (泛型). Convert current node to target type object
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        public T Pull<T>() => _Caches.Converter(typeof(T), _con, false)._GetValueWrapErr<T>(_spa._buf, _spa._off, _spa._len, true);

        /// <summary>
        /// 复制当前节点部分的字节数组. Get byte array of current node
        /// </summary>
        public byte[] PullList() => _spa._buf._ToBytes(_spa._off, _spa._len);

        /// <summary>
        /// 从当前节点读取目标类型对象集合. Convert current node to target type object collection
        /// </summary>
        /// <param name="type">Target type</param>
        public IEnumerable PullList(Type type) => new _Enumerable(this, type);

        /// <summary>
        /// 从当前节点读取目标类型对象集合 (泛型). Convert current node to target type object collection
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        public IEnumerable<T> PullList<T>() => new _Enumerable<T>(this);

        /// <summary>
        /// 打印对象类型, 子节点个数和字节长度. Show byte count or node count
        /// </summary>
        public override string ToString()
        {
            var stb = new StringBuilder(nameof(PacketReader));
            stb.Append(" with ");
            if (_Init())
                stb.AppendFormat("{0} node(s), ", _dic.Count);
            stb.AppendFormat("{0} byte(s)", _spa._len);
            return stb.ToString();
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) => new _DynamicReader(parameter, this);

        internal bool _Convert(Type type, out object value)
        {
            var val = default(object);
            var con = default(IPacketConverter);
            if ((con = _Caches.Converter(type, _con, true)) != null)
                val = con._GetValueWrapErr(_spa, true);
            else if (type._IsGenericEnumerable(out var inn))
                val = _Caches.Enumerable(this, inn);
            else if (type._IsArray(out inn))
                val = _Caches.Array(this, inn);
            else if (type._IsList(out inn))
                val = _Caches.List(this, inn);
            else goto fail;

            value = val;
            return true;

            fail:
            value = null;
            return false;
        }

        /// <summary>
        /// 反序列化当前节点. Deserialize current node
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="anonymous">Anonymous object</param>
        public T Deserialize<T>(T anonymous) => (T)_Deserialize(this, typeof(T));

        /// <summary>
        /// 反序列化当前节点. Deserialize current node
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        public T Deserialize<T>() => (T)_Deserialize(this, typeof(T));

        /// <summary>
        /// 反序列化当前节点. Deserialize current node
        /// </summary>
        /// <param name="target">Target type</param>
        public object Deserialize(Type target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return _Deserialize(this, target);
        }

        internal static object _Deserialize(PacketReader reader, Type type)
        {
            if (type == typeof(object))
                return reader;

            if (reader._Convert(type, out var ret))
                return ret;

            var res = _Caches.SetMethods(type);
            if (res is _Caches._AnonInfo ann)
            {
                var arr = ann._args;
                var fun = ann._func;
                var obj = new object[arr.Length];

                for (int i = 0; i < arr.Length; i++)
                    obj[i] = _Deserialize(reader._GetItem(arr[i]._name, false), arr[i]._type);
                var val = fun.Invoke(obj);
                return val;
            }
            else if (res is _Caches._SetInfo set)
            {
                var arr = set._sets;
                var fun = set._func;
                var obj = fun.Invoke();

                for (int i = 0; i < arr.Length; i++)
                {
                    var cur = arr[i];
                    var val = _Deserialize(reader._GetItem(cur._name, false), cur._type);
                    cur._func.Invoke(obj, val);
                }
                return obj;
            }
            throw new PacketException(PacketError.InvalidType);
        }
    }
}
