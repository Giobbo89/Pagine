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
    public partial class ModifyPage : ContentPage
    {
        static Profilo profilo;
        static byte[] img;

        public ModifyPage()
        {
            InitializeComponent();
            img = null;
            entryName.Text = profilo.utente.name;
            entrySurname.Text = profilo.utente.surname;
            entryMail.Text = profilo.utente.email;
            entryBirthday.Date = profilo.utente.birth_date;
            if (profilo.utente.sex == "M")
                switchGender.IsToggled = false;
            else
                switchGender.IsToggled = true;
        }

        public static void SetProfile(Profilo pro)
        {
            profilo = pro;
        }

        async void OnCameraClicked(object sender, EventArgs args)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("Attenzione", "Camera non trovata", "ok");
                return;
            }
            // DA QUI COMINCIA IL CONTROLLO SULLE PERMISSION
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
                    Name = "profile-" + profilo.utente.username + ".jpg"
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
                img = memoryStream.ToArray();
                labelPhoto.Text = "Nuova foto scelta!";
            }
        }

        async void OnGalleryClicked(object sender, EventArgs args)
        {
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                CompressionQuality = 50
            });
            if (file == null)
            {
                return;
            }
            var memoryStream = new MemoryStream();
            file.GetStream().CopyTo(memoryStream);
            file.Dispose();
            img = memoryStream.ToArray();
            labelPhoto.Text = "Nuova foto scelta!";
        }

        async void OnConfirmClicked(object sender, EventArgs args)
        {
            confirmBtn.IsEnabled = false;
            backBtn.IsEnabled = false;
            galleryBtn.IsEnabled = false;
            cameraBtn.IsEnabled = false;
            profilo.utente.name = entryName.Text;
            profilo.utente.surname = entrySurname.Text;
            profilo.utente.email = entryMail.Text;
            profilo.utente.username = entryMail.Text;
            profilo.utente.birth_date = entryBirthday.Date;
            if (switchGender.IsToggled)
                profilo.utente.sex = "F";
            else
                profilo.utente.sex = "M";
            bool cambiata = false;
            if (img != null)
            {
                profilo.immagine = img;
                cambiata = true;
            }

            MobileServiceClient client = new MobileServiceClient(Constants.ApplicationURL);
            Dictionary<string, string> argoments = new Dictionary<string, string>()
            {
                {"cambiata", cambiata.ToString()},
                {"b", "a"},
                {"c", "a"},
            };
            string json = JsonConvert.SerializeObject(profilo);
            HttpContent content = new StringContent(json);
            HttpResponseMessage response = await client.InvokeApiAsync("Login/ModifyUser", content, HttpMethod.Post, null, argoments);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Setting.saveUserForLogin(profilo.utente.name + " " + profilo.utente.surname, profilo.utente.email);
                await DisplayAlert("Avviso", "Le informazioni sono state modificate correttamente", "Ok");
                App.Current.MainPage = new HomePage(profilo, null, null, null, null);
            }
            else
            {
                await DisplayAlert("Attenzione", "Comunicazione col server fallita", "OK");
                App.Current.MainPage = new HomePage();
            }
        }

        async void OnBackClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new HomePage();
        }
    }
}
