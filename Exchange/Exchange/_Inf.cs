﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Mikodev.Network
{
    internal sealed class _Inf
    {
        internal const int Enum = 1;
        internal const int Array = 2;
        internal const int List = 4;
        internal const int Enumerable = 8;
        internal const int EnumerableImpl = 16;
        internal const int Collection = 32;
        internal const int Dictionary = 64;
        internal const int EnumerableKeyValuePair = 128;

        internal Type ElementType { get; set; }

        internal Type IndexType { get; set; }

        internal Func<_Element, IPacketConverter, object> GetArray { get; set; }

        internal Func<_Element, IPacketConverter, object> GetList { get; set; }

        internal Func<_Element, IPacketConverter, object> GetCollection { get; set; }

        internal Func<_Element, IPacketConverter, object> GetEnumerable { get; set; }

        internal Func<_Element, IPacketConverter, IPacketConverter, object> GetDictionary { get; set; }

        internal Func<object[], object> CastToArray { get; set; }

        internal Func<object[], object> CastToList { get; set; }

        internal Func<object[], object> CastToCollection { get; set; }

        internal Func<IEnumerable<KeyValuePair<object, object>>, object> CastToDictionary { get; set; }

        internal Func<IPacketConverter, object, MemoryStream> FromEnumerable { get; set; }

        internal Func<IPacketConverter, IPacketConverter, object, MemoryStream> FromEnumerableKeyValuePair { get; set; }

        internal Func<IPacketConverter, object, IEnumerable<KeyValuePair<byte[], object>>> GetAdapter { get; set; }

        internal int Flags { get; set; }
    }
}