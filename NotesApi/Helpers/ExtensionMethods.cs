using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using NotesApi.Models;

namespace NotesApi.Helpers;

public static class ExtensionMethods
{
    public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj);

    public static T? FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json);

    public static ProblemDetails ToProblemDetails(this ServiceException exception)
    {
        return new ProblemDetails
        {
            Status = exception.StatusCode ?? (int?)ServiceErrors.ServerError.StatusCode,
            Title = exception.Title ?? ServiceErrors.ServerError.Title,
            Detail = exception.Message
        };
    }

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

    public static bool IsSearchItemPresent(this Note note, string? searchItem)
    {
        // return string.IsNullOrWhiteSpace(searchItem) || 
        //     note.Title.Contains(searchItem?.Trim() ?? "") || 
        //     searchItem?.Trim()?.Contains(note.Title) == true;

        if (string.IsNullOrWhiteSpace(searchItem)) return true;
        return note.Title == searchItem;
    }

    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}