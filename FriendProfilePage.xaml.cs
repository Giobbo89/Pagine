using emotizeapp.Model;
using System;
using System.IO;

using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class FriendProfilePage : ContentPage
    {
        static Profilo profilo;
        static Utente utente;
        static string operation;

        public FriendProfilePage(Profilo pro, string op)
        {
            InitializeComponent();
            operation = op;
            if (pro != null)
                profilo = pro;
            labelUser.Text = profilo.utente.name.ToUpper() + " " + profilo.utente.surname.ToUpper();
            if (profilo.utente.sex == "M")
                labelGender.Text = "Maschio";
            else
                labelGender.Text = "Femmina";
            labelDate.Text = profilo.utente.birth_date.ToString();
            imgProfile.Source = ImageSource.FromStream(() => new MemoryStream(pro.immagine));
        }

        public FriendProfilePage(Utente user, string op)
        {
            InitializeComponent();
            operation = op;
            if (user != null)
                utente = user;
            labelUser.Text = utente.name.ToUpper() + " " + utente.surname.ToUpper();
            if (utente.sex == "M")
                labelGender.Text = "Maschio";
            else
                labelGender.Text = "Femmina";
            labelDate.Text = String.Format("{0:dd/MM/yyyy}", utente.birth_date.ToString());
        }

        void OnBackClicked(object sender, EventArgs args)
        {
            if (operation == "FaceMatch")
            {
                App.Current.MainPage = new FaceMatchResultPage(profilo);
            }
            else
            {
                if (operation == "FriendList")
                    App.Current.MainPage = new ListOfFriendsPage();
                else
                    App.Current.MainPage = new RequestsPage();
            }
            
            
        }
    }
}
