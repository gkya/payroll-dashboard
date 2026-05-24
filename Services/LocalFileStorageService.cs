using Microsoft.AspNetCore.Http;

namespace PayrollDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
  private readonly string _uploadDirectory;
  

  public LocalFileStorageService(IConfiguration configuration)
  {
    _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? "uploads";
    if (!Directory.Exists(_uploadDirectory))
    {
      Directory.CreateDirectory(_uploadDirectory);
    }
  }

  public string SaveFile(IFormFile file)
  {
    var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
    var fileName = $"{timestamp}_{file.FileName}";
    var filePath = Path.Combine(_uploadDirectory, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
      file.CopyTo(stream);
    }

    return filePath;
  }
}
