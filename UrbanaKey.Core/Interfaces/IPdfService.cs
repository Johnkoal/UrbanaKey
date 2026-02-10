using System.Collections.Generic;
using UrbanaKey.Core.Domain;

namespace UrbanaKey.Core.Interfaces;

public interface IPdfService
{
    byte[] GenerateAssemblyReport(Assembly assembly, List<Vote> votes);
}
