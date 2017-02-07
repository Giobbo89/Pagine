using System;
using System.IO;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class EmotionResultPage : ContentPage
    {
        public EmotionResultPage(byte[] img, string emotion)
        {
            InitializeComponent();
            imgEmotion.Source = ImageSource.FromStream(() => new MemoryStream(img));
            labelEmotion.Text = emotion.ToUpper();
        }

        void OnBackClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new PhotoActionsPage();
        }
    }
}
