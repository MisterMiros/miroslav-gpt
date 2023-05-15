using MiroslavGPT.Model.Personalities;

namespace MiroslavGPT.Admin.API.Models.Personalities;

public record ApiPersonalityMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }

    public static ApiPersonalityMessage From(PersonalityMessage message)
    {
        return new()
        {
            Text = message.Text,
            IsAssistant = message.IsAssistant
        };
    }
}