using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using careerhive.application.Interfaces.IService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace careerhive.application.Services;
public class SafeBrowsingService : ISafeBrowsingService
{
    private readonly string _apiKey;
    private readonly string _clientId;
    private readonly HttpClient _httpClient;

    public SafeBrowsingService(IConfiguration configuration)
    {
        _apiKey = configuration["SafeBrowsing:ApiKey"] ?? throw new ArgumentException("Missing SafeBrousing:ApiKey");
        _clientId = configuration["SafeBrowsing:ClientId"] ?? throw new ArgumentException("Missing SafeBrousing:ClientId");
        _httpClient = new HttpClient();
    }

    public async Task<bool> IsUrlSafeAsync(string url)
    {
        var requestBody = new
        {
            client = new
            {
                clientId = _clientId,
                clientVersion = "1.0.0"
            },
            threatInfo = new
            {
                threatTypes = new string[] { "MALWARE", "SOCIAL_ENGINEERING", "POTENTIALLY_HARMFUL_APPLICATION" },
                platformTypes = new string[] { "WINDOWS" },
                threatEntryTypes = new string[] { "URL" },
                threatEntries = new[]
                {
                    new { url = url }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"https://safebrowsing.googleapis.com/v4/threatMatches:find?key={_apiKey}", content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseBody);

            return result!.matches == null;
        }

        return false;
    }
}
