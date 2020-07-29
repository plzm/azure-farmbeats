using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace FarmBeats.Api.Client
{
	public class Authentication
	{
		public async Task<AuthenticationResult> GetAuthenticationResultAsync(string tenantId, string clientId, string clientSecret, string apiEndpoint)
		{
			IConfidentialClientApplication cca = ConfidentialClientApplicationBuilder
				.Create(clientId)
				.WithTenantId(tenantId)
				.WithClientSecret(clientSecret)
				.WithClientName("FarmBeats Client")
				.WithRedirectUri(apiEndpoint)
				.Build();

			string scope = apiEndpoint + "/.default";
			var scopes = new[] { scope };

			var token = cca.AcquireTokenForClient(scopes);

			AuthenticationResult authResult = await token.ExecuteAsync();

			return authResult;
		}
	}
}
