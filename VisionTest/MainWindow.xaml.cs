using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Expression.Encoder.Devices;
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
		public Collection<EncoderDevice> VideoDevices { get; set; }
		public EncoderDevice VideoDevice { get; set; }
		public Collection<EncoderDevice> AudioDevices { get; set; }
		public EncoderDevice AudioDevice { get; set; }
		public string FilePath => "C:\\WebcamSnapshots";

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = this;

			VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);
			VideoDevice = EncoderDevices.FindDevices(EncoderDeviceType.Video).Single(x => x.Name.Contains("Integrated"));

			AudioDevices = EncoderDevices.FindDevices(EncoderDeviceType.Audio);
			AudioDevice = EncoderDevices.FindDevices(EncoderDeviceType.Audio).Single(x => x.Name.Contains("Intern"));
		}

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
			try
			{
				// Display webcam video
				WebcamViewer.StartPreview();
				IsCameraRunning = true;
			}
			catch (Microsoft.Expression.Encoder.SystemErrorException ex)
			{
				MessageBox.Show("Device is in use by another application");
			}
		}

		private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
		{
			// Stop the display of webcam video.
			WebcamViewer.StopPreview();
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

			var visualFeatures = new[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };

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

			var visualFeatures = new[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };

			var result = await visionServiceClient.AnalyzeImageAsync(ImageUrl, visualFeatures);

			PresentVisionResult(result, ImageUrl);
		}

		private static VisionServiceClient CreateVisionServiceClient()
		{
			var endPoint = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0";
			var subscriptionKey = "cf6f42df8d3e4cc3b7ff65a10e1bb15d";

			var visionServiceClient = new VisionServiceClient(subscriptionKey, endPoint);
			return visionServiceClient;
		}

		private void PresentVisionResult(AnalysisResult result, string imageUri)
		{
			var attributes = new List<string> { result.Description.Captions.First().Text };

			var resultWindow = new ResultWindow
			{
				InputImage = new BitmapImage(new Uri(imageUri)),
				Attributes = attributes
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

			var endPoint = "https://westeurope.api.cognitive.microsoft.com/face/v1.0";
			var subscriptionKey = "c0b87cbb9685463d8311928015e2b29f";

			var faceServiceClient = new FaceServiceClient(subscriptionKey, endPoint);

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
				Attributes = attributes
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

			var endPoint = "https://westus.api.cognitive.microsoft.com/emotion/v1.0";
			var subscriptionKey = "001a99ad90424d27a3dc9a44ba88feea";

			var emotionServiceClient = new EmotionServiceClient(subscriptionKey, endPoint);

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
				Attributes = emotions
			};
			resultWindow.ShowDialog();

			fileStream.Dispose();
		}

		private string TakeSnapshot()
		{
			var fileName = DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss");

			WebcamViewer.TakeSnapshot(fileName);

			return Path.Combine(FilePath, fileName + ".Jpeg");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
