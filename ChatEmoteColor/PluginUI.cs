using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChatEmoteColor 
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;
        private ChatGui chat;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, ChatGui chat)
        {
            this.configuration = configuration;
            this.chat = chat;
        }

        public void Dispose()
        {

        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("ChatEmoteColor", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text($"Player->Self emote color:  {this.configuration.EmoteColor_PlayerToSelf}");
                ImGui.Text($"Self->Player emote color: {this.configuration.EmoteColor_SelfToPlayer}");
                ImGui.Text($"Player->Player emote color: {this.configuration.EmoteColor_PlayerToPlayer}");

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }

            }
            ImGui.End();
        }

        public void PreviewChatColor(ushort chatColor) {

            List<Payload> newPayloads = new() {
                new UIForegroundPayload(chatColor),
                new TextPayload("Color preview"),
                new UIForegroundPayload(0)
            };

            var chatEntry = new XivChatEntry {
                Message = new SeString(newPayloads)
            };

            this.chat.PrintChat(chatEntry);  
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(700, 200), ImGuiCond.Always);
            if (ImGui.Begin("ChatEmoteColor Configuration", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue_EmoteColor_PlayerToSelf = this.configuration.EmoteColor_PlayerToSelf;
                var configValue_EmoteColor_SelfToPlayer = this.configuration.EmoteColor_SelfToPlayer;
                var configValue_EmoteColor_PlayerToPlayer = this.configuration.EmoteColor_PlayerToPlayer;

                ImGui.Text($"Use -1 value to disable recoloring");

                if (ImGui.InputInt("Player->Self emote color", ref configValue_EmoteColor_PlayerToSelf)) {

                    this.configuration.EmoteColor_PlayerToSelf = configValue_EmoteColor_PlayerToSelf;
                    // preview color
                    PreviewChatColor((ushort)this.configuration.EmoteColor_PlayerToSelf);
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }

                if (ImGui.InputInt("Self->Player emote color", ref configValue_EmoteColor_SelfToPlayer)) {

                    this.configuration.EmoteColor_SelfToPlayer = configValue_EmoteColor_SelfToPlayer;
                    // preview color
                    PreviewChatColor((ushort)this.configuration.EmoteColor_SelfToPlayer);
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }

                if (ImGui.InputInt("Player->Player emote color", ref configValue_EmoteColor_PlayerToPlayer)) {

                    this.configuration.EmoteColor_PlayerToPlayer = configValue_EmoteColor_PlayerToPlayer;
                    // preview color
                    PreviewChatColor((ushort)this.configuration.EmoteColor_PlayerToPlayer);
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
