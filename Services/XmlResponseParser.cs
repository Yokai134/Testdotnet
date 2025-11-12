using System.Xml;
using Testdotnet.Interface;
using Testdotnet.Model;
using Microsoft.JSInterop;

namespace Testdotnet.Services
{
    public class XmlResponseParser : IResponseParser
    {
        private readonly IJSRuntime _jsRuntime;

        public XmlResponseParser(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<Login> ParseLoginResponse(string xmlResponse)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("console.log", "=== XML PARSING STARTED ===");

                var doc = new XmlDocument();
                doc.LoadXml(xmlResponse);

                var faultNode = doc.SelectSingleNode("//*[local-name()='faultstring']");
                if (faultNode != null)
                {
                    await _jsRuntime.InvokeVoidAsync("console.error", $"SOAP Fault: {faultNode.InnerText}");
                    return new Login
                    {
                        Success = false,
                        ErrorMessage = faultNode.InnerText
                    };
                }

                var resultNode = doc.SelectSingleNode("//*[local-name()='return']");
                if (resultNode != null)
                {
                    var jsonResponse = resultNode.InnerText.Trim();
                    await _jsRuntime.InvokeVoidAsync("console.log", $"JSON Response: {jsonResponse}");

                    if (jsonResponse.Contains("\"EntityId\""))
                    {
                        await _jsRuntime.InvokeVoidAsync("console.log", "✅ SUCCESS: EntityId found");
                        return new Login
                        {
                            Success = true,
                            Details = jsonResponse
                        };
                    }
                    else if (jsonResponse.Contains("\"ResultCode\":-1"))
                    {
                        var errorMatch = System.Text.RegularExpressions.Regex.Match(jsonResponse, "\"ResultMessage\":\"([^\"]+)\"");
                        var errorMessage = errorMatch.Success ? errorMatch.Groups[1].Value : "Login failed";

                        await _jsRuntime.InvokeVoidAsync("console.error", $"❌ LOGIN FAILED: {errorMessage}");
                        return new Login
                        {
                            Success = false,
                            ErrorMessage = errorMessage
                        };
                    }
                }

                await _jsRuntime.InvokeVoidAsync("console.warn", "⚠️ Unknown response format");
                return new Login
                {
                    Success = false,
                    ErrorMessage = "Invalid response format"
                };
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"XML Parsing Exception: {ex.Message}");
                return new Login
                {
                    Success = false,
                    ErrorMessage = $"Parsing error: {ex.Message}"
                };
            }
        }
    }
}