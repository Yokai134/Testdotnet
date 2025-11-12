using Testdotnet.Model;

namespace Testdotnet.Interface
{
    public interface ISoapService
    {
        Task<Login> LoginAsync(string username, string password);
    }
}
