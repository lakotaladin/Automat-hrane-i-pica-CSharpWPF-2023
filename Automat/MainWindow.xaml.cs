using Automat.Modeli;
using Automat.Stranice;
using System;
using System.Collections.ObjectModel;
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

        public MainWindow()
        {
            InitializeComponent();
            Window_Loaded(null, null); // Dodato da se učita sadržaj prilikom otvaranja prozora

            // Event za prikaz trenutnog vremena
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => vremeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();

            // Event za otvaranje prozora Login.xaml.cs na CTRL+D
            this.KeyDown += MainWindow_KeyDown;
        }

        // Funkcija koja prikazuje komponente unutar forme kada se ucita
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Proizvod> b = db.GetProizvode();

            // Prvi pregled (70% širine)
            for (int i = 0; i < b.Count; i++)
            {
                // StackPanel za svaki proizvod
                StackPanel proizvodPanel = new StackPanel();
                proizvodPanel.Orientation = Orientation.Vertical;
                proizvodPanel.Margin = new Thickness(10);


                // Prikaži popust ako postoji
                if (b[i].Promocija > 0)
                {
                    Grid popustGrid = new Grid();
                    popustGrid.Width = 60;
                    popustGrid.Height = 30;
                    popustGrid.Background = Brushes.Red;
                    //popustGrid.Margin = new Thickness(1);
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

                // Prikaz slike i njena velicina
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

                // Kredit
                TextBlock kreditTextBlock = new TextBlock();
                kreditTextBlock.Text = $"Kredit: {b[i].Cena.ToString()} CRD";
                kreditTextBlock.FontSize = 18;
                kreditTextBlock.Margin = new Thickness(5);
                kreditTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                kreditTextBlock.FontWeight = FontWeights.Bold; // Zadebljaj tekst
                proizvodPanel.Children.Add(kreditTextBlock);

                

                // Naziv proizvoda
                Label nazivProizvoda = new Label();
                nazivProizvoda.Content = $"Naziv: {b[i].Ime}";
                nazivProizvoda.FontSize = 16;
                nazivProizvoda.FontWeight = FontWeights.Bold;
                nazivProizvoda.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(nazivProizvoda);

                // Sifra proizvoda
                Label sifraProizvoda = new Label();
                sifraProizvoda.Content = $"Šifra: {b[i].Sifra}";
                sifraProizvoda.FontSize = 16;
                sifraProizvoda.FontWeight = FontWeights.Bold;
                sifraProizvoda.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(sifraProizvoda);

                // Opis
                Label opis = new Label();
                opis.Content = $"Opis: {b[i].Opis}";
                sifraProizvoda.FontSize = 16;
                opis.Foreground = Brushes.Black;
                opis.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(opis);

                // Lager dinamicko kreiranje
                TextBox kolicina = new TextBox();
                kolicina.Text = b[i].Lager.ToString();
                kolicina.Margin = new Thickness(5); // Dodaj marginu
                kolicina.Width = 30;
                kolicina.TextAlignment = TextAlignment.Center;
                proizvodPanel.Children.Add(kolicina);

                // Dugme
                Button dugme = new Button();
                dugme.Width = 200;
                dugme.Height = 30;
                dugme.FontSize = 18;
                dugme.Foreground = Brushes.Black;
                dugme.FontWeight = FontWeights.Bold;
                dugme.Background = Brushes.Gold;
                dugme.Margin = new Thickness(0); // Dodaj marginu
                dugme.HorizontalAlignment = HorizontalAlignment.Center;
                dugme.Content = "Dodaj u korpu+";

                int index = i;

                // Event na dugme da poveca kolicinu za +1
                dugme.Click += (sender, args) =>
                {
                    b[index].OnKliknuo();
                    kolicina.Text = b[index].Lager.ToString();
                };

                proizvodPanel.Children.Add(dugme);

                // Dodajte StackPanel sa proizvodom u "stackPanel"
                stackPanel.Children.Add(proizvodPanel);
            }
        }

        // Event za pritisak na broj
        private void Broj_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            brojeviTextBox.Text += button.Content.ToString();
        }

        // Event za brisanje unetih brojeva
        private void Brisi_Click(object sender, RoutedEventArgs e)
        {
            brojeviTextBox.Text = "";
        }

        // Event za potvrdu unosa
        private void Potvrdi_Click(object sender, RoutedEventArgs e)
        {
            if (brojeviTextBox.Text == "")
            {
                MessageBox.Show("Niste uneli šifru artikla!");
            }
            else
            {
                MessageBox.Show("Potvrđen unos!");
            }
        }

        // Event handler for CTRL+D to open Login.xaml.cs
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
