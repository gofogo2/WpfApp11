using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

public class WebApiHelper
{
    private static WebApiHelper _instance;
    private static readonly object _lock = new object();
    private readonly HttpClient _httpClient;
    private string _baseUrl;
    private string _username;
    private string _password;

    private WebApiHelper()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public static WebApiHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new WebApiHelper();
                    }
                }
            }
            return _instance;
        }
    }

    public void Initialize(string baseUrl, string username, string password)
    {
        _baseUrl = baseUrl;
        _username = username;
        _password = password;
    }

    private async Task<string> PostAsync(string endpoint, Dictionary<string, string> additionalParams)
    {
        var parameters = new Dictionary<string, string>
        {
            {"usr", _username},
            {"pwd", _password}
        };

        foreach (var param in additionalParams)
        {
            parameters.Add(param.Key, param.Value);
        }

        var content = new FormUrlEncodedContent(parameters);

        HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

   

    public async Task<string> RebootAll(string url)
    {
        _baseUrl = $"http://{url}/";
        return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "reboot"}
        });
    }

    public async Task<string> OnAll(string url)
    {
        _baseUrl =  $"http://{url}/";
        return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "on"}
        });
    }

    public async Task<string> OffAll(string url)
    {
        _baseUrl = $"http://{url}/";
        return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "off"}
        });
    }
}