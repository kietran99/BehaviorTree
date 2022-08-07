using System;

namespace RR.AI
{  
    public struct BBValue<T> : IBBValueBase
    {
        public readonly T value;

        public BBValue(T value)
        {
            this.value = value;
        }

        public static implicit operator BBValue<T>(T val) => new BBValue<T>(val);
        public static implicit operator T(BBValue<T> val) => val.value;

        public object ValueAsObject => value as object;
        public Type ValueType => typeof(T);
    }
}