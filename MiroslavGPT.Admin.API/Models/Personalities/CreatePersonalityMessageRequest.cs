﻿namespace MiroslavGPT.Admin.API.Models.Personalities;

public record CreatePersonalityMessageRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsAssistant { get; set; }
}