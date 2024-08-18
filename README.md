
# WahHooks Plugin

WahHooks is a Dalamud plugin for Final Fantasy XIV that allows users to manage Discord webhooks and send messages to these webhooks directly from within the game. The plugin also supports Inter-Plugin Communication (IPC), enabling other plugins to trigger webhook messages.

## Features

- **Manage Discord Webhooks**: Add, remove, and nickname your webhooks through a user-friendly interface.
- **Send Messages via Commands**: Easily send messages to specific webhooks using in-game commands.
- **IPC Integration**: Other plugins can send messages to your webhooks using IPC.

## Commands

### `/wahhook <index or nickname> <message>`

- **Description**: Sends a message to the specified webhook.
- **Usage**:
  - `/wahhook 1 "This is a message to webhook 1!"`
  - `/wahhook raid "This is a message to the webhook with the nickname 'cat'!"`
- **Arguments**:
  - `index or nickname`: Either the index (starting from 1) or the nickname of the webhook.
  - `message`: The message you want to send to the webhook.

### `/wahhookconfig`

- **Description**: Opens the WahHooks configuration interface where you can manage your webhooks.
- **Usage**: Simply type `/wahhookconfig` in the chat to open the configuration window.

## Configuration Interface

The configuration interface allows you to:

- **Manage Webhooks**: View all added webhooks, update their nicknames, or remove them.
- **Add Webhook**: Add a new webhook by entering its URL and assigning it a nickname.

The configuration interface is divided into two tabs:

1. **Manage Webhooks**: Here, you can view all your webhooks, update their nicknames, or remove them.
2. **Add Webhook**: Here, you can add new webhooks by entering the webhook URL and an optional nickname.

### Example of Managing Webhooks

```plaintext
Webhook 1 (raid): https://discord.com/api/webhooks/...
[Copy] [Remove]

Nickname: raid
```

### Example of Adding a New Webhook

```plaintext
New Webhook URL: https://discord.com/api/webhooks/...
New Webhook Nickname: daily-alerts
[Add Webhook]
```

## IPC Integration

WahHooks provides an IPC method that allows other plugins to send messages to your managed webhooks.

### IPC Method: `WahHook.SendWebhookMessage`

- **Function Name**: `WahHook.SendWebhookMessage`
- **Parameters**: 
  - `int`: Index of the webhook (starting from 1).
  - `string`: The message to be sent.
- **Returns**: 
  - `bool`: Returns `true` if the message was successfully sent, `false` otherwise.

### Using the IPC in Another Plugin

Here’s an example of how another plugin might use the IPC method provided by WahHooks:

```csharp
var sendWebhookMessage = PluginInterface.GetIpcSubscriber<(int, string), bool>("WahHook.SendWebhookMessage");

// Example usage: send a message to the first webhook
bool result = sendWebhookMessage.InvokeFunc((1, "This is a message from another plugin!"));

if (result)
{
    DalamudApi.Chat.Print("Message sent successfully!");
}
else
{
    DalamudApi.Chat.PrintError("Failed to send the message.");
}
```

### IPC Usage Scenarios

- **Automated Alerts**: Other plugins can send automated alerts to Discord when certain in-game events occur.
- **Data Sharing**: Plugins that collect or generate data can push updates to Discord through WahHooks.
