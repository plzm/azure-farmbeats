using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using pelazem.http;

namespace FarmBeats.Api.Client
{
	public class ApiClient
	{
		#region Enums

		public enum ResourceType
		{
			Farm,
			Device,
			DeviceModel,
			Sensor,
			SensorModel,
			Telemetry,
			Job,
			JobType,
			ExtendedType,
			Partner,
			Scene,
			SceneFile,
			Rule,
			Alert,
			RoleDefinition,
			RoleAssignment
		}

		#endregion

		#region Constants

		public const string AUTHORIZATION_HEADER = "Authorization";
		public const string ACCEPT_HEADER = "accept";
		public const string CONTENT_TYPE_HEADER = "Content-Type";

		public const string CONTENT_TYPE_JSON = "application/json";

		#endregion

		#region Variables

		private Uri _baseUri = null;

		private string _bearerToken = string.Empty;

		#endregion

		#region Properties

		public string TenantId { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }

		public string ApiEndpoint { get; set; }

		public Uri ApiEndpointUri
		{
			get
			{
				if (_baseUri == null)
					_baseUri = new Uri(this.ApiEndpoint);

				return _baseUri;
			}
		}

		private HttpUtil HttpUtil { get; } = new HttpUtil();

		private string BearerToken
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_bearerToken))
				{
					AuthenticationResult authenticationResult = GetAuthenticationResult().Result;
					_bearerToken = "Bearer " + authenticationResult.AccessToken;
				}

				return _bearerToken;
			}
		}

		#endregion

		#region Constructors

		private ApiClient() { }

		public ApiClient(string tenantId, string clientId, string clientSecret, string apiEndpoint)
		{
			this.TenantId = tenantId;
			this.ClientId = clientId;
			this.ClientSecret = clientSecret;
			this.ApiEndpoint = apiEndpoint;
		}

		#endregion

		#region Utility

		private async Task<AuthenticationResult> GetAuthenticationResult()
		{
			Authentication auth = new Authentication();

			AuthenticationResult authenticationResult = await auth.GetAuthenticationResultAsync(this.TenantId, this.ClientId, this.ClientSecret, this.ApiEndpoint);

			return authenticationResult;
		}

		private void PrepareGetRequestHeaders()
		{
			this.HttpUtil.RemoveAllRequestHeaders();

			//this.HttpUtil.AddRequestHeader(CONTENT_TYPE_HEADER, CONTENT_TYPE_JSON);

			this.HttpUtil.AddRequestHeader(AUTHORIZATION_HEADER, this.BearerToken);
		}

		private void PreparePostRequestHeaders()
		{
			PrepareGetRequestHeaders();

			this.HttpUtil.AddRequestHeader(ACCEPT_HEADER, CONTENT_TYPE_JSON);
		}

		public string GetHttpResponseHeaders(HttpResponseMessage httpResponseMessage, bool asFormattedJson = false)
		{
			if (httpResponseMessage?.Headers == null || httpResponseMessage.Headers.Count() == 0)
				return string.Empty;

			string result = string.Empty;

			if (!asFormattedJson)
			{
				foreach (var header in httpResponseMessage.Headers)
					result += header.Key + " = " + header.Value + Environment.NewLine;
			}
			else
				result = this.HttpUtil.AsJson(httpResponseMessage.Headers, true);

			return result;
		}

		public async Task<string> GetHttpResponseContentAsync(HttpResponseMessage httpResponseMessage, bool asFormattedJson = false)
		{
			return await this.HttpUtil.GetHttpResponseContentAsync(httpResponseMessage, asFormattedJson);
		}

		#endregion

		#region API

		public async Task<HttpResponseMessage> GetResourcesAsync(ResourceType resourceType)
		{
			PrepareGetRequestHeaders();

			Uri uri = new Uri(this.ApiEndpointUri, resourceType.ToString());

			HttpResponseMessage httpResponseMessage = await this.HttpUtil.HttpClient.GetAsync(uri);

			return httpResponseMessage;
		}

		#endregion
	}
}
