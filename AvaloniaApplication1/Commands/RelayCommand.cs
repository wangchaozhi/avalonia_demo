﻿// using System;
// using System.Windows.Input;
//
// namespace AvaloniaApplication1.Commands
// {
//     // 泛型版本的 RelayCommand
//     public class RelayCommand<T> : ICommand
//     {
//         private readonly Action<T> _execute;
//         private readonly Func<T, bool> _canExecute;
//
//         public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
//         {
//             _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//             _canExecute = canExecute;
//         }
//
//         public event EventHandler CanExecuteChanged;
//
//         public bool CanExecute(object parameter)
//         {
//             return _canExecute == null || _canExecute((T)parameter);
//         }
//
//         public void Execute(object parameter)
//         {
//             _execute((T)parameter);
//         }
//
//         public void RaiseCanExecuteChanged()
//         {
//             CanExecuteChanged?.Invoke(this, EventArgs.Empty);
//         }
//     }
//
//     // 无参数的专用版本（非泛型）
//     public class RelayCommand : ICommand
//     {
//         private readonly Action _execute;
//         private readonly Func<bool> _canExecute;
//
//         public RelayCommand(Action execute, Func<bool> canExecute = null)
//         {
//             _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//             _canExecute = canExecute;
//         }
//
//         public event EventHandler CanExecuteChanged;
//
//         public bool CanExecute(object parameter)
//         {
//             return _canExecute == null || _canExecute();
//         }
//
//         public void Execute(object parameter)
//         {
//             _execute();
//         }
//
//         public void RaiseCanExecuteChanged()
//         {
//             CanExecuteChanged?.Invoke(this, EventArgs.Empty);
//         }
//     }
// }