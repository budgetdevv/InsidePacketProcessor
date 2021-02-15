using System;
using System.IO;
using InsideUtilities;
using ProtoBuf;

namespace Test
{
    class Program
    {
        private static InsidePacketProcessor IPP;
        
        [ProtoContract]
        private struct FooStruct
        {
            [ProtoMember(1)]
            public int Int { get; set; }

            [ProtoMember(2)]
            public ulong Ulong { get; set; }

            public FooStruct(int _Int, ulong _Ulong)
            {
                Int = _Int;

                Ulong = _Ulong;
            }
        }
        

        [ProtoContract]
        private class FooClass
        {
            [ProtoMember(1)]
            public int Int { get; set; }

            [ProtoMember(2)]
            public ulong Ulong { get; set; }

            public FooClass(int _Int, ulong _Ulong)
            {
                Int = _Int;

                Ulong = _Ulong;
            }
        }
        
        static void Main(string[] args)
        {
            //Test1();

            Test2();
        }

        private static void Test1()
        {
            IPP = InsidePacketProcessor.CreateProcessor();

            var MS = new MemoryStream(1000);

            var FooStruct = new FooStruct(69, 1258);
            
            var FooClass = new FooClass(69, 1258);

            IPP.Serialize(ref FooStruct, MS, 0);
            
            IPP.Serialize(ref FooClass, MS, 500);

            IPP.SubscribeToType((ref FooStruct x) =>
            {
                Console.WriteLine($"FooStruct! {x.Int} | {x.Ulong}");
            });
            
            IPP.SubscribeToTypeReusable((ref FooClass x) =>
            {
                Console.WriteLine($"FooClass! {x.Int} | {x.Ulong}");
            });
            
            IPP.Deserialize(MS, 0);
            
            IPP.Deserialize(MS, 0);
            
            IPP.Deserialize(MS, 500);
            
            IPP.Deserialize(MS, 500);
            
            IPP.UnsubType<FooClass>();

            Console.WriteLine("After Unsub");
            
            IPP.Deserialize(MS, 0);
            
            IPP.Deserialize(MS, 500); //This shouldn't run!
        }
        
        private static void Test2()
        {
            IPP = InsidePacketProcessor.CreateProcessor();

            var MS = new MemoryStream(1000);

            var FooStruct = new FooStruct(69, 1258);
            
            var FooClass = new FooClass(69, 1258);

            IPP.Serialize(ref FooStruct, MS, 0);
            
            IPP.Serialize(ref FooClass, MS, 500);

            IPP.SubscribeToType((ref FooStruct x) =>
            {
                Console.WriteLine($"FooStruct! {x.Int} | {x.Ulong}");
            });
            
            //This is commented out, and hence nothing should happen!
            
            // IPP.SubscribeToTypeReusable((ref FooClass x) =>
            // {
            //     Console.WriteLine($"FooClass! {x.Int} | {x.Ulong}");
            // });

            IPP.Deserialize(MS, 0);
            
            IPP.Deserialize(MS, 500); //This shouldn't run!
        }
    }
}