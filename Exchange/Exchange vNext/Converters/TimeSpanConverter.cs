﻿using System;
using System.Runtime.CompilerServices;
using T = System.Int64;

namespace Mikodev.Binary.Converters
{
    internal sealed class TimeSpanConverter : Converter<TimeSpan>
    {
        public TimeSpanConverter() : base(Unsafe.SizeOf<T>()) { }

        public override void ToBytes(Allocator allocator, TimeSpan value) => UnmanagedValueConverter<T>.Bytes(allocator, value.Ticks);

        public override TimeSpan ToValue(Block block) => new TimeSpan(UnmanagedValueConverter<T>.Value(block));
    }
}