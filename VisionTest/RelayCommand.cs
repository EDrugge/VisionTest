using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace VisionTest
{
	/// <summary>
	/// A command whose sole purpose is to 
	/// relay its functionality to other
	/// objects by invoking delegates. The
	/// default return value for the CanExecute
	/// method is 'true'.
	/// </summary>
	public class RelayCommand : RelayCommand<object>
	{
		public RelayCommand(Action execute, Func<bool> canExecute = null)
			: this(_ => execute(), canExecute == null ? default(Predicate<object>) : _ => canExecute())
		{
		}

		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
			: base(execute, canExecute)
		{
		}

		public RelayCommand(Action execute, IReadableObservable<bool> observable)
			: base(_ => execute(), observable)
		{
		}
	}

	public class RelayCommand<T> : System.Windows.Input.ICommand
	{
		readonly Action<T> _execute;
		readonly Predicate<T> _canExecute;

		/// <summary>
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
			CommandManager = WpfCommandManager.Instance;
		}

		public RelayCommand(Action<T> execute, IReadableObservable<bool> observable)
			: this(execute, _ => observable.Value)
		{
			observable.PropertyChanged += delegate { CommandManager.InvalidateRequerySuggested(); };
		}

		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			if (_canExecute != null && ReferenceEquals(BindingOperations.DisconnectedSource, parameter))
			{
				//Unable to cast object of type 'MS.Internal.NamedObject' to type 'CAB.CSP.Estimate.Client.Home.ViewItems.CabasCaseViewItem'.
				return false;
			}

			return _canExecute == null || _canExecute((T)parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter) => _execute((T)parameter);

		public ICommandManager CommandManager { get; set; }
		
	}

	internal class WpfCommandManager : ICommandManager
	{
		private WpfCommandManager() { }

		static WpfCommandManager()
		{
			Instance = new WpfCommandManager();
		}

		public static readonly WpfCommandManager Instance;

		public event EventHandler RequerySuggested
		{
			add { System.Windows.Input.CommandManager.RequerySuggested += value; }
			remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
		}

		public void InvalidateRequerySuggested()
		{
			System.Windows.Input.CommandManager.InvalidateRequerySuggested();
		}
	}

	public interface ICommandManager
	{
		event EventHandler RequerySuggested;
		void InvalidateRequerySuggested();
	}

	public interface IReadableObservable : INotifyPropertyChanged, IDataErrorInfo, IDisposable
	{
		bool IsValueChanging { get; }
		event EventHandler ValueChanging;
		event EventHandler ValueChanged;
	}

	public interface IReadableObservable<T> : IReadableObservable
	{
		T Value { get; }
	}
}
