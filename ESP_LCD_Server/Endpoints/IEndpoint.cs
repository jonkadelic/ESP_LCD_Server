namespace ESP_LCD_Server.Endpoints
{
    public interface IEndpoint
    {
        /// <summary>
        /// String to use as the UDP endpoint key
        /// </summary>
        public string Endpoint { get; }

        /// <summary>
        /// Get the endpoint response as a byte array.
        /// </summary>
        /// <param name="request">String containing the exact request made to the UDP socket.</param>
        /// <returns>Byte array containing the binary response to the UDP request.</returns>
        public byte[] GetResponseBody(string request);
    }
}
