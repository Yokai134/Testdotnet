using System.Security;
using System.Text;
using Testdotnet.Interface;
using Testdotnet.Model;
using Microsoft.JSInterop;

namespace Testdotnet.Services
{
    public class SoapServices : ISoapService
    {
        private readonly HttpClient _httpClient;
        private readonly IResponseParser _responseParser;
        private readonly IJSRuntime _jsRuntime;
        private const string SoapEndpoint = "http://isapi.mekashron.com/icu-tech/icutech-test.dll/soap/IICUTech";

        public SoapServices(HttpClient httpClient, IResponseParser responseParser, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _responseParser = responseParser;
            _jsRuntime = jsRuntime;
        }

        public async Task<Login> LoginAsync(string username, string password)
        {
            try
            {
                var soapEnvelope = BuildLoginEnvelope(username, password);
                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "urn:ICUTech.Intf-IICUTech#Login");


                await _jsRuntime.InvokeVoidAsync("console.log", "=== SOAP REQUEST ===");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Endpoint: {SoapEndpoint}");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Username: '{username}'");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Password length: {password?.Length}");
                await _jsRuntime.InvokeVoidAsync("console.log", "SOAP Action: urn:ICUTech.Intf-IICUTech#Login");

                var response = await _httpClient.PostAsync(SoapEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();


                await _jsRuntime.InvokeVoidAsync("console.log", "=== SOAP RESPONSE ===");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Status Code: {response.StatusCode}");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Response Length: {responseContent.Length} chars");


                var preview = responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent;
                await _jsRuntime.InvokeVoidAsync("console.log", $"Response Preview: {preview}");

                var result = await _responseParser.ParseLoginResponse(responseContent);


                await _jsRuntime.InvokeVoidAsync("console.log", "=== PARSING RESULT ===");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Success: {result.Success}");
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    await _jsRuntime.InvokeVoidAsync("console.error", $"Error: {result.ErrorMessage}");
                if (!string.IsNullOrEmpty(result.Details))
                    await _jsRuntime.InvokeVoidAsync("console.log", $"Details: {result.Details}");

                return result;
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"EXCEPTION: {ex.Message}");
                return new Login
                {
                    Success = false,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        private string BuildLoginEnvelope(string username, string password)
        {
            return $"""
             <?xml version="1.0" encoding="utf-8"?>
             <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"
                           xmlns:soapenc="http://schemas.xmlsoap.org/soap/encoding/"
                           xmlns:tns="http://tempuri.org/"
                           xmlns:types="urn:ICUTech.Intf"
                           xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                           xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                 <soap:Body soap:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">
                     <ns1:Login xmlns:ns1="urn:ICUTech.Intf-IICUTech">
                         <UserName xsi:type="xsd:string">{SecurityElement.Escape(username)}</UserName>
                         <Password xsi:type="xsd:string">{SecurityElement.Escape(password)}</Password>
                         <IPs xsi:type="xsd:string">127.0.0.1</IPs>
                     </ns1:Login>
                 </soap:Body>
             </soap:Envelope>
             """;
        }
    }
}