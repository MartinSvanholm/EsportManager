using System.Collections.Frozen;
using EsportManager.Resources.Strings;

namespace EsportManager.Features.Process.RobocopyProcess.Models;

public static class RobocopyErrors
{
    public record RobocopyError(string Message, bool ShouldBreak = false);

    public static readonly FrozenDictionary<int, RobocopyError> Errors = new Dictionary<int, RobocopyError>
    {
        [53] = new(AppStrings.RobocopyErrorPathNotFound, ShouldBreak: true),
        [1326] = new(AppStrings.RobocopyErrorIncorrectCredentials, ShouldBreak: true),
    }.ToFrozenDictionary();
}
