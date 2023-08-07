using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Automat.Modeli
{
    public class Proizvod : INotifyPropertyChanged
    {
        private string slika;
        private string ime;
        private double cena;
        private int lager;
        private string opis;
        private string sifra;
        private int id;
        private float promocija;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Sifra
        {
            get { return sifra; }
            set { sifra = value; OnPropertyChanged(); }
        }

        public int Id // Polje za prikazivanje Id-a
        {
            get { return id; }
            set { id = value; OnPropertyChanged(); }
        }

        public Proizvod()
        {
            slika = "";
            ime = "";
            cena = 0;
            lager = 0;
            opis = "";
            id = 0;
            promocija = 0;
        }

        public Proizvod(string slika, string ime, double cena, int lager, string opis, float promocija)
        {
            this.slika = slika;
            this.ime = ime;
            this.cena = cena;
            this.lager = lager;
            this.opis = opis;
            id = 0; // Inicijalna vrednost za Id
            this.promocija = promocija;
        }

        public event EventHandler Kliknuo;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Slika
        {
            get { return slika; }
            set { slika = value; OnPropertyChanged(); }
        }

        public string Ime
        {
            get { return ime; }
            set { ime = value; OnPropertyChanged(); }
        }

        public string Opis
        {
            get { return opis; }
            set { opis = value; OnPropertyChanged(); }
        }

        public double Cena
        {
            get { return cena; }
            set { cena = value; OnPropertyChanged(); }
        }

        public int Lager
        {
            get { return lager; }
            set { lager = value; OnPropertyChanged(); }
        }

        public float Promocija
        {
            get { return promocija; }
            set { promocija = value; OnPropertyChanged(); }
        }

        public virtual void OnKliknuo()
        {
            if (lager > 0)
            {
                lager--;
                Kliknuo?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show("Nema artikala na lageru, obavestite vlasnika automata!");
            }
        }
    }
}
