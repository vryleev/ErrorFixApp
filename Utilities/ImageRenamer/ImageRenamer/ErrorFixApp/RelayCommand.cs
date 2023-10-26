using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ErrorFixApp
{
    
    public class RelayCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void RefreshCommand()
        {
            var cec = CanExecuteChanged;
            if (cec != null)
                cec(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : base(_ => execute(),
                _ => canExecute == null || canExecute())
        {

        }
    }
    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    // public class RelayCommand : ICommand
    // {
    //     #region Fields
    //
    //     readonly Action<object> _execute;
    //     readonly Predicate<object> _canExecute;        
    //
    //     #endregion // Fields
    //
    //     #region Constructors
    //
    //     /// <summary>
    //     /// Creates a new command.
    //     /// </summary>
    //     /// <param name="execute">The execution logic.</param>
    //     /// <param name="canExecute">The execution status logic.</param>
    //     public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    //     {
    //         _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    //         _canExecute = canExecute;           
    //     }
    //
    //     #endregion // Constructors
    //
    //     #region ICommand Members
    //
    //     [DebuggerStepThrough]
    //     public bool CanExecute(object parameters)
    //     {
    //         return _canExecute == null || _canExecute(parameters);
    //     }
    //
    //     public event EventHandler CanExecuteChanged
    //     {
    //         add => CommandManager.RequerySuggested += value;
    //         remove => CommandManager.RequerySuggested -= value;
    //     }
    //
    //     public void Execute(object parameters)
    //     {
    //         _execute(parameters);
    //     }
    //
    //     #endregion // ICommand Members
    // }
}