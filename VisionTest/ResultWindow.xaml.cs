using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using VisionTest.Annotations;

namespace VisionTest
{
	public partial class ResultWindow : INotifyPropertyChanged
	{
		private BitmapImage _inputImage;
		private List<string> _attributes;

		public ResultWindow()
		{
			InitializeComponent();

			this.DataContext = this;
		}

		public BitmapImage InputImage
		{
			get { return _inputImage; }
			set
			{
				if (Equals(value, _inputImage)) return;
				_inputImage = value;
				OnPropertyChanged();
			}
		}

		public List<string> Attributes
		{
			get { return _attributes; }
			set
			{
				if (Equals(value, _attributes)) return;
				_attributes = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
