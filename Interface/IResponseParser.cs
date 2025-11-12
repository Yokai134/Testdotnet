using Testdotnet.Model;

namespace Testdotnet.Interface
{
    public interface IResponseParser
    {
        Task<Login> ParseLoginResponse(string xmlResponse);
    }
}
