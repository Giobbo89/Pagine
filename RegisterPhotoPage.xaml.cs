using emotizeapp.Model;
using Microsoft.WindowsAzure.MobileServices;
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
    public partial class RegisterPhotoPage : ContentPage
    {
        static Utente utente;

        public RegisterPhotoPage(Utente user)
        {
            InitializeComponent();
            utente = user;
        }

        async void OnCameraClicked(object sender, EventArgs args)
        {
            cameraBtn.IsEnabled = false;
            cameraBtn.IsVisible = false;
            galleryBtn.IsEnabled = false;
            galleryBtn.IsVisible = false;
            spinner.IsVisible = true;
            spinner.IsVisible = true;
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("Attenzione", "Camera non trovata", "ok");
                return;
            }
            // DA QUI COMINCIA IL CONTROLLO SULLE PERMISSION
            try
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
                        SaveToAlbum = false,
                        CompressionQuality = 50,
                        Name = "profile-" + utente.username + ".jpg"
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
                    HttpContent content = new ByteArrayContent(img);
                    MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
                    Dictionary<string, string> argoments = new Dictionary<string, string>()
                    {
                        {"id", utente.ID.ToString()}
                    };
                    HttpResponseMessage response = await client.InvokeApiAsync("Register/RegisterPhoto", content, HttpMethod.Post, null, argoments);
                    if (response.IsSuccessStatusCode)
                    {
                        cameraBtn.IsEnabled = true;
                        cameraBtn.IsVisible = true;
                        galleryBtn.IsEnabled = true;
                        galleryBtn.IsVisible = true;
                        spinner.IsVisible = false;
                        spinner.IsVisible = false;
                        var answer = await DisplayAlert("Avviso", "Vuoi collegare il tuo profilo di Facebook?", "Si", "No");
                        if (answer)
                        {
                            if (App.Authenticator != null)
                            {
                                Autenticazione authenticated = await App.Authenticator.Authenticate("Facebook");
                                if (authenticated != null)
                                {
                                    MobileServiceUser user = new MobileServiceUser(authenticated.user.UserId);
                                    user.MobileServiceAuthenticationToken = authenticated.user.MobileServiceAuthenticationToken;
                                    client.CurrentUser = user;

                                    Dictionary<string, string> argo = new Dictionary<string, string>()
                                    {
                                        {"id", utente.ID + ""},
                                        {"user", user.UserId },
                                        {"token", user.MobileServiceAuthenticationToken },
                                        {"socialName", "Facebook" }
                                    };

                                    response = await client.InvokeApiAsync("Register/ConnectFacebookProfile", null, HttpMethod.Post, null, argo);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        await DisplayAlert("Avviso", "Il tuo profilo Facebook è stato collegato correttamente al tuo account", "Ok");
                                        await DisplayAlert("Avviso", "La registrazione è stata effettuata con successo! Effettua il" +
                                            " login per accedere", "OK");
                                        App.Current.MainPage = new IntroPage();
                                    }

                                    await DisplayAlert("Attenzione", "C'è stato un problema con il collegamento al tuo account di Facebook", "ok");
                                }
                            }
                        }
                        await DisplayAlert("Avviso", "La registrazione è stata effettuata con successo! Effettua il" +
                                           " login per accedere", "OK");
                        App.Current.MainPage = new IntroPage();
                    }
                    else
                    {
                        // BISOGNA CANCELLARE L?UTENTE CREATO IN PRECEDENZA
                        cameraBtn.IsEnabled = true;
                        cameraBtn.IsVisible = true;
                        galleryBtn.IsEnabled = true;
                        galleryBtn.IsVisible = true;
                        spinner.IsVisible = false;
                        spinner.IsVisible = false;
                        await DisplayAlert("Attenzione", "La registrazione non è andata a buon fine a causa di un errore"
                            + " di comunicazione col server", "OK");
                        App.Current.MainPage = new IntroPage();
                    }
                }

            }
            catch (Exception e) { }
        }

        async void OnGalleryClicked(object sender, EventArgs args)
        {
            try
            {
                cameraBtn.IsEnabled = false;
                cameraBtn.IsVisible = false;
                galleryBtn.IsEnabled = false;
                galleryBtn.IsVisible = false;
                spinner.IsVisible = true;
                spinner.IsVisible = true;
                var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    CompressionQuality = 70
                });
                if (file == null)
                {
                    return;
                }
                var memoryStream = new MemoryStream();
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                byte[] img = memoryStream.ToArray();
                HttpContent content = new ByteArrayContent(img);
                MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
                Dictionary<string, string> argoments = new Dictionary<string, string>()
                    {
                        {"id", utente.ID.ToString()}
                    };
                HttpResponseMessage response = await client.InvokeApiAsync("Register/RegisterPhoto", content, HttpMethod.Post, null, argoments);
                if (response.IsSuccessStatusCode)
                {
                    cameraBtn.IsEnabled = true;
                    cameraBtn.IsVisible = true;
                    galleryBtn.IsEnabled = true;
                    galleryBtn.IsVisible = true;
                    spinner.IsVisible = false;
                    spinner.IsVisible = false;
                    var answer = await DisplayAlert("Avviso", "Vuoi collegare il tuo profilo di Facebook?", "Si", "No");
                    if (answer)
                    {
                        if (App.Authenticator != null)
                        {
                            Autenticazione authenticated = await App.Authenticator.Authenticate("Facebook");
                            if (authenticated != null)
                            {
                                MobileServiceUser user = new MobileServiceUser(authenticated.user.UserId);
                                user.MobileServiceAuthenticationToken = authenticated.user.MobileServiceAuthenticationToken;
                                client.CurrentUser = user;

                                Dictionary<string, string> argo = new Dictionary<string, string>()
                                    {
                                        {"id", utente.ID + ""},
                                        {"user", user.UserId },
                                        {"token", user.MobileServiceAuthenticationToken },
                                        {"socialName", "Facebook" }
                                    };

                                response = await client.InvokeApiAsync("Register/ConnectFacebookProfile", null, HttpMethod.Post, null, argo);
                                if (response.IsSuccessStatusCode)
                                {
                                    await DisplayAlert("Avviso", "Il tuo profilo Facebook è stato collegato correttamente al tuo account", "Ok");
                                    await DisplayAlert("Avviso", "La registrazione è stata effettuata con successo! Effettua il" +
                                        " login per accedere", "OK");
                                    App.Current.MainPage = new IntroPage();
                                }
                                else
                                {
                                    await DisplayAlert("Attenzione", "C'è stato un problema con il collegamento al tuo account di Facebook", "ok");
                                }
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Avviso", "La registrazione è stata effettuata con successo! Effettua il" +
                                       " login per accedere", "OK");
                        App.Current.MainPage = new IntroPage();
                    }
                }
                else
                {
                    // BISOGNA CANCELLARE L?UTENTE CREATO IN PRECEDENZA
                    cameraBtn.IsEnabled = true;
                    cameraBtn.IsVisible = true;
                    galleryBtn.IsEnabled = true;
                    galleryBtn.IsVisible = true;
                    spinner.IsVisible = false;
                    spinner.IsVisible = false;
                    await DisplayAlert("Attenzione", "La registrazione non è andata a buon fine a causa di un errore"
                        + " di comunicazione col server", "OK");
                    App.Current.MainPage = new IntroPage();
                }
            }
            catch (Exception e) { }
        }
    }
}
