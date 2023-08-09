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
    public partial class AdministratorPageLogin : Page, INotifyPropertyChanged
    {
        public AdministratorPageLogin(string Ime)
        {
            InitializeComponent();
            Proiz = new Proizvod();
            Ocitaj_proizvode();

            // Pocetne vrednosti za doba dana
            startTimePeriodComboBox.SelectedIndex = 0;
            endTimePeriodComboBox.SelectedIndex = 0;
        }

        public ObservableCollection<Proizvod> Proizvodi { set; get; }
        public Db db = new Db();

        private Proizvod proizvod;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Proizvod Proiz
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
            Proiz.Promocija = float.Parse(promocija.Text);

            if (!string.IsNullOrEmpty(promocija.Text))
            {
                Proiz.Promocija = float.Parse(promocija.Text);
            }
            else
            {
                Proiz.Promocija = 0;
            }

            db.UnesiProizvod(Proiz);
            Ocitaj_proizvode();

            // Resetovanje polja na prazan unos nakon uspešnog dodavanja proizvoda
            Iproizvod.Text = "";
            txtBK.Text = "";
            cena.Text = "";
            lager.Text = "";
            opis.Text = "";
            promocija.Text = "";
            Proiz.Slika = null;
            imgPreview.Source = null;

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
            txtBK.IsReadOnly = false;
            Proiz.Ime = Iproizvod.Text;
            Proiz.Sifra = txtBK.Text;
            Proiz.Lager = int.Parse(lager.Text);
            Proiz.Opis = opis.Text;
            Proiz.Cena = float.Parse(cena.Text);
            Proiz.Promocija = float.Parse(promocija.Text);
            var temp = Proiz.Sifra;
            db.IzmeniProizvod(temp, Proiz);
            lvProizvodi.SelectedItem = null;
            Iproizvod.Text = string.Empty;
            txtBK.Text = string.Empty;
            cena.Text = string.Empty;
            lager.Text = string.Empty;
            opis.Text = string.Empty;
            promocija.Text = string.Empty;
            imgPreview.Source = null;
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
                Iproizvod.Text = Proiz.Ime;
                txtBK.Text = Proiz.Sifra;
                cena.Text = Proiz.Cena.ToString();
                lager.Text = Proiz.Lager.ToString();
                opis.Text = Proiz.Opis;
                promocija.Text = Proiz.Promocija.ToString();
                imgPreview.Source = new BitmapImage(new Uri(Proiz.Slika));
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
                Iproizvod.Text =string.Empty;
                txtBK.Text = string.Empty;
                cena.Text = string.Empty;
                lager.Text = string.Empty;
                opis.Text = string.Empty;
                promocija.Text = string.Empty;
                imgPreview.Source = null ;
            }
            else
            {
                MessageBox.Show("Odaberite proizvod za brisanje");
            }
        }

        // Funkcija za input Opis da može da primi max 200 karaktera
        private void Opis_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length > 200)
            {
                textBox.Text = textBox.Text.Substring(0, 200);
                textBox.SelectionStart = textBox.Text.Length;
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

        // Dodat događaj za unos samo brojeva u TextBox txtBK i promocija
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private void BtnSacuvajVreme_Click(object sender, RoutedEventArgs e)
        {
            DateTime selectedDate = datePicker.SelectedDate ?? DateTime.Today;
            TimeSpan startTime;
            TimeSpan endTime;

            if (TimeSpan.TryParse(startTimeTextBox.Text, out startTime) && TimeSpan.TryParse(endTimeTextBox.Text, out endTime))
            {
                // Dodajte vrednost perioda (AM ili PM) na vreme
                if (startTimePeriodComboBox.SelectedItem as string == "PM" && startTime.Hours < 12)
                {
                    startTime = startTime.Add(new TimeSpan(12, 0, 0));
                }

                if (endTimePeriodComboBox.SelectedItem as string == "PM" && endTime.Hours < 12)
                {
                    endTime = endTime.Add(new TimeSpan(12, 0, 0));
                }

                DateTime selectedStartDateTime = selectedDate.Add(startTime);
                DateTime selectedEndDateTime = selectedDate.Add(endTime);

                // Pozovite ažuriranu metodu iz Db klase za čuvanje vremena rada u bazi
                db.SacuvajVremeRada(selectedStartDateTime, selectedEndDateTime);

                // Resetujte unose nakon što se vreme sačuva
                startTimeTextBox.Text = "";
                endTimeTextBox.Text = "";
                startTimePeriodComboBox.SelectedIndex = -1;
                endTimePeriodComboBox.SelectedIndex = -1;

                
            }
            else
            {
                MessageBox.Show("Unesite validna vremena.");
            }
        }


    }
}
