using System;
using RecipeRage.Modules.Lobbies.Data;

namespace RecipeRage.Modules.Lobbies.Events
{
    /// <summary>
    /// Event raised when a lobby is created
    /// </summary>
    public class LobbyCreatedEvent
    {
        /// <summary>
        /// The created lobby
        /// </summary>
        public LobbyInfo Lobby { get; }
        
        /// <summary>
        /// Create a new LobbyCreatedEvent
        /// </summary>
        /// <param name="lobby">The created lobby</param>
        public LobbyCreatedEvent(LobbyInfo lobby)
        {
            Lobby = lobby;
        }
    }
    
    /// <summary>
    /// Event raised when a lobby is joined
    /// </summary>
    public class LobbyJoinedEvent
    {
        /// <summary>
        /// The joined lobby
        /// </summary>
        public LobbyInfo Lobby { get; }
        
        /// <summary>
        /// Create a new LobbyJoinedEvent
        /// </summary>
        /// <param name="lobby">The joined lobby</param>
        public LobbyJoinedEvent(LobbyInfo lobby)
        {
            Lobby = lobby;
        }
    }
    
    /// <summary>
    /// Event raised when a lobby is left
    /// </summary>
    public class LobbyLeftEvent
    {
        /// <summary>
        /// ID of the left lobby
        /// </summary>
        public string LobbyId { get; }
        
        /// <summary>
        /// Create a new LobbyLeftEvent
        /// </summary>
        /// <param name="lobbyId">ID of the left lobby</param>
        public LobbyLeftEvent(string lobbyId)
        {
            LobbyId = lobbyId;
        }
    }
    
    /// <summary>
    /// Event raised when a lobby is updated
    /// </summary>
    public class LobbyUpdatedEvent
    {
        /// <summary>
        /// The updated lobby
        /// </summary>
        public LobbyInfo Lobby { get; }
        
        /// <summary>
        /// Create a new LobbyUpdatedEvent
        /// </summary>
        /// <param name="lobby">The updated lobby</param>
        public LobbyUpdatedEvent(LobbyInfo lobby)
        {
            Lobby = lobby;
        }
    }
    
    /// <summary>
    /// Event raised when a member joins a lobby
    /// </summary>
    public class MemberJoinedEvent
    {
        /// <summary>
        /// ID of the lobby
        /// </summary>
        public string LobbyId { get; }
        
        /// <summary>
        /// The joined member
        /// </summary>
        public LobbyMember Member { get; }
        
        /// <summary>
        /// Create a new MemberJoinedEvent
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="member">The joined member</param>
        public MemberJoinedEvent(string lobbyId, LobbyMember member)
        {
            LobbyId = lobbyId;
            Member = member;
        }
    }
    
    /// <summary>
    /// Event raised when a member leaves a lobby
    /// </summary>
    public class MemberLeftEvent
    {
        /// <summary>
        /// ID of the lobby
        /// </summary>
        public string LobbyId { get; }
        
        /// <summary>
        /// The member who left
        /// </summary>
        public LobbyMember Member { get; }
        
        /// <summary>
        /// Create a new MemberLeftEvent
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="member">The member who left</param>
        public MemberLeftEvent(string lobbyId, LobbyMember member)
        {
            LobbyId = lobbyId;
            Member = member;
        }
    }
    
    /// <summary>
    /// Event raised when a member is updated
    /// </summary>
    public class MemberUpdatedEvent
    {
        /// <summary>
        /// ID of the lobby
        /// </summary>
        public string LobbyId { get; }
        
        /// <summary>
        /// The updated member
        /// </summary>
        public LobbyMember Member { get; }
        
        /// <summary>
        /// Create a new MemberUpdatedEvent
        /// </summary>
        /// <param name="lobbyId">ID of the lobby</param>
        /// <param name="member">The updated member</param>
        public MemberUpdatedEvent(string lobbyId, LobbyMember member)
        {
            LobbyId = lobbyId;
            Member = member;
        }
    }
    
    /// <summary>
    /// Event raised when an invite is received
    /// </summary>
    public class InviteReceivedEvent
    {
        /// <summary>
        /// ID of the invite
        /// </summary>
        public string InviteId { get; }
        
        /// <summary>
        /// ID of the sender
        /// </summary>
        public string SenderId { get; }
        
        /// <summary>
        /// Display name of the sender
        /// </summary>
        public string SenderDisplayName { get; }
        
        /// <summary>
        /// ID of the lobby
        /// </summary>
        public string LobbyId { get; }
        
        /// <summary>
        /// Create a new InviteReceivedEvent
        /// </summary>
        /// <param name="inviteId">ID of the invite</param>
        /// <param name="senderId">ID of the sender</param>
        /// <param name="senderDisplayName">Display name of the sender</param>
        /// <param name="lobbyId">ID of the lobby</param>
        public InviteReceivedEvent(string inviteId, string senderId, string senderDisplayName, string lobbyId)
        {
            InviteId = inviteId;
            SenderId = senderId;
            SenderDisplayName = senderDisplayName;
            LobbyId = lobbyId;
        }
    }
    
    /// <summary>
    /// Event raised when matchmaking starts
    /// </summary>
    public class MatchmakingStartedEvent
    {
        /// <summary>
        /// Options used for matchmaking
        /// </summary>
        public MatchmakingOptions Options { get; }
        
        /// <summary>
        /// Ticket ID for the matchmaking request
        /// </summary>
        public string TicketId { get; }
        
        /// <summary>
        /// Create a new MatchmakingStartedEvent
        /// </summary>
        /// <param name="options">Options used for matchmaking</param>
        /// <param name="ticketId">Ticket ID for the matchmaking request</param>
        public MatchmakingStartedEvent(MatchmakingOptions options, string ticketId)
        {
            Options = options;
            TicketId = ticketId;
        }
    }
    
    /// <summary>
    /// Event raised when matchmaking is canceled
    /// </summary>
    public class MatchmakingCanceledEvent
    {
        /// <summary>
        /// Ticket ID for the matchmaking request
        /// </summary>
        public string TicketId { get; }
        
        /// <summary>
        /// Create a new MatchmakingCanceledEvent
        /// </summary>
        /// <param name="ticketId">Ticket ID for the matchmaking request</param>
        public MatchmakingCanceledEvent(string ticketId)
        {
            TicketId = ticketId;
        }
    }
    
    /// <summary>
    /// Event raised when matchmaking completes successfully
    /// </summary>
    public class MatchmakingCompleteEvent
    {
        /// <summary>
        /// Ticket ID for the matchmaking request
        /// </summary>
        public string TicketId { get; }
        
        /// <summary>
        /// The created or joined lobby
        /// </summary>
        public LobbyInfo Lobby { get; }
        
        /// <summary>
        /// Create a new MatchmakingCompleteEvent
        /// </summary>
        /// <param name="ticketId">Ticket ID for the matchmaking request</param>
        /// <param name="lobby">The created or joined lobby</param>
        public MatchmakingCompleteEvent(string ticketId, LobbyInfo lobby)
        {
            TicketId = ticketId;
            Lobby = lobby;
        }
    }
    
    /// <summary>
    /// Event raised when matchmaking fails
    /// </summary>
    public class MatchmakingFailedEvent
    {
        /// <summary>
        /// Ticket ID for the matchmaking request
        /// </summary>
        public string TicketId { get; }
        
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Create a new MatchmakingFailedEvent
        /// </summary>
        /// <param name="ticketId">Ticket ID for the matchmaking request</param>
        /// <param name="errorMessage">Error message</param>
        public MatchmakingFailedEvent(string ticketId, string errorMessage)
        {
            TicketId = ticketId;
            ErrorMessage = errorMessage;
        }
    }
    
    /// <summary>
    /// Event raised when matchmaking status is updated
    /// </summary>
    public class MatchmakingStatusEvent
    {
        /// <summary>
        /// Current status of matchmaking
        /// </summary>
        public MatchmakingStatus Status { get; }
        
        /// <summary>
        /// Create a new MatchmakingStatusEvent
        /// </summary>
        /// <param name="status">Current status of matchmaking</param>
        public MatchmakingStatusEvent(MatchmakingStatus status)
        {
            Status = status;
        }
    }
} 