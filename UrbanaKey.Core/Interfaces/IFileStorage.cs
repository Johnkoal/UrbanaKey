using System.IO;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Interfaces;

public interface IFileStorage
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string module, Guid tenantId);
}
