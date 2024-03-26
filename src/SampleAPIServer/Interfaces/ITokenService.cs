using System.Threading.Tasks;

namespace SampleAPIServer.Interfaces
{
    public interface ITokenService
    {
        Task<string> GetAuthorizationTokenAsync();
    }
}
