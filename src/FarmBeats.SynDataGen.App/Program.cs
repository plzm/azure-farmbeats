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
		private static DataGenerator _generator = new DataGenerator();

		static void Main(string[] args)
		{
			_config = GetConfiguration();
			_apiClient = new ApiClient(_config.TenantId, _config.ClientId, _config.ClientSecret, _config.ApiEndpoint);

			//IList<FarmResponse> farmResponses = CreateFarmsAsync().Result;
			IList<FarmResponse> farmResponses = null;

			IList <DeviceModelResponse> deviceModelResponses = CreateDeviceModelsAsync().Result;

			IList<DeviceResponse> deviceResponses = CreateDevicesAsync(farmResponses, deviceModelResponses).Result;

			IList<SensorModelResponse> sensorModelResponses = CreateSensorModelsAsync().Result;

			IList<SensorResponse> sensorResponses = CreateSensorsAsync(deviceResponses, sensorModelResponses).Result;

			Console.WriteLine("Done - press any key");
			Console.ReadKey();
		}

		static async Task<IList<FarmResponse>> CreateFarmsAsync()
		{
			int createThisMany = 5;

			IList<FarmResponse> responses = new List<FarmResponse>(createThisMany);
			IList<FarmRequest> requests = _generator.GetFarmRequests(createThisMany);

			foreach (FarmRequest request in requests)
			{
				HttpResponseMessage httpResponseMessage = await _apiClient.PostResourceAsync(request);
				FarmResponse response = await _apiClient.GetResourceFromHttpResponseAsync<FarmResponse>(httpResponseMessage);
				responses.Add(response);
			}

			return responses;
		}

		static async Task<IList<DeviceModelResponse>> CreateDeviceModelsAsync()
		{
			int createThisMany = 25;

			IList<DeviceModelResponse> responses = new List<DeviceModelResponse>(createThisMany);
			IList<DeviceModelRequest> requests = _generator.GetDeviceModelRequests(createThisMany);

			foreach (DeviceModelRequest request in requests)
			{
				HttpResponseMessage httpResponseMessage = await _apiClient.PostResourceAsync(request);
				DeviceModelResponse response = await _apiClient.GetResourceFromHttpResponseAsync<DeviceModelResponse>(httpResponseMessage);
				responses.Add(response);
			}

			return responses;
		}

		static async Task<IList<DeviceResponse>> CreateDevicesAsync(IList<FarmResponse> farmResponses, IList<DeviceModelResponse> deviceModelResponses)
		{
			int createThisMany = 1000;

			IList<DeviceResponse> responses = new List<DeviceResponse>(createThisMany);
			IList<DeviceRequest> requests = _generator.GetDeviceRequests
			(
				createThisMany,
				(farmResponses == null || farmResponses.Count() == 0 ? null : farmResponses.Select(r => r.Id)),
				deviceModelResponses.Select(r => r.Id)
			);

			foreach (DeviceRequest request in requests)
			{
				HttpResponseMessage httpResponseMessage = await _apiClient.PostResourceAsync(request);
				DeviceResponse response = await _apiClient.GetResourceFromHttpResponseAsync<DeviceResponse>(httpResponseMessage);
				responses.Add(response);
			}

			return responses;
		}

		static async Task<IList<SensorModelResponse>> CreateSensorModelsAsync()
		{
			int createThisMany = 200;

			IList<SensorModelResponse> responses = new List<SensorModelResponse>(createThisMany);
			IList<SensorModelRequest> requests = _generator.GetSensorModelRequests(createThisMany);

			foreach (SensorModelRequest request in requests)
			{
				HttpResponseMessage httpResponseMessage = await _apiClient.PostResourceAsync(request);
				SensorModelResponse response = await _apiClient.GetResourceFromHttpResponseAsync<SensorModelResponse>(httpResponseMessage);
				responses.Add(response);
			}

			return responses;
		}

		static async Task<IList<SensorResponse>> CreateSensorsAsync(IList<DeviceResponse> deviceResponses, IList<SensorModelResponse> sensorModelResponses)
		{
			int createThisMany = 5000;

			IList<SensorResponse> responses = new List<SensorResponse>(createThisMany);
			IList<SensorRequest> requests = _generator.GetSensorRequests
			(
				createThisMany,
				deviceResponses.Select(r => r.Id),
				sensorModelResponses.Select(r => r.Id)
			);

			foreach (SensorRequest request in requests)
			{
				HttpResponseMessage httpResponseMessage = await _apiClient.PostResourceAsync(request);
				SensorResponse response = await _apiClient.GetResourceFromHttpResponseAsync<SensorResponse>(httpResponseMessage);
				responses.Add(response);
			}

			return responses;
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
