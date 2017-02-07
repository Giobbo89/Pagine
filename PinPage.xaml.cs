using emotizeapp.Helpers;
using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class PinPage : ContentPage
    {
        static byte[] verifyImg;
        static string mail;

        public PinPage(byte[] img, string email)
        {
            InitializeComponent();
            mail = email;
            verifyImg = img;
            spinner.IsRunning = false;
            spinner.IsVisible = false;
        }

        async void OnConfirmPinClicked(object sender, EventArgs args)
        {
            pinBtn.IsEnabled = false;
            entryPin.IsEnabled = false;
            spinner.IsRunning = true;
            spinner.IsVisible = true;
            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            string pin = entryPin.Text;
            HttpContent content = new ByteArrayContent(verifyImg);
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"mail", mail},
                {"pin", pin }
            };
            HttpResponseMessage response = await client.InvokeApiAsync("Login/LoginUser", content, HttpMethod.Post, null, argoments);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                Profilo profilo = JsonConvert.DeserializeObject<Profilo>(json);
                Setting.saveUserForLogin(profilo.utente.name + " " + profilo.utente.surname, profilo.utente.email);
                content = new ByteArrayContent(profilo.immagine);
                argoments = new Dictionary<string, string>()
                {
                    {"a", mail}
                };
                response = await client.InvokeApiAsync("Emotize/EmotionRecognize", content, HttpMethod.Post, null, argoments);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    json = await response.Content.ReadAsStringAsync();
                    Dictionary<string, decimal> emotions = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json);
                    decimal max = -1;
                    string emotion = null;
                    foreach (var emo in emotions)
                    {
                        if (emo.Value > max)
                        {
                            max = emo.Value;
                            emotion = emo.Key;
                        }
                    }
                    argoments = new Dictionary<string, string>()
                    {
                        {"id", profilo.utente.ID.ToString()}
                    };
                    response = await client.InvokeApiAsync("Friends/CheckRequest", null, HttpMethod.Post, null, argoments);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        json = await response.Content.ReadAsStringAsync();
                        Dictionary<int, Profilo> requests = JsonConvert.DeserializeObject<Dictionary<int, Profilo>>(json);
                        argoments = new Dictionary<string, string>()
                        {
                            {"id", profilo.utente.ID.ToString()},
                            {"a","a" },
                            {"b","b" },
                            {"c","c" }
                        };
                        response = await client.InvokeApiAsync("Friends/ListOfFriends", null, HttpMethod.Post, null, argoments);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            json = await response.Content.ReadAsStringAsync();
                            List<Utente> amici = JsonConvert.DeserializeObject<List<Utente>>(json);
                            argoments = new Dictionary<string, string>()
                            {
                                {"id", profilo.utente.ID.ToString()}
                            };
                            response = await client.InvokeApiAsync("Social/GetUserPost", null, HttpMethod.Post, null, argoments);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                json = await response.Content.ReadAsStringAsync();
                                List<Post> posts = JsonConvert.DeserializeObject<List<Post>>(json);
                                spinner.IsRunning = false;
                                spinner.IsVisible = false;
                                pinBtn.IsEnabled = true;
                                Application.Current.MainPage = new HomePage(profilo, emotion, requests, amici, posts);
                            } else
                            {
                                await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                                App.Current.MainPage = new IntroPage();
                            }
                        }
                        else
                        {
                            await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                            App.Current.MainPage = new IntroPage();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                        App.Current.MainPage = new IntroPage();
                    }
                }
                else
                {
                    await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                    App.Current.MainPage = new IntroPage();
                }
            }
            else
            {
                spinner.IsRunning = false;
                pinBtn.IsEnabled = true;
                if (response.StatusCode == System.Net.HttpStatusCode.ResetContent)
                    await DisplayAlert("Attenzione", "La persona della foto utilizzata per effettuare il login " +
                                       "non corrisponde a quella del profilo. Accesso negato.", "OK");
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    await DisplayAlert("Attenzione", "Non è stato trovato l'utente con cui si vuole effettuare " +
                                       "l'accesso", "OK");
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                App.Current.MainPage = new IntroPage();
            }
        }
    }
}
