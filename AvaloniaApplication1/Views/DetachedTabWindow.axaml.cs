/*
 Author: wangchaozhi
 Date: 2026/02/10
*/

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.Service;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views
{
    public partial class DetachedTabWindow : BorderlessDraggableWindow
    {
        private readonly MainWindowViewModel _mainWindowViewModel = null!;
        private readonly TabViewModel _tab = null!;
        private ContentControl _contentHost = null!;
        private object? _content;
        private bool _suppressDockOnClose;

        private Point? _dragStartPoint;
        private bool _dragInProgress;

        public DetachedTabWindow()
        {
            InitializeComponent();
        }

        public DetachedTabWindow(TabViewModel tab, MainWindowViewModel mainWindowViewModel)
            : this()
        {
            _tab = tab;
            _mainWindowViewModel = mainWindowViewModel;

            // Take ownership of the content while detached to avoid "already has a visual parent".
            _content = tab.Content;
            tab.Content = null;

            DataContext = tab;
            // 无边框窗口不显示标题栏文字，这里保持为空避免出现“一大坨标签名”
            Title = string.Empty;
            _contentHost.Content = _content;

            Closing += (_, e) =>
            {
                // 用户通过 Alt+F4 等方式关闭时，也视为“回到标签页”
                if (_suppressDockOnClose)
                    return;

                if (_content is null)
                    return;

                _suppressDockOnClose = true;
                e.Cancel = true;
                DockBack();
            };

            Closed += (_, _) =>
            {
                // Avoid stale references if user closes window directly.
                if (_tab.DetachedWindow == this)
                    _tab.DetachedWindow = null;
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _contentHost = this.FindControl<ContentControl>("ContentHost")!;
        }

        public object? ReleaseContent()
        {
            var content = _content;
            _content = null;
            _contentHost.Content = null;
            return content;
        }

        public void CloseWithoutDock()
        {
            _suppressDockOnClose = true;
            Close();
        }

        protected override void OnCloseButtonInvoked()
        {
            // 点击继承的右上角 X：回到标签页（不真正关闭 Tab）
            DockBack();
        }

        private void DockBack()
        {
            // Release visual before putting it back into TabControl.
            _tab.Content = ReleaseContent();

            if (!_mainWindowViewModel.Tabs.Contains(_tab))
                _mainWindowViewModel.Tabs.Add(_tab);

            _mainWindowViewModel.SelectedTabIndex = _mainWindowViewModel.Tabs.IndexOf(_tab);
            _tab.DetachedWindow = null;
            _suppressDockOnClose = true;
            Close();
        }

        private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            // 设计目标：
            // - 默认行为：像系统标题栏一样拖动窗口（由基类 BorderlessDraggableWindow.BeginMoveDrag 处理）
            // - Ctrl + 拖动：触发 DragDrop，把页签拖回主窗口进行停靠
            if ((e.KeyModifiers & KeyModifiers.Control) == 0)
                return;

            _dragStartPoint = e.GetPosition(this);
            _dragInProgress = false;
            // Ctrl 模式下我们需要拖拽数据；必须拦截，否则基类会先 BeginMoveDrag 导致无法触发 DragDrop
            e.Handled = true;
        }

        private async void OnHeaderPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_dragInProgress)
                return;

            if (_dragStartPoint is null)
                return;

            var pt = e.GetCurrentPoint(this);
            if (!pt.Properties.IsLeftButtonPressed)
                return;

            var pos = e.GetPosition(this);
            var delta = pos - _dragStartPoint.Value;
            if (Math.Abs(delta.X) < 6 && Math.Abs(delta.Y) < 6)
                return;

            _dragInProgress = true;
            e.Handled = true;

            var token = TabDockDragDrop.Register(_tab);
            using var dataTransfer = new DataTransfer();
            dataTransfer.Add(DataTransferItem.Create(TabDockDragDrop.TabTokenFormat, token));

            // Dragging from detached window; if user drops back onto main tab area, it will dock there.
            var result = await DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Move);
            TabDockDragDrop.Unregister(token);

            // If dropped nowhere, keep detached.
            _dragStartPoint = null;
            _dragInProgress = false;
        }
    }
}

