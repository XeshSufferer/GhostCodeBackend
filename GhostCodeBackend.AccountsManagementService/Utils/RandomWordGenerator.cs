namespace GhostCodeBackend.AccountsManagementService.Utils;

public class RandomWordGenerator : IRandomWordGenerator
{
    private readonly Random _random = new Random();
    private readonly string _chars = "1234567890!@#$%^&*()qwertyuiop[]asdfghjkl;zxcvbnm,./QWERTYUIOP{}ASDFGHJKL:ZXCVBNM<>?";

    private char GetRandomChar()
    {
        return _chars[_random.Next(_chars.Length)];
    }

    public string GetRandomWord(int length)
    {
        var word = "";

        for (int i = 0; i != length; i++)
        {
            word += GetRandomChar();
        }
        
        return word;
    }
    
}