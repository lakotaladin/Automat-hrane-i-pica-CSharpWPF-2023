using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
