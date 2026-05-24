using Microsoft.AspNetCore.Http;

namespace PayrollDashboard.Services;

public interface IFileStorageService
{
  string SaveFile(IFormFile file);
}