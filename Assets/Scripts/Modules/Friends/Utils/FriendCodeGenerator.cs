using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RecipeRage.Modules.Friends.Utils
{
    /// <summary>
    /// Utility class for generating and validating friend codes
    /// 
    /// Complexity Rating: 2
    /// </summary>
    public static class FriendCodeGenerator
    {
        // Characters used in the friend code
        private const string CODE_CHARACTERS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        
        // Length of the friend code (excluding hyphens)
        private const int CODE_LENGTH = 12;
        
        // Format: XXXX-XXXX-XXXX
        private const string CODE_FORMAT = @"^[A-HJ-NP-Z2-9]{4}-[A-HJ-NP-Z2-9]{4}-[A-HJ-NP-Z2-9]{4}$";
        
        /// <summary>
        /// Generate a unique friend code for a user
        /// </summary>
        /// <param name="userId">The user's unique identifier</param>
        /// <param name="salt">Optional salt to make codes unique across different auth systems</param>
        /// <returns>Friend code in the format XXXX-XXXX-XXXX</returns>
        public static string GenerateFriendCode(string userId, string salt = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("Cannot generate friend code for empty user ID");
                return null;
            }
            
            // Add salt to make codes unique across different auth systems
            string input = string.IsNullOrEmpty(salt) ? userId : userId + salt;
            
            // Use SHA256 to generate a hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                
                // Convert hash to a string of characters from our set
                StringBuilder codeBuilder = new StringBuilder();
                for (int i = 0; i < CODE_LENGTH; i++)
                {
                    // Use 5 bits from the hash (32 possible values) for each character
                    int byteIndex = i / 8 * 4;  // Each 8 characters use 4 bytes of the hash
                    int bitOffset = (i % 8) * 5; // Each character uses 5 bits
                    
                    // Extract 5 bits from the hash
                    int value = 0;
                    int remainingBits = 5;
                    while (remainingBits > 0)
                    {
                        int currentByteIndex = byteIndex + bitOffset / 8;
                        int currentBitOffset = bitOffset % 8;
                        
                        // Don't read past the end of the hash
                        if (currentByteIndex >= hashBytes.Length)
                            break;
                        
                        int bitsFromCurrentByte = Math.Min(remainingBits, 8 - currentBitOffset);
                        int mask = ((1 << bitsFromCurrentByte) - 1) << currentBitOffset;
                        int bits = (hashBytes[currentByteIndex] & mask) >> currentBitOffset;
                        
                        value |= bits << (5 - remainingBits);
                        remainingBits -= bitsFromCurrentByte;
                        bitOffset += bitsFromCurrentByte;
                    }
                    
                    // Ensure the value is within the range of our character set
                    value = value % CODE_CHARACTERS.Length;
                    codeBuilder.Append(CODE_CHARACTERS[value]);
                    
                    // Add hyphens for readability
                    if ((i + 1) % 4 == 0 && i < CODE_LENGTH - 1)
                    {
                        codeBuilder.Append('-');
                    }
                }
                
                return codeBuilder.ToString();
            }
        }
        
        /// <summary>
        /// Validate a friend code against the expected format
        /// </summary>
        /// <param name="friendCode">Friend code to validate</param>
        /// <returns>True if the code is valid</returns>
        public static bool IsValidFriendCode(string friendCode)
        {
            if (string.IsNullOrEmpty(friendCode))
                return false;
            
            // Normalize: uppercase and remove any extra whitespace
            friendCode = friendCode.Trim().ToUpper();
            
            // Check format using regex
            return Regex.IsMatch(friendCode, CODE_FORMAT);
        }
        
        /// <summary>
        /// Format a friend code with proper spacing and capitalization
        /// </summary>
        /// <param name="friendCode">Friend code to format</param>
        /// <returns>Formatted friend code</returns>
        public static string FormatFriendCode(string friendCode)
        {
            if (string.IsNullOrEmpty(friendCode))
                return string.Empty;
            
            // Remove all non-alphanumeric characters
            string code = Regex.Replace(friendCode, @"[^A-Z0-9]", "").ToUpper();
            
            // Replace easily confused characters
            code = code.Replace('O', '0').Replace('0', 'Q').Replace('Q', '0');
            code = code.Replace('I', '1').Replace('1', 'I').Replace('I', '1');
            code = code.Replace('0', 'O').Replace('O', '0');
            code = code.Replace('1', 'I').Replace('I', '1');
            
            // Ensure the code has no ambiguous characters
            StringBuilder safeCode = new StringBuilder();
            foreach (char c in code)
            {
                if (CODE_CHARACTERS.IndexOf(c) >= 0)
                {
                    safeCode.Append(c);
                }
                else if (c == '0')
                {
                    safeCode.Append('Q');
                }
                else if (c == '1')
                {
                    safeCode.Append('I');
                }
            }
            
            code = safeCode.ToString();
            
            // Ensure code is at least CODE_LENGTH characters
            if (code.Length < CODE_LENGTH)
            {
                return string.Empty;
            }
            
            // Format with hyphens
            return $"{code.Substring(0, 4)}-{code.Substring(4, 4)}-{code.Substring(8, 4)}";
        }
    }
} 