namespace GhostCodeBackend.GitUsageService.Helpers;

public class RandomWordHelper : IRandomWordHelper
{
    private readonly Random _random = new Random();
    private readonly string _chars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

    public string GetRandomWord(int length)
    {

        var word = "";
        
        for (int i = 0; i != length; i++)
        {
            word += _chars[_random.Next(0, _chars.Length)];
        }
        return word;
    }
}