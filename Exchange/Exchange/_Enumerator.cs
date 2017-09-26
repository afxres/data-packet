﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Mikodev.Network
{
    internal class _Enumerator : _EnumeratorBase, IEnumerator, IDisposable
    {
        internal readonly IPacketConverter _con = null;

        internal object _cur = null;

        internal _Enumerator(PacketReader source, IPacketConverter converter) : base(source, converter) => _con = converter;

        object IEnumerator.Current => _cur;

        public bool MoveNext()
        {
            if (_idx >= _max)
                return false;
            var val = _bit;
            var idx = _idx;
            if ((_bit < 1 && _buf._Read(ref idx, out val, _max) == false) || (_bit > 0 && idx + val > _max))
                throw new PacketException(PacketError.Overflow);
            _cur = _con.GetValue(_buf, idx, val);
            _idx = idx + val;
            return true;
        }

        public void Reset()
        {
            _idx = _off;
            _cur = null;
        }
    }

    internal class _Enumerator<T> : _Enumerator, IEnumerator<T>
    {
        internal _Enumerator(PacketReader reader, IPacketConverter converter) : base(reader, converter) { }

        T IEnumerator<T>.Current => (_cur != null) ? (T)_cur : default(T);
    }
}
