using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Dalamud.Configuration;

namespace WahHooks;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public List<WebhookEntry> Webhooks { get; set; } = new List<WebhookEntry>();

    private string ConfigFilePath => Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "wahhook_config.json");

    public void Save()
    {
        File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public static Configuration Load()
    {
        var configFilePath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "wahhook_config.json");

        if (File.Exists(configFilePath))
        {
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configFilePath));
        }
        else
        {
            return new Configuration();
        }
    }
}

[Serializable]
public class WebhookEntry
{
    public string Url { get; set; }
    public string Nickname { get; set; } = "Unnamed Webhook";
}
