using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace WahHooks.Windows;

public class WebhookGui : Window, IDisposable
{
    private Configuration Configuration;
    private string newWebhookUrl = string.Empty;
    private string newWebhookNickname = string.Empty;

    public WebhookGui(Configuration config) : base("WahHook Configuration")
    {
        Configuration = config;
        Size = new Vector2(500, 400);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("WebhookTabBar"))
        {
            if (ImGui.BeginTabItem("Manage Webhooks"))
            {
                DrawManageWebhooksTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Add Webhook"))
            {
                DrawAddWebhookTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawManageWebhooksTab()
    {
        ImGui.Text("Manage your Discord Webhooks:");
        ImGui.Separator();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 10)); // Add some spacing between items

        // Display the list of webhooks
        for (int i = 0; i < Configuration.Webhooks.Count; i++)
        {
            var webhook = Configuration.Webhooks[i];

            // Create local variables to store the values
            string nickname = webhook.Nickname;
            string url = webhook.Url;

            ImGui.Text($"Webhook {i + 1} ({nickname}):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(350);
            if (ImGui.InputText($"##Url{i}", ref url, 512, ImGuiInputTextFlags.ReadOnly))
            {
                webhook.Url = url;
                Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button($"Copy##{i}", new Vector2(80, 25)))
            {
                ImGui.SetClipboardText(url); // Copy webhook URL to clipboard
            }

            ImGui.SetNextItemWidth(200);
            if (ImGui.InputText($"Nickname##{i}", ref nickname, 256))
            {
                webhook.Nickname = nickname; // Update the nickname property if the user edits it
                Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button($"Remove##{i}", new Vector2(80, 25)))
            {
                Configuration.Webhooks.RemoveAt(i);
                Configuration.Save();
            }

            ImGui.Separator();
        }

        ImGui.PopStyleVar();
    }

    private void DrawAddWebhookTab()
    {
        ImGui.Text("Add a New Webhook:");
        ImGui.Separator();

        ImGui.SetNextItemWidth(400);
        ImGui.InputText("New Webhook URL", ref newWebhookUrl, 512);
        ImGui.SetNextItemWidth(400);
        ImGui.InputText("New Webhook Nickname", ref newWebhookNickname, 256);

        ImGui.Spacing();

        if (ImGui.Button("Add Webhook", new Vector2(150, 30)))
        {
            if (!string.IsNullOrEmpty(newWebhookUrl))
            {
                Configuration.Webhooks.Add(new WebhookEntry { Url = newWebhookUrl, Nickname = string.IsNullOrEmpty(newWebhookNickname) ? "Unnamed Webhook" : newWebhookNickname });
                Configuration.Save();
                newWebhookUrl = string.Empty;
                newWebhookNickname = string.Empty;
            }
            else
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Please enter a valid URL."); // Display error message in red
            }
        }
    }

    public void Dispose() { }
}
