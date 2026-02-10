using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.Service;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

public partial class MainContentControl : UserControl
{
    private TabControl? _tabsHost;
    private Point? _dragStartPoint;
    private TabViewModel? _dragTab;
    private bool _dragInProgress;

    public MainContentControl()
    {
        InitializeComponent();
        _tabsHost = this.FindControl<TabControl>("TabsHost");
    }

    private int GetInsertIndex(MainWindowViewModel vm, DragEventArgs e)
    {
        if (_tabsHost is null)
            return vm.Tabs.Count;

        var pos = e.GetPosition(_tabsHost);
        var hit = _tabsHost.InputHitTest(pos) as Control;

        while (hit is not null && hit is not TabItem)
            hit = hit.Parent as Control;

        if (hit is not TabItem tabItem)
            return vm.Tabs.Count;

        if (tabItem.DataContext is not TabViewModel targetTab)
            return vm.Tabs.Count;

        var targetIndex = vm.Tabs.IndexOf(targetTab);
        if (targetIndex < 0)
            return vm.Tabs.Count;

        var localToItem = e.GetPosition(tabItem);
        return localToItem.X > tabItem.Bounds.Width / 2 ? targetIndex + 1 : targetIndex;
    }

    private void OnTabHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control c)
            return;

        if (c.DataContext is not TabViewModel tab)
            return;

        if (e.GetCurrentPoint(c).Properties.IsLeftButtonPressed)
        {
            _dragStartPoint = e.GetPosition(c);
            _dragTab = tab;
            _dragInProgress = false;
        }
    }

    private async void OnTabHeaderPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragInProgress)
            return;

        if (_dragStartPoint is null || _dragTab is null)
            return;

        if (sender is not Control c)
            return;

        var pt = e.GetCurrentPoint(c);
        if (!pt.Properties.IsLeftButtonPressed)
            return;

        var pos = e.GetPosition(c);
        var delta = pos - _dragStartPoint.Value;
        if (Math.Abs(delta.X) < 6 && Math.Abs(delta.Y) < 6)
            return;

        _dragInProgress = true;

        var token = TabDockDragDrop.Register(_dragTab);
        using var dataTransfer = new DataTransfer();
        dataTransfer.Add(DataTransferItem.Create(TabDockDragDrop.TabTokenFormat, token));

        var result = await DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Move);

        // If user dropped nowhere, treat as detach.
        if (result == DragDropEffects.None)
        {
            DetachTab(_dragTab, e);
        }
        TabDockDragDrop.Unregister(token);

        _dragStartPoint = null;
        _dragTab = null;
        _dragInProgress = false;
    }

    private void DetachTab(TabViewModel tab, PointerEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (!vm.Tabs.Contains(tab))
            return;

        // Remove from tab strip
        var oldIndex = vm.Tabs.IndexOf(tab);
        vm.Tabs.Remove(tab);

        if (vm.Tabs.Count > 0)
        {
            vm.SelectedTabIndex = Math.Clamp(oldIndex, 0, vm.Tabs.Count - 1);
        }
        else
        {
            vm.SelectedTabIndex = -1;
        }

        // Open detached window
        var window = new DetachedTabWindow(tab, vm);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not null)
        {
            var screenPoint = topLevel.PointToScreen(e.GetPosition(topLevel));
            window.Position = new PixelPoint(Math.Max(0, screenPoint.X - 40), Math.Max(0, screenPoint.Y - 15));
        }

        tab.DetachedWindow = window;
        window.Show();
    }

    private void OnTabsDragOver(object? sender, DragEventArgs e)
    {
        var token = e.DataTransfer.TryGetValue(TabDockDragDrop.TabTokenFormat);
        if (TabDockDragDrop.Resolve(token) is not null)
        {
            e.DragEffects = DragDropEffects.Move;
            e.Handled = true;
        }
    }

    private void OnTabsDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        var token = e.DataTransfer.TryGetValue(TabDockDragDrop.TabTokenFormat);
        var tab = TabDockDragDrop.Resolve(token);
        if (tab is null)
            return;

        // If tab comes from a detached window, release the visual first to avoid double-parenting.
        if (tab.DetachedWindow is DetachedTabWindow detached)
        {
            tab.Content = detached.ReleaseContent();
            tab.DetachedWindow = null;
            detached.Close();
        }

        var insertIndex = GetInsertIndex(vm, e);
        var oldIndex = vm.Tabs.IndexOf(tab);
        if (oldIndex >= 0)
        {
            vm.Tabs.RemoveAt(oldIndex);
            if (oldIndex < insertIndex)
                insertIndex--;
        }

        insertIndex = Math.Clamp(insertIndex, 0, vm.Tabs.Count);
        vm.Tabs.Insert(insertIndex, tab);

        vm.SelectedTabIndex = insertIndex;
        TabDockDragDrop.Unregister(token);

        e.DragEffects = DragDropEffects.Move;
        e.Handled = true;
    }
}