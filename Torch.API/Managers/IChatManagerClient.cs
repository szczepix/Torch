﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using VRage.Network;

namespace Torch.API.Managers
{
    /// <summary>
    /// Represents a scripted or user chat message.
    /// </summary>
    public struct TorchChatMessage
    {
        /// <summary>
        /// Creates a new torch chat message with the given author and message.
        /// </summary>
        /// <param name="author">Author's name</param>
        /// <param name="message">Message</param>
        public TorchChatMessage(string author, string message)
        {
            Timestamp = DateTime.Now;
            AuthorSteamId = null;
            Author = author;
            Message = message;
            Font = "Blue";
        }

        /// <summary>
        /// Creates a new torch chat message with the given author and message.
        /// </summary>
        /// <param name="authorSteamId">Author's steam ID</param>
        /// <param name="message">Message</param>
        public TorchChatMessage(ulong authorSteamId, string message)
        {
            Timestamp = DateTime.Now;
            AuthorSteamId = authorSteamId;
            Author = MyMultiplayer.Static?.GetMemberName(authorSteamId) ?? "Player";
            Message = message;
            Font = "Blue";
        }

        /// <summary>
        /// This message's timestamp.
        /// </summary>
        public DateTime Timestamp;
        /// <summary>
        /// The author's steam ID, if available.  Else, null.
        /// </summary>
        public ulong? AuthorSteamId;
        /// <summary>
        /// The author's name, if available.  Else, null.
        /// </summary>
        public string Author;
        /// <summary>
        /// The message contents.
        /// </summary>
        public string Message;
        /// <summary>
        /// The font, or null if default.
        /// </summary>
        public string Font;
    }

    /// <summary>
    /// Callback used to indicate that a messaage has been recieved.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="consumed">If true, this event has been consumed and should be ignored</param>
    public delegate void MessageRecievedDel(TorchChatMessage msg, ref bool consumed);

    /// <summary>
    /// Callback used to indicate the user is attempting to send a message locally.
    /// </summary>
    /// <param name="msg">Message the user is attempting to send</param>
    /// <param name="consumed">If true, this event has been consumed and should be ignored</param>
    public delegate void MessageSendingDel(string msg, ref bool consumed);

    public interface IChatManagerClient : IManager
    {
        /// <summary>
        /// Event that is raised when a message addressed to us is recieved.  <see cref="MessageRecievedDel"/>
        /// </summary>
        event MessageRecievedDel MessageRecieved;

        /// <summary>
        /// Event that is raised when we are attempting to send a message.  <see cref="MessageSendingDel"/>
        /// </summary>
        event MessageSendingDel MessageSending;

        /// <summary>
        /// Triggers the <see cref="MessageSending"/> event,
        /// typically raised by the user entering text into the chat window.
        /// </summary>
        /// <param name="message">The message to send</param>
        void SendMessageAsSelf(string message);

        /// <summary>
        /// Displays a message on the UI given an author name and a message.
        /// </summary>
        /// <param name="author">Author name</param>
        /// <param name="message">Message content</param>
        /// <param name="font">font to use</param>
        void DisplayMessageOnSelf(string author, string message, string font = "Blue" );
    }
}
