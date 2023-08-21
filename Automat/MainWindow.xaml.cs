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
using System.Windows.Threading;
using System.Windows.Documents;
using System.Printing;

namespace Automat
{

    public partial class MainWindow : Window
    {
        public Db db = new Db();
        private List<Proizvod> korpa = new List<Proizvod>();
        private List<Button> dugmadDodajUKorpu = new List<Button>();
        private TextBox pretragaTextBox;
        // Brojac za iteme u korpi, kako bi se brisali prozivodi sa lagera za onoliko koliko se proizvoda kupi.
        public string temp = "";
        public int brojac = 1;
        
        
        
        // Glavni program
        public MainWindow()
        {
            InitializeComponent();
            Window_Loaded(null, null);



            // Trenutno vreme
            DateTime trenutnoVreme = DateTime.Now;

            // Prikaz radnog vremena automata i upoređivanje sa trenutnim vremenom
            TimeSpan vremePocetka = db.GetLastInsertedTime1();
            TimeSpan vremeZavrsetka = db.GetLastInsertedTime();

            // Izvuci sate i minute iz trenutnog vremena
            int trenutniSati = trenutnoVreme.Hour;
            int trenutniMinuti = trenutnoVreme.Minute;

            // Izvuci sate i minute iz vremena početka i završetka rada automata
            int pocetakSati = vremePocetka.Hours;
            int pocetakMinuti = vremePocetka.Minutes;
            int zavrsetakSati = vremeZavrsetka.Hours;
            int zavrsetakMinuti = vremeZavrsetka.Minutes;


            // Upoređivanje samo po satima i minutima
            if ((pocetakSati < trenutniSati || (pocetakSati == trenutniSati && pocetakMinuti <= trenutniMinuti)) &&
                (zavrsetakSati > trenutniSati || (zavrsetakSati == trenutniSati && zavrsetakMinuti >= trenutniMinuti)))
            {
                radnoVremeTextBlock.Text = "Radno vreme automata: " + pocetakSati.ToString("D2") + ":" + pocetakMinuti.ToString("D2") + "h" + " do " + zavrsetakSati.ToString("D2") + ":" + zavrsetakMinuti.ToString("D2") + "h";
                radnoVremeTextBlock.Background = Brushes.Gold;
                radnoVremeTextBlock.Height = 20;
                radnoVremeTextBlock.FontSize = 16;
            }
            else
            {
                radnoVremeTextBlock.Text = "Automat trenutno ne radi.";
                radnoVremeTextBlock.Background = Brushes.Gold;
                radnoVremeTextBlock.Height = 16;
                radnoVremeTextBlock.Height = 25;
                korpaButton.IsEnabled = false;
                stampajButton.IsEnabled = false;
                potvrdiButton.IsEnabled = false;
            }



            //Prikaz trenutnog vremena
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => vremeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();

            this.KeyDown += MainWindow_KeyDown;
            // Korpa
            korpaButton.Click += PrikaziKorpu;
            // Pretraga
            KreirajPretraguUI();
        }

        // Dinamicko kreiranje pretrage (komponente)
        private void KreirajPretraguUI()
        {
            StackPanel pretragaStackPanel = new StackPanel();
            pretragaStackPanel.Orientation = Orientation.Horizontal;
            pretragaStackPanel.Margin = new Thickness(0, 10, 0, 10);

            TextBlock pretragaLabel = new TextBlock();
            pretragaLabel.Text = "Pretraga proizvoda po imenu: ";
            pretragaLabel.FontSize = 16;
            pretragaLabel.FontWeight = FontWeights.Bold;
            pretragaLabel.VerticalAlignment = VerticalAlignment.Center;

            pretragaTextBox = new TextBox();
            pretragaTextBox.FontSize = 16;
            pretragaTextBox.Width = 200;
            pretragaTextBox.Margin = new Thickness(10, 0, 10, 0);
            pretragaTextBox.TextChanged += PretragaTextBox_TextChanged;

            Button pretragaDugme = new Button();
            pretragaDugme.Content = "🔍";
            pretragaDugme.FontSize = 16;
            pretragaDugme.Width = 70;
            pretragaDugme.Background = Brushes.Green;
            pretragaDugme.Foreground = Brushes.White;
            pretragaDugme.Margin = new Thickness(10, 0, 0, 0);
            pretragaDugme.Click += PretragaDugme_Click;

            Button brisanjeDugme = new Button();
            brisanjeDugme.Content = "X";
            brisanjeDugme.FontSize = 16;
            brisanjeDugme.Width= 70;
            brisanjeDugme.Background = Brushes.Red;
            brisanjeDugme.Foreground = Brushes.White;
            brisanjeDugme.Margin = new Thickness(5, 0, 0, 0);
            brisanjeDugme.Click += BrisanjeDugme_Click;

            pretragaStackPanel.Children.Add(pretragaLabel);
            pretragaStackPanel.Children.Add(pretragaTextBox);
            pretragaStackPanel.Children.Add(pretragaDugme);
            pretragaStackPanel.Children.Add(brisanjeDugme);

            gridpretraga.Children.Insert(0, pretragaStackPanel);
           

        }

        

        // Input za pretragu proizvoda
        private void PretragaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string unetiTekst = pretragaTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(unetiTekst))
            {
                string validniKarakteri = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";
                string validanUnos = new string(unetiTekst.Where(c => validniKarakteri.Contains(c)).ToArray());

                if (validanUnos != unetiTekst)
                {
                    pretragaTextBox.Text = validanUnos;
                    pretragaTextBox.SelectionStart = validanUnos.Length;
                }
            }
        }

        // Dugme pretrazi
        private void PretragaDugme_Click(object sender, RoutedEventArgs e)
        {
            string trazeniProizvod = pretragaTextBox.Text.Trim();
            PretraziProizvode(trazeniProizvod);
        }
        
        // Dugme za brisanje vrednosti iz pretrage
        private void BrisanjeDugme_Click(object sender, RoutedEventArgs e)
        {
            pretragaTextBox.Clear();
            PretraziProizvode("");
        }

        // Pretraga proizvoda
        private void PretraziProizvode(string trazeniProizvod)
        {
            ObservableCollection<Proizvod> b = db.GetProizvode();
            proizvodiStackPanel.Children.Clear();

            foreach (var proizvod in b)
            {
                if (proizvod.Ime.ToLower().Contains(trazeniProizvod.ToLower()))
                {
                    StackPanel proizvodPanel = new StackPanel();
                    proizvodPanel.Orientation = Orientation.Vertical;
                    proizvodPanel.Margin = new Thickness(5);
                    

                    if (proizvod.Promocija > 0)
                    {
                        Grid popustGrid = new Grid();
                        popustGrid.Width = 60;
                        popustGrid.Height = 30;
                        popustGrid.Background = Brushes.Red;
                        popustGrid.HorizontalAlignment = HorizontalAlignment.Right;
                        popustGrid.VerticalAlignment = VerticalAlignment.Top;

                        TextBlock popustTextBlock = new TextBlock();
                        popustTextBlock.Text = $"- {proizvod.Promocija}%";
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

                    if (System.IO.File.Exists(proizvod.Slika))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(proizvod.Slika);
                        bitmap.EndInit();
                        slikaa.Source = bitmap;
                    }
                    else
                    {
                        MessageBox.Show(proizvod.Ime + "Slika nije pronađena na datoj putanji: " + proizvod.Slika);
                    }

                    proizvodPanel.Children.Add(slikaa);

                    TextBlock kreditTextBlock = new TextBlock();
                    kreditTextBlock.Text = $"Kredit: {proizvod.Cena.ToString()} CRD";
                    kreditTextBlock.FontSize = 18;
                    kreditTextBlock.Margin = new Thickness(5);
                    kreditTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    kreditTextBlock.FontWeight = FontWeights.Bold;
                    proizvodPanel.Children.Add(kreditTextBlock);

                    Label nazivProizvoda = new Label();
                    nazivProizvoda.Content = $"Naziv: {proizvod.Ime}";
                    nazivProizvoda.FontSize = 16;
                    nazivProizvoda.FontWeight = FontWeights.Bold;
                    nazivProizvoda.HorizontalAlignment = HorizontalAlignment.Left;
                    proizvodPanel.Children.Add(nazivProizvoda);

                    Label sifraProizvoda = new Label();
                    sifraProizvoda.Content = $"Šifra: {proizvod.Sifra}";
                    sifraProizvoda.FontSize = 16;
                    sifraProizvoda.FontWeight = FontWeights.Bold;
                    sifraProizvoda.HorizontalAlignment = HorizontalAlignment.Left;
                    proizvodPanel.Children.Add(sifraProizvoda);

                    TextBlock opis = new TextBlock();
                    opis.Text = $"Opis: {proizvod.Opis}";
                    opis.FontSize = 14;
                    opis.TextWrapping = TextWrapping.Wrap;
                    opis.Foreground = Brushes.Black;
                    opis.HorizontalAlignment = HorizontalAlignment.Left;
                    proizvodPanel.Children.Add(opis);


                    proizvodiStackPanel.Children.Add(proizvodPanel);
                }
            }
        }

        // Prikaz svih proizvoda (glavni program)
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Proizvod> b = db.GetProizvode();



            for (int i = 0; i < b.Count; i++)
            {
                StackPanel proizvodPanel = new StackPanel();
                proizvodPanel.Orientation = Orientation.Vertical;
                proizvodPanel.Margin = new Thickness(3);
               

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
                    //MessageBox.Show(b[i].Ime + "Slika nije pronađena na datoj putanji: " + b[i].Slika);

                    // Postavljanje default slike ako slika nije pronađena
                    BitmapImage defaultBitmap = new BitmapImage();
                    defaultBitmap.BeginInit();
                    defaultBitmap.UriSource = new Uri("D:\\Laki Podaci\\Fakultet\\IV semestar 2020 2021\\Objektno orjentisano programiranje 2 C#\\Projekat\\Resources\\no-image.png");
                    defaultBitmap.EndInit();
                    slikaa.Source = defaultBitmap;
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


                TextBlock opis = new TextBlock();
                opis.Text = $"Opis: {b[i].Opis}";
                opis.FontSize = 16;
                opis.MaxWidth = 260;
                //opis.Background = Brushes.Red;
                //opis.TextTrimming = TextTrimming.WordEllipsis;
                opis.TextWrapping = TextWrapping.Wrap;
                opis.Foreground = Brushes.Black;
                opis.Margin = new Thickness(0, 0, 0, 15);
                opis.HorizontalAlignment = HorizontalAlignment.Left;
                proizvodPanel.Children.Add(opis);
                


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
                    MessageBox.Show("Proizvoda nema na lageru!", "Obaveštenje o proizvodu", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Moguće je dodati najviše 3 proizvoda u korpu.");
            }
        }

        // Prikaz korpe u grid2
        private void PrikaziKorpu(object sender, RoutedEventArgs e)
        {
            cartStackPanel.Children.Clear();

            if (korpa.Count == 0)
            {
                MessageBox.Show("Korpa je prazna. Dodajte proizvode pre nego što platite.", "Prazna korpa!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }else if (korpa.Count > 3)
            {
                MessageBox.Show("Korpa je puna, možete da kupite samo 3 proizvoda odjednom!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                foreach (Proizvod proizvod in korpa)
            {
            
            TextBlock cartItemTextBlock = new TextBlock();
            cartItemTextBlock.FontSize = 20;
            cartItemTextBlock.FontWeight = FontWeights.Bold;
            cartItemTextBlock.TextAlignment = TextAlignment.Center;
            cartItemTextBlock.VerticalAlignment = VerticalAlignment.Center;
            cartItemTextBlock.Margin = new Thickness(5);
            cartItemTextBlock.Text = $"x1 {proizvod.Ime} = {proizvod.Cena} CRD - {proizvod.Promocija}% popusta";

            Button deleteButton = new Button();
            deleteButton.Content = "X";
            deleteButton.Background = Brushes.Red;
            deleteButton.Foreground = Brushes.White;
            deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
            deleteButton.FontSize = 14;
            deleteButton.Width = 30;
            deleteButton.Height = 30;
            deleteButton.Tag = proizvod;
            deleteButton.Click += DeleteButton_Click;

            StackPanel itemStackPanel = new StackPanel();
            itemStackPanel.Orientation = Orientation.Horizontal;
            itemStackPanel.Children.Add(cartItemTextBlock);
            itemStackPanel.Children.Add(deleteButton);

            cartStackPanel.Children.Add(itemStackPanel);
        }

                foreach (Button dugme in dugmadDodajUKorpu)
                {
                    dugme.IsEnabled = korpa.Count < 3;
                }

                // Racun ukupne cene
                double ukupnaCena = korpa.Sum(proizvod => proizvod.Cena);
                double cenaSaPopustom = korpa.Sum(proizvod => proizvod.Cena * (1 - proizvod.Promocija / 100.0));

                TextBlock ukupnaCenaBlock = new TextBlock();
                ukupnaCenaBlock.Text = $"Ukupna cena: {ukupnaCena} CRD";
                ukupnaCenaBlock.TextAlignment = TextAlignment.Left;
                ukupnaCenaBlock.VerticalAlignment = VerticalAlignment.Center;
                ukupnaCenaBlock.FontSize = 20;
                ukupnaCenaBlock.FontWeight = FontWeights.Bold;
                ukupnaCenaBlock.Background = Brushes.Gold;
                ukupnaCenaBlock.Margin = new Thickness(0, 50, 0, 2);
                cartStackPanel.Children.Add(ukupnaCenaBlock);

                TextBlock cenaSaPopustomBlock = new TextBlock();
                cenaSaPopustomBlock.Text = $"Cena sa popustom: {cenaSaPopustom} CRD";
                cenaSaPopustomBlock.Background = Brushes.Gold;
                cenaSaPopustomBlock.TextAlignment = TextAlignment.Left;
                cenaSaPopustomBlock.VerticalAlignment = VerticalAlignment.Center;
                cenaSaPopustomBlock.Margin = new Thickness(0, 0, 0, 10);
                cenaSaPopustomBlock.FontSize = 20;
                cenaSaPopustomBlock.FontWeight = FontWeights.Bold;
                cartStackPanel.Children.Add(cenaSaPopustomBlock);
            }


            // Nalov vrste placanja

            TextBlock vrstaPlacanja = new TextBlock();
            vrstaPlacanja.Text = "Odaberite vrstu plaćanja: ";
            vrstaPlacanja.FontWeight = FontWeights.Bold;
            vrstaPlacanja.TextAlignment = TextAlignment.Center;
            vrstaPlacanja.FontSize = 20;
            vrstaPlacanja.VerticalAlignment = VerticalAlignment.Center;
            vrstaPlacanja.Margin = new Thickness(0, 10, 0, 5);
            cartStackPanel.Children.Add(vrstaPlacanja);

            // Stack panel sa dugmicima za placanje

            StackPanel paymentOptionsPanel = new StackPanel();
            paymentOptionsPanel.Orientation = Orientation.Horizontal;
            paymentOptionsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            paymentOptionsPanel.VerticalAlignment = VerticalAlignment.Center;
            paymentOptionsPanel.Margin = new Thickness(0, 20, 0, 5);

            RadioButton cashRadioButton = new RadioButton();
            cashRadioButton.Content = "💵Gotovina";
            //cashRadioButton.FontWeight = FontWeights.Bold;
            cashRadioButton.Margin = new Thickness(4);
            cashRadioButton.FontSize = 20;
            cashRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            cashRadioButton.VerticalContentAlignment = VerticalAlignment.Center;
            cashRadioButton.GroupName = "PaymentOptions";

            RadioButton cardRadioButton = new RadioButton();
            cardRadioButton.Content = "💳Kartica";
            //cardRadioButton.FontWeight = FontWeights.Bold;
            cardRadioButton.Margin = new Thickness(4);
            cardRadioButton.FontSize = 20;
            cardRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            cardRadioButton.VerticalContentAlignment = VerticalAlignment.Center;
            cardRadioButton.GroupName = "PaymentOptions";

            RadioButton paypalRadioButton = new RadioButton();
            paypalRadioButton.Content = "💸PayPal";
            //paypalRadioButton.FontWeight = FontWeights.Bold;
            paypalRadioButton.Margin = new Thickness(4);
            paypalRadioButton.FontSize = 20;
            paypalRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            paypalRadioButton.VerticalContentAlignment = VerticalAlignment.Center;
            paypalRadioButton.GroupName = "PaymentOptions";



            cashRadioButton.IsChecked = true;

            paymentOptionsPanel.Children.Add(cashRadioButton);
            paymentOptionsPanel.Children.Add(cardRadioButton);
            paymentOptionsPanel.Children.Add(paypalRadioButton);

            cartStackPanel.Children.Add(paymentOptionsPanel);
            
        }


        // Dugme "Potvrdi" za dodavanje proizvoda u korpu
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


        // Dugme za placanje proizvoda
        private void Plati_Click(object sender, RoutedEventArgs e)
        {
            brojac = 1;
            temp = "";
            if (korpa.Count == 0)
            {
                MessageBox.Show("Korpa je prazna. Dodajte proizvode pre nego što izvršite plaćanje.");
                return;
            }
            else if (korpa.Count > 3)
            {
                MessageBox.Show("Moguće je dodati najviše 3 proizvoda u korpu.");
                return;
            }
            else
            {
                double ukupnaCena = korpa.Sum(proizvod => proizvod.Cena);
                double cenaSaPopustom = korpa.Sum(proizvod => proizvod.Cena * (1 - proizvod.Promocija / 100.0));


                var kolicine = new Dictionary<string, int>();

                foreach (var proizvod in korpa)
                {
                    if (kolicine.ContainsKey(proizvod.Sifra))
                    {
                        kolicine[proizvod.Sifra]++;
                    } else
                    {
                        kolicine[proizvod.Sifra] = 1;
                    }
                }

                foreach (var (sifra, kolicina) in kolicine) {
                    var proizvod = korpa.Find(p => p.Sifra == sifra);
                    if (proizvod!.Lager - kolicina < 0)
                    {
                        MessageBox.Show("Nema više proizvoda na lageru, molimo vas obratite se vlasniku automata.");
                        korpa.Clear();
                        return;
                    }

                    proizvod.Lager -= kolicina;
                    db.IzmeniProizvod(proizvod.Sifra, proizvod);
                }

                korpa.Clear();
                MessageBox.Show("Plaćanje je uspešno izvršeno!");
                cartStackPanel.Children.Clear();
                stampajButton.Visibility = Visibility.Visible;
               }
                
            }

        // Logika za input u kome moze samo da se unosi sifra atikla
        private void Broj_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (brojeviTextBox.Text.Length > 0)
            {
                brojeviTextBox.Text = Regex.Replace(brojeviTextBox.Text, "[^0-9]", "");
                brojeviTextBox.CaretIndex = brojeviTextBox.Text.Length;
            }
        }

        // Event za pritisak na broj u grid2
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

        // Logika za stampanje racuna
        private void StampajRacun_Click(object sender, RoutedEventArgs e)
        {
            if (korpa.Count == 0 || korpa.Count > 3)
            {
                MessageBox.Show("Morate dodati proizvode u korpu pre nego što štampate račun.");
                return;
            }

            // Dobijanje liste proizvoda iz korpe
            List<Proizvod> korpaLista = new List<Proizvod>(korpa);

            // Kreiranje FlowDocument za prikaz u PrintPreview
            FlowDocument flowDocument = new FlowDocument();
            flowDocument.Background = Brushes.White; // Postavljanje belog pozadinskog boje
            flowDocument.PageWidth = 96 * 8.5; // Širina stranice u pikselima (A4 veličina)
            flowDocument.TextAlignment = TextAlignment.Center; // Centriranje sadržaja
            Thickness margin = flowDocument.PagePadding;
            margin.Left = (flowDocument.PageWidth - flowDocument.ColumnWidth) / 2;
            margin.Right = (flowDocument.PageWidth - flowDocument.ColumnWidth) / 2;
            flowDocument.PagePadding = margin;

            // Dodavanje naslova
            Paragraph naslovParagraph = new Paragraph(new Run("Automat hrane i pića"));
            naslovParagraph.FontSize = 20;
            naslovParagraph.FontWeight = FontWeights.Bold;
            flowDocument.Blocks.Add(naslovParagraph);

            // Dodavanje podnaslova
            Paragraph podnaslovParagraph = new Paragraph(new Run("-- Kupljeni proizvodi -- "));
            podnaslovParagraph.FontSize = 16;
            flowDocument.Blocks.Add(podnaslovParagraph);

            // Dodavanje informacija o proizvodima iz korpe
            foreach (var proizvod in korpaLista)
            {
                Paragraph proizvodParagraph = new Paragraph();
                proizvodParagraph.Inlines.Add(new Run($"*********************************************"));
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new Bold(new Run($"{proizvod.Ime}")));
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new Run($"Šifra: {proizvod.Sifra}"));
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new Run($"Ostvareni popust: {proizvod.Promocija} %"));
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new Run($"Opis: {proizvod.Opis}"));
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new Bold(new Run($"Cena: {proizvod.Cena} CRD")));
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.TextAlignment = TextAlignment.Left; // Poravnanje na levoj strani
                proizvodParagraph.Inlines.Add(new LineBreak());
                proizvodParagraph.Inlines.Add(new Run($"*********************************************"));
                proizvodParagraph.Inlines.Add(new LineBreak());
                flowDocument.Blocks.Add(proizvodParagraph);

            }

            // Dodavanje ukupne cene i cene sa popustom iz korpe
            double ukupnaCena = korpaLista.Sum(proizvod => proizvod.Cena);
            double cenaSaPopustom = korpaLista.Sum(proizvod => proizvod.Cena * (1 - proizvod.Promocija / 100.0));
            
            Paragraph ukupnaCenaParagraph = new Paragraph(new Bold(new Run($"Ukupna cena: {ukupnaCena} CRD = {ukupnaCena} RSD")));
            ukupnaCenaParagraph.TextAlignment = TextAlignment.Left; // Poravnanje na levoj strani
            ukupnaCenaParagraph.Foreground = Brushes.Green; // Postavljanje zelene boje teksta
            flowDocument.Blocks.Add(ukupnaCenaParagraph);

            Paragraph cenaSaPopustomParagraph = new Paragraph(new Bold(new Run($"Cena sa popustom: {cenaSaPopustom} CRD = {cenaSaPopustom} RSD")));
            cenaSaPopustomParagraph.TextAlignment = TextAlignment.Left; // Poravnanje na levoj strani
            cenaSaPopustomParagraph.Foreground = Brushes.Green; // Postavljanje zelene boje teksta
            flowDocument.Blocks.Add(cenaSaPopustomParagraph);

            // Dodavanje datuma i vremena kupovine na dno
            Paragraph datumParagraph = new Paragraph(new Run($"Datum i vreme kupovine: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}h"));
            datumParagraph.Inlines.Add(new LineBreak());
            datumParagraph.FontSize = 14;
            datumParagraph.TextAlignment = TextAlignment.Left; // Poravnanje na levoj strani
            datumParagraph.Margin = new Thickness(0, 30, 0, 2);
            datumParagraph.Inlines.Add(new LineBreak());
            datumParagraph.Inlines.Add(new Run($"*********************************************"));
            datumParagraph.Inlines.Add(new LineBreak());
            flowDocument.Blocks.Add(datumParagraph);



            // Kontakt tekst
            Paragraph kontakt = new Paragraph(new Run("Kontakt (Skenirajte QR kod): "));
            kontakt.FontSize = 14;
            kontakt.Margin = new Thickness(0, 30, 0, 10);
            kontakt.TextAlignment = TextAlignment.Left; // Poravnanje na levoj strani
            flowDocument.Blocks.Add(kontakt);

            // Slika
            Image slika = new Image();
            slika.Width = 130;
            slika.Height = 130;
            BitmapImage bitmap = new BitmapImage(new Uri("D:\\Laki Podaci\\Fakultet\\IV semestar 2020 2021\\Objektno orjentisano programiranje 2 C#\\Projekat\\Resources\\qr-code.png")); // Promenite putanju
            slika.Source = bitmap;
            BlockUIContainer blockUIContainer = new BlockUIContainer(slika);
            blockUIContainer.TextAlignment = TextAlignment.Left; // Poravnanje slike na levoj strani
            flowDocument.Blocks.Add(blockUIContainer);


          
            // Hvala na kupovini!
            Paragraph hvala = new Paragraph(new Run("HVALA NA KUPOVINI!"));
            hvala.FontSize = 14;
            hvala.TextAlignment = TextAlignment.Center; 
            hvala.FontWeight = FontWeights.Bold;
            hvala.Foreground = Brushes.Green;
            flowDocument.Blocks.Add(hvala);

            // Prikazivanje PrintPreview-a
            var printDialog = new PrintDialog();
            printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
            printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

            if (printDialog.ShowDialog() == true)
            {
                DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                printDialog.PrintDocument(paginator, "Automat racun");
            }
        }


       // Dugme za brisanje proizvoda iz korpe
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is Proizvod proizvod)
            {
                korpa.Remove(proizvod);
                PrikaziKorpu(null, null);
            }
        }
      
        // CTRL + D (Admin panel) i CTRL+ R (osvezi ekran proizvoda)
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                Login loginWindow = new Login();
                loginWindow.ShowDialog();
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
            {
                proizvodiStackPanel.Children.Clear();
                ObservableCollection<Proizvod> b = db.GetProizvode();

                for (int i = 0; i < b.Count; i++)
                {
                    StackPanel proizvodPanel = new StackPanel();
                    proizvodPanel.Orientation = Orientation.Vertical;
                    proizvodPanel.Margin = new Thickness(3);

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

                    TextBlock opis = new TextBlock();
                    opis.Text = $"Opis: {b[i].Opis}";
                    opis.FontSize = 16;
                    opis.MaxWidth = 260;
                    //opis.Background = Brushes.Red;
                    //opis.TextTrimming = TextTrimming.WordEllipsis;
                    opis.TextWrapping = TextWrapping.Wrap;
                    opis.Foreground = Brushes.Black;
                    opis.Margin = new Thickness(0, 0, 0, 15);
                    opis.HorizontalAlignment = HorizontalAlignment.Left;
                    proizvodPanel.Children.Add(opis);


                    proizvodiStackPanel.Children.Add(proizvodPanel);
                }

            }
        }
    }
}
