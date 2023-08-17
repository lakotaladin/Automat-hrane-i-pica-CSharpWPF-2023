using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Collections.Generic;

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
        public void SaveChanges(List<Proizvod> korpa)
        {
            try
            {
                conn.Open();

                foreach (Proizvod proizvod in korpa)
                {
                    string updateQuery = $"UPDATE [Automat].[dbo].[proizvod] SET lager = lager - 1 WHERE Id = {proizvod.Id}";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, conn))
                    {
                        updateCommand.ExecuteNonQuery();
                    }
                }

        

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
        public void SacuvajVremeRada(TimeSpan vremePocetka, TimeSpan vremeZavrsetka)
        {
            try
            {
                conn.Open();
                comm.CommandText = $"INSERT INTO [dbo].[VremeRadaAutomata] ([VremePocetka], [VremeZavrsetka]) VALUES ('{vremePocetka}', '{vremeZavrsetka}')";
                comm.ExecuteNonQuery();
                //MessageBox.Show("Vreme rada automata je sačuvano.");
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

        public TimeSpan GetLastInsertedTime()
        {
            TimeSpan lastInsertedTime = TimeSpan.MinValue;

            try
            {
                conn.Open();
                comm.CommandText = "SELECT TOP 1 [VremeZavrsetka] FROM VremeRadaAutomata ORDER BY [ID] DESC";
                object result = comm.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    lastInsertedTime = (TimeSpan)result;
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
        public TimeSpan GetLastInsertedTime1()
        {
            TimeSpan lastInsertedTime = TimeSpan.MinValue;

            try
            {
                conn.Open();
                comm.CommandText = "SELECT TOP 1 [VremePocetka] FROM VremeRadaAutomata ORDER BY [ID] DESC";
                object result = comm.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    lastInsertedTime = (TimeSpan)result;
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
