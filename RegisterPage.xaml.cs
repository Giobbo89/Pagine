using emotizeapp.Model;
using System;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            entryBirthday.MaximumDate = DateTime.Now;
            entryBirthday.MinimumDate = DateTime.Now.AddYears(-100);
        }

        async void OnContinueClicked(object sender, EventArgs args)
        {
            Utente utente = new Utente();
            utente.name = entryName.Text;
            utente.surname = entrySurname.Text;
            utente.email = entryMail.Text;
            utente.username = entryMail.Text;
            utente.birth_date = entryBirthday.Date;
            if (switchGender.IsToggled)
                utente.sex = "F";
            else
                utente.sex = "M";
            App.Current.MainPage = new RegisterPinPage(utente);
        }
    }
}
