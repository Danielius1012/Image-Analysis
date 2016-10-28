/*
    The MIT License (MIT) 
    Copyright (c) 2016 Daniel Heinze

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 
*/
using DescribeImage.Model;
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
        // Get the keys from the cognitive services portal https://www.microsoft.com/cognitive-services/en-US/subscriptions
        private string WebSearchKey = "614ba201ddb74c6499c93c21d6027076";
        private string VisionSubscriptionKey = "27e1e3fcec1441ca85531c1cd1b85e73";
        private string SpeechClientId = "Image-Analysis";
        private string SpeechClientSecret = "nsCluSRPgvX92BJJrXBr5TWv60yutk6+B3VfS7vvWOA=";

        VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };

        public MainPage()
        {
            this.InitializeComponent();
        }

        #region Web Search

        private void WebSearch_Click(object sender, RoutedEventArgs e)
        {
            WebImageSearch();

            AnalysisResultLabel.Text = "Image Analysis Result";
        }

        private void UrlInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                WebImageSearch();

                AnalysisResultLabel.Text = "Image Analysis Result";
            }
        }

        private async void WebImageSearch()
        {
            try
            {
                var client = new HttpClient();
                var input = UrlInput.Text != "" ? UrlInput.Text : "cat";
                var queryString = $"q={input}";

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", WebSearchKey);

                // Request parameters
                var uri = "https://api.cognitive.microsoft.com/bing/v5.0/images/search?" + queryString;

                var response = await client.GetStringAsync(uri);
                var results = JsonConvert.DeserializeObject<WebImageSearchResult>(response);

                var imageList = new List<Image> { Image1, Image2, Image3, Image4 };

                Random r = new Random();

                foreach (var image in imageList)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = new Uri(results.value[r.Next(0, results.value.Length)].contentUrl);

                    image.Source = bitmapImage;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        #endregion

        #region Image Analysis

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AnalysisResultLabel.Text = "Thinking...";

            Image image = (Image)sender;

            await GetAnalysisResult(((BitmapImage)image.Source).UriSource.AbsoluteUri);
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
            VisionServiceClient VisionServiceClient = new VisionServiceClient(VisionSubscriptionKey);
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageUrl, domainModel);
            return analysisResult;
        }

        #endregion

        #region Audio

        private async void CreateAudioOutput(string textToSpeak)
        {
            SpeechSynthesizer speech = new SpeechSynthesizer(SpeechClientId, SpeechClientSecret);
            var audio = await speech.GetSpeakStreamAsync(textToSpeak);

            var mediaSource = Windows.Media.Core.MediaSource.CreateFromStream(audio, "audio/wav");
            AudioPlayer.SetPlaybackSource(mediaSource);

        }

        #endregion

    }
}
