using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.Win32;
using VisionTest.Annotations;

namespace VisionTest
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		private bool _isCameraRunning;
		private string _imageUrl;
		private readonly WebCam _webCamera;
		public string FilePath { get; set; }

		public MainWindow()
		{
			InitializeComponent();

			if (ApiKeysIncorrect())
			{
				MessageBox.Show(
					"You seem to be missing an API-key. Check your API-key settings in App.config...",
					"API-keys",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
				Application.Current.Shutdown();
			}

			SetupWebcamImageDirectory();

			SpeechRecognitionViewModel = new SpeechRecognitionViewModel();
			
			_webCamera = new WebCam();
			_webCamera.InitializeWebCam(ref WebCameraImage);

			DataContext = this;
		}

		private static bool ApiKeysIncorrect()
		{
			return
				string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("VisionApiKey"))
				|| string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("EmotionApiKey"))
				|| string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("FaceApiKey"))
				|| string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SpeechRecognitionApiKey"));

		}

		private void SetupWebcamImageDirectory()
		{
			var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? @"C:\";

			FilePath = Path.Combine(
				directory,
				"WebCameraImages");

			if (!Directory.Exists(FilePath))
			{
				Directory.CreateDirectory(FilePath);
			}
		}

		public SpeechRecognitionViewModel SpeechRecognitionViewModel { get; set; }

		public bool IsCameraRunning
		{
			get { return _isCameraRunning; }
			set
			{
				if (value == _isCameraRunning) return;
				_isCameraRunning = value;
				OnPropertyChanged();
			}
		}

		public string ImageUrl
		{
			get { return _imageUrl; }
			set
			{
				if (value == _imageUrl) return;
				_imageUrl = value;
				OnPropertyChanged();
			}
		}

		private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
		{
			_webCamera.Start();

			IsCameraRunning = true;
		}

		private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
		{
			_webCamera.Stop();

			WebCameraImage.Source = null;

			IsCameraRunning = false;
		}

		private void VisionSnapshotButton_Click(object sender, RoutedEventArgs e)
		{
			var filePath = TakeSnapshot();

			GetVisionResultForImage(filePath);
		}

		private void VisionBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}

			var filePath = openFileDialog.FileName;

			GetVisionResultForImage(filePath);
		}

		private async void GetVisionResultForImage(string filePath)
		{
			var fileStream = File.OpenRead(filePath);

			var visionServiceClient = CreateVisionServiceClient();

			var visualFeatures = new[]
			{
				VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces,
				VisualFeature.ImageType, VisualFeature.Tags
			};

			var result = await visionServiceClient.AnalyzeImageAsync(fileStream, visualFeatures);

			fileStream.Dispose();

			PresentVisionResult(result, filePath);
		}

		private async void VisionUrlButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(ImageUrl))
			{
				return;
			}

			var visionServiceClient = CreateVisionServiceClient();

			var visualFeatures = new[]
			{
				VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces,
				VisualFeature.ImageType, VisualFeature.Tags
			};

			var result = await visionServiceClient.AnalyzeImageAsync(ImageUrl, visualFeatures);

			PresentVisionResult(result, ImageUrl);
		}

		private static VisionServiceClient CreateVisionServiceClient()
		{
			var visionServiceClient = new VisionServiceClient(
				ConfigurationManager.AppSettings.Get("VisionApiKey"), 
				ConfigurationManager.AppSettings.Get("VisionApiEndpoint"));
			return visionServiceClient;
		}

		private void PresentVisionResult(AnalysisResult result, string imageUri)
		{
			var attributes = new List<string> {result.Description.Captions.First().Text};

			var resultWindow = new ResultWindow
			{
				InputImage = new BitmapImage(new Uri(imageUri)),
				Attributes = attributes,
				Owner = this
			};
			resultWindow.ShowDialog();
			resultWindow.InputImage = null;
		}

		private void FaceApiButton_Click(object sender, RoutedEventArgs e)
		{
			var filePath = TakeSnapshot();

			GetFaceResultForImage(filePath);
		}

		private void FaceBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}

			var filePath = openFileDialog.FileName;

			GetFaceResultForImage(filePath);
		}

		private async void GetFaceResultForImage(string filePath)
		{
			var fileStream = File.OpenRead(filePath);

			var faceServiceClient = new FaceServiceClient(
				ConfigurationManager.AppSettings.Get("FaceApiKey"), 
				ConfigurationManager.AppSettings.Get("FaceApiEndpoint"));

			var result = await faceServiceClient.DetectAsync(
				fileStream,
				true,
				true,
				new List<FaceAttributeType>
				{
					FaceAttributeType.Accessories,
					FaceAttributeType.Age,
					FaceAttributeType.Blur,
					FaceAttributeType.Emotion,
					FaceAttributeType.Exposure,
					FaceAttributeType.FacialHair,
					FaceAttributeType.Gender,
					FaceAttributeType.Glasses,
					FaceAttributeType.Hair,
					FaceAttributeType.HeadPose,
					FaceAttributeType.Makeup,
					FaceAttributeType.Noise,
					FaceAttributeType.Occlusion,
					FaceAttributeType.Smile
				});

			var attributes = new List<string>();

			foreach (var face in result)
			{
				attributes.Add($"Age: {face.FaceAttributes.Age}");
				attributes.Add($"Gender: {face.FaceAttributes.Gender}");
				attributes.Add($"Glasses: {face.FaceAttributes.Glasses.ToString()}");
				attributes.Add($"Beard: {face.FaceAttributes.FacialHair.Beard}");
				attributes.Add($"Moustache: {face.FaceAttributes.FacialHair.Moustache}");
				attributes.Add($"Sideburns: {face.FaceAttributes.FacialHair.Sideburns}");
			}

			var resultWindow = new ResultWindow
			{
				InputImage = new BitmapImage(new Uri(filePath)),
				Attributes = attributes,
				Owner = this
			};
			resultWindow.ShowDialog();

			fileStream.Dispose();
		}

		private void EmotionApiButton_Click(object sender, RoutedEventArgs e)
		{
			var filePath = TakeSnapshot();

			GetEmotionResultForImage(filePath);
		}

		private void EmotionBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}

			var filePath = openFileDialog.FileName;

			GetEmotionResultForImage(filePath);
		}

		private async void GetEmotionResultForImage(string filePath)
		{
			var fileStream = File.OpenRead(filePath);

			var emotionServiceClient = new EmotionServiceClient(
				ConfigurationManager.AppSettings.Get("EmotionApiKey"), 
				ConfigurationManager.AppSettings.Get("EmotionApiEndpoint"));

			var result = await emotionServiceClient.RecognizeAsync(fileStream);

			var emotions = new List<string>();
			foreach (var emotion in result)
			{
				var keyValuePairs = emotion.Scores.ToRankedList();
				var activeEmotions = keyValuePairs.Where(x => x.Value > 0.01).OrderByDescending(x => x.Value);

				foreach (var activeEmotion in activeEmotions)
				{
					var emotionInPercent = (activeEmotion.Value * 100).ToString("#0.##");
					emotions.Add($"{activeEmotion.Key} {emotionInPercent}%");
				}
			}

			var resultWindow = new ResultWindow
			{
				InputImage = new BitmapImage(new Uri(filePath)),
				Attributes = emotions,
				Owner = this
			};
			resultWindow.ShowDialog();

			fileStream.Dispose();
		}

		private string TakeSnapshot()
		{
			var filePath = Path.Combine(
				FilePath,
				DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss") + ".png");
			
			var image = WebCameraImage.Source;
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));
			
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				encoder.Save(stream);
			}

			return filePath;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			SpeechRecognitionViewModel.Dispose();
		}
	}
}