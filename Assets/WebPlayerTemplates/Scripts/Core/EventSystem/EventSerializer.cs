// using System;
// using Unity.Collections;
// using Unity.Netcode;
// using UnityEngine;
//
// namespace Core.EventSystem
// {
//     public static class EventSerializer
//     {
//         public static byte[] Serialize(INetworkEventArgs args)
//         {
//             using (FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp))
//             {
//                 args.Serialize(writer);
//                 return writer.ToArray();
//             }
//         }
//
//         public static INetworkEventArgs Deserialize(Type type, byte[] data)
//         {
//             if (Activator.CreateInstance(type) is INetworkEventArgs args)
//             {
//                 using (FastBufferReader reader = new FastBufferReader(data, Allocator.Temp))
//                 {
//                     args.Deserialize(reader);
//                 }
//                 return args;
//             }
//             Debug.LogError($"Failed to create instance of type {type}");
//             return null;
//         }
//     }
// }
