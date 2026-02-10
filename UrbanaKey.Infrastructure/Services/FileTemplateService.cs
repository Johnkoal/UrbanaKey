using System;
using System.Collections.Generic;
using System.IO;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class FileTemplateService : ITemplateService
{
    public string GetTemplate(string templateName, Dictionary<string, string> placeholders)
    {
        // Assumes templates are in "Templates" folder in the base directory
        var path = Path.Combine(AppContext.BaseDirectory, "Templates", $"{templateName}.html");
        if (!File.Exists(path)) return string.Empty;

        var content = File.ReadAllText(path);
        foreach (var item in placeholders)
        {
            content = content.Replace($"{{{{{item.Key}}}}}", item.Value);
        }
        return content;
    }
}
