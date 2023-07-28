﻿using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RichWebApi.Entities;
using RichWebApi.Hubs;
using RichWebApi.Models;
using RichWebApi.Persistence;
using RichWebApi.Tests;
using RichWebApi.Tests.DependencyInjection;
using RichWebApi.Tests.Logging;
using RichWebApi.Tests.Moq;
using RichWebApi.Tests.Resources;
using Xunit.Abstractions;

namespace RichWebApi.Operations;

public class PatchWeatherForecastTests : UnitTest
{
	private readonly IResourceScope _testResources;
	private readonly IServiceProvider _serviceProvider;

	public PatchWeatherForecastTests(ITestOutputHelper testOutputHelper, ResourceRepositoryFixture resourceRepository,
	                                 UnitDependencyContainerFixture container) : base(testOutputHelper)
	{
		_testResources = resourceRepository.CreateTestScope(this);
		var parts = new AppPartsCollection()
			.AddWeather();
		_serviceProvider = container
			.WithXunitLogging(TestOutputHelper)
			.WithTestScopeInMemoryDatabase(parts)
			.WithMockedSignalRHubContext<WeatherHub, IWeatherHubClient>(
				configureHubClients: (_, mock, client) => mock
					.Setup(x => x.Group(It.Is<string>(s => s == WeatherHubConstants.GroupName)))
					.Returns(client)
					.Verifiable("should access the weather group"))
			.ConfigureServices(s => s.AddAppParts(parts))
			.BuildServiceProvider();
	}

	[Fact]
	public async Task UpdatesOne()
	{
		var sharedInput = _testResources.GetJsonInputResource<PatchWeatherForecast>();
		using var caseResources = _testResources.CreateMethodScope();
		var mediator = _serviceProvider.GetRequiredService<IMediator>();

		await mediator.Send(sharedInput);
		var expected = await mediator.Send(new GetWeatherForecast(0, 1));
		caseResources.CompareWithJsonExpectation(TestOutputHelper, expected);
	}

	[Fact]
	public async Task NotifiesAboutUpdates()
	{
		var sharedInput = _testResources.GetJsonInputResource<PatchWeatherForecast>();

		var clientMock = _serviceProvider.GetRequiredService<Mock<IWeatherHubClient>>();
		var groupManagerMock = _serviceProvider.GetRequiredService<Mock<IHubClients<IWeatherHubClient>>>();

		await _serviceProvider
			.GetRequiredService<IMediator>()
			.Send(sharedInput);

		clientMock.Verify(x => x.OnWeatherUpdate(It.IsAny<WeatherForecastDto>()), Times.Once());
		groupManagerMock.Verify(x => x.Group(It.Is<string>(s => s == WeatherHubConstants.GroupName)), Times.Once());
	}

	public override async Task InitializeAsync()
	{
		await base.InitializeAsync();
		var sharedEntity = _testResources.GetJsonInputResource<WeatherForecast>("entity");
		await _serviceProvider
			.GetRequiredService<IRichWebApiDatabase>()
			.PersistEntityAsync(sharedEntity);
	}
}