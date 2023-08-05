using Automat.Modeli;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Automat.Stranice
{
    public partial class AdministratorPageLogin : Page
    {
        public AdministratorPageLogin(string Ime)
        {
            InitializeComponent();
            Proiz = new Proizvod();
            Ocitaj_proizvode();
        }

        public ObservableCollection<Proizvod> Proizvodi { set; get; }
        public Db db = new Db();

        private Proizvod proizvod;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Proizvod? Proiz
        {
            get { return proizvod; }
            set { proizvod = value; OnPropertyChanged(); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Proiz.Ime = Iproizvod.Text;
            Proiz.Sifra = txtBK.Text;
            Proiz.Lager = int.Parse(lager.Text);
            Proiz.Opis = opis.Text;
            Proiz.Cena = float.Parse(cena.Text);

            db.UnesiProizvod(Proiz);
            Ocitaj_proizvode();

            // Resetovanje polja na prazan unos nakon uspešnog dodavanja proizvoda
            Iproizvod.Text = "";
            txtBK.Text = "";
            cena.Text = "";
            lager.Text = "";
            opis.Text = "";

            Proiz = new Proizvod();
        }

        private void BtnDodajSliku_Click(object sender, RoutedEventArgs e)
        {
            // Dijalog za odabir slike
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                // Dobijanje putanje odabrane slike
                string selectedImagePath = openFileDialog.FileName;

                // Ažuriranje atributa "slika" klase Proizvod
                Proiz.Slika = selectedImagePath;

                // Prikaz odabrane slike u Image kontroleri
                imgPreview.Source = new BitmapImage(new Uri(selectedImagePath));
            }
        }

        private void Ocitaj_proizvode()
        {
            Proizvodi = db.GetProizvode();

            // Ažuriranje Id-eva pre nego što se prikažu u tabeli
            int id = 1;
            foreach (var proizvod in Proizvodi)
            {
                proizvod.Id = id;
                id++;
            }

            lvProizvodi.ItemsSource = null;
            lvProizvodi.ItemsSource = Proizvodi;
        }

        private void BtnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            var temp = Proiz.Sifra;
            txtBK.IsReadOnly = false;
            db.IzmeniProizvod(temp, Proiz);
            lvProizvodi.SelectedItem = null;
            Proiz = null;
            Proiz = new Proizvod();
        }

        private void LvProizvodi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvProizvodi.SelectedIndex > -1)
            {
                btnIzmeni.IsEnabled = true;
                btnIzbrisi.IsEnabled = true;
                txtBK.IsReadOnly = true;
                Proiz = (Proizvod)lvProizvodi.SelectedItem;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Provera da li postoji otvoren prozor tipa MainWindow
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow != null)
            {
                // Ako postoji, samo ga donesemo na vrh
                mainWindow.Activate();
            }
            else
            {
                // Ako ne postoji, otvorimo novi prozor
                MainWindow mw = new MainWindow();
                mw.Show();
            }

            // Zatvaranje trenutnog prozora (AdministratorPageLogin)
            var win = Window.GetWindow(this);
            win.Close();
        }

        private void BtnIzbrisi_Click(object sender, RoutedEventArgs e)
        {
            if (lvProizvodi.SelectedItem != null)
            {
                db.IzbrisiProizvod((lvProizvodi.SelectedItem as Proizvod).Sifra);
                Ocitaj_proizvode();
                lvProizvodi.SelectedItem = null;
            }
            else
            {
                MessageBox.Show("Odaberite proizvod za brisanje");
            }
        }

        private void BtnPretrazi_Click(object sender, RoutedEventArgs e)
        {
            string pretraga = txtPretraga.Text.ToLower();

            // Primena pretrage na kolekciju Proizvoda
            var rezultat = Proizvodi.Where(p => p.Ime.ToLower().Contains(pretraga) ||
                                               p.Sifra.ToLower().Contains(pretraga) ||
                                               p.Opis.ToLower().Contains(pretraga) ||
                                               p.Lager.ToString().Contains(pretraga) ||
                                               p.Cena.ToString().Contains(pretraga));

            // Ažuriranje tabele sa rezultatom pretrage
            lvProizvodi.ItemsSource = rezultat.ToList();
        }

        private void BtnObrisi_Click(object sender, RoutedEventArgs e)
        {
            // Očisti sadržaj inputa za pretragu
            txtPretraga.Text = "";

            // Poništi pretragu i prikaži sve proizvode
            lvProizvodi.ItemsSource = Proizvodi;
        }

        // Dodat događaj za unos samo brojeva u TextBox txtBK
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }
    }
}
