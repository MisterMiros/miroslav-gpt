using System.Text;
using System.Text.RegularExpressions;

namespace MiroslavGPT.Domain.Extensions;

public static class StringExtensions
{
    private static readonly Regex SpecialMarkdownCharacters = new("([*_{}\\[\\]()#+-.!])", RegexOptions.Compiled);

    public static string EscapeUsernames(this string markdown, IEnumerable<string> usernames)
    {
        var stringBuilder = new StringBuilder(markdown);
        var escapedUsernames = usernames.Select(u => (original: u, escaped: SpecialMarkdownCharacters.Replace(u, "\\$1")));

        stringBuilder = escapedUsernames.Aggregate(stringBuilder, (current, username) => current.Replace(username.original, username.escaped));

        return stringBuilder.ToString();
    }
}