using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace ChatEmoteColor
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public int EmoteColor_PlayerToSelf { get; set; } = -1;
        public int EmoteColor_SelfToPlayer { get; set; } = -1;
        public int EmoteColor_PlayerToPlayer { get; set; } = -1;

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
