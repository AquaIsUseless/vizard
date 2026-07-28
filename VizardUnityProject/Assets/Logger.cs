using NetMQ;
using System;
using UnityEngine;

public class MessageLogger
{
    private DateTime? _lastMessageTime;

    public void LogReceived(string messageType, NetMQMessage message)
    {
        DateTime now = DateTime.Now;

        TimeSpan? delta = _lastMessageTime.HasValue
            ? now - _lastMessageTime.Value
            : (TimeSpan?)null;

        _lastMessageTime = now;

        Debug.Log(
            $"[{now:HH:mm:ss.fff}] " +
            $"(+{(delta?.TotalMilliseconds.ToString("F1") ?? "N/A")} ms) " +
            $"RECV {messageType} ({message.FrameCount} frames)"
        );
    }

    public void LogSent(string messageType)
    {
        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] SENT {messageType}");
    }

    public void Log(string text)
    {
        Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] {text}");
    }
}