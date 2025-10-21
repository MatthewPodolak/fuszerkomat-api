namespace fuszerkomat_api.Helpers
{
    public class PhotoSaver
    {
        public static async Task<string> SavePhotoAsync(IFormFile file, string physicalFolder, string requestPathPrefix, CancellationToken ct)
        {

            var folder = Path.IsPathRooted(physicalFolder)
                ? physicalFolder
                : Path.Combine(Directory.GetCurrentDirectory(), physicalFolder);

            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext.ToLowerInvariant())) throw new InvalidOperationException("Unsupported file type");

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream, ct);

            return $"{requestPathPrefix}/{fileName}";
        }
    }
}
