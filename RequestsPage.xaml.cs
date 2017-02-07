using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class RequestsPage : ContentPage
    {
        static Dictionary<int, Profilo> richieste;
        static Profilo profilo;
        static int position = 1;
        static int idUtente = 0;

        public RequestsPage(int i, int id)
        {
            InitializeComponent();
            idUtente = id;
            position = i;
            Profilo profile = richieste[i];
            profilo = profile;
            labelUser.Text = profilo.utente.name + " " + profilo.utente.surname;
            imgHome.Source = ImageSource.FromStream(() => new MemoryStream(profilo.immagine));
            if (i == 1)
            {
                left.IsVisible = false;
                accept.Margin = new Thickness(90, 30, 0, 0);
            }
            if (i == richieste.Count)
                right.IsVisible = false;
            var tapImage1 = new TapGestureRecognizer();
            tapImage1.Tapped += OnLeftClicked;
            left.GestureRecognizers.Add(tapImage1);
            var tapImage2 = new TapGestureRecognizer();
            tapImage2.Tapped += OnRightClicked;
            right.GestureRecognizers.Add(tapImage2);
            var tapImage3 = new TapGestureRecognizer();
            tapImage3.Tapped += OnAcceptClicked;
            accept.GestureRecognizers.Add(tapImage3);
            var tapImage4 = new TapGestureRecognizer();
            tapImage4.Tapped += OnDeclineClicked;
            decline.GestureRecognizers.Add(tapImage4);
        }

        public RequestsPage()
        {
            InitializeComponent();
            if (position == 1)
            {
                left.IsVisible = false;
                accept.Margin = new Thickness(90, 30, 0, 0);
            }
            if (position == richieste.Count)
                right.IsVisible = false;
            var tapImage1 = new TapGestureRecognizer();
            tapImage1.Tapped += OnLeftClicked;
            left.GestureRecognizers.Add(tapImage1);
            var tapImage2 = new TapGestureRecognizer();
            tapImage2.Tapped += OnRightClicked;
            right.GestureRecognizers.Add(tapImage2);
            var tapImage3 = new TapGestureRecognizer();
            tapImage3.Tapped += OnAcceptClicked;
            accept.GestureRecognizers.Add(tapImage3);
            var tapImage4 = new TapGestureRecognizer();
            tapImage4.Tapped += OnDeclineClicked;
            decline.GestureRecognizers.Add(tapImage4);
            labelUser.Text = profilo.utente.name + " " + profilo.utente.surname;
            imgHome.Source = ImageSource.FromStream(() => new MemoryStream(profilo.immagine));
        }

        public static void SetRequests(Dictionary<int, Profilo> req)
        {
            richieste = req;
        }


        void OnProfileClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new FriendProfilePage(profilo, "Request");
        }

        void OnLeftClicked(object sender, EventArgs args)
        {
            int i = position - 1;
            App.Current.MainPage = new RequestsPage(i, idUtente);
        }

        void OnRightClicked(object sender, EventArgs args)
        {
            int i = position + 1;
            App.Current.MainPage = new RequestsPage(i, idUtente);
        }

        async void OnAcceptClicked(object sender, EventArgs args)
        {
            var answer = await DisplayAlert("Conferma", "Vuoi accettare l'amicizia?", "Sì", "No");
            if (answer)
            {
                MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
                Dictionary<string, string> argoments = new Dictionary<string, string>()
                {
                    {"idUtente", idUtente.ToString()},
                    {"idRichiedente",  profilo.utente.ID.ToString()},
                    {"accettata", true.ToString()}
                };
                HttpResponseMessage response = await client.InvokeApiAsync("Friends/RequestResponse", null, HttpMethod.Post, null, argoments);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await DisplayAlert("Avviso", "Amicizia accettata", "Ok");
                    ListOfFriendsPage.AddAmico(profilo.utente);
                    Dictionary<int, Profilo> req = new Dictionary<int, Profilo>();
                    richieste.Remove(position);
                    if (richieste.Count != 0)
                    {
                        foreach (var r in richieste)
                        {
                            if (r.Key < position)
                                req.Add(r.Key, r.Value);
                            else
                                req.Add(r.Key - 1, r.Value);
                        }
                        richieste = req;
                        App.Current.MainPage = new RequestsPage(1, idUtente);
                    }
                    else
                    {
                        HomePage.ResetRequests();
                        await DisplayAlert("Avviso", "Non hai più richieste pendenti", "Torna alla home");
                        App.Current.MainPage = new HomePage();
                    }
                }
                else
                {
                    await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                    App.Current.MainPage = new HomePage();
                }
            }
        }

        async void OnDeclineClicked(object sender, EventArgs args)
        {
            var answer = await DisplayAlert("Conferma", "Vuoi rifiutare l'amicizia?", "Sì", "No");
            if (answer)
            {
                MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
                Dictionary<string, string> argoments = new Dictionary<string, string>()
                {
                    {"idUtente", idUtente.ToString()},
                    {"idRichiedente",  profilo.utente.ID.ToString()},
                    {"accettata", false.ToString()}
                };
                HttpResponseMessage response = await client.InvokeApiAsync("Friends/RequestResponse", null, HttpMethod.Post, null, argoments);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Avviso", "Amicizia accettata", "Ok");
                    Dictionary<int, Profilo> req = new Dictionary<int, Profilo>();
                    richieste.Remove(position);
                    if (richieste.Count != 0)
                    {
                        foreach (var r in richieste)
                        {
                            if (r.Key < position)
                                req.Add(r.Key, r.Value);
                            else
                                req.Add(r.Key - 1, r.Value);
                        }
                        richieste = req;
                        App.Current.MainPage = new RequestsPage(1, idUtente);
                    }
                    else
                    {
                        await DisplayAlert("Avviso", "Non hai più richieste pendenti", "Torna alla home");
                        App.Current.MainPage = new HomePage();
                    }
                }
            }
        }

        void OnBackHomeClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new HomePage();
        }
    }
}
