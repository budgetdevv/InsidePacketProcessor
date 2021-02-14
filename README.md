# InsidePacketProcessor
A serializer that automatically deserializes a Stream to the correct type! Uses protobuf-net internally.

Note: As it uses protobuf-net internally, you will have to include the appropriate attributes inside the class / struct you're registering! More info can be found @ https://github.com/protobuf-net/protobuf-net ! Examples can be found in Tests/Test c;

# Notable features include

- Ability to Serialize / Deserialize classes / structs without public parameterless constructor ( Apparently this is a limitation of protobuf-net )

- Zero-garbage! This library doesn't allocate anything that is tracked by the GC! ( This is assuming that you use .SubscribeToTypeReusable<T>() for classes! )

- Easy to use! Just register the appropriate type with .SubscribeToType<T>() or its reusable variant!
  
  
# APIs:

- Serialize<T>(ref T Item, Stream Stream, int WriteIndex = 0): Attempt to serialize a specific class / struct into a Stream
  
  Item: The class / struct to serialize
  
  Stream: The stream to write to
  
  WriteIndex: An optional parameter to specify the Stream position to start writing to. This is by default 0.

- Deserialize(Stream Stream, int ReadIndex = 0): Attempt to deserialize a specific type by reading from the specified Stream! If the type is Subscribed to ( Using .SubscribeToType<T>() or its reusable variant ), it would invoke the registered anonymous function!
  
  Stream: The stream to read from
  
  ReadIndex: An optional parameter to specify the Stream position to start reading from. This is by default 0.
  
- SubscribeToType<T>(PacketProcessorAct<T> Act): Allows you to subscribe to a specific type with an anonymous function, such that when .Deserialize() deserializes to the subscribed type, it would invoke the supplied anonymous function!
  
  PacketProcessorAct<T> Act: The anonymous function!

- SubscribeToTypeReusable<T>(PacketProcessorAct<T> Act): Reusable variant of the above subscribe method! This is useful if you wish to reuse an instance of a class instead of instantiating a new one when deserialization occurs! This is useful for reducing GC pressure...Consequentially, the class instance's properties may be overwritten on next deserialize!

  PacketProcessorAct<T> Act: The anonymous function!

- UnsubType<T>(): Unsubscribes the type, and consequentially, previously subscribed anonymous function would no longer be invoked on deserialization of the type.
