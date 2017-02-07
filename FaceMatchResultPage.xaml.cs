using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class FaceMatchResultPage : ContentPage
    {
        static Profilo profilo;

        public FaceMatchResultPage(Profilo pro)
        {
            InitializeComponent();
            if (pro != null)
            {
                profilo = pro;
                labelUser.Text = profilo.utente.name.ToUpper() + " " + profilo.utente.surname.ToUpper();
                result.Source = ImageSource.FromStream(() => new MemoryStream(profilo.immagine));
                profileBtn.Text = "Vedi profilo";
            }
            else
            {
                result.Source = "notfound.png";
                requestBtn.IsEnabled = false;
                labelUser.Text = "Non è stato trovato nessun utente somigliante";
                profileBtn.Text = "Torna alla home";
            }
        }

        async void OnProfileClicked(object sender, EventArgs args)
        {
            if (profileBtn.Text == "Vedi profilo")
            {
                App.Current.MainPage = new FriendProfilePage(profilo, "FaceMatch");
            }
            else
            {
                requestBtn.IsEnabled = true;
                App.Current.MainPage = new HomePage();
            }
        }

        async void OnRequestClicked(object sender, EventArgs args)
        {
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            int idUtente = HomePage.GetID();
            int idAmico = profilo.utente.ID;
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"idUtente", idUtente.ToString()},
                {"idAmico", idAmico.ToString()}
            };
            string response = await client.InvokeApiAsync<string>("Friends/SendRequest", HttpMethod.Post, argoments);
            if (response == "OK")
                await DisplayAlert("Avviso", "La richiesta è stata inviata", "Ok");
            if (response == "Friend")
                await DisplayAlert("Avviso", "Siete già amici", "Ok");
            if (response == "Already")
                await DisplayAlert("Avviso", "Hai già inviato una richiesta a questa persona", "Ok");
            if (response == "Error")
                await DisplayAlert("Avviso", "Si è verificato un'errore durante l'invio della richiesta", "Ok");
        }

        async void OnBackClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new PhotoActionsPage();
        }
    }
}
