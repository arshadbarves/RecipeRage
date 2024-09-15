// using System;
// using System.Linq;
// using System.Reflection;
//
// namespace Core.EventSystem
// {
//     public static class EventRegistration
//     {
//         public static void RegisterAllEvents(EventManager eventManager)
//         {
//             var assembly = Assembly.GetExecutingAssembly();
//             var eventHandlerTypes = assembly.GetTypes()
//                 .Where(t => typeof(IEventHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
//
//             foreach (var handlerType in eventHandlerTypes)
//             {
//                 var handler = Activator.CreateInstance(handlerType) as IEventHandler;
//                 var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
//                     .Where(m => m.Name is "Handle" or "HandleNetwork");
//
//                 foreach (var method in methods)
//                 {
//                     var parameters = method.GetParameters();
//                     if (parameters.Length == 1)
//                     {
//                         var eventArgType = parameters[0].ParameterType;
//                         if (typeof(INetworkEventArgs).IsAssignableFrom(eventArgType))
//                         {
//                             var registerMethod = typeof(EventManager).GetMethod("AddNetworkListener")
//                                 ?.MakeGenericMethod(eventArgType);
//                             registerMethod?.Invoke(eventManager, new object[] { handler });
//                         }
//                         else if (typeof(IEventArgs).IsAssignableFrom(eventArgType))
//                         {
//                             var registerMethod = typeof(EventManager).GetMethod("AddLocalListener")
//                                 ?.MakeGenericMethod(eventArgType);
//                             registerMethod?.Invoke(eventManager, new object[] { handler });
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }