using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherAlertAPI.Models
{
    /// <summary>
    /// Base class for hypermedia responses with HATEOAS links
    /// </summary>
    public class HypermediaResponse<T>
    {
        /// <summary>
        /// The main data of the response
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }

        /// <summary>
        /// Collection of hypermedia links for resource navigation
        /// </summary>
        [JsonPropertyName("_links")]
        public Dictionary<string, Link> Links { get; set; } = new();
    }

    /// <summary>
    /// Represents a hypermedia link
    /// </summary>
    public class Link
    {
        public Link(string href, string method = "GET")
        {
            Href = href;
            Method = method;
        }

        /// <summary>
        /// The URL of the link
        /// </summary>
        [JsonPropertyName("href")]
        public string Href { get; set; }

        /// <summary>
        /// The HTTP method to use with this link
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }
    }
}
