/*
 Author: wangchaozhi
 Date: 2026/02/10
*/
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using AvaloniaApplication1.Service;
using AvaloniaApplication1.Views;

namespace AvaloniaApplication1.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _consoleOutput = "Console initialized.\n";

        [ObservableProperty]
        private ObservableCollection<TabViewModel> _tabs = new ObservableCollection<TabViewModel>();

        [ObservableProperty]
        private int _selectedTabIndex = 0;

        private readonly ToolBarControlModel _toolBarControlModel;

        // 同一个“菜单/功能入口”只对应一个 Tab（即使 Tab 被拖拽分离）
        private readonly Dictionary<string, TabViewModel> _tabsByKey = new(StringComparer.Ordinal);

        public MainWindowViewModel()
        {
            _toolBarControlModel = new ToolBarControlModel(this);
            // 默认展示菜单（工具栏）第一个页面：音乐
            AddMusicTab();
        }

        public ToolBarControlModel ToolBarControlModel => _toolBarControlModel;

        internal void AppendConsoleOutput(string message)
        {
            ConsoleOutput += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            Log.Information("Console: {Message}", message);
        }

        internal void CloseTab(TabViewModel tab)
        {
            // 如果该 Tab 正在分离窗口中，关闭窗口但不要触发“自动回到标签页”
            if (tab.DetachedWindow is DetachedTabWindow detached)
            {
                detached.CloseWithoutDock();
                tab.DetachedWindow = null;
            }
            else if (tab.DetachedWindow is Window w)
            {
                w.Close();
                tab.DetachedWindow = null;
            }

            Tabs.Remove(tab);

            if (_tabsByKey.TryGetValue(tab.Key, out var current) && ReferenceEquals(current, tab))
                _tabsByKey.Remove(tab.Key);

            AppendConsoleOutput($"Closed tab: {tab.Header}");
        }

        private void ActivateTab(TabViewModel tab)
        {
            // 如果 Tab 已分离：置顶分离窗口
            if (tab.DetachedWindow is Window w)
            {
                WindowActivationService.Activate(w);
                return;
            }

            if (!Tabs.Contains(tab))
                Tabs.Add(tab);

            SelectedTabIndex = Tabs.IndexOf(tab);
        }

        public void AddMusicTab()
        {
            const string key = "music";

            if (_tabsByKey.TryGetValue(key, out var existingTab))
            {
                ActivateTab(existingTab);
                AppendConsoleOutput($"Activated existing tab: {existingTab.Header}");
                return;
            }

            var musicViewModel = new MusicPageViewModel(this);
            var musicControl = new MusicPageControl { DataContext = musicViewModel };
            var tab = new TabViewModel(this, key)
            {
                Header = "音乐",
                Content = musicControl
            };

            _tabsByKey[key] = tab;
            Tabs.Add(tab);
            SelectedTabIndex = Tabs.Count - 1;
            AppendConsoleOutput($"Opened tab: {tab.Header}");
        }
    }
}