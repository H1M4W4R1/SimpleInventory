using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;

namespace Systems.SimpleInventory.Data.Native.Item
{
    /// <summary>
    ///     Item identifier, unique thanks to Ticks which are selected at moment of identifier creation
    ///     and shift that prevents collisions between different identifiers created at exactly same time.
    /// </summary>
    [BurstCompile] [Serializable] [StructLayout(LayoutKind.Explicit)]
    public readonly struct ItemID : IEquatable<ItemID>, IComparable<ItemID>
    {
        /// <summary>
        ///     Invalid item identifier
        /// </summary>
        public static readonly ItemID Invalid = new(0, 0);
        
        /// <summary>
        ///     Local iterator to prevent collisions.
        /// </summary>
        private static long _iterator;

        [FieldOffset(0)] private readonly int4 vectorized;
        [FieldOffset(0)] public readonly long ticks;
        [FieldOffset(8)] public readonly long shift;

        /// <summary>
        ///     Creates a new instance of <see cref="ItemID"/>. Do not use, see <see cref="New"/>
        /// </summary>
        public ItemID(long ticks, long shift)
        {
            vectorized = int4.zero;
            this.ticks = ticks;
            this.shift = shift;
        }

        [BurstDiscard] public static ItemID New() => new(DateTimeOffset.UtcNow.Ticks, _iterator++);

        public bool IsCreated => ticks != 0;
        

#region IEquatable<ItemID>

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(ItemID other)
            => math.all(vectorized == other.vectorized);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj)
            => obj is ItemID other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode()
            => vectorized.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in ItemID left, in ItemID right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in ItemID left, in ItemID right) => !left.Equals(right);

#endregion

#region IComparable<ItemID>

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public int CompareTo(ItemID other)
        {
            int ticksCompareResult = ticks.CompareTo(other.ticks);
            if (Hint.Unlikely(ticksCompareResult == 0)) return shift.CompareTo(other.shift);
            return ticksCompareResult;
        }

#endregion
    }
}