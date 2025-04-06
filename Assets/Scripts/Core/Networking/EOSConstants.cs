using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;

namespace RecipeRage.Core.Networking
{
    /// <summary>
    /// Constants and enums for EOS SDK compatibility.
    /// </summary>
    public static class EOSConstants
    {
        /// <summary>
        /// Advertisement type for session attributes.
        /// </summary>
        public enum AttributeAdvertisementType
        {
            /// <summary>
            /// Don't advertise the attribute.
            /// </summary>
            DontAdvertise = 0,

            /// <summary>
            /// Advertise the attribute.
            /// </summary>
            Advertise = 1
        }

        /// <summary>
        /// Permission level for sessions.
        /// </summary>
        public enum OnlineSessionPermissionLevel
        {
            /// <summary>
            /// Anyone can find this session.
            /// </summary>
            PublicAdvertised = 0,

            /// <summary>
            /// Only friends can find this session.
            /// </summary>
            JoinViaPresence = 1,

            /// <summary>
            /// Only invited users can find this session.
            /// </summary>
            InviteOnly = 2
        }

        /// <summary>
        /// Convert our AttributeAdvertisementType to the SDK's type.
        /// </summary>
        /// <param name="type">Our advertisement type.</param>
        /// <returns>The SDK's advertisement type.</returns>
        public static Epic.OnlineServices.Sessions.SessionAttributeAdvertisementType ConvertToSDKAttributeAdvertisementType(AttributeAdvertisementType type)
        {
            return (Epic.OnlineServices.Sessions.SessionAttributeAdvertisementType)(int)type;
        }

        /// <summary>
        /// Convert our OnlineSessionPermissionLevel to the SDK's type.
        /// </summary>
        /// <param name="level">Our permission level.</param>
        /// <returns>The SDK's permission level.</returns>
        public static Epic.OnlineServices.Sessions.OnlineSessionPermissionLevel ConvertToSDKOnlineSessionPermissionLevel(OnlineSessionPermissionLevel level)
        {
            return (Epic.OnlineServices.Sessions.OnlineSessionPermissionLevel)(int)level;
        }
    }
}
