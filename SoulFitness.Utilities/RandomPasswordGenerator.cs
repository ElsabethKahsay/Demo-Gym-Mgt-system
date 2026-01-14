using System;
using System.Collections.Generic;
using System.Text;

namespace SoulFitness.Utilities
{
    public static class RandomPasswordGenerator
    {
        public static string CreateRandomPassword(int length = 8)
        {
            string numbers = "0123456789";
            string upperCases = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            string lowerCases = "abcdefghijklmnopqrstuvwxyz";
            string specialCharacters = "!@#$%^&*?_-";

            Random random = new Random();

            char[] nums = new char[4];
            for (int i = 0; i < 4; i++)
            {
                nums[i] = numbers[random.Next(0, numbers.Length)];
            }

            char[] upperCase = new char[2];
            for (int i = 0; i < 2; i++)
            {
                upperCase[i] = upperCases[random.Next(0, upperCases.Length)];
            }

            char[] lowerCase = new char[2];
            for (int i = 0; i < 2; i++)
            {
                lowerCase[i] = lowerCases[random.Next(0, lowerCases.Length)];
            }

            char[] specialChars = new char[2];
            for (int i = 0; i < 2; i++)
            {
                specialChars[i] = specialCharacters[random.Next(0, specialCharacters.Length)];
            }

            //return "Abcd@1234"; //new string(upperCase) + new string(lowerCase) + new string(specialChars) + new string(nums);
            return new string(upperCase) + new string(lowerCase) + new string(specialChars) + new string(nums);
        }
    }
}
