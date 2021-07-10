﻿
using Newtonsoft.Json;
using System;
using System.Buffers;

namespace Rsbc.Dmf.PhsaAdapter.Formatters
{
    internal class JsonArrayPool : IArrayPool<char>
    {
        private readonly ArrayPool<char> _inner;

        public JsonArrayPool(ArrayPool<char> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public char[] Rent(int minimumLength)
        {
            return _inner.Rent(minimumLength);
        }

        public void Return(char[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            _inner.Return(array);
        }
    }
}
