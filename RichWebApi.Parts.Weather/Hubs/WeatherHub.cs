﻿using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;

namespace RichWebApi.Hubs;

[UsedImplicitly]
internal class WeatherHub : Hub<IWeatherHubClient>
{
	public override async Task OnConnectedAsync()
	{
		await base.OnConnectedAsync();
		await Groups.AddToGroupAsync(Context.ConnectionId, WeatherHubConstants.GroupName);
	}
}