using System;

namespace LumStoreAPI.Libraries.Helpers;

public class StringHelper
{
    public static string GenerateCode()
    {
        int number = Random.Shared.Next(0, 1000000);
        string code = number.ToString("D6");
        return code;
    }
}
