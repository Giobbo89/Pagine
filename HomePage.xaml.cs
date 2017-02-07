using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class HomePage : ContentPage
    {
        static Profilo profilo;
        static string emozione;
        static Dictionary<int, Profilo> richieste;
        static List<Utente> amici;
        static List<Post> listaPost;

        public HomePage(Profilo pro, string emo, Dictionary<int, Profilo> req, List<Utente> friends, List<Post> posts)
        {
            InitializeComponent();
            var tapImage = new TapGestureRecognizer();
            tapImage.Tapped += OnActionsClicked;
            imgHome.GestureRecognizers.Add(tapImage);
            profilo = pro;
            ModifyPage.SetProfile(profilo);
            ProfilePage.SetMineProfile(profilo);
            userBtn.Text = profilo.utente.name + " " + profilo.utente.surname;
            if (emo != null)
            {
                emozione = emo;
                labelEmo.Text = "Vedo che al momento provi " + emozione;
            }
            if (friends != null)
            {
                amici = friends;
                ListOfFriendsPage.SetFriends(amici);
                ProfilePage.SetFriends(amici);
            }
            if (posts != null)
            {
                listaPost = posts;
                BoardPage.SetPosts(listaPost);
            }

            ListOfFriendsPage.SetFriends(amici);
            if (req != null)
            {
                richieste = req;
                RequestsPage.SetRequests(richieste);
                this.CheckRequests();
            }

        }


        public static int GetID()
        {
            return profilo.utente.ID;
        }

        public async void CheckRequests()
        {
           await DisplayAlert("Avviso", "Hai richieste di amicizia da gestire", "Gestisci ora");
           App.Current.MainPage = new RequestsPage(1, profilo.utente.ID);
        }

        public static void ResetRequests()
        {
            richieste = null;
        }

        public HomePage()
        {
            InitializeComponent();
            var tapImage = new TapGestureRecognizer();
            tapImage.Tapped += OnActionsClicked;
            imgHome.GestureRecognizers.Add(tapImage);
            userBtn.Text = profilo.utente.name + " " + profilo.utente.surname;
            labelEmo.Text = "Vedo che al momento provi " + emozione;
            if (richieste != null)
            {
                RequestsPage.SetRequests(richieste);
                this.CheckRequests();
            }
        }

        void OnProfileClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new ProfilePage();
        }

        async void OnActionsClicked(object sender, EventArgs args)
        {
            MediaFile file = null;
            var answer = await DisplayAlert("Login", "Foto da?", "Fotocamera", "Galleria");
            if (answer)
            {
                await CrossMedia.Current.Initialize();
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No camera", "No camera available", "ok");
                    return;
                }
                // DA QUI COMINCIA IL CONTROLLO SULLE PERMISSION
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                    {
                        await DisplayAlert("Alert", "Camera non trovata", "OK");
                        return;
                    }
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                    status = results[Permission.Camera];
                }
                if (status == PermissionStatus.Granted)
                {
                    file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        SaveToAlbum = false,
                        CompressionQuality = 50,
                        Name = "facematch.jpg"
                    });
                    string path = file.Path;
                    if (file == null)
                    {
                        await DisplayAlert("Attenzione", "Devi fare una foto per completare l'operazione", "OK");
                        return;
                    }
                }
            }
            else
            {
                file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    CompressionQuality = 50
                });
                string path = file.Path;
                if (file == null)
                {
                    await DisplayAlert("Attenzione", "Devi fare una foto per completare l'operazione", "OK");
                    return;
                }
            }
            imgHome.IsVisible = false;
            var memoryStream = new MemoryStream();
            file.GetStream().CopyTo(memoryStream);
            file.Dispose();
            byte[] img = memoryStream.ToArray();
            Application.Current.MainPage = new PhotoActionsPage(img);
        }

        async void OnEmotionClicked(object sender, EventArgs args)
        {
            MediaFile file = null;
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No camera", "No camera available", "ok");
                return;
            }
            // DA QUI COMINCIA IL CONTROLLO SULLE PERMISSION
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                {
                    await DisplayAlert("Alert", "Camera non trovata", "OK");
                    return;
                }
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                status = results[Permission.Camera];
            }
            if (status == PermissionStatus.Granted)
            {
                file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = false,
                    CompressionQuality = 50,
                    Name = "facematch.jpg"
                });
                string path = file.Path;
                if (file == null)
                {
                    await DisplayAlert("Attenzione", "Devi fare una foto per completare l'operazione", "OK");
                    return;
                }
                var memoryStream = new MemoryStream();
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                byte[] img = memoryStream.ToArray();
                MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
                HttpContent content = new ByteArrayContent(img);
                Dictionary<string, string> argoments = new Dictionary<string, string>()
                {
                    {"a", "a"}
                };
                HttpResponseMessage response = await client.InvokeApiAsync("Emotize/EmotionRecognize", content, HttpMethod.Post, null, argoments);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string json = await response.Content.ReadAsStringAsync();
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
                    await DisplayAlert("Avviso", "Stato aggiornato!", "Ok");
                    App.Current.MainPage = new HomePage(profilo, emotion, null, null, null);
                }
                else
                {
                    await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                }
            }
        }

        async void OnFriendsClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new RequestsPage();
        }

        async void OnLogoutClicked(object sender, EventArgs args)
        {
            var answer = await DisplayAlert("Logout", "Eseguire il logout?", "Sì", "Annulla");
            if (answer)
            {
                profilo = null;
                App.Current.MainPage = new IntroPage();
            }
            else
            {
                return;
            }
        }
    }
}
