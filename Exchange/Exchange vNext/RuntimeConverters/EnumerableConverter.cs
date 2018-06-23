﻿using System;
using System.Collections.Generic;

namespace Mikodev.Binary.RuntimeConverters
{
    internal sealed class EnumerableConverter<TE, TV> : Converter<TE>, IDelegateConverter where TE : IEnumerable<TV>
    {
        private readonly Converter<TV> converter;
        private readonly Func<IEnumerable<TV>, TE> toValue;

        public Delegate ToBytesFunction => null;

        public Delegate ToValueFunction => toValue;

        public EnumerableConverter(Converter<TV> converter, Func<IEnumerable<TV>, TE> toValue) : base(0)
        {
            this.converter = converter;
            this.toValue = toValue;
        }

        public override void ToBytes(Allocator allocator, TE value)
        {
            if (value == null)
                return;
            if (converter.Length == 0)
            {
                int offset;
                var stream = allocator.stream;
                foreach (var i in value)
                {
                    offset = stream.BeginModify();
                    converter.ToBytes(allocator, i);
                    stream.EndModify(offset);
                }
            }
            else
            {
                foreach (var i in value)
                {
                    converter.ToBytes(allocator, i);
                }
            }
        }

        public override TE ToValue(Block block)
        {
            if (toValue == null)
                throw new InvalidOperationException($"Unable to get collection, type : {typeof(TE)}");
            var enumerable = converter.Length == 0
                ? (IEnumerable<TV>)ListConverter<TV>.Value(block, converter)
                : ArrayConverter<TV>.Value(block, converter);
            return toValue.Invoke(enumerable);
        }
    }
}