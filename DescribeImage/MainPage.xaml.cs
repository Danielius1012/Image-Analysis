﻿using DescribeImage.Model;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TranslatorService.Speech;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DescribeImage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string SubscriptionKey = "4fd45b186142417ab8d3cf0c0ae93dd2";
        VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task GetAnalysisResult(string imageUrl)
        {
            AnalysisResult analysisResult;
            analysisResult = await AnalyzeInDomainUrl(imageUrl, visualFeatures);

            AnalysisResultLabel.Text = analysisResult.Description.Captions[0].Text;
            CreateAudioOutput(analysisResult.Description.Captions[0].Text);
        }

        private async Task<AnalysisResult> AnalyzeInDomainUrl(string imageUrl, VisualFeature[] domainModel)
        {
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageUrl, domainModel);
            return analysisResult;
        }

        private async void CreateAudioOutput(string textToSpeak)
        {


            SpeechSynthesizer speech = new SpeechSynthesizer("Image-Analysis", "nsCluSRPgvX92BJJrXBr5TWv60yutk6+B3VfS7vvWOA=");
            var audio = await speech.GetSpeakStreamAsync(textToSpeak);

            var mediaSource = Windows.Media.Core.MediaSource.CreateFromStream(audio, "audio/wav");
            AudioPlayer.SetPlaybackSource(mediaSource);

        }

        private async void WebImageSearch()
        {
            var client = new HttpClient();
            var input = UrlInput.Text != "" ? UrlInput.Text : "cat";
            var queryString = $"q={input}";

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "533b5b92d2f2408e8d9350884623e88b");

            // Request parameters
            var uri = "https://bingapis.azure-api.net/api/v5/images/search?" + queryString;

            var response = await client.GetStringAsync(uri);
            var results = JsonConvert.DeserializeObject<WebImageSearchResult>(response);

            var imageList = new List<Image> { Image1, Image2, Image3, Image4 };

            Random r = new Random();

            foreach (var image in imageList)
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(results.value[r.Next(0,results.value.Length)].contentUrl);

                image.Source = bitmapImage;
            }
        }

        #region Interaction_Events
        
        private void WebSearch_Click(object sender, RoutedEventArgs e)
        {
            WebImageSearch();

            AnalysisResultLabel.Text = "Image Analysis Result";
        }

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AnalysisResultLabel.Text = "Thinking...";

            Image image = (Image)sender;

            await GetAnalysisResult(((BitmapImage)image.Source).UriSource.AbsoluteUri);
        }

        private void UrlInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                WebImageSearch();

                AnalysisResultLabel.Text = "Image Analysis Result";
            }
        }

        #endregion


    }
}