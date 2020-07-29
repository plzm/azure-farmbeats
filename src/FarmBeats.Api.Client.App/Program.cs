using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using FarmBeats.Api.Client;
using FarmBeats.Api.Models;

namespace FarmBeats.Api.Client.App
{
	class Program
	{
		private static ApiClient _apiClient = null;

		static void Main(string[] args)
		{
			dynamic config = GetConfiguration();

			_apiClient = new ApiClient(config.TenantId, config.ClientId, config.ClientSecret, config.ApiEndpoint);

			// List Resources
			HandleList<PartnerResponse>(ApiClient.ResourceType.Partner);
			HandleList<FarmResponse>(ApiClient.ResourceType.Farm);
			HandleList<DeviceModelResponse>(ApiClient.ResourceType.DeviceModel);

			HandleList<DeviceResponse>(ApiClient.ResourceType.Device);

			HandleList<SensorModelResponse>(ApiClient.ResourceType.SensorModel);
			HandleList<SensorResponse>(ApiClient.ResourceType.Sensor);
			HandleList<ExtendedTypeResponse>(ApiClient.ResourceType.ExtendedType);
			HandleList<JobTypeResponse>(ApiClient.ResourceType.JobType);
			HandleList<JobResponse>(ApiClient.ResourceType.Job);
			HandleList<SceneResponse>(ApiClient.ResourceType.Scene);
			HandleList<GetSceneFileResponse>(ApiClient.ResourceType.SceneFile);
			HandleList<RuleResponse>(ApiClient.ResourceType.Rule);
			HandleList<AlertResponse>(ApiClient.ResourceType.Alert);
			HandleList<RoleDefinitionResponse>(ApiClient.ResourceType.RoleDefinition);
			HandleList<RoleAssignmentResponse>(ApiClient.ResourceType.RoleAssignment);
			//

			Console.WriteLine();
			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}
		
		static void HandleList<T>(ApiClient.ResourceType resourceType)
		{
			HttpResponseMessage httpResponseMessage = _apiClient.GetResourcesAsync(resourceType).Result;

			string resourceTypeName = resourceType.ToString();
			string statusCode = httpResponseMessage.StatusCode.ToString();
			string responseContent = _apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result;
			int listCount = 0;

			JObject response = JsonConvert.DeserializeObject<JObject>(responseContent);

			if (httpResponseMessage.IsSuccessStatusCode)
			{
				List<T> list = response["items"].Children().AsParallel().Select(i => i.ToObject<T>()).ToList();
				listCount = list.Count;
			}

			WriteOut(resourceTypeName, statusCode, responseContent, listCount);
		}

		static void WriteOut(string resourceTypeName, string statusCode, string responseContent, int listCount)
		{
			Console.WriteLine();
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine(resourceTypeName);
			Console.WriteLine(statusCode);
			Console.WriteLine(listCount.ToString());
			Console.WriteLine(responseContent);
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine();
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
