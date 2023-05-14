using Common.Exchange;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Common
{
    public class HttpRequest
    {
        public static Dictionary<string, string> SendGetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;

                    // Check if the response contains JSON data
                    if (IsJsonResponse(response))
                    {
                        Dictionary<string, string> result = DeserializeJson(responseBody);
                        return result;
                    }
                    else
                    {
                        Console.WriteLine("The response does not contain JSON data.");
                        return new Dictionary<string, string>();
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return new Dictionary<string, string>();
                }
            }
        }

        private static bool IsJsonResponse(HttpResponseMessage response)
        {
            IEnumerable<string> contentTypes;
            if (response.Content.Headers.TryGetValues("Content-Type", out contentTypes))
            {
                foreach (string contentType in contentTypes)
                {
                    if (contentType.Contains("application/json"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Dictionary<string, string> DeserializeJson(string jsonString)
        {
            try
            {
                Dictionary<string, string> result = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                return result;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }
    }
}
