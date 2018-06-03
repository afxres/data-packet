﻿using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Network.Converters
{
    internal class UnmanagedArrayConverter<T> : PacketConverter<T[]> where T : unmanaged
    {
        internal static readonly bool ReverseEndianness = BitConverter.IsLittleEndian != PacketConvert.UseLittleEndian && Unsafe.SizeOf<T>() != 1;
        internal static readonly T[] EmptyArray = new T[0];

        internal static byte[] ToBytes(T[] source)
        {
            if (source == null || source.Length == 0)
                return UnmanagedArrayConverter<byte>.EmptyArray;
            var targetLength = source.Length * Unsafe.SizeOf<T>();
            var target = new byte[targetLength];
            Unsafe.CopyBlockUnaligned(ref target[0], ref Unsafe.As<T, byte>(ref source[0]), (uint)targetLength);
            if (ReverseEndianness)
                Extension.ReverseEndiannessExplicitly<T>(target);
            return target;
        }

        internal static T[] ToValue(byte[] buffer, int offset, int length)
        {
            if (length == 0)
                return EmptyArray;
            if (buffer == null || length < 0 || offset < 0 || buffer.Length - offset < length || (length % Unsafe.SizeOf<T>()) != 0)
                throw PacketException.Overflow();
            var target = new T[length / Unsafe.SizeOf<T>()];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref target[0]), ref buffer[offset], (uint)length);
            if (ReverseEndianness)
                Extension.ReverseEndianness(target);
            return target;
        }

        public override int Length => 0;

        public override byte[] GetBytes(T[] value) => ToBytes(value);

        public override byte[] GetBytes(object value) => ToBytes((T[])value);

        public override object GetObject(byte[] buffer, int offset, int length) => ToValue(buffer, offset, length);

        public override T[] GetValue(byte[] buffer, int offset, int length) => ToValue(buffer, offset, length);
    }
}