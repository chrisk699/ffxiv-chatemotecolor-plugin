using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.ClientState;

namespace ChatEmoteColor {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "ChatEmoteColor";

        private const string commandName = "/chatemotecolor";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private ChatEmoteParser ChatEmoteParser;

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

            this.ChatEmoteParser = new ChatEmoteParser(Chat, ClientState, Configuration);

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;

            this.PluginUi = new PluginUI(this.Configuration, Chat);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand));

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

            this.ChatEmoteParser.HandleChatMessage(type, senderId, ref sender, ref message, ref isHandled);
        }
    }
}
