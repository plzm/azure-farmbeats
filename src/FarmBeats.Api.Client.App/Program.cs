using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FarmBeats.Api.Models;

namespace FarmBeats.Api.Client.App
{
	class Program
	{
		private static dynamic _config = null;
		private static ApiClient _apiClient = null;

		static void Main(string[] args)
		{
			_config = GetConfiguration();
			_apiClient = new ApiClient(_config.TenantId, _config.ClientId, _config.ClientSecret, _config.ApiEndpoint);

			GetLists();

			//DeviceResponse dr = GetResource<DeviceResponse>("a8e79ba4-8383-4267-8cb1-4d7d577323f2");

			//DeviceModelResponse dmresp = CreateDeviceModel(GetDeviceModelRequest());

			Console.WriteLine();
			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		private static DeviceModelRequest GetDeviceModelRequest()
		{
			ICollection<DevicePort> ports = new[] { new DevicePort() { Name = "DevicePort1", Type = DevicePortType.Analog } };

			Dictionary<string, object> props = new Dictionary<string, object>();
			props["Foo"] = "Bar";

			DeviceModelRequest result = new DeviceModelRequest()
			{
				Description = "Description",
				Manufacturer = "Manufacturer",
				Name = "Name",
				Ports = ports,
				ProductCode = "ProductCode",
				Properties = props,
				Type = DeviceModelRequestType.Node
			};

			return result;
		}

		private static DeviceModelResponse CreateDeviceModel(DeviceModelRequest deviceModelRequest)
		{
			HttpResponseMessage httpResponseMessage = _apiClient.PostResourceAsync<DeviceModelRequest>(deviceModelRequest).Result;
			
			DeviceModelResponse dmresp = _apiClient.GetResourceFromHttpResponseAsync<DeviceModelResponse>(httpResponseMessage).Result;
			
			return dmresp;
		}

		private static void GetLists()
		{
			//GetList<PartnerResponse>();
			//GetList<FarmResponse>();
			GetList<DeviceModelResponse>();

			GetList<DeviceResponse>();

			GetList<SensorModelResponse>();
			GetList<SensorResponse>();
			GetList<ExtendedTypeResponse>();
			//GetList<JobTypeResponse>();
			//GetList<JobResponse>();
			//GetList<SceneResponse>();
			//GetList<GetSceneFileResponse>();
			//GetList<RuleResponse>();
			//GetList<AlertResponse>();
			//GetList<RoleDefinitionResponse>();
			//GetList<RoleAssignmentResponse>();
		}

		private static void GetList<T>()
			where T : new()
		{
			HttpResponseMessage httpResponseMessage = _apiClient.GetResourcesAsync<T>().Result;

			string resourceTypeName = typeof(T).Name;
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

		private static T GetResource<T>(string id)
			where T : new()
		{
			HttpResponseMessage httpResponseMessage = _apiClient.GetResourceAsync<T>(id).Result;

			T result = _apiClient.GetResourceFromHttpResponseAsync<T>(httpResponseMessage).Result;

			return result;
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
