using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Automat.Modeli
{
    public class Db
    {
        readonly SqlConnection conn;
        readonly SqlCommand comm;

        // Kreiranje konekcije sa bazom
        public Db()
        {
            conn = new SqlConnection(@"Data Source=ALADIN-LAKOTA;Initial Catalog=Automat;Integrated security=true");
            comm = conn.CreateCommand();
        }

        // dodata mogucnost pracenja stanja lagera, to je radjeno u MainWindow.xaml.cs datoteci da bih pratio lager
        public void SaveChanges()
        {
            try
            {
                conn.Open();
                comm.CommandText = "commit";
                comm.ExecuteNonQuery();
                MessageBox.Show("Promene su sačuvane.");
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public void DodajKorisnika(Korisnik korisnik)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"INSERT INTO [dbo].[Korisnik] ([korisnickoIme],[lozinka]) VALUES('{korisnik.KorisnickoIme}','{korisnik.Lozinka}')";
                comm.ExecuteNonQuery();
                MessageBox.Show("Uspesno dodavanje nove osobe");
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public Korisnik VratiKorisnika(string Ime)
        {
            Korisnik korisnik = new Korisnik();
            try
            {
                conn.Open();
                comm.CommandText = $"SELECT korisnickoIme,lozinka FROM Korisnik  WHERE korisnickoIme='{Ime}'";
                SqlDataReader r = comm.ExecuteReader();
                while (r.Read())
                {
                    korisnik.KorisnickoIme = r["korisnickoIme"].ToString();
                    korisnik.Lozinka = r["lozinka"].ToString();
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return korisnik;
        }

        public ObservableCollection<Proizvod> GetProizvode()
        {
            ObservableCollection<Proizvod> proizvodi = new ObservableCollection<Proizvod>();
            try
            {
                conn.Open();
                comm.CommandText = "SELECT * FROM Proizvod";
                SqlDataReader r = comm.ExecuteReader();
                while (r.Read())
                {
                    Proizvod p = new Proizvod
                    {
                        Sifra = r["sifra"].ToString(),
                        Cena = 0,
                        Ime = r["ime"].ToString(),
                        Slika = r["slika"].ToString()
                    };

                    double CenaValue;
                    if (double.TryParse(r["Cena"].ToString(), out CenaValue))
                    {
                        p.Cena = CenaValue;
                    }
                    else
                    {
                        // Ukoliko vrednost za 'Cena' nije ispravna, postavi na neku podrazumevanu vrednost (npr. 0)
                        p.Cena = 0;
                    }

                    try
                    {
                        p.Cena = double.Parse(r["cena"].ToString());
                        p.Lager = int.Parse(r["lager"].ToString());
                        p.Opis = r["opis"].ToString();

                        float PromocijaValue;
                        if (float.TryParse(r["promocija"].ToString(), out PromocijaValue))
                        {
                            p.Promocija = PromocijaValue;
                        }
                        else
                        {
                            // Ukoliko vrednost za 'Promocija' nije ispravna, postavi na nulu
                            p.Promocija = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Neuspešno čitanje proizvoda iz baze");
                        MessageBox.Show(e.Message);
                    }
                    proizvodi.Add(p);
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return proizvodi;
        }

        public void UnesiProizvod(Proizvod p)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"INSERT INTO [dbo].[Proizvod] ([sifra],[slika],[opis],[ime],[cena],[lager],[promocija]) VALUES('{p.Sifra}','{p.Slika}','{p.Opis}','{p.Ime}','{p.Cena}','{p.Lager}','{p.Promocija.ToString().Replace(',', '.')}')";
                comm.ExecuteNonQuery();
                MessageBox.Show("Uspešno dodavanje novog proizvoda");
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public void IzmeniProizvod(string staraSifra, Proizvod p)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"UPDATE [dbo].[Proizvod] SET [sifra]='{p.Sifra}',[slika]='{p.Slika}',[Ime]='{p.Ime}',[Cena]='{p.Cena}','{p.Lager}','{p.Promocija.ToString().Replace(',', '.')}' WHERE [sifra]='{staraSifra}'";
                comm.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public void IzbrisiProizvod(string sifra)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"DELETE FROM [dbo].[Proizvod] WHERE [sifra]='{sifra}'";
                comm.ExecuteNonQuery();
                MessageBox.Show("Uspešno brisanje proizvoda");
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }
    }
}
