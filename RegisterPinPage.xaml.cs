using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class RegisterPinPage : ContentPage
    {
        static Utente utente;

        public RegisterPinPage(Utente user)
        {
            InitializeComponent();
            utente = user;
        }

        async void OnSetPinClicked(object sender, EventArgs args)
        {
            setPinBtn.IsEnabled = false;
            spinner.IsRunning = true;
            utente.pinApp = Int32.Parse(entryPin.Text);
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"name", utente.name},
                {"surname", utente.surname},
                {"email", utente.email },
                {"username", utente.username},
                {"sex", "M"},
                {"pin", utente.pinApp.ToString()}
            };
            utente = null;
            utente = await client.InvokeApiAsync<Utente>("Register/RegisterUser", HttpMethod.Post, argoments);
            if (utente != null)
            {
                if (utente.username != "Exists")
                {
                    setPinBtn.IsEnabled = true;
                    spinner.IsRunning = false;
                    App.Current.MainPage = new RegisterPhotoPage(utente);
                }
                else
                {
                    setPinBtn.IsEnabled = true;
                    spinner.IsRunning = false;
                    await DisplayAlert("Attenzione", "Mail già utilizzata per un'altra registrazione. Se si intende registrarsi, " +
                                   "sceglierne un'altra", "OK");
                    App.Current.MainPage = new RegisterPage();
                }
            }
            else
            {
                setPinBtn.IsEnabled = true;
                spinner.IsRunning = false;
                await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                App.Current.MainPage = new IntroPage();
            }
        }
    }
}
