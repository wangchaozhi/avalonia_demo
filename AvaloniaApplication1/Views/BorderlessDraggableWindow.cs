/*
 Author: wangchaozhi
 Date: 2026/02/10
*/

using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Platform;

namespace AvaloniaApplication1.Views
{
    /// <summary>
    /// 无边框、可拖拽的窗口基类。
    /// - 默认禁用系统边框（SystemDecorations=None）
    /// - 启用 ExtendClientArea（NoChrome）
    /// - 鼠标按下即可拖动窗口（BeginMoveDrag）
    /// </summary>
    public abstract class BorderlessDraggableWindow : BaseWindow
    {
        public static readonly StyledProperty<bool> ShowSystemCloseButtonProperty =
            AvaloniaProperty.Register<BorderlessDraggableWindow, bool>(nameof(ShowSystemCloseButton), true);

        public static readonly DirectProperty<BorderlessDraggableWindow, ICommand> CloseWindowCommandProperty =
            AvaloniaProperty.RegisterDirect<BorderlessDraggableWindow, ICommand>(
                nameof(CloseWindowCommand),
                o => o.CloseWindowCommand);

        private readonly ICommand _closeWindowCommand;

        protected BorderlessDraggableWindow()
        {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            // 固定一个可预期的标题栏高度，避免在部分 Windows 组合上出现“顶部黑条/未绘制区域”
            ExtendClientAreaTitleBarHeightHint = 36;
            SystemDecorations = SystemDecorations.None;

            _closeWindowCommand = new ActionCommand(OnCloseButtonInvoked);

            // 整个窗口任意区域按下拖拽（如需只拖某个区域，可在派生类里覆盖/解绑）
            PointerPressed += OnWindowPointerPressed;
        }

        public bool ShowSystemCloseButton
        {
            get => GetValue(ShowSystemCloseButtonProperty);
            set => SetValue(ShowSystemCloseButtonProperty, value);
        }

        public ICommand CloseWindowCommand => _closeWindowCommand;

        /// <summary>
        /// 关闭按钮被点击时的默认行为：关闭窗口。
        /// 派生类可重写实现“点击 X 仅回到标签页”等行为。
        /// </summary>
        protected virtual void OnCloseButtonInvoked()
        {
            Close();
        }

        private static bool IsInInteractiveControl(Control? c)
        {
            while (c is not null)
            {
                if (c is Button ||
                    c is TextBox ||
                    c is ToggleButton ||
                    c is Slider ||
                    c is ComboBox ||
                    c is SelectingItemsControl)
                    return true;

                c = c.Parent as Control;
            }

            return false;
        }

        protected virtual void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse)
            {
                if (IsInInteractiveControl(e.Source as Control))
                    return;

                var pt = e.GetCurrentPoint(this);
                if (pt.Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            }
        }

        private sealed class ActionCommand : ICommand
        {
            private readonly Action _execute;

            public ActionCommand(Action execute) => _execute = execute;

            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter) => _execute();
        }
    }
}

