using emotizeapp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class ProfilePage : ContentPage
    {
        static Profilo mioProfilo;
        static Profilo altroProfilo;
        static List<Utente> amici;
        static string operation;

        public ProfilePage()
        {
            InitializeComponent();
            labelUser.Text = mioProfilo.utente.name.ToUpper() + " " + mioProfilo.utente.surname.ToUpper();
            if (mioProfilo.utente.sex == "M")
                labelGender.Text = "Maschio";
            else
                labelGender.Text = "Femmina";

            labelDate.Text = String.Format("{0:dd/MM/yyyy}", mioProfilo.utente.birth_date);
            imgProfile.Source = ImageSource.FromStream(() => new MemoryStream(mioProfilo.immagine));
        }

        public ProfilePage(Profilo pro, string op)
        {
            InitializeComponent();
            operation = op;
            if (pro != null)
                altroProfilo = pro;
            modifyBtn.IsVisible = false;
            backToHomeBtn.Margin = new Thickness(30, 0, 0, 0);
            backToHomeBtn.Text = "Indietro";
            labelUser.Text = pro.utente.name.ToUpper() + " " + pro.utente.surname.ToUpper();
            if (mioProfilo.utente.sex == "M")
                labelGender.Text = "Maschio";
            else
                labelGender.Text = "Femmina";
            labelDate.Text = pro.utente.birth_date.ToString();
            imgProfile.Source = ImageSource.FromStream(() => new MemoryStream(pro.immagine));
        }

        public static void SetMineProfile(Profilo pro)
        {
            mioProfilo = pro;
        }

        public static void SetFriends(List<Utente> friends)
        {
            amici = friends;
        }

        void OnModifyClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new ModifyPage();
        }

        void OnFriendsClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new ListOfFriendsPage();
        }

        void OnBoardClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new BoardPage();
        }

        void OnBackHomeClicked(object sender, EventArgs args)
        {
            if (operation == "FaceMatch")
            {
                App.Current.MainPage = new FaceMatchResultPage(mioProfilo);
            }
            else
            {
                if (operation == "Request")
                {
                    App.Current.MainPage = new RequestsPage();
                }
                modifyBtn.IsVisible = true;
                backToHomeBtn.Margin = new Thickness(0, 0, 0, 0);
                App.Current.MainPage = new HomePage();
            }
        }
    }
}
