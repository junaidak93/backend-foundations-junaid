using System.Text;

namespace NotesApi.Helpers;

public static class ExtensionMethods
{
    public static string ToHex(this byte[] data)
    {
        // Create a new StringBuilder to collect the bytes and create a string
        StringBuilder sBuilder = new();

        // Loop through each byte of the hashed data and format each one as a hexadecimal string
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string
        return sBuilder.ToString();
    }
}