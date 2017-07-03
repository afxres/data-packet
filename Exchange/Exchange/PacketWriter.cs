﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using PushFunc = System.Func<object, byte[]>;

namespace Mikodev.Network
{
    /// <summary>
    /// 数据包生成器
    /// </summary>
    public partial class PacketWriter : IDynamicMetaObjectProvider
    {
        internal byte[] _dat = null;
        internal Dictionary<string, PacketWriter> _dic = null;
        internal Dictionary<Type, PushFunc> _funs = null;

        /// <summary>
        /// 创建新的数据包生成器
        /// </summary>
        /// <param name="funcs">类型转换工具词典 为空时使用默认词典</param>
        public PacketWriter(Dictionary<Type, PushFunc> funcs = null)
        {
            _funs = funcs ?? PacketExtensions.PushFuncs();
        }

        internal PushFunc _Func(Type type, bool nothrow = false)
        {
            if (_funs.TryGetValue(type, out var fun))
                return fun;
            if (type.IsValueType())
                return (val) => PacketExtensions.GetBytes(val, type);
            if (nothrow)
                return null;
            throw new PacketException(PacketErrorCode.InvalidType);
        }

        internal PacketWriter _Item(string key, PacketWriter another = null)
        {
            _dat = null;
            if (_dic == null)
                _dic = new Dictionary<string, PacketWriter>();
            if (_dic.TryGetValue(key, out var val))
                return val;
            val = another ?? new PacketWriter();
            _dic.Add(key, val);
            return val;
        }

        internal PacketWriter _Push(string key, byte[] buffer)
        {
            var val = _Item(key);
            val._dic = null;
            val._dat = buffer;
            return this;
        }

        /// <summary>
        /// 写入标签和另一个实例的数据
        /// </summary>
        public PacketWriter Push(string key, PacketWriter other)
        {
            _Item(key, other);
            return this;
        }

        /// <summary>
        /// 写入标签和数据
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="key">字符串标签</param>
        /// <param name="value">待写入数据</param>
        public PacketWriter Push<T>(string key, T value) => Push(key, typeof(T), value);

        /// <summary>
        /// 写入标签和数据
        /// </summary>
        /// <param name="key">字符串标签</param>
        /// <param name="type">目标类型</param>
        /// <param name="value">待写入数据</param>
        public PacketWriter Push(string key, Type type, object value)
        {
            var fun = _Func(type);
            var buf = fun.Invoke(value);
            return _Push(key, buf);
        }

        /// <summary>
        /// 写入标签和字节数据
        /// </summary>
        public PacketWriter PushList(string key, byte[] buffer) => _Push(key, buffer);

        /// <summary>
        /// 写入标签和对象集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">标签</param>
        /// <param name="value">数据集合</param>
        /// <param name="withLengthInfo">是否写入长度信息 (仅针对值类型)</param>
        public PacketWriter PushList<T>(string key, IEnumerable<T> value, bool withLengthInfo = false) => PushList(key, typeof(T), value, withLengthInfo);

        /// <summary>
        /// 写入标签和对象集合
        /// </summary>
        /// <param name="key">标签</param>
        /// <param name="type">对象类型</param>
        /// <param name="value">数据集合</param>
        /// <param name="withLengthInfo">是否写入长度信息 (仅针对值类型)</param>
        public PacketWriter PushList(string key, Type type, IEnumerable value, bool withLengthInfo = false)
        {
            var inf = withLengthInfo || type.IsValueType() == false;
            var str = new MemoryStream();
            var fun = _Func(type);
            foreach (var v in value)
                str.Write(fun.Invoke(v), inf);
            return _Push(key, str.ToArray());
        }

        internal void _GetBytes(Stream str, Dictionary<string, PacketWriter> dic)
        {
            foreach (var i in dic)
            {
                str.Write(i.Key.GetBytes(), true);
                var val = i.Value;
                if (val._dat != null)
                {
                    str.Write(val._dat, true);
                    continue;
                }
                if (val._dic == null)
                {
                    str.Write(0);
                    continue;
                }

                var pos = str.Position;
                str.Write(0);
                _GetBytes(str, val._dic);
                var end = str.Position;
                str.Seek(pos, SeekOrigin.Begin);
                str.Write((int)(end - pos - sizeof(int)));
                str.Seek(end, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// 生成数据包
        /// </summary>
        public byte[] GetBytes()
        {
            if (_dic == null)
                return new byte[0];
            var mst = new MemoryStream();
            _GetBytes(mst, _dic);
            return mst.ToArray();
        }

        /// <summary>
        /// 将所有数据写入到目标流中
        /// Write all data to target stream
        /// </summary>
        public void WriteTo(Stream stream)
        {
            if (stream.CanSeek == false || stream.CanWrite == false)
                throw new ArgumentException();
            if (_dic == null)
                return;
            _GetBytes(stream, _dic);
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DynamicPacketWriter(parameter, this);
        }

        /// <summary>
        /// 在字符串中输出键值和元素
        /// </summary>
        public override string ToString()
        {
            var stb = new StringBuilder(nameof(PacketWriter));
            stb.Append(" with ");
            if (_dat != null)
                stb.AppendFormat("{0} byte(s)", _dat.Length);
            else if (_dic != null)
                stb.AppendFormat("{0} node(s)", _dic.Count);
            else
                stb.Append("none");
            return stb.ToString();
        }
    }
}
