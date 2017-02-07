using System;
using System.IO;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class TagResultPage : ContentPage
    {
        public TagResultPage(byte[] img, string[] tags)
        {
            InitializeComponent();
            imgTag.Source = ImageSource.FromStream(() => new MemoryStream(img));
            int i = 0;
            if (i < tags.Length)
                labelTag1.Text = tags[i];
            i++;
            if (i < tags.Length)
                labelTag2.Text = tags[i];
            i++;
            if (i < tags.Length)
                labelTag3.Text = tags[i];
            i++;
            if (i < tags.Length)
                labelTag4.Text = tags[i];
            i++;
            if (i < tags.Length)
                labelTag5.Text = tags[i];
            i++;
            if (i < tags.Length)
                labelTag6.Text = tags[i];
        }

        void OnBackClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new PhotoActionsPage();
        }
    }
}
