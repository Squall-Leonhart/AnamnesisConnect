﻿// © Anamnesis Connect.
// Licensed under the MIT license.

namespace AnamnesisConnect
{
	using System;
	using System.Diagnostics;
	using Dalamud.Game.Command;
	using Dalamud.Game.Gui;
	using Dalamud.Game.Text;
	using Dalamud.Game.Text.SeStringHandling;
	using Dalamud.Game.Text.SeStringHandling.Payloads;
	using Dalamud.IoC;
	using Dalamud.Logging;
	using Dalamud.Plugin;

	public sealed class Plugin : IDalamudPlugin
    {
		private readonly ChatGui? chat;
		private readonly CommFile comm;
		private readonly CommandManager commandManager;
		private readonly PenumbraInterface? penumbraInterface;

		public Plugin(
			[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
			[RequiredVersion("1.0")] ChatGui chatGui,
			[RequiredVersion("1.0")] CommandManager commandManager)
        {
			this.PluginInterface = pluginInterface;
			this.chat = chatGui;
			this.commandManager = commandManager;

			PluginLog.Information("Starting Anamnesis Connect");

			Process proc = Process.GetCurrentProcess();
			this.comm = new CommFile(proc);
			this.comm.OnCommandRecieved = (s) =>
			{
				if (!this.commandManager.ProcessCommand(s))
					this.SendChat($"Anamnesis Connect: {s}", XivChatType.Debug);

				PluginLog.Information($"Recieved Anamnesis command: \"{s}\"");
			};

			try
			{
				this.penumbraInterface = new();
			}
			catch (Exception ex)
			{
				PluginLog.Error(ex, "Error in penumbra interface");
			}

			this.SendChat("Anamnesis Connect has started", XivChatType.Debug);
		}

		public DalamudPluginInterface PluginInterface { get; private set; }
		public string Name => "Anamnesis Connect";

		public void SendChat(string message, XivChatType chatType = XivChatType.Debug)
		{
			if (this.chat == null)
				return;

			TextPayload textPayload = new(message);
			SeString seString = new(textPayload);
			XivChatEntry entry = new();
			entry.Message = seString;
			entry.Type = chatType;
			this.chat.PrintChat(entry);
		}

		public void Dispose()
        {
			PluginLog.Information("Disposing Anamnesis Connect");
			this.comm?.Stop();
		}
	}
}
