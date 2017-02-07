using emotizeapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace emotizeapp.Pages
{
    public partial class ListOfFriendsPage : ContentPage
    {
        static List<string> nomi;
        static List<Utente> amici;

        public ListOfFriendsPage()
        {
            InitializeComponent();
            if (nomi != null)
                friendslist.ItemsSource = nomi;
        }

        public static void SetFriends(List<Utente> friends)
        {
            if (friends != null)
            {
                amici = friends;
                nomi = new List<string>();
                foreach (var amico in amici)
                {
                    nomi.Add(amico.name + " " + amico.surname);
                }
            }
        }

        public static void AddAmico(Utente utente)
        {
            try
            {
                if (amici != null)
                {
                    amici.Add(utente);
                    nomi.Add(utente.name + " " + utente.surname);
                    amici = amici.OrderBy(u => u.name).ToList();
                    nomi = nomi.OrderBy(u => u).ToList();
                } else
                {
                    amici = new List<Utente>();
                    nomi = new List<string>();
                    amici.Add(utente);
                    nomi.Add(utente.name + " " + utente.surname);
                    amici = amici.OrderBy(u => u.name).ToList();
                    nomi = nomi.OrderBy(u => u).ToList();
                }
                
            }
            catch (Exception ex)
            {
            }
            
        }

        void OnSelectFriend(object sender, SelectedItemChangedEventArgs e)
        {
            int index = (friendslist.ItemsSource as List<string>).IndexOf(e.SelectedItem as string);
            Utente amico = amici[index];
            App.Current.MainPage = new FriendProfilePage(amico, "FriendList");
        }

        void OnBackHomeClicked(object sender, EventArgs args)
        {
            App.Current.MainPage = new HomePage();
        }

    }
}
