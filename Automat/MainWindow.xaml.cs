using Automat.Modeli;
using Automat.Stranice;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Automat
{
    public partial class MainWindow : Window
    {
        public Db db = new Db();
        private List<Proizvod> korpa = new List<Proizvod>();
        private List<Button> dugmadDodajUKorpu = new List<Button>();

        public MainWindow()
        {
            InitializeComponent();
            Window_Loaded(null, null);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => vremeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();

            this.KeyDown += MainWindow_KeyDown;
            korpaButton.Click += PrikaziKorpu;
            potvrdiButton.Click += Potvrdi_Click;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Proizvod> b = db.GetProizvode();

            for (int i = 0; i < b.Count; i++)
            {
                StackPanel proizvodPanel = new StackPanel();
                proizvodPanel.Orientation = Orientation.Vertical;
                proizvodPanel.Margin = new Thickness(10);

                if (b[i].Promocija > 0)
                {
                    Grid popustGrid = new Grid();
                    popustGrid.Width = 60;
                    popustGrid.Height = 30;
                    popustGrid.Background = Brushes.Red;
                    popustGrid.HorizontalAlignment = HorizontalAlignment.Right;
                    popustGrid.VerticalAlignment = VerticalAlignment.Top;

                    TextBlock popustTextBlock = new TextBlock();
                    popustTextBlock.Text = $"- {b[i].Promocija}%";
                    popustTextBlock.Foreground = Brushes.White;
                    popustTextBlock.FontWeight = FontWeights.Bold;
                    popustTextBlock.FontSize = 18;
                    popustTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    popustTextBlock.VerticalAlignment = VerticalAlignment.Center;

                    popustGrid.Children.Add(popustTextBlock);

                    proizvodPanel.Children.Add(popustGrid);
                }

                Image slikaa = new Image();
                slikaa.Width = 200;
                slikaa.Height = 200;

                if (System.IO.File.Exists(b[i].Slika))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(b[i].Slika);
                    bitmap.EndInit();
                    slikaa.Source = bitmap;
                }
                else
                {
                    MessageBox.Show(b[i].Ime + "Slika nije pronađena na datoj putanji: " + b[i].Slika);
                }

                proizvodPanel.Children.Add(slikaa);

                TextBlock kreditTextBlock = new TextBlock();
                kreditTextBlock.Text = $"Kredit: {b[i].Cena.ToString()} CRD";
                kreditTextBlock.FontSize = 18;
                kreditTextBlock.Margin = new Thickness(5);
                kreditTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                kreditTextBlock.FontWeight = FontWeights.Bold;
                proizvodPanel.Children.Add(kreditTextBlock);

                Label nazivProizvoda = new Label();
                nazivProizvoda.Content = $"Naziv: {b[i].Ime}";
                nazivProizvoda.FontSize = 16;
                nazivProizvoda.FontWeight = FontWeights.Bold;
                nazivProizvoda.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(nazivProizvoda);

                Label sifraProizvoda = new Label();
                sifraProizvoda.Content = $"Šifra: {b[i].Sifra}";
                sifraProizvoda.FontSize = 16;
                sifraProizvoda.FontWeight = FontWeights.Bold;
                sifraProizvoda.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(sifraProizvoda);

                Label opis = new Label();
                opis.Content = $"Opis: {b[i].Opis}";
                opis.FontSize = 16;
                opis.Foreground = Brushes.Black;
                opis.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(opis);

                TextBox kolicina = new TextBox();
                kolicina.Text = b[i].Lager.ToString();
                kolicina.Margin = new Thickness(5);
                kolicina.Width = 30;
                kolicina.TextAlignment = TextAlignment.Center;
                proizvodPanel.Children.Add(kolicina);

                

                proizvodiStackPanel.Children.Add(proizvodPanel);
            }
        }

        private void DodajUKorpu_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Proizvod proizvod = (Proizvod)clickedButton.Tag;

            if (korpa.Count < 3)
            {
                if (proizvod.Lager > 0)
                {
                    korpa.Add(proizvod);
                    PrikaziKorpu(null, null);
                }
                else
                {
                    MessageBox.Show("Proizvod nije dostupan na lageru.");
                }
            }
            else
            {
                MessageBox.Show("Moguće je dodati najviše 3 proizvoda u korpu.");
            }
        }

        private void PrikaziKorpu(object sender, RoutedEventArgs e)
        {
            cartStackPanel.Children.Clear();

            if (korpa.Count == 0)
            {
                TextBlock praznaKorpaTextBlock = new TextBlock();
                praznaKorpaTextBlock.Text = "Korpa je prazna.";
                cartStackPanel.Children.Add(praznaKorpaTextBlock);
                korpaButton.IsEnabled = false;
            }
            else
            {
                foreach (Proizvod proizvod in korpa)
                {
                    TextBlock cartItemTextBlock = new TextBlock();
                    cartItemTextBlock.FontSize = 20;
                    cartItemTextBlock.TextAlignment = TextAlignment.Center;
                    cartItemTextBlock.VerticalAlignment = VerticalAlignment.Center;
                    cartItemTextBlock.Margin = new Thickness(5);
                    cartItemTextBlock.Text = $"x1 {proizvod.Ime} = {proizvod.Cena} CRD - {proizvod.Promocija}% popusta";
                    cartStackPanel.Children.Add(cartItemTextBlock);
                }

                foreach (Button dugme in dugmadDodajUKorpu)
                {
                    dugme.IsEnabled = korpa.Count < 3;
                }

                double ukupnaCena = korpa.Sum(proizvod => proizvod.Cena);
                double cenaSaPopustom = korpa.Sum(proizvod => proizvod.Cena * (1 - proizvod.Promocija / 100.0));

                TextBlock ukupnaCenaBlock = new TextBlock();
                ukupnaCenaBlock.Text = $"Ukupna cena: {ukupnaCena} CRD";
                ukupnaCenaBlock.TextAlignment = TextAlignment.Left;
                ukupnaCenaBlock.VerticalAlignment = VerticalAlignment.Center;
                ukupnaCenaBlock.FontSize = 30;
                ukupnaCenaBlock.FontWeight = FontWeights.Bold;
                cartStackPanel.Children.Add(ukupnaCenaBlock);

                TextBlock cenaSaPopustomBlock = new TextBlock();
                cenaSaPopustomBlock.Text = $"Cena sa popustom: {cenaSaPopustom} CRD";
                cenaSaPopustomBlock.TextAlignment = TextAlignment.Left;
                cenaSaPopustomBlock.VerticalAlignment = VerticalAlignment.Center;
                cenaSaPopustomBlock.FontSize = 30;
                cenaSaPopustomBlock.FontWeight = FontWeights.Bold;
                cartStackPanel.Children.Add(cenaSaPopustomBlock);
            }
        }

        private async void Potvrdi_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(brojeviTextBox.Text))
            {
                MessageBox.Show("Niste uneli šifru artikla!");
            }
            else
            {
                string unetaSifra = brojeviTextBox.Text;
                Proizvod pronadjenProizvod = db.GetProizvode().FirstOrDefault(proizvod => proizvod.Sifra == unetaSifra);

                if (pronadjenProizvod != null)
                {
                    if (korpa.Count < 3)
                    {
                        if (pronadjenProizvod.Lager > 0)
                        {
                            korpa.Add(pronadjenProizvod);
                            PrikaziKorpu(null, null);

                            // Dodat async/await za malo kašnjenje pre resetovanja input polja
                            await Task.Delay(100);
                            brojeviTextBox.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("Proizvod nije dostupan na lageru.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Moguće je dodati najviše 3 proizvoda u korpu.");
                    }
                }
                else
                {
                    MessageBox.Show("Proizvod sa unetom šifrom nije pronađen.");
                }
            }
        }






        private void Plati_Click(object sender, RoutedEventArgs e)
        {
            if (korpa.Count == 0)
            {
                MessageBox.Show("Korpa je prazna. Dodajte proizvode pre nego što izvršite plaćanje.");
            }
            else if (korpa.Count >= 3)
            {
                MessageBox.Show("Moguće je dodati najviše 3 proizvoda u korpu.");
            }
            else
            {
                double ukupnaCena = korpa.Sum(proizvod => proizvod.Cena);
                double cenaSaPopustom = korpa.Sum(proizvod => proizvod.Cena * (1 - proizvod.Promocija / 100.0));

                // Promenjena poruka i prikaz
                MessageBox.Show($"Ukupna cena: {ukupnaCena} CRD\n\nCena sa popustom: {cenaSaPopustom} CRD\nUplata je uspešno izvršena!");

                // Smanjivanje lagera
                foreach (Proizvod proizvod in korpa)
                {
                    proizvod.Lager--;
                }

                // Sačuvaj izmene u bazi
                db.SaveChanges();

                korpa.Clear();
                korpaButton.IsEnabled = false;
                cartStackPanel.Children.Clear();
            }
        }

        private void Broj_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (brojeviTextBox.Text.Length > 0)
            {
                brojeviTextBox.Text = Regex.Replace(brojeviTextBox.Text, "[^0-9]", "");
                brojeviTextBox.CaretIndex = brojeviTextBox.Text.Length;
            }
        }

        // Event za pritisak na broj
        private void Broj_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string broj = clickedButton.Content.ToString();

                if (brojeviTextBox != null)
                {
                    brojeviTextBox.Text += broj;
                }
            }
        }

        // Dugme za brisanje brojeva iz input-a u gird2
        private void Brisi_Click(object sender, RoutedEventArgs e)
        {
            if (brojeviTextBox.Text.Length > 0)
            {
                brojeviTextBox.Text = brojeviTextBox.Text.Substring(0, brojeviTextBox.Text.Length - 1);
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                Login loginWindow = new Login();
                loginWindow.ShowDialog();
            }
        }
    }
}
