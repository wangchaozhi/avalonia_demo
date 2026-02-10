/*
 Author: wangchaozhi
 Date: 2026/02/10
*/

using System;
using System.Collections.Concurrent;
using Avalonia.Input;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Service
{
    public static class TabDockDragDrop
    {
        // Only ASCII letters/digits/dot/hyphen are allowed.
        public static readonly DataFormat<string> TabTokenFormat =
            DataFormat.CreateStringApplicationFormat("AvaloniaApplication1.TabToken");

        private static readonly ConcurrentDictionary<string, TabViewModel> TabsByToken = new();

        public static string Register(TabViewModel tab)
        {
            var token = Guid.NewGuid().ToString("N");
            TabsByToken[token] = tab;
            return token;
        }

        public static TabViewModel? Resolve(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return TabsByToken.TryGetValue(token, out var tab) ? tab : null;
        }

        public static void Unregister(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            TabsByToken.TryRemove(token, out _);
        }
    }
}

