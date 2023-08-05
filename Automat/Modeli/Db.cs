﻿using System;
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
                        Cena = 0, // postavi na neku inicijalnu vrednost
                        Ime = r["ime"].ToString(),
                        Slika = r["slika"].ToString()
                    };

                    int CenaValue;
                    if (int.TryParse(r["Cena"].ToString(), out CenaValue))
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
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Neuspesno citanje proizvoda iz baze");
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
                comm.CommandText = $"INSERT INTO [dbo].[Proizvod] ([sifra],[slika],[opis],[ime],[cena],[lager]) VALUES('{p.Sifra}','{p.Slika}','{p.Opis}','{p.Ime}','{p.Cena}','{p.Lager}')";
                comm.ExecuteNonQuery();
                MessageBox.Show("Uspesno dodavanje novog proizvoda");
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
                comm.CommandText = $"UPDATE [dbo].[Proizvod] SET [sifra]='{p.Sifra}',[slika]='{p.Slika}',[Ime]='{p.Ime}',[Cena]='{p.Cena}',[Lager]='{p.Lager}' WHERE [sifra]='{staraSifra}'";
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
                MessageBox.Show("Uspesno brisanje proizvoda");
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