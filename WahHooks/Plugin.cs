using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using WahHooks.Windows;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace WahHooks 
{
    public sealed class Plugin : IDalamudPlugin 
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IChatGui Chat { get; private set; } = null!;

        private const string CommandName = "/wahhook";
        private const string ConfigCommandName = "/wahhookconfig";

        public Configuration Configuration { get; init; }

        public readonly WindowSystem WindowSystem = new("WahHooks");
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        private WebhookGui WebhookGui { get; init; }

        public Plugin() 
        {
            Configuration = Configuration.Load();
            WebhookGui = new WebhookGui(Configuration);

            WindowSystem.AddWindow(WebhookGui);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnWahHookCommand)
            {
                HelpMessage = "Send a message to a Discord webhook: /wahhook <index or nickname> <message>"
            });

            CommandManager.AddHandler(ConfigCommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the WahHook Configuration GUI."
            });

            // Register the IPC method
            PluginInterface.GetIpcProvider<(int, string), bool>("WahHook.SendWebhookMessage").RegisterFunc(SendWebhookMessageViaIPC);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png"));

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
        } 

        public void Dispose() 
        {
            Configuration.Save();
            WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();
            WebhookGui.Dispose();

            CommandManager.RemoveHandler(CommandName);
            CommandManager.RemoveHandler(ConfigCommandName);

            PluginInterface.GetIpcProvider<(int, string), bool>("WahHook.SendWebhookMessage").UnregisterFunc();
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;
        } 

        private void OnWahHookCommand(string command, string args)
        {
            Chat.Print("Received /wahhook command.");

            var splitArgs = args.Split(' ', 2);
            if (splitArgs.Length < 2)
            {
                Chat.PrintError("Usage: /wahhook <index or nickname> <message>");
                return;
            }

            string identifier = splitArgs[0];
            string message = splitArgs[1];

            WebhookEntry webhook = null;

            // Try to parse as an index
            if (int.TryParse(identifier, out int index))
            {
                if (index < 1 || index > Configuration.Webhooks.Count)
                {
                    Chat.PrintError("Invalid webhook index.");
                    return;
                }

                webhook = Configuration.Webhooks[index - 1];
            }
            else
            {
                // Otherwise, treat as a nickname
                webhook = Configuration.Webhooks.Find(w => w.Nickname.Equals(identifier, StringComparison.OrdinalIgnoreCase));
                if (webhook == null)
                {
                    Chat.PrintError($"No webhook found with nickname: {identifier}");
                    return;
                }
            }

            Chat.Print($"Sending message to webhook {identifier}: {message}");

            SendWebhookMessage(webhook.Url, message);
        }

        private void OnConfigCommand(string command, string args)
        {
            WebhookGui.Toggle();
        }

        private void SendWebhookMessage(string url, string message)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var jsonPayload = JsonConvert.SerializeObject(new { content = message });
                    Chat.Print($"Sending JSON Payload: {jsonPayload}");

                    var content = new System.Net.Http.StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                    var response = client.PostAsync(url, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        Chat.Print("Message sent successfully.");
                    }
                    else
                    {
                        Chat.PrintError($"Failed to send message. Status code: {response.StatusCode}");
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        Chat.PrintError($"Response body: {responseBody}");
                    }
                }
            }
            catch (Exception ex)
            {
                Chat.PrintError($"Exception occurred: {ex.Message}");
            }
        }

        private bool SendWebhookMessageViaIPC((int, string) args)
        {
            int index = args.Item1;
            string message = args.Item2;

            if (index < 1 || index > Configuration.Webhooks.Count)
            {
                Chat.PrintError("Invalid webhook index received via IPC.");
                return false;
            }

            string webhookUrl = Configuration.Webhooks[index - 1].Url;
            SendWebhookMessage(webhookUrl, message);
            return true;
        }

        private void DrawUI() => WindowSystem.Draw();

        public void ToggleConfigUI() => WebhookGui.Toggle();
        public void ToggleMainUI() => MainWindow.Toggle();
    } 
} 
