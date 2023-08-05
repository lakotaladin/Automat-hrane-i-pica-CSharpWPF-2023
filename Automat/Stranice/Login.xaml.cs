using Automat.Modeli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Automat.Stranice
{
    
    public partial class Login : Window
    {
        public Db db = new Db();
        private Korisnik korisnik;

        public Korisnik K
        {
            get { return korisnik; }
            set { korisnik = value; OnPropertyChanged(); }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            K = new Korisnik();
            K.KorisnickoIme = kImetxt.Text;
            if (!string.IsNullOrEmpty(K.KorisnickoIme) || !string.IsNullOrWhiteSpace(K.KorisnickoIme))
            {
                K = db.VratiKorisnika(K.KorisnickoIme);
                if (string.IsNullOrEmpty(K.KorisnickoIme))
                {
                    MessageBox.Show("Ne postoji korisnik sa takvim korisnickim imenom");
                    K.KorisnickoIme = string.Empty;
                    txtLozinka.Password = string.Empty;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(txtLozinka.Password) || !string.IsNullOrEmpty(txtLozinka.Password))
                    {
                        if (txtLozinka.Password.Trim() == K.Lozinka)
                        {
                  
                                AdministratorPageLogin sza = new AdministratorPageLogin(K.KorisnickoIme);
                                main.Content = sza;
                                main.Title = "Prozor za administratore";
                        }
                        else
                        {
                            MessageBox.Show("Pogresna lozinka");
                            txtLozinka.Password = string.Empty;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unesite lozinku");
                    }
                }
            }
            else
            {
                MessageBox.Show("Unesite korisnicko ime");
            }

        }

      
    }
}
