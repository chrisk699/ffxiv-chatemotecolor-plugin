using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState;

namespace ChatEmoteColor {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "ChatEmoteColor";

        private const string commandName = "/chatemotecolor";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        [PluginService]
        public ChatGui Chat { get; init; }

        [PluginService]
        public ClientState ClientState { get; init; }
        public object? RawText { get; private set; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            if (this.Chat != null) {

                this.Chat.ChatMessage += ChatOnOnChatMessage;
            }

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var imagePath = Path.Combine(Path.GetDirectoryName(assemblyLocation)!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration, Chat);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);

            if (this.Chat != null) {

                this.Chat.ChatMessage -= ChatOnOnChatMessage;
            }
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }

        private void ChatOnOnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled) {

            if (type == XivChatType.StandardEmote) {

                List<Payload> newPayloads = new List<Payload>();

                // PlayerToSelf or SelfToPlayer interaction
                if (message.Payloads.Count == 4) {

                    var playerName = this.ClientState.LocalPlayer.Name.TextValue;
                    var playerNameComponents = playerName.Split(' ');

                    bool playerIsAddressed = message.Payloads[3] != null && (
                        message.Payloads[3].ToString().Contains("you") || 
                        (message.Payloads[3].ToString().Contains(playerName)) ||
                        (message.Payloads[3].ToString().Contains(playerNameComponents[0] + " " + playerNameComponents[1].Substring(0,1))) ||
                        (message.Payloads[3].ToString().Contains(playerNameComponents[0].Substring(0, 1) + ". " + playerNameComponents[1]))
                    );

                    if (message.Payloads[0].GetType() == typeof(PlayerPayload) && playerIsAddressed) {

                        // If payload starts with player payload, it's a PlayerToSelf interaction

                        if (this.Configuration.EmoteColor_PlayerToSelf >= 0) {
                            newPayloads.Add(new UIForegroundPayload((ushort)this.Configuration.EmoteColor_PlayerToSelf));
                        }
                    }
                   
                } else if (message.Payloads.Count == 5) {

                    // If payload count is 5, it's a SelfToPlayer interaction
                    if (this.Configuration.EmoteColor_Self >= 0) {
                        newPayloads.Add(new UIForegroundPayload((ushort)this.Configuration.EmoteColor_Self));
                    }
                } else if (message.Payloads.Count == 8) {

                    // If payload count is 8, it's a player to player interaction
                    if (this.Configuration.EmoteColor_PlayerToPlayer >= 0) {
                        newPayloads.Add(new UIForegroundPayload((ushort)this.Configuration.EmoteColor_PlayerToPlayer));
                    }
                }

                newPayloads.AddRange(message.Payloads);
                newPayloads.Add(new UIForegroundPayload(0));
                message = new SeString(newPayloads);

                isHandled = false;
            } else {

                isHandled = false;
            }

        }
    }
}
