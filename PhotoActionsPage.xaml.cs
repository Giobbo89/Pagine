using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class PhotoActionsPage : ContentPage
    {
        static byte[] immagine;

        public PhotoActionsPage(byte[] img)
        {
            InitializeComponent();
            immagine = img;
            imgAction.Source = ImageSource.FromStream(() => new MemoryStream(immagine));
        }

        public PhotoActionsPage()
        {
            InitializeComponent();
            imgAction.Source = ImageSource.FromStream(() => new MemoryStream(immagine));
        }

        async void OnFindSimilarClicked(object sender, EventArgs args)
        {
            this.VisibleSpinner();
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            HttpContent content = new ByteArrayContent(immagine);
            HttpResponseMessage response = await client.InvokeApiAsync("Emotize/FaceMatch", content, HttpMethod.Post, null, null);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                Profilo profilo = JsonConvert.DeserializeObject<Profilo>(json);
                this.InvisibleSpinner();
                Application.Current.MainPage = new FaceMatchResultPage(profilo);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    this.InvisibleSpinner();
                    Application.Current.MainPage = new FaceMatchResultPage(null);
                }
                else
                {
                    this.InvisibleSpinner();
                    await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                    App.Current.MainPage = new HomePage();
                }
            }
        }

        async void OnScanEmotionClicked(object sender, EventArgs args)
        {
            this.VisibleSpinner();
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            HttpContent content = new ByteArrayContent(immagine);
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"a", "g"}
            };
            HttpResponseMessage response = await client.InvokeApiAsync("Emotize/EmotionRecognize", content, HttpMethod.Post, null, argoments);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                Dictionary<string, decimal> emotions = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);
                decimal max = -1;
                string nameEmotion = null;
                foreach (var emotion in emotions)
                {
                    if (emotion.Value > max)
                    {
                        max = emotion.Value;
                        nameEmotion = emotion.Key;
                    }
                }
                this.InvisibleSpinner();
                App.Current.MainPage = new EmotionResultPage(immagine, nameEmotion);
            }
            else
            {
                this.InvisibleSpinner();
                await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                App.Current.MainPage = new HomePage();
            }
        }

        async void OnFindTagClicked(object sender, EventArgs args)
        {
            this.VisibleSpinner();
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            HttpContent content = new ByteArrayContent(immagine);
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"a", "a"},
                {"b", "a"},
                {"c", "a"},
                {"d","a"}
            };
            HttpResponseMessage response = await client.InvokeApiAsync("Emotize/AutoTag", content, HttpMethod.Post, null, argoments);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                string[] tags = JsonConvert.DeserializeObject<string[]>(json);
                this.InvisibleSpinner();
                App.Current.MainPage = new TagResultPage(immagine, tags);
            }
            else
            {
                this.InvisibleSpinner();
                await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                App.Current.MainPage = new HomePage();
            }
        }

        async void OnBackHomeClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new HomePage();
        }

        private void VisibleSpinner()
        {
            imgAction.IsVisible = false;
            spinner.IsVisible = true;
            similarBtn.IsEnabled = false;
            tagBtn.IsEnabled = false;
            emotionBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
        }

        private void InvisibleSpinner()
        {
            imgAction.IsVisible = true;
            spinner.IsVisible = false;
            similarBtn.IsEnabled = true;
            tagBtn.IsEnabled = true;
            emotionBtn.IsEnabled = true;
            homeBtn.IsEnabled = true;
        }
    }
}
