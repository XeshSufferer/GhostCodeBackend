using GhostCodeBackend.UserContentService.Repositories;
using Minio;
using Minio.DataModel.Args;

namespace GhostCodeBackend.UserContentService.Services;

public class CustomizerService : ICustomizerService
{
    
    private readonly string[] _imageExtensions =
    {
        ".png",
        ".jpg",
        ".jpeg"
    };
    
    private readonly ILiteUserRepository _userRepository;
    private readonly IStorage _storage;

    public CustomizerService(ILiteUserRepository userRepository, IStorage storage)
    {
        _userRepository = userRepository;
        _storage = storage;
    }

    public async Task<(bool result, string filename)> SetHeader(IFormFile file, string userid)
    {
        Console.WriteLine($"=== SetHeader START ===");
        Console.WriteLine($"UserID: {userid}");
        Console.WriteLine($"File Name: {file?.FileName}");
        Console.WriteLine($"File Length: {file?.Length}");
        Console.WriteLine($"File ContentType: {file?.ContentType}");

        // Проверка файла
        if (file == null)
        {
            Console.WriteLine("❌ FILE IS NULL");
            return (false, "");
        }

        if (file.Length <= 0)
        {
            Console.WriteLine("❌ FILE LENGTH IS 0 OR LESS");
            return (false, "");
        }

        // Проверка расширения
        string extension = Path.GetExtension(file.FileName);
        Console.WriteLine($"File Extension: {extension}");

        if (string.IsNullOrEmpty(extension))
        {
            Console.WriteLine("❌ FILE EXTENSION IS NULL OR EMPTY");
            return (false, "");
        }

        if (!_imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"❌ INVALID EXTENSION: {extension}. Allowed: {string.Join(", ", _imageExtensions)}");
            return (false, "");
        }

        Console.WriteLine("✅ File validation passed");

        // Получение пользователя
        Console.WriteLine($"Getting user from repository...");
        var userGettingResult = await _userRepository.GetUser(userid);
        
        if (userGettingResult == null)
        {
            Console.WriteLine("❌ USER NOT FOUND IN DATABASE");
            return (false, "");
        }
        
        Console.WriteLine($"✅ User found: {userGettingResult.Id}");

        // Загрузка в хранилище
        Console.WriteLine($"Uploading file to storage (bucket: Headers)...");
        var result = await _storage.Upload(file, "headers");
        
        Console.WriteLine($"Storage upload result: {result.result}");
        Console.WriteLine($"Storage upload imageid: {result.imageid}");
        
        if (!result.result)
        {
            Console.WriteLine("❌ STORAGE UPLOAD FAILED");
            return (false, "");
        }

        // Обновление пользователя
        Console.WriteLine($"Updating user header link to: {result.imageid}");
        userGettingResult.HeaderLink = result.imageid;
        
        Console.WriteLine("Calling user repository update...");
        var userUpdateResult = await _userRepository.UpdateUser(userGettingResult);
        
        Console.WriteLine($"User update result: {userUpdateResult.result}");
        
        if (!userUpdateResult.result)
        {
            Console.WriteLine("❌ USER UPDATE FAILED");
            return (false, "");
        }

        // Финальная проверка
        bool finalResult = result.result && userUpdateResult.result && userGettingResult != null;
        Console.WriteLine($"Final result calculation: {result.result} && {userUpdateResult.result} && {userGettingResult != null} = {finalResult}");

        if (finalResult)
        {
            Console.WriteLine($"✅ SetHeader SUCCESS - Filename: {result.imageid}");
        }
        else
        {
            Console.WriteLine($"❌ SetHeader FAILED - Final check failed");
        }

        Console.WriteLine($"=== SetHeader END ===\n");
        
        return (finalResult, result.imageid);
    }

    public async Task<(bool result, string filename)> SetAvatar(IFormFile file, string userid)
    {
        if (file.Length <= 0)
            return (false, "");
    
    
        string extension = Path.GetExtension(file.FileName);
        
        
        if (!_imageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return (false, "");

        var userGettingResult = await _userRepository.GetUser(userid);
        var result = await _storage.Upload(file, "avatars");
        userGettingResult.AvatarLink = result.imageid;
        var userUpdateResult = await _userRepository.UpdateUser(userGettingResult);
        
        return (result.result && userUpdateResult.result && userGettingResult != null, result.imageid);
    }
}