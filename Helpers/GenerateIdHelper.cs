namespace NLPHelpDesk.Helpers;

/// <summary>
/// Provides helper methods for generating unique IDs.
/// </summary>
public class GenerateIdHelper
{
    /// <summary>
    /// The character set used for generating IDs (uppercase letters A-Z).
    /// </summary>
    private static readonly char[] _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    
    /// <summary>
    /// The base of the character set (26, the number of letters in the alphabet).
    /// </summary>
    private static readonly int _base = _alphabet.Length;

    /// <summary>
    /// Generates a unique ID of a specified length.
    /// </summary>
    /// <param name="length">The desired length of the ID.</param>
    /// <returns>A unique ID string.</returns>
    public static string GenerateId(int length)
    {
        string id = "";
        
        // Use a GUID as a source of randomness.
        byte[] guidBytes = Guid.NewGuid().ToByteArray();
        string base36 = Base36Encode(guidBytes);

        // Extract letters from the base36 string.
        string alphaOnly = new string(base36.Where(c => char.IsLetter(c)).ToArray());

        // Pad with random letters if necessary.
        while (alphaOnly.Length < length)
        {
            alphaOnly += _alphabet[new Random().Next(_base)];
        }

        // Return generated unique ID
        return alphaOnly.Substring(0, length).ToUpper();
    }

    /// <summary>
    /// Encodes a byte array to a Base36 string.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <returns>The Base36 encoded string.</returns>
    private static string Base36Encode(byte[] data)
    {
        System.Numerics.BigInteger number = new System.Numerics.BigInteger(data);
        
        if (number.Sign < 0)
        {
            // Handle negative numbers
            number = -number;
        }

        if (number == 0) return "0";

        string result = "";
        while (number > 0)
        {
            var remainder = (int)(number % _base);
            result = _alphabet[remainder] + result;
            number /= _base;
        }

        return result;
    }
}