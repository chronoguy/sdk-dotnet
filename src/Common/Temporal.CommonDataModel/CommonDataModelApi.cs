using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Temporal.CommonDataModel
{
    public class CommonDataModelApi
    {
    }

    public class PayloadsCollection : IReadOnlyList<Payload>
    {
        public static readonly PayloadsCollection Empty = new PayloadsCollection();

        public int Count { get; }
        public Payload this[int index] { get { return null; } }
        public IEnumerator<Payload> GetEnumerator() { return null; }
        IEnumerator IEnumerable.GetEnumerator() { return null; }
    }

    public class MutablePayloadsCollection : PayloadsCollection, ICollection<Payload>
    {
        public bool IsReadOnly { get { return false; } }
        public void Add(Payload item) { }
        public void Clear() { }
        public bool Contains(Payload item) { return false; }
        public void CopyTo(Payload[] array, int arrayIndex) { }
        public bool Remove(Payload item) { return false; }        
    }

    public class Payload
    {
        public IReadOnlyDictionary<string, Stream> Metadata { get; }
        public Stream Data { get; }
    }
}
