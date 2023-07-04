using System.Text;
using System.Text.RegularExpressions;

namespace MiroslavGPT.Domain.Extensions;

public static class StringExtensions
{
    private static readonly Regex Usernames = new(@"@\w{5,64}", RegexOptions.Compiled, TimeSpan.FromSeconds(3));
    private static readonly Regex SpecialMarkdownCharacters = new(@"([*_{}\[\]()#+-.!])", RegexOptions.Compiled, TimeSpan.FromSeconds(3));

    public static string EscapeUsernames(this string markdown)
    {
        var stringBuilder = new StringBuilder(markdown);
        var usernames = Usernames.Matches(markdown).Select(m => m.Groups[0].ToString());
        var escapedUsernames = usernames.Select(u => (original: u, escaped: SpecialMarkdownCharacters.Replace(u, @"\$1")));

        stringBuilder = escapedUsernames.Aggregate(stringBuilder, (current, username) => current.Replace(username.original, username.escaped));

        return stringBuilder.ToString();
    }
}