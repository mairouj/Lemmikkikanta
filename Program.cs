using System.Data;
using Microsoft.Data.Sqlite;

namespace LemmikkiKanta
{
    class Program
    {
        static void Main()
        {
            // Luodaan yhteys tietokantaan
            using (var yhteys = new SqliteConnection("Data source=Lemmikkikanta.db"))
            {
                // Avataan yhteys
                yhteys.Open();
                // Luodaan taulukot, jos niitä ei ole
                LuoTaulukot(yhteys);

                while(true)
                {    
                    Console.WriteLine("Valitse numeroilla: 1:Lisää Omistaja. 2:Lisää Lemmikki. 3:Päivitä omistajan puhelinnumero. 4:Etsi omistajan puhelinnumero lemmikkin nimen perusteella, 5:Katsele tietokannan tietoja. 6:Poistu. T:tyhjennä tietokanta" );
                    string? valinta = Console.ReadLine();

                    switch (valinta)
                    {
                        case "1":
                            // Lisätään omistaja
                            string omistajanNimi = KysySyote("Anna omistajan nimi.");
                            string puhelinNumero = KysySyote("Anna omistajan puhelinnumero.");
                            LisaaOmistaja(yhteys, omistajanNimi, puhelinNumero);
                            break;

                        case "2":
                            // Lisätään lemmikki
                            string lemmikinNimi = KysySyote("Anna lemmikin nimi.");
                            string lemmikinLaji = KysySyote("Anna lemmikinlaji.");
                            string lemmikinOmistajanNimi = KysySyote("Anna lemmikin omistajan nimi.");
                            LisaaLemmikki(yhteys, lemmikinNimi, lemmikinLaji, lemmikinOmistajanNimi);
                            break;
                        
                        case "3":
                            // Päivitetään omistajan puhelinnumero
                            string puhelinNumeronOmistajanNimi = KysySyote("Anna omistajan nimi, jonka haluat vaihtaa.");
                            string uusiPuhelinNumero = KysySyote("Anna uusi puhelinnumero.");
                            PuhelinNumeronPaivittaminen(yhteys, puhelinNumeronOmistajanNimi, uusiPuhelinNumero);
                            break;

                        case "4":
                            // Etsitään lemmikin nimen perusteella omistajan puhelinnumero
                            string omistajanLemmikinNimi = KysySyote("Anna lemmikin nimi.");
                            EtsiPuhelinNumero(yhteys, omistajanLemmikinNimi);
                            break;

                        case "5":
                            // Tulostetaan tietokannan tiedot
                            Console.WriteLine("Tietokannan tiedot");
                            TulostaTietokannanTiedot(yhteys);
                            break;

                        case "6":
                            // Suljetaan yhteys ja poistutaan ohjelmasta
                            yhteys.Close();
                            return;
                        
                         case "T":
                            // Tyhjennetään tietokanta
                            TyhjennaTietokanta(yhteys);
                            break;
                    }             
                }    
                
            }

            static string KysySyote(string kysymys)
            {
                // Tarkistetaan että käyttäjän syötteet ei ole tyhjiä
                string? syote;
                while (true)
                {
                    Console.WriteLine(kysymys);
                    syote = Console.ReadLine();
                    if (!string.IsNullOrEmpty(syote))
                    {
                        return syote;
                    }

                    Console.WriteLine("Syöte ei voi olla tyhjä!");
                }

            }


            static void LuoTaulukot(SqliteConnection yhteys)
            {
                // Luodaan taulu omistajille
                var omistajaTaulu = yhteys.CreateCommand();
                omistajaTaulu.CommandText = "CREATE TABLE IF NOT EXISTS Omistaja(id INTEGER PRIMARY KEY, nimi TEXT , puhelinnumero TEXT)";
                omistajaTaulu.ExecuteNonQuery();

                // Luodaan taulu lemmikeille
                var lemmikkiTaulu = yhteys.CreateCommand();
                lemmikkiTaulu.CommandText = "CREATE TABLE IF NOT EXISTS Lemmikki(id INTEGER PRIMARY KEY, nimi TEXT , laji TEXT, omistaja_id INTEGER)";
                lemmikkiTaulu.ExecuteNonQuery();
            }

    
            static void LisaaOmistaja(SqliteConnection yhteys, string nimi, string puhelinNumero)
            {
                // Luodaan uusi SQL komento (käytetään annettua tietokanta yhteyttä)
                var lisaaOmistaja = yhteys.CreateCommand();

                // SQL komento, joka lisää uuden rivin Omistaja tauluun. Käytetään parametrejä, jotta vältytään SQL injektiolta.
                lisaaOmistaja.CommandText = "INSERT INTO Omistaja(nimi, puhelinnumero) VALUES($nimi, $puhelinNumero)";

                // Lisätään parametrit
                lisaaOmistaja.Parameters.AddWithValue("$nimi", nimi);
                lisaaOmistaja.Parameters.AddWithValue("$puhelinNumero", puhelinNumero);
                // Päivitetään tietokanta. Käytetään ExecuteNonQuery metodia, koska ei tarvita palautettavaa arvoa.
                lisaaOmistaja.ExecuteNonQuery();
            }

            
            static void LisaaLemmikki(SqliteConnection yhteys, string nimi, string laji, string omistajanNimi)
            {
                // Haetaan omistajan id omistajan nimen perusteella
                var omistajanHaku = yhteys.CreateCommand();
                omistajanHaku.CommandText = "SELECT id FROM Omistaja WHERE nimi = $nimi";
                omistajanHaku.Parameters.AddWithValue("$nimi", omistajanNimi);

                //Tämä muuttuja saa arvokseen omistajan id:n, jos se löytyy tietokannasta
                int omistajaId = 0;

                // Tämä lukee tietokannan rivi riviltä ja etsii omistajan id:n
                using (var lukija = omistajanHaku.ExecuteReader())
                {
                    if (lukija.Read())
                    {
                        // Haetaan omistajan id
                        omistajaId = lukija.GetInt32(0);
                    }
                }

            
                if (omistajaId == 0)
                {
                    Console.WriteLine("Omistajaa ei löytynyt.");
                    return;
                }

                // Luodaan uusi SQL komento (käytetään annettua tietokanta yhteyttä)
                var lisaaLemmikki = yhteys.CreateCommand();
                lisaaLemmikki.CommandText = "INSERT INTO Lemmikki(nimi, laji, omistaja_id) VALUES ($nimi, $laji, $omistaja_id)";

                // Lisätään parametrit (nimi, laji, omistaja_id)
                lisaaLemmikki.Parameters.AddWithValue("$nimi", nimi);
                lisaaLemmikki.Parameters.AddWithValue("$laji", laji);
                lisaaLemmikki.Parameters.AddWithValue("$omistaja_id", omistajaId);
                lisaaLemmikki.ExecuteNonQuery();
            }

            static void PuhelinNumeronPaivittaminen(SqliteConnection yhteys, string omistajanNimi, string uusiPuhelinNumero)
            {
                // Luodaan uusi SQL komento (käytetään annettua tietokanta yhteyttä)
                var paivitaPuhelinNumero = yhteys.CreateCommand();

                // SQL komento, joka päivittää omistajan puhelinnumero
                paivitaPuhelinNumero.CommandText = "UPDATE Omistaja SET puhelinnumero = $uusiPuhelinNumero WHERE nimi = $nimi";

                // Lisätään parametrit (nimi, joka saa arvokseen metodin parametrin omistajanNimi ja uusiPuhelinNumero, joka saa arvokseen metodin parametrin uusiPuhelinNumero)
                paivitaPuhelinNumero.Parameters.AddWithValue("$nimi", omistajanNimi);
                paivitaPuhelinNumero.Parameters.AddWithValue("$uusiPuhelinNumero", uusiPuhelinNumero);
                // Päivitetään tietokanta. Käytetään ExecuteNonQuery metodia, koska ei tarvita palautettavaa arvoa.
                paivitaPuhelinNumero.ExecuteNonQuery();
            }

            static void EtsiPuhelinNumero(SqliteConnection yhteys, string lemmikinNimi)
            {
                // Luodaan uusi SQL komento (käytetään annettua tietokanta yhteyttä)
                var etsiPuhelinNumero = yhteys.CreateCommand();

                // Etsitään omistajan puhelinnumero lemmikin nimen perusteella
                etsiPuhelinNumero.CommandText = @"
                    SELECT Omistaja.puhelinnumero 
                    FROM Omistaja 
                    JOIN Lemmikki ON Omistaja.id = Lemmikki.omistaja_id 
                    WHERE Lemmikki.nimi = $nimi";
                etsiPuhelinNumero.Parameters.AddWithValue("$nimi", lemmikinNimi);

                // Suoritetaan SQL komento ja luetaan tulokset
                using (var lukija = etsiPuhelinNumero.ExecuteReader())
                {
                    if (lukija.Read())
                    {
                        // Haetaan puhelinnumero tuloksista
                        string puhelinnumero = lukija.GetString(0);
                        Console.WriteLine($"Omistajan puhelinnumero: {puhelinnumero}");
                    }
                    else
                    {
                        Console.WriteLine("Omistajaa ei löytynyt lemmikin nimellä.");
                    }
                }
            }

            static void TulostaTietokannanTiedot(SqliteConnection yhteys)
            {
                // Luodaan uusi SQL komento (käytetään annettua tietokanta yhteyttä)
                var haeTiedot = yhteys.CreateCommand();
                haeTiedot.CommandText = @"
                    SELECT Omistaja.id AS OmistajanId, Omistaja.nimi AS OmistajanNimi, Lemmikki.nimi AS LemmikinNimi, Lemmikki.laji, Omistaja.puhelinnumero
                    FROM Omistaja
                    JOIN Lemmikki ON Omistaja.id = Lemmikki.omistaja_id";

                
                using (var lukija = haeTiedot.ExecuteReader())
                {
                    while (lukija.Read())
                    {
                        // Haetaan tiedot tuloksista
                        int omistajanId = lukija.GetInt32(0);
                        string omistajanNimi = lukija.GetString(1);
                        string lemmikinNimi = lukija.GetString(2);
                        string lemmikinLaji = lukija.GetString(3);
                        string puhelinnumero = lukija.GetString(4);

                        // Tulostetaan tiedot
                        Console.WriteLine($"Omistaja ID: {omistajanId}, Omistaja: {omistajanNimi}, Lemmikki: {lemmikinNimi}, Laji: {lemmikinLaji}, Puhelinnumero: {puhelinnumero}");
                    }
                }
            }

            // Tyhjennetään tietokanta metodi

             static void TyhjennaTietokanta(SqliteConnection yhteys)
            {
                
                var tyhjennaOmistajaTaulu = yhteys.CreateCommand();
                tyhjennaOmistajaTaulu.CommandText = "DELETE FROM Omistaja";
                tyhjennaOmistajaTaulu.ExecuteNonQuery();

                var tyhjennaLemmikkiTaulu = yhteys.CreateCommand();
                tyhjennaLemmikkiTaulu.CommandText = "DELETE FROM Lemmikki";
                tyhjennaLemmikkiTaulu.ExecuteNonQuery();
                Console.WriteLine("Tietokanta on tyhjennetty.");
            } 
        }
    }
}
