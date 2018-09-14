﻿using System.Collections;
using System.Collections.Generic;

namespace Mikodev.Network
{
    internal sealed class DictionaryAdapter<TK, TV> : IEnumerable<KeyValuePair<byte[], object>>
    {
        private readonly PacketConverter<TK> converter;

        private readonly IEnumerable<KeyValuePair<TK, TV>> dictionary;

        internal DictionaryAdapter(PacketConverter converter, IEnumerable<KeyValuePair<TK, TV>> dictionary)
        {
            this.converter = (PacketConverter<TK>)converter;
            this.dictionary = dictionary;
        }

        private IEnumerator<KeyValuePair<byte[], object>> Enumerator()
        {
            foreach (var i in dictionary)
                yield return new KeyValuePair<byte[], object>(converter.GetBytesChecked(i.Key), i.Value);
        }

        IEnumerator IEnumerable.GetEnumerator() => Enumerator();

        IEnumerator<KeyValuePair<byte[], object>> IEnumerable<KeyValuePair<byte[], object>>.GetEnumerator() => Enumerator();
    }
}
