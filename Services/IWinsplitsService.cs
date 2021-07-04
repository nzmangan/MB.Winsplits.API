using System.Threading.Tasks;

namespace MB.Winsplits.API {
  public interface IWinsplitsService {
    Task<APIResponse> Create(byte[] rawFile, ResultListOverrides resultListOverrides = null);
    Task<APIResponse> Delete(string eventId, string password);
    Task<APIResponse> Update(string eventId, string password, byte[] rawFile, ResultListOverrides resultListOverrides = null);
  }
}