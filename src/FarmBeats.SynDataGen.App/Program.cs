using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FarmBeats.Api.Client;
using FarmBeats.Api.Models;
using FarmBeats.SynDataGen;
using Microsoft.Extensions.Configuration;

namespace FarmBeats.SynDataGen.App
{
	class Program
	{
		private static dynamic _config = null;
		private static ApiClient _apiClient = null;

		static void Main(string[] args)
		{
			_config = GetConfiguration();
			_apiClient = new ApiClient(_config.TenantId, _config.ClientId, _config.ClientSecret, _config.ApiEndpoint);

			List<DeviceModelResponse> deviceModelResponses = CreateDeviceModelsAsync().Result;

			Console.WriteLine("Done - press any key");
			Console.ReadKey();
		}

		static async Task<List<DeviceModelResponse>> CreateDeviceModelsAsync()
		{
			DataGenerator generator = new DataGenerator();

			int numberOfDeviceModels = 10;

			List<DeviceModelResponse> deviceModelReponses = new List<DeviceModelResponse>(numberOfDeviceModels);
			List<DeviceModelRequest> deviceModelRequests = generator.GetDeviceModelRequests(numberOfDeviceModels);

			foreach (DeviceModelRequest deviceModelRequest in deviceModelRequests)
			{
				HttpResponseMessage httpResponseMessage = await _apiClient.PostResourceAsync(deviceModelRequest);
				DeviceModelResponse deviceModelResponse = await _apiClient.GetResourceFromHttpResponseAsync<DeviceModelResponse>(httpResponseMessage);
				deviceModelReponses.Add(deviceModelResponse);
			}

			return deviceModelReponses;
		}

		private static dynamic GetConfiguration()
		{
			var builder = new ConfigurationBuilder()
			   .SetBasePath(Directory.GetCurrentDirectory())
			   .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);

			IConfigurationRoot configuration = builder.Build();
			ExpandoObject config = new ExpandoObject();
			configuration.GetSection("Settings").Bind(config);

			return config;
		}

	}
}
