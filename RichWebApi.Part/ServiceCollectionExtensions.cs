﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RichWebApi.Core.Exceptions;
using RichWebApi.MediatR;

namespace RichWebApi;

public static class ServiceCollectionExtensions
{
	public static IMvcCoreBuilder AddCore(this IMvcCoreBuilder builder)
	{
		builder.AddPart<RichWebApiException>();
		builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
		builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceLoggingBehavior<,>));
		return builder;
	}
}