﻿namespace MiroslavGPT.Domain.Models.Threads;

public record MessageThread
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public List<ThreadMessage> Messages { get; set; } = new();
}