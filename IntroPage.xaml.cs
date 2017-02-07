using emotizeapp.Helpers;
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
    public partial class IntroPage : ContentPage
    {
        string utente = null;
        string mail = null;

        public IntroPage()
        {
            InitializeComponent();
            string user = Setting.user;
            string email = Setting.mail;
            if (email != "" && email != null && user != "" && user != null)
            {
                utente = user;
                mail = email;
                loginBtn.Text = "Login come " + utente;
            }
        }

        async void OnFacebookClicked(object sender, EventArgs args)
        {
            if (App.Authenticator != null)
            {
                var authenticated = await App.Authenticator.Authenticate("Facebook");
                if (authenticated != null)
                {
                    MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
                    Setting.saveCredential(authenticated.user.UserId, authenticated.user.MobileServiceAuthenticationToken);
                    Dictionary<string, string> argoments = new Dictionary<string, string>()
                    {
                        {"provider", "Facebook"},
                        { "a", "a"},
                        { "b", "a"},
                        { "c", "a"}
                    };

                    MobileServiceUser user = new MobileServiceUser(Setting.username);
                    user.MobileServiceAuthenticationToken = Setting.password;
                    client.CurrentUser = user;
                    HttpResponseMessage response = null;
                    try
                    {
                        response = await client.InvokeApiAsync("Login/LoginWithFacebook", null, HttpMethod.Post, null, argoments);
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            Profilo profilo = JsonConvert.DeserializeObject<Profilo>(json);
                            argoments = new Dictionary<string, string>()
                            {
                                {"id", profilo.utente.ID.ToString()}
                            };
                            response = await client.InvokeApiAsync("Friends/CheckRequest", null, HttpMethod.Post, null, argoments);
                            if (response.IsSuccessStatusCode)
                            {
                                json = await response.Content.ReadAsStringAsync();
                                Dictionary<int, Profilo> requests = JsonConvert.DeserializeObject<Dictionary<int, Profilo>>(json);
                                response = await client.InvokeApiAsync("Friends/ListOfFriends", null, HttpMethod.Post, null, argoments);
                                if (response.IsSuccessStatusCode)
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
                                        Application.Current.MainPage = new HomePage(profilo, null, requests, amici, posts);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        void OnRegisterClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new RegisterPage();
        }

            async void OnLoginClicked(object sender, EventArgs args)
        {
            string email = null;
            if (loginBtn.Text == "Login" || sender.Equals(otherLoginBtn))
                email = await InputBox.TextAlert(this.Navigation);
            else
                email = mail;
            if (email != null)
            {
                var answer = await DisplayAlert("Login", "Cosa vuoi fare?", "Fotocamera", "Da Galleria");
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
                        var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                        {
                            SaveToAlbum = true,
                            CompressionQuality = 50,
                            Name = "login.jpg"
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
                        Application.Current.MainPage = new PinPage(img, email);
                    }
                    else
                    {
                        await DisplayAlert("Attenzione", "Problema coi permessi della camera", "OK");
                        return;
                    }

                }
                else
                {
                    var file = await CrossMedia.Current.PickPhotoAsync();
                    if (file == null)
                        return;
                    var memoryStream = new MemoryStream();
                    file.GetStream().CopyTo(memoryStream);
                    file.Dispose();
                    byte[] img = memoryStream.ToArray();
                    Application.Current.MainPage = new PinPage(img, email);
                }
            }
        }
    }
}
