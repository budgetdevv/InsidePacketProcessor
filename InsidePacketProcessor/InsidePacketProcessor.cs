using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProtoBuf;

namespace InsideUtilities
{
    public readonly unsafe struct InsidePacketProcessor: IDisposable
    {
        private readonly Dictionary<int, Action<Stream>> Dict;

        private readonly int* TypeDefBuffer;

        private readonly GCHandle TypeDefGCHandle;
        
        public delegate void PacketProcessorAct<T>(ref T Item);
        
        public static InsidePacketProcessor CreateProcessor(int InitialSize = 10)
        {
            return new InsidePacketProcessor(InitialSize);
        }
        
        private InsidePacketProcessor(int InitialSize)
        {
            Dict = new Dictionary<int, Action<Stream>>(InitialSize);

            var Arr = GC.AllocateUninitializedArray<int>(1, true);

            fixed (int* x = Arr)
            {
                TypeDefBuffer = x;
            }

            TypeDefGCHandle = GCHandle.Alloc(Arr, GCHandleType.Pinned);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T Item, Stream Stream)
        {
            Serialize(ref Item, Stream, Stream.Position);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ref T Item, Stream Stream)
        {
            Serialize(ref Item, Stream, Stream.Position);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T Item, Stream Stream, long WriteIndex)
        {
            Serialize(ref Item, Stream, WriteIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ref T Item, Stream Stream, long WriteIndex)
        {
            Stream.Position = WriteIndex;

            WriteTypeDef<T>(Stream);

            Serializer.SerializeWithLengthPrefix(Stream, Item, PrefixStyle.Base128);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteTypeDef<T>(Stream Stream)
        {
            // ReSharper disable once PossibleNullReferenceException
            *TypeDefBuffer = GetHashFromType<T>();
            
            Stream.Write(new Span<byte>(TypeDefBuffer, 4));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(Stream Stream)
        {
            Deserialize(Stream, Stream.Position);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(Stream Stream, long ReadIndex)
        {
            Stream.Position = ReadIndex;

            ReadTypeDefAndGetAct(Stream)?.Invoke(Stream);
        }
        private Action<Stream> ReadTypeDefAndGetAct(Stream Stream)
        {
            var TypeBufferSpan = new Span<byte>(TypeDefBuffer, 4);

            Stream.Read(TypeBufferSpan);
            
            var Hash = *TypeDefBuffer;

            Dict.TryGetValue(Hash, out var Act);

            return Act;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHashFromType<T>()
        {
            // ReSharper disable once PossibleNullReferenceException
            var Str = typeof(T).FullName;
            
            unchecked
            {
                int Hash = 23;

                // ReSharper disable once ForCanBeConvertedToForeach
                for (int I = 0; I < Str.Length; I++)
                {
                    Hash *= 31;

                    Hash += Str[I];
                }
                
                return Hash;
            }
        }

        public void SubscribeToTypeReusable<T>(PacketProcessorAct<T> Act) where T: class
        {
            var Hash = GetHashFromType<T>();
            
            var Instance = Unsafe.As<T>(RuntimeHelpers.GetUninitializedObject(typeof(T)));

            var Success = Dict.TryAdd(Hash, Stream =>
            {
                Serializer.MergeWithLengthPrefix(Stream, Instance, PrefixStyle.Base128);

                Act.Invoke(ref Instance);
            });

            if (!Success)
            {
                throw new Exception($"Type of {typeof(T).FullName} is already registered! Consider Unsubscribing!");
            }
        }
        
        public void SubscribeToType<T>(PacketProcessorAct<T> Act)
        {
            var Hash = GetHashFromType<T>();

            var Success = Dict.TryAdd(Hash, Stream =>
            {
                var Instance = Serializer.DeserializeWithLengthPrefix<T>(Stream, PrefixStyle.Base128);
                
                Act.Invoke(ref Instance);
            });

            if (!Success)
            {
                throw new Exception($"Type of {typeof(T).FullName} is already registered! Consider Unsubscribing!");
            }
        }
        
        public void UnsubType<T>()
        {
            var Hash = GetHashFromType<T>();

            if (!Dict.Remove(Hash))
            {
                throw new Exception($"Cannot unsubscribe type of {typeof(T).FullName} if it wasn't Subscribed to!");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsubAllTypes()
        {
            Dict.Clear();
        }

        public void Dispose()
        {
            var Handle = TypeDefGCHandle;
            
            Handle.Free();
        }
    }
}