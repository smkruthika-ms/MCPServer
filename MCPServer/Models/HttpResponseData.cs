using System.IO;
using System.Net;
using System.Collections.Generic;

namespace MCPServer.Models
{
    /// <summary>
    /// A representation of the outgoing HTTP response matching Azure Functions format.
    /// </summary>
    public class HttpResponseData
    {
        /// <summary>
        /// Gets or sets the status code for the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// Gets or sets the response body stream.
        /// </summary>
        public Stream Body { get; set; }

        /// <summary>
        /// Gets or sets the response body as string (convenience property).
        /// </summary>
        public string? BodyAsString { get; set; }

        /// <summary>
        /// Initializes a new instance of the HttpResponseData class.
        /// </summary>
        public HttpResponseData()
        {
            StatusCode = HttpStatusCode.OK;
            Body = new MemoryStream();
        }

        /// <summary>
        /// Initializes a new instance of the HttpResponseData class with a status code.
        /// </summary>
        public HttpResponseData(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Body = new MemoryStream();
        }

        /// <summary>
        /// Sets the body content from a string.
        /// </summary>
        public void SetBodyAsString(string content)
        {
            BodyAsString = content;
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// Gets the body content as a string.
        /// </summary>
        public async Task<string> GetBodyAsStringAsync()
        {
            if (!string.IsNullOrEmpty(BodyAsString))
            {
                return BodyAsString;
            }

            if (Body != null && Body.CanSeek)
            {
                Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(Body))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            return string.Empty;
        }
    }
}
