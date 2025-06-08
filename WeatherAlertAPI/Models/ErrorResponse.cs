using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherAlertAPI.Models
{
    /// <summary>
    /// Represents an API error response with hypermedia links
    /// </summary>
    public class ErrorResponse
    {
        public ErrorResponse(string code, string message)
        {
            Error = new ErrorDetail
            {
                Code = code,
                Message = message
            };
            Links = new Dictionary<string, Link>();
        }

        [JsonPropertyName("error")]
        public ErrorDetail Error { get; set; }

        [JsonPropertyName("_links")]
        public Dictionary<string, Link> Links { get; set; }

        public void AddLink(string rel, string href, string method = "GET")
        {
            Links[rel] = new Link(href, method);
        }
    }

    public class ErrorDetail
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
