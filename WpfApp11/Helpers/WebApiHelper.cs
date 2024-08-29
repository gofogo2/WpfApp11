using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;

public class WebApiHelper
{
    private static WebApiHelper _instance;
    private static readonly object _lock = new object();
    private readonly HttpClient _httpClient;
    private string _baseUrl;
    private string _username= "administrator";
    private string _password= "password";

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

    private async Task<string> GetAsync(string endpoint, Dictionary<string, string> additionalParams)
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

        var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}?{queryString}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Dictionary<string, string>> StatusPDU(string url, string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            string response = await GetAsync("/api/outlet/relay", new Dictionary<string, string>
            {
                {"index", index}
            });

            XDocument doc = XDocument.Parse(response);
            var outletRelay = doc.Element("OutletRelay");
            var result = new Dictionary<string, string>();

            if (outletRelay != null)
            {
                foreach (var element in outletRelay.Elements())
                {
                    result[element.Name.LocalName] = element.Value.Trim();
                }
            }

            return result;
        }
        catch (Exception e)
        {
            return new Dictionary<string, string> { { "Error", "Fail" } };
        }
    }



    public async Task<string> RebootAll(string url)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "reboot"}
        });
        }catch(Exception e)
        {
            MessageBox.Show(e.Message);
            return "Fail";
        }
    }

    public async Task<string> OnPDUAll(string url)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "on_immediate"}
        });
        }catch(Exception e)
        {
            MessageBox.Show(e.Message);
            return "Fail";
        }
    }

    public async Task<string> OnPDU(string url,string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/outlet/relay", new Dictionary<string, string>
        {
            {"index", index},
            {"method", "on_immediate"}
        });
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return "Fail";
        }
    }

    public async Task<string> RebootPDU(string url,string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/outlet/relay", new Dictionary<string, string>
        {
            {"index", index},
            {"method", "reboot"}

        });
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return "Fail";
        }
    }


    public async Task<string> OffPDU(string url, string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/outlet/relay", new Dictionary<string, string>
        {
            {"index", index},
            {"method", "off_immediate"}

        });
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return "Fail";
        }
    }


    public async Task<string> OffPDUAll(string url)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "off_immediate"}
        });
        }catch(Exception e)
        {
            MessageBox.Show(e.Message);
            return "Fail";
        }
    }
}