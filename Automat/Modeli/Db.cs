using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
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

        // Dodata mogucnost pracenja stanja lagera, to je radjeno u MainWindow.xaml.cs datoteci da bih pratio stanje lagera
        public void SaveChanges()
        {
            try
            {
                conn.Open();
                comm.CommandText = "Potvrdjeno";
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


        // Logika za dodavanje korisnika
       // public void DodajKorisnika(Korisnik korisnik)
       // {
         //   try
           // {
             //   conn.Open();
               // comm.CommandText = $"INSERT INTO [dbo].[Korisnik] ([korisnickoIme],[lozinka]) VALUES('{korisnik.KorisnickoIme}','{korisnik.Lozinka}')";
               // comm.ExecuteNonQuery();
               // MessageBox.Show("Uspesno dodavanje nove osobe");
           // }
           // catch (SqlException e)
           // {
            //    MessageBox.Show(e.Message);
           // }
           // finally
           // {
            //    if (conn != null)
             //   {
               //     conn.Close();
               // }
          //  }
      //  }

        // Logika za dohvatanje opodataka od tabele Korisnik
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


        // Logika za dohvatanje podataka iz tabele Proizvod
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


        // Logika za unos proizvoda u bazu
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

       

        // Radno vreme automata - unos podataka
        public void SacuvajVremeRada(DateTime vremePocetka, DateTime vremeZavrsetka)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"INSERT INTO [dbo].[VremeRadaAutomata] ([VremePocetka], [VremeZavrsetka], [Radi]) VALUES ('{vremePocetka.ToString("yyyy-MM-dd HH:mm:ss")}', '{vremeZavrsetka.ToString("yyyy-MM-dd HH:mm:ss")}', 1)";
                comm.ExecuteNonQuery();
                MessageBox.Show("Vreme rada automata je sačuvano.");
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

        // Poslednje ubaceno vreme prikazi - vreme kada automat zavrsava sa radom

        public DateTime GetLastInsertedTime()
        {
            DateTime lastInsertedTime = DateTime.MinValue;

            try
            {
                conn.Open();
                comm.CommandText = "SELECT TOP 1 [VremeZavrsetka] FROM VremeRadaAutomata ORDER BY [ID] DESC";
                object result = comm.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    lastInsertedTime = Convert.ToDateTime(result);
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

            return lastInsertedTime;
        }


        // Pocetak radnog vremena automata
        public DateTime GetLastInsertedTime1()
        {
            DateTime lastInsertedTime = DateTime.MinValue;

            try
            {
                conn.Open();
                comm.CommandText = "SELECT TOP 1 [VremePocetka] FROM VremeRadaAutomata ORDER BY [ID] DESC";
                object result = comm.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    lastInsertedTime = Convert.ToDateTime(result);
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

            return lastInsertedTime;
        }


        // Provera da li automat radi, u bazi postoji bool i na osnovu toga se zna da li radi ili ne radi. Podrazumevana vrednost je 1 (radi)
        public bool RadiLi()
        {
            bool result = false;
            try
            {
                conn.Open();
                comm.CommandText = "SELECT TOP 1 [Radi] FROM VremeRadaAutomata ORDER BY [ID] DESC";
                SqlDataReader r = comm.ExecuteReader();
                while (r.Read())
                {
                result =(bool) r["Radi"];
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

            return result;
        }



        // Logika za izmenu podataka od prozivoda 
        public void IzmeniProizvod(string staraSifra, Proizvod p)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"UPDATE [dbo].[Proizvod] SET [sifra]='{p.Sifra}',[slika]='{p.Slika}',[ime]='{p.Ime}',[cena]='{p.Cena}',[lager]='{p.Lager}',[promocija]='{p.Promocija.ToString().Replace(',', '.')}' WHERE [sifra]='{staraSifra}'";
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



        // Logika za brisanje proizvoda iz tabele u Admin panel
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
