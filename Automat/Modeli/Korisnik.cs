using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Automat.Modeli
{
    public class Korisnik : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected  virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string? korisnickoIme;

        public string KorisnickoIme
        {
            get { return korisnickoIme; }
            set { korisnickoIme = value; OnPropertyChanged(); }
        }
        private string? lozinka;

        public string Lozinka
        {
            get { return lozinka; }
            set { lozinka = value; }
        }

    }
}
