﻿namespace Mikodev.Network.Tokens
{
    internal class ValueArray : Token
    {
        internal readonly byte[][] data;

        internal readonly int length;

        public override object Data => data;

        public ValueArray(byte[][] data, int length)
        {
            this.data = data;
            this.length = length;
        }

        public override void FlushTo(Allocator context, int level)
        {
            if (data == null)
                return;
            if (length > 0)
                for (var i = 0; i < data.Length; i++)
                    context.Append(data[i]);
            else
                for (var i = 0; i < data.Length; i++)
                    context.AppendValueExtend(data[i]);
        }
    }
}
