using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatEmoteColor {

    class ChatEmoteParser {

        private ChatGui Chat;
        private ClientState ClientState;
        private Configuration Configuration;

        public enum ChatEmoteType {
            Unknown,
            Self,
            Player,
            PlayerToSelf,
            PlayerToPlayer
        }

        public ChatEmoteParser(ChatGui chat, ClientState clientState, Configuration configuration) {
            this.Chat = chat;
            this.ClientState = clientState;
            this.Configuration = configuration;
        }

        private bool IsLocalPlayer(PlayerPayload playerPayload) {

            return ClientState.LocalPlayer.Name.TextValue.Contains(playerPayload.PlayerName);
        }

        private bool MessageContainsLocalPlayerReference(string message) {

            var playerName = this.ClientState.LocalPlayer.Name.TextValue;
            var playerNameComponents = playerName.Split(' ');

            bool playerIsAddressed = 
                message.ToLower().Contains("you") ||
                (message.Contains(playerName)) ||
                (message.Contains(playerNameComponents[0] + " " + playerNameComponents[1].Substring(0, 1))) ||
                (message.Contains(playerNameComponents[0].Substring(0, 1) + ". " + playerNameComponents[1])) ||
                (message.Contains(playerNameComponents[0].Substring(0, 1) + ". " + playerNameComponents[1].Substring(0, 1)))
            ;

            return playerIsAddressed;
        }

        private ChatEmoteType ParseMessage(ref SeString message) {

            List<PlayerPayload> playerPayloads = new();

            foreach (Payload p in message.Payloads) {

                if (p.GetType() == typeof(PlayerPayload)) {

                    playerPayloads.Add((PlayerPayload)p);
                }
            }

            if (playerPayloads.Count == 1) {


                if (message.Payloads[0].GetType() == typeof(TextPayload)) {

                    TextPayload firstTextSegment = (TextPayload)message.Payloads[0];

                    bool localPlayerRef = MessageContainsLocalPlayerReference(firstTextSegment.Text);

                    if (localPlayerRef) {

                        return ChatEmoteType.Self;
                    } else {

                        if (message.Payloads.Last().GetType() == typeof(TextPayload)) {

                            TextPayload lastTextSegment = (TextPayload)message.Payloads.Last();

                            localPlayerRef = MessageContainsLocalPlayerReference(lastTextSegment.Text);

                            if (localPlayerRef) {

                                return ChatEmoteType.PlayerToSelf;
                            } else {

                                return ChatEmoteType.Player;
                            }
                        }

                        return ChatEmoteType.Player;
                    }

                }
                else if (message.Payloads[0].GetType() == typeof(PlayerPayload)) {

                    if (message.Payloads.Last().GetType() == typeof(TextPayload)) {
                        TextPayload lastTextSegment = (TextPayload)message.Payloads.Last();

                        bool localPlayerRef = MessageContainsLocalPlayerReference(lastTextSegment.Text);

                        if (localPlayerRef) {

                            return ChatEmoteType.PlayerToSelf;
                        } else {

                            return ChatEmoteType.Player;
                        }
                    }
                }

            } else if (playerPayloads.Count == 2) {

                if (IsLocalPlayer(playerPayloads[0])) {

                    return ChatEmoteType.Self;
                } else {

                    return ChatEmoteType.PlayerToPlayer;
                }
            } else if (playerPayloads.Count == 0) {

                if (message.Payloads.Count == 1 && message.Payloads[0].GetType() == typeof(TextPayload)) {

                    bool localPlayerRef = MessageContainsLocalPlayerReference(((TextPayload)message.Payloads[0]).Text);
                    if (localPlayerRef) {

                        return ChatEmoteType.Self;
                    }
                }
            }

            return ChatEmoteType.Unknown;
        }

        private ushort GetColorForEmoteType(ChatEmoteType type) {

            ushort color = 0;

            switch (type) {
                case ChatEmoteType.Self:
                    color = (ushort)this.Configuration.EmoteColor_Self;
                    break;
                case ChatEmoteType.Player:
                    color = (ushort)this.Configuration.EmoteColor_Player;
                    break;
                case ChatEmoteType.PlayerToSelf:
                    color = (ushort)this.Configuration.EmoteColor_PlayerToSelf;
                    break;
                case ChatEmoteType.PlayerToPlayer:
                    color = (ushort)this.Configuration.EmoteColor_PlayerToPlayer;
                    break;
            }

            return color > 0 ? color : (ushort)0;
        }

        public void HandleChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled) {

            if (type == XivChatType.StandardEmote) {

                List<Payload> newPayloads = new();

                ChatEmoteType emoteType = ParseMessage(ref message);
                ushort emoteColor = GetColorForEmoteType(emoteType);

                if (emoteColor != 0) {
                    newPayloads.Add(new UIForegroundPayload(emoteColor));
                    newPayloads.AddRange(message.Payloads);
                    newPayloads.Add(new UIForegroundPayload(0));
                    message = new SeString(newPayloads);
                }

            }

            isHandled = false;
        }
    }
}
