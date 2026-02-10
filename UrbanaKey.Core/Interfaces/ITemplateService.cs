using System.Collections.Generic;

namespace UrbanaKey.Core.Interfaces;

public interface ITemplateService
{
    string GetTemplate(string templateName, Dictionary<string, string> placeholders);
}
