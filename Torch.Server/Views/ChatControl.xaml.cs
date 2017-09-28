﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Torch;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using SteamSDK;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Session;
using Torch.Managers;
using Torch.Server.Managers;
using VRage.Game;

namespace Torch.Server
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        private TorchBase _server;

        public ChatControl()
        {
            InitializeComponent();
        }

        public void BindServer(ITorchServer server)
        {
            _server = (TorchBase)server;
            Dispatcher.Invoke(() =>
            {
                ChatItems.Inlines.Clear();
            });

            var sessionManager = server.Managers.GetManager<ITorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionStateChanged;
        }

        private void SessionStateChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loading:
                    Dispatcher.Invoke(() => ChatItems.Inlines.Clear());
                    break;
                case TorchSessionState.Loaded:
                    {
                        var chatMgr = session.Managers.GetManager<IChatManagerClient>();
                        if (chatMgr != null)
                            chatMgr.MessageRecieved += OnMessageRecieved;
                    }
                    break;
                case TorchSessionState.Unloading:
                    {
                        var chatMgr = session.Managers.GetManager<IChatManagerClient>();
                        if (chatMgr != null)
                            chatMgr.MessageRecieved -= OnMessageRecieved;
                    }
                    break;
                case TorchSessionState.Unloaded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnMessageRecieved(TorchChatMessage msg, ref bool consumed)
        {
            InsertMessage(msg);
        }

        private static readonly Dictionary<string, Brush> _brushes = new Dictionary<string, Brush>();
        private static Brush LookupBrush(string font)
        {
            if (_brushes.TryGetValue(font, out Brush result))
                return result;
            Brush brush = typeof(Brushes).GetField(font, BindingFlags.Static)?.GetValue(null) as Brush ?? Brushes.Blue;
            _brushes.Add(font, brush);
            return brush;
        }

        private void InsertMessage(TorchChatMessage msg)
        {
            if (Dispatcher.CheckAccess())
            {
                bool atBottom = ChatScroller.VerticalOffset + 8 > ChatScroller.ScrollableHeight;
                var span = new Span();
                span.Inlines.Add($"{msg.Timestamp} ");
                span.Inlines.Add(new Run(msg.Author) { Foreground = LookupBrush(msg.Font) });
                span.Inlines.Add($": {msg.Message}");
                span.Inlines.Add(new LineBreak());
                ChatItems.Inlines.Add(span);
                if (atBottom)
                    ChatScroller.ScrollToBottom();
            }
            else
                Dispatcher.Invoke(() => InsertMessage(msg));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            OnMessageEntered();
        }

        private void Message_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnMessageEntered();
        }

        private void OnMessageEntered()
        {
            //Can't use Message.Text directly because of object ownership in WPF.
            var text = Message.Text;
            if (string.IsNullOrEmpty(text))
                return;

            var commands = _server.CurrentSession?.Managers.GetManager<Torch.Commands.CommandManager>();
            if (commands != null && commands.IsCommand(text))
            {
                InsertMessage(new TorchChatMessage("Server", text) { Font = MyFontEnum.DarkBlue });
                _server.Invoke(() =>
                {
                    string response = commands.HandleCommandFromServer(text);
                    if (!string.IsNullOrWhiteSpace(response))
                        InsertMessage(new TorchChatMessage("Server", response) { Font = MyFontEnum.Blue });
                });
            }
            else
            {
                _server.CurrentSession?.Managers.GetManager<IChatManagerClient>().SendMessageAsSelf(text);
            }
            Message.Text = "";
        }
    }
}
