using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VisionTest.Annotations;

namespace VisionTest
{
	public partial class CatWindow : INotifyPropertyChanged
	{
		private string _imageSource;

		public CatWindow()
		{
			InitializeComponent();

			ImageSource = "https://thecatapi.com/api/images/get";

			DataContext = this;
		}

		public string ImageSource
		{
			get => _imageSource;
			set
			{
				if (value == _imageSource) return;
				_imageSource = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			Close();
		}
	}
}
