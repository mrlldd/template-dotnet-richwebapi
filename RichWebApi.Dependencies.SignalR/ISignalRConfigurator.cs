﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;

namespace RichWebApi;

public interface ISignalRConfigurator
{
	ISignalRConfigurator WithHub<T>(string pattern, Action<HttpConnectionDispatcherOptions>? configureOptions = null,
									Action<HubEndpointConventionBuilder>? configureConventions = null) where T : Hub;

	ISignalRConfigurator WithHub<T>(
		Func<IServiceProvider, (string Pattern, Action<HttpConnectionDispatcherOptions>? ConfigureOptions,
			Action<HubEndpointConventionBuilder>? ConfigureConventions)> configure) where T : Hub;
}