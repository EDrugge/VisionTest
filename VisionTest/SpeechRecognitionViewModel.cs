using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CognitiveServices.SpeechRecognition;
using VisionTest.Annotations;

namespace VisionTest
{
	public class SpeechRecognitionViewModel : INotifyPropertyChanged, IDisposable
	{
		private string _recognizedText;
		private bool _isListening;
		private bool _isShowingCatWindow;
		private readonly MicrophoneRecognitionClient _microphoneRecognitionClient;

		public SpeechRecognitionViewModel()
		{
			StartListeningCommand = new RelayCommand(StartListening, () => !IsListening);
			StopListeningCommand = new RelayCommand(StopListening, () => IsListening);

			_microphoneRecognitionClient = SpeechRecognitionServiceFactory
				.CreateMicrophoneClient(
					SpeechRecognitionMode.LongDictation,
					"en-US",
					ConfigurationManager.AppSettings.Get("SpeechRecognitionApiKey"));

			_microphoneRecognitionClient.OnPartialResponseReceived += MicrophoneRecognitionClientOnOnPartialResponseReceived;
			_microphoneRecognitionClient.OnResponseReceived += MicrophoneRecognitionClientOnOnResponseReceived;
		}

		private void StartListening()
		{
			_microphoneRecognitionClient.StartMicAndRecognition();

			IsListening = true;
		}

		private void MicrophoneRecognitionClientOnOnPartialResponseReceived(
			object sender,
			PartialSpeechResponseEventArgs partialSpeechResponseEventArgs)
		{
			RecognizedText = partialSpeechResponseEventArgs.PartialResult;
		}

		private void MicrophoneRecognitionClientOnOnResponseReceived(
			object sender,
			SpeechResponseEventArgs speechResponseEventArgs)
		{
			RecognizedText = speechResponseEventArgs
				.PhraseResponse
				.Results
				.Aggregate("", (s, phrase) => s += $"{phrase.DisplayText} ");
		}


		private void StopListening()
		{
			_microphoneRecognitionClient.EndMicAndRecognition();

			IsListening = false;
		}

		public bool IsListening
		{
			get => _isListening;
			set
			{
				if (value == _isListening) return;
				_isListening = value;
				OnPropertyChanged();
			}
		}

		public string RecognizedText
		{
			get => _recognizedText;
			set
			{
				if (value == _recognizedText) return;
				_recognizedText = value;

				if (!_isShowingCatWindow && value.Contains("cat"))
				{
					_isShowingCatWindow = true;
					Task.Run(() =>
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							var catWindow = new CatWindow
							{
								Owner = Application.Current.MainWindow
							};
							catWindow.ShowDialog();
							_isShowingCatWindow = false;
						});
					});
				}

				OnPropertyChanged();
			}
		}

		public RelayCommand StartListeningCommand { get; set; }
		public RelayCommand StopListeningCommand { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Dispose()
		{
			_microphoneRecognitionClient.OnPartialResponseReceived -= MicrophoneRecognitionClientOnOnPartialResponseReceived;
			_microphoneRecognitionClient.OnResponseReceived -= MicrophoneRecognitionClientOnOnResponseReceived;
			_microphoneRecognitionClient.Dispose();

			_microphoneRecognitionClient?.Dispose();
		}
	}
}