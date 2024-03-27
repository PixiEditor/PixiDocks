using System.Collections.Generic;
using System.IO;

namespace HexEditor.Models;

public class Document
{
    public string FilePath { get; set; }
    public string FileName => Path.GetFileName(FilePath);
    public byte[] Data { get; set; }

    public Document(string filePath)
    {
        FilePath = filePath;
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
        {
            return;
        }

        Data = File.ReadAllBytes(FilePath);
    }

    public List<string> ToLines()
    {
        var lines = new List<string>();
        for (var i = 0; i < Data.Length; i += 16)
        {
            var line = string.Empty;
            for (var j = 0; j < 16; j++)
            {
                if (i + j < Data.Length)
                {
                    line += $"{Data[i + j]:X2} ";
                }
                else
                {
                    line += "   ";
                }
            }

            lines.Add(line);
        }

        return lines;
    }
}