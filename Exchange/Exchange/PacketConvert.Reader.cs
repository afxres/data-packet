﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Mikodev.Network
{
    partial class PacketConvert
    {
        public static object GetValue(this PacketReader reader, Type type)
        {
            ThrowIfArgumentError(type);
            ThrowIfArgumentError(reader);
            var con = _Caches.Converter(type, reader._con, false);
            var val = con._GetValueWrapError(reader._spa, true);
            return val;
        }

        public static T GetValue<T>(this PacketReader reader)
        {
            ThrowIfArgumentError(reader);
            var con = _Caches.Converter<T>(reader._con, false);
            var val = con._GetValueWrapErrorAuto<T>(reader._spa, true);
            return val;
        }

        public static byte[] GetBytes(this PacketReader reader)
        {
            ThrowIfArgumentError(reader);
            return reader._spa.GetBytes();
        }

        public static sbyte[] GetSBytes(this PacketReader reader)
        {
            ThrowIfArgumentError(reader);
            return reader._spa.GetSBytes();
        }

        public static IEnumerable GetEnumerable(this PacketReader reader, Type type)
        {
            ThrowIfArgumentError(type);
            ThrowIfArgumentError(reader);
            return new _Enumerable(reader, type);
        }

        public static IEnumerable<T> GetEnumerable<T>(this PacketReader reader)
        {
            ThrowIfArgumentError(reader);
            return new _Enumerable<T>(reader);
        }

        public static T[] GetArray<T>(this PacketReader reader)
        {
            ThrowIfArgumentError(reader);
            var con = _Caches.Converter<T>(reader._con, false);
            var val = reader._spa.Array<T>(con);
            return val;
        }

        public static List<T> GetList<T>(this PacketReader reader)
        {
            ThrowIfArgumentError(reader);
            var con = _Caches.Converter<T>(reader._con, false);
            var val = reader._spa.List<T>(con);
            return val;
        }

        public static PacketReader GetItem(this PacketReader reader, string key, bool nothrow = false)
        {
            ThrowIfArgumentError(reader);
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return reader._GetItem(key, nothrow);
        }

        public static PacketReader GetItem(this PacketReader reader, IEnumerable<string> keys, bool nothrow = false)
        {
            ThrowIfArgumentError(reader);
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            return reader._GetItem(keys, nothrow);
        }
    }
}
