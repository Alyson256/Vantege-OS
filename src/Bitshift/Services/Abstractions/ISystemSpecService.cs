using System.Threading.Tasks;
using Bitshift.Models;

namespace Bitshift.Services.Abstractions
{
    public interface ISystemSpecService
    {
        Task<SystemSpecInfo> GetSpecsAsync();
    }
}
