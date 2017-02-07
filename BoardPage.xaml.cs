using emotizeapp.Helpers;
using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class BoardPage : ContentPage
    {
        static List<Post> listaPost;

        public BoardPage()
        {
            InitializeComponent();
            this.BindingContext = new PostViewModel(listaPost);
        }

        public BoardPage(List<Post> posts)
        {
            InitializeComponent();
            this.BindingContext = new PostViewModel(posts);
            newPost.IsVisible = false;
            newPostBtn.IsVisible = false;
            backToHomeBtn.Text = "Indietro";
        }

        public static void SetPosts(List<Post> posts)
        {
            listaPost = posts;
        }

        async void OnNewPostClicked(object sender, EventArgs args)
        {
            newPost.IsEnabled = false;
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            int id = HomePage.GetID();
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"IdUtente", id.ToString()},
                {"contenuto", newPost.Text },
                {"a", "a" },
                {"b", "a" },
                {"c", "a" }
            };
            HttpResponseMessage response = await client.InvokeApiAsync("Social/Post", null, HttpMethod.Post, null, argoments);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Post post = new Post();
                post.IDUtente = id;
                post.Contenuto = newPost.Text;
                post.DataInserimento = DateTime.Now;
                listaPost.Insert(0, post);
                await DisplayAlert("Avviso", "Nuovo post inserito", "Ok");
            }
            else
            {
                await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                App.Current.MainPage = new HomePage();
            }
        }

        void OnBackHomeClicked(object sender, EventArgs args)
        {
            if (backToHomeBtn.Text == "Indietro")
                App.Current.MainPage = new ProfilePage();
            else
                App.Current.MainPage = new HomePage();
        }
    }
}
