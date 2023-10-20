using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cascadiumtool;
internal static class PathUtils
{
    private const double UnitKb = 1024;
    private const double UnitMb = UnitKb * 1024;
    private const double UnitGb = UnitMb * 1024;

    public static string FileSize(long len)
    {
        return (double)len switch
        {
            (> 0) and (< UnitKb) => $"{Math.Round((double)len, 2)} bytes",
            (>= UnitKb) and (< UnitMb) => $"{Math.Round(len / UnitKb, 2)} kb",
            (>= UnitMb) and (< UnitGb) => $"{Math.Round(len / UnitMb, 2)} mb",
            (>= UnitGb) => $"{Math.Round(len / UnitGb, 2)} gb",
            _ => $"{len} bytes"
        };
    }
    
    public static void EnsureExistence(string directoryPath)
    {
        string fullPath = ResolvePath(directoryPath);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);
    }

    public static string ResolvePath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        } else
        {
            return NormalizedCombine(Program.CurrentDirectory, path);
        }
    }

    /// <summary>
    /// Combines strings into a normalized path by the running environment.
    /// </summary>
    /// <param name="paths">An array of parts of the path.</param>
    /// <returns>The combined and normalized paths.</returns>
    public static string NormalizedCombine(params string[] paths)
    {
        if (paths.Length == 0) return "";

        bool startsWithSepChar = paths[0].StartsWith("/") || paths[0].StartsWith("\\");
        char environmentPathChar = Path.DirectorySeparatorChar;
        List<string> tokens = new List<string>();

        for (int ip = 0; ip < paths.Length; ip++)
        {
            string path = paths[ip]
                ?? throw new ArgumentNullException($"The path string at index {ip} is null.");

            string normalizedPath = path
                .Replace('/', environmentPathChar)
                .Replace('\\', environmentPathChar)
                .Trim(environmentPathChar);

            string[] pathIdentities = normalizedPath.Split(
                environmentPathChar,
                StringSplitOptions.RemoveEmptyEntries
            );

            tokens.AddRange(pathIdentities);
        }

        Stack<int> insertedIndexes = new Stack<int>();
        StringBuilder pathBuilder = new StringBuilder();
        foreach (string token in tokens)
        {
            if (token == ".")
            {
                continue;
            }
            else if (token == "..")
            {
                pathBuilder.Length = insertedIndexes.Pop();
            }
            else
            {
                insertedIndexes.Push(pathBuilder.Length);
                pathBuilder.Append(token);
                pathBuilder.Append(environmentPathChar);
            }
        }

        if (startsWithSepChar)
            pathBuilder.Insert(0, environmentPathChar);

        return pathBuilder.ToString().TrimEnd(environmentPathChar);
    }
}
