using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Confluent.SmtpAuth.Standard;
using Confluent.Configuration;
using Confluent.FunctionApp.SourceService.Interfaces;
using Confluent.FunctionApp.SourceService.Services;
using Confluent.FunctionApp.DestinationService.Interfaces;
using Confluent.FunctionApp.DestinationService.Services;
using Confluent.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using Confluent.AzureStorageRepository;
using Confluent.AzureStorageRepository.Interfaces;
using Confluent.AzureStorageRepository.SourceService;
using Confluent.AzureStorageRepository.DestinationService;
using Confluent.AzureStorageRepository.Repository;
using Confluent.AzureStorageRepository.Delete;

//[assembly: FunctionsStartup(typeof(Confluent.FunctionApp.Startup))]
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
    // Dependency Injection
    .AddSingleton<IDataAccessService, DataAccessService>();
    .AddScoped<ISourceService, SourceService>();
    .AddScoped<ISourceConnectorService, SourceConnectorService>();
    .AddScoped<IDestinationService, DestinationService>();
    .AddScoped<IDestinationConnectorService, DestinationConnectorService>();
    .AddScoped<ISmtpService, SmtpService>();
    .AddSingleton<IRepositoryContext, RepositoryContext>();
    .AddSingleton<IAzureStorageTableWorkOrderRepository, AzureStorageTableWorkOrderRepository>();
    .AddSingleton<ISourceStorageTableWorkOrderRepository, SourceStorageTableWorkOrderRepository>();
    .AddSingleton<IDeleteStorageTableWorkOrderRepository, DeleteStorageTableWorkOrderRepository>();
// Configuration
builder.Configuration.AddEnvironmentVariables();
var configuration = builder.Configuration.Build();
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.Configure<SMTPOptions>(configuration.GetSection("SmtpOptions"));

// Set static helpers configuration
AzureStorageHelper.SetConfiguration(configuration);
AzureBlobStorageHelper.SetConfiguration(configuration);
AzureTableStorageHelper.SetConfiguration(configuration);


builder.Build().Run();
