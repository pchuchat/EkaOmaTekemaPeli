using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

///Funktio osaaminen osoitettu demo tehtävillä.
///demo5 tehv5 a ja demo7 tehv.1.
/// @author Chuchat Phatchanon
/// @version 2020
/// <summary>
///Pelin nimi Äkänen Koutsi.Peli idea on päästä maaliin mahdollisimman nopeasti .
/// </summary>
public class Äkänen_Koutsi : PhysicsGame
{
    private const int KENTANKORKEUS = 800;
    private const int KENTANLEVEYS = 800;
    private const double RUUDUN_LEVEYS = KENTANLEVEYS / 30;
    private const double RUUDUN_KORKEUS = RUUDUN_LEVEYS;
    private const double LIIKUVOIMA = 200;
    private const double HYPPYVOIMA = 400;
    private int TASO = 1;
    private PlatformCharacter ukkeli;
    private int pelaajanTerveys = 5;
    private ScoreList topLista = new ScoreList(10,true,100);
    private Timer aikaLaskuri;


    private static readonly Image lattiakuva = LoadImage("KIVI");
    private static readonly Image pelaaja = LoadImage("kaitsu");
    private static readonly Image vihollinen = LoadImage("Api");
    private static readonly Image laavaa = LoadImage("lava");
    private static readonly Image boss = LoadImage("koutsi_apina");
    private static readonly Image juoma = LoadImage("potion");
    private static readonly Image maali = LoadImage("Maali");
  

    /// <summary>
    /// Pääohjelma
    /// </summary>
    public override void Begin()
    {
        AloitaPeli();

        SetWindowSize(KENTANLEVEYS, KENTANKORKEUS);

        Camera.ZoomToLevel(100);
        Camera.ZoomFactor = 2;
        Camera.StayInLevel = true;
        Level.Background.Image = Level.Background.CreateGradient(Color.Aqua, Color.Black);
    }


    /// <summary>
    /// Luodaan  peli aikalaskuri
    /// </summary>
    /// <param name="aika">aika</param>
    private void LuoAikaLaskuri(double aika)
    {
        aikaLaskuri = new Timer();
        aikaLaskuri.CurrentTime =aika;
        aikaLaskuri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.X = Screen.Left + 100;
        aikaNaytto.Y = Screen.Top - 100;
        aikaNaytto.TextColor = Color.White;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
        Add(aikaNaytto);
    }


    /// <summary>
    /// Tallennetaan parhaat pisteet
    /// </summary>
    private void Top10()
    {
        topLista = DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml");
        HighScoreWindow topIkkuna = new HighScoreWindow("Parhaat pisteet", "Onneksi olkoon, pääsit listalle pisteillä %p! Syötä nimesi:", DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml"), aikaLaskuri.CurrentTime);
        topIkkuna.List.ScoreFormat = "{0:0.00}";
        Add(topIkkuna);
        topIkkuna.Closed += delegate (Window ikkuna)
        {
            DataStorage.Save<ScoreList>(topLista, "parhaatPisteet.xml");
            AloitaAlusta();
        };
    }


    /// <summary>
    /// Kun pelaaja on kuollut
    /// </summary>
    /// <param name="pelaaja">ukkeli</param>
    /// <param name="kohde">törmättävän kohde</param>
    private void Pelaajaakuoli(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        pelaaja.Destroy();
        aikaLaskuri.Stop();
        NaytaPisteet();
    }
   

    /// <summary>
    /// Aloitus Ikkuna 
    /// </summary>
    private void AloitaPeli()
    {
        MultiSelectWindow valikko = new MultiSelectWindow("Tervetuloa peliin", "Aloita peli", "Parhaat pisteet", "Lopeta");
        valikko.ItemSelected += PainettiinValikonNappia;
        Add(valikko);

    }
  

    /// <summary>
    /// Aloitetaan peli.
    /// </summary>
    private void AloitaAlusta()
    {
        TASO = 1;
        ValitseKentta();
    }


    /// <summary>
    /// valitaan kenttä.
    /// </summary>
    private void ValitseKentta()
    {

       ClearAll();

        switch (TASO)
        {

            case 1:
                LuoKentta("TASO1");
                LuoAikaLaskuri(0);
                pelaajanTerveys = 5;
                Gravity = new Vector(0, -800);
                Level.Background.Image = Level.Background.CreateGradient(Color.Aqua, Color.Black);
                
                break;
            case 2:
                LuoKentta("TASO2");
                LuoAikaLaskuri(aikaLaskuri.CurrentTime);
                Gravity = new Vector(0, -800);
                Level.Background.Image = Level.Background.CreateGradient(Color.Aqua, Color.Black);
               
                break;
            case 3:
                LuoKentta("TASO3");
                LuoAikaLaskuri(aikaLaskuri.CurrentTime);
                Gravity = new Vector(0, -800);
                Level.Background.Image = Level.Background.CreateGradient(Color.Aqua, Color.Black);
                
                break;
            default:
                Exit();
                break;
        }
    }


    /// <summary>
    /// Painikkeiden tapahtumat.
    /// </summary>
    /// <param name="valinta">Valikon painikkeet</param>
    private void PainettiinValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                TASO = 1;
                ValitseKentta();
                Level.Background.Image = Level.Background.CreateGradient(Color.Aqua, Color.Black);
                break;
            case 1:
                NaytaPisteet();
                break;
            case 2:
                Exit();
                break;
        }
    }


    /// <summary>
    /// Luodaan kenttä 1
    /// </summary>
    private void LuoKentta(string nimi)
    {
        TileMap kentta = TileMap.FromLevelAsset(nimi);
        kentta.SetTileMethod('X', LuoLattia, lattiakuva,"lattia");
        kentta.SetTileMethod('H', LuoLattia,laavaa,"lava");
        kentta.SetTileMethod('K', Luopelaaja);
        kentta.SetTileMethod('9', Luovihu, vihollinen, 9);
        kentta.SetTileMethod('*', EriKoisvoima);
        kentta.SetTileMethod('B', Luobossi, boss, 30);
        kentta.SetTileMethod('M', LuoMaali);
        kentta.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
    }


    /// <summary>
    /// Luodaa lattia
    /// </summary>
    /// <param name="paikka">laittien sijainti</param>
    /// <param name="leveys"> lattian leveys</param>
    /// <param name="korkeus">lattian korkeus</param>
    /// <param name="kuva">lattian kuva</param>
    private void LuoLattia(Vector paikka, double leveys, double korkeus, Image kuva,string tagi)
    {
        PhysicsObject lattia = new PhysicsObject(leveys, korkeus);
        lattia.Position = paikka;
        lattia.Image = kuva;
        lattia.MakeStatic();
        lattia.Tag = tagi;
        Add(lattia);

    }


    /// <summary>
    ///  Antaa lisää elämää.
    /// </summary>
    /// <param name="paikka">Juoman sijanti</param>
    /// <param name="leveys">Juoman leveys</param>
    /// <param name="korkeus">Juoman korkeus</param>
    private void EriKoisvoima(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kerattava = new PhysicsObject(leveys, korkeus);
        kerattava.IgnoresCollisionResponse = false;
        kerattava.Position = paikka;
        kerattava.Image = juoma;
        kerattava.Tag = "health";
        AddCollisionHandler(kerattava, "ukkeli", CollisionHandler.DestroyObject);
        AddCollisionHandler(kerattava, "ukkeli", PelaajaParantuu);
        kerattava.MakeStatic();
        Add(kerattava);
    }
    

    /// <summary>
    /// Liikkuminen
    /// </summary>
    /// <param name="ukkeli">pelaaja</param>
    /// <param name="suunta">pelaajan liikkumis suunta</param>
    private void Liikuta(PlatformCharacter ukkeli, double suunta)
    {
        ukkeli.Walk(suunta);
    }

    
    /// <summary>
    /// Hyppiminen
    /// </summary>
    /// <param name="ukkeli">pelaaja</param>
    private void Hyppy(PlatformCharacter ukkeli)
    {
        ukkeli.Jump(HYPPYVOIMA);
    }


    /// <summary>
    /// Pelaajan elämäpistelaskuri
    /// </summary>
    /// <param name="pelaaja">ukkeli</param>
    /// <param name="kohde">mihin törmätään ja menetää elämä pisteet</param>
    private void PelaajaOsuu(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        pelaajanTerveys--;

        if (pelaajanTerveys <= 0)
        {

            pelaaja.Destroy();
            aikaLaskuri.Stop();
            HighScoreWindow pisteet = new HighScoreWindow("Parhaat pisteet", DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml"));
            pisteet.List.ScoreFormat = "{0:0.00}";
            Add(pisteet);
            pisteet.Closed += delegate (Window ikkuna)
            {
                AloitaAlusta();
            };
        }
           
    }


    /// <summary>
    /// Saa lisää elämäpisteitäs
    /// </summary>
    /// <param name="pelaaja">Ukkeli</param>
    /// <param name="kohde">Juoma</param>
    private void PelaajaParantuu(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        pelaajanTerveys++;
        MessageDisplay.Add("Sait lisää elämää ");
    }


    /// <summary>
    /// Luodaan pelaaja.
    /// </summary>
    /// <param name="paikka">pelaajan sijainti</param>
    /// <param name="leveys">pelaajan leveys</param>
    /// <param name="korkeus">pelaajan korkeus</param>
    private void Luopelaaja(Vector paikka, double leveys, double korkeus)
    {
        ukkeli = new PlatformCharacter(leveys, korkeus);
        ukkeli.Position = paikka;
        ukkeli.Mass = 4;
        ukkeli.Image = pelaaja;
        ukkeli.Tag = "ukkeli";
        AddCollisionHandler(ukkeli, "vihollisenAmmus", PelaajaOsuu);
        AddCollisionHandler(ukkeli, "pahis", PelaajaOsuu);
        AddCollisionHandler(ukkeli, "vihollisenAmmus", PelaajaTormasi);
        AddCollisionHandler(ukkeli, "pahis", PelaajaTormasi);
        AddCollisionHandler(ukkeli, "maali", TormasiMaaliin);
        AddCollisionHandler(ukkeli,"laava", Pelaajaakuoli);
        Add(ukkeli);
      
        Camera.Follow(ukkeli);

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", ukkeli, LIIKUVOIMA);
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "´Liiikuta pelaajaa vasemmalle", ukkeli, -LIIKUVOIMA);
        Keyboard.Listen(Key.Space, ButtonState.Down, Hyppy, "pelaaja hyppää yös", ukkeli);
        Keyboard.Listen(Key.Right, ButtonState.Pressed, Heita, "pelaajan ammus", ukkeli, "pelaajanAmmus");
        Keyboard.Listen(Key.Left, ButtonState.Pressed, Heita, "pelaajan ammus", ukkeli, "pelaajanAmmus");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaAlusta, "Aloita alusta");
    }


    /// <summary>
    /// Luodaan vihollinen.
    /// </summary>
    /// <param name="paikka">vihollisen sijainti</param>
    /// <param name="leveys">vihollisen leveys</param>
    /// <param name="korkeus">vihollisen korkeus</param>
    /// <param name="kuva">vihollisen kuva</param>
    /// <param name="liikuvamaara">vihollisen liikuminen</param>
    private void Luovihu(Vector paikka, double leveys, double korkeus, Image kuva, int liikuvamaara)
    {
        Vihollinen vihollinen = new Vihollinen(leveys, korkeus, 3);
        Vector muuttuja = new Vector(0, korkeus * 0.05);
        vihollinen.Position = paikka + muuttuja;
        vihollinen.Mass = 0.2;
        vihollinen.Image = kuva;
        vihollinen.Tag = "pahis";
        vihollinen.CanRotate = false;
        vihollinen.IgnoresGravity = true;
        Add(vihollinen);

        PathFollowerBrain pfb = new PathFollowerBrain();
        List<Vector> reitti = new List<Vector>();
        reitti.Add(vihollinen.Position);
        Vector seuraavaPiste = vihollinen.Position + new Vector(liikuvamaara * RUUDUN_LEVEYS, 0);
        reitti.Add(seuraavaPiste);
        pfb.Path = reitti;
        pfb.Loop = true;
        pfb.Speed = 120;
        pfb.Active = true;
        vihollinen.Brain = pfb;

        Timer heittoajastin = new Timer();
        heittoajastin.Interval = 0.5;
        heittoajastin.Timeout += delegate () { Heita(vihollinen, "vihollisenAmmus"); };
        heittoajastin.Start();
        AddCollisionHandler(vihollinen, "pelaajanAmmus", CollisionHandler.DestroyTarget);
        AddCollisionHandler(vihollinen, "pelaajanAmmus", CollisionHandler.AddMeterValue(vihollinen.HP, -1));
        vihollinen.Destroyed += delegate () { heittoajastin.Stop(); };
    }


    /// <summary>
    /// Ammus luominen.
    /// </summary>
    /// <param name="heittavaOlio">heitettävä olio</param>
    /// <param name="tagit">ammuksen tagi</param>
    private void Heita(PhysicsObject heittavaOlio, string tagit)
    {
        PhysicsObject heitettava = new PhysicsObject(RUUDUN_LEVEYS / 2, RUUDUN_KORKEUS / 2, Shape.Circle);

        heitettava.Color = Color.AshGray;

        if (ukkeli.FacingDirection == Direction.Left)
        {
            heitettava.Position = heittavaOlio.Position + new Vector(RUUDUN_LEVEYS - 2 * RUUDUN_LEVEYS, RUUDUN_KORKEUS / 2);
            heitettava.Hit(new Vector(-400, 50));
        }
        else
        {
            heitettava.Position = heittavaOlio.Position + new Vector(RUUDUN_LEVEYS / 2, RUUDUN_KORKEUS / 2);
            heitettava.Hit(new Vector(400, 50));
        }
        heitettava.Tag = tagit;
        heitettava.MaximumLifetime = TimeSpan.FromSeconds(1.0);
        Add(heitettava, 1);
    }


    /// <summary>
    /// Luodaan pomo.
    /// </summary>
    /// <param name="paikka">pomon sijainti</param>
    /// <param name="leveys">pomon leveys</param>
    /// <param name="korkeus">pomon korkeus</param>
    /// <param name="kuva">pomon kuvan</param>
    /// <param name="liikuvamaara">pomon liikuminen</param>
    private void Luobossi(Vector paikka, double leveys, double korkeus, Image kuva, int liikuvamaara)
    {
        Vihollinen pomo = new Vihollinen(leveys * 3, korkeus * 3, 30);
        Vector muuttuja = new Vector(0, korkeus * 0.05);
        pomo.Position = paikka + muuttuja;
        pomo.Mass = 0.5;
        pomo.Image = kuva;
        pomo.Tag = "bossi";
        pomo.CanRotate = false;
        pomo.IgnoresGravity = true;
        AddCollisionHandler(pomo, "pelaajanAmmus", CollisionHandler.IncreaseObjectSize(10,10));
        Add(pomo);

        PathFollowerBrain pfb = new PathFollowerBrain();
        List<Vector> reitti = new List<Vector>();
        reitti.Add(pomo.Position);
        Vector seuraavaPiste = pomo.Position + new Vector(liikuvamaara * RUUDUN_LEVEYS, 0);
        reitti.Add(seuraavaPiste);
        pfb.Path = reitti;
        pfb.Loop = true;
        pfb.Speed = 250;
        pfb.Active = true;
        pomo.Brain = pfb;

        Timer heittoajastin = new Timer();
        heittoajastin.Interval = 1.0;
        heittoajastin.Timeout += delegate () { Heita(pomo, "vihollisenAmmus"); };
        heittoajastin.Start();
        AddCollisionHandler(pomo, "pelaajanAmmus", CollisionHandler.DestroyTarget);
        AddCollisionHandler(pomo, "pelaajanAmmus", CollisionHandler.AddMeterValue(pomo.HP, -1));
        pomo.Destroyed += delegate () { 
            heittoajastin.Stop();
            Top10();
        };
    }


    /// <summary>
    /// Luodaan maali
    /// </summary>
    /// <param name="paikka">maalin sijainti</param>
    /// <param name="leveys">maalin leveys</param>
    /// <param name="korkeus">maalin korkeus</param>
    private void LuoMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tormattava = new PhysicsObject(leveys, korkeus);
        tormattava.IgnoresCollisionResponse = true;
        tormattava.Position = paikka;
        tormattava.Image = maali;
        tormattava.MakeStatic();
        Add(tormattava);
        tormattava.Tag = "maali";
    }


    /// <summary>
    /// Luodaan maali
    /// </summary>
    /// <param name="tormaaja">pelaaja</param>
    /// <param name="maali"> seuraava kenttä</param>
    private void TormasiMaaliin(PhysicsObject tormaaja, PhysicsObject maali)
    {
        TASO++;
        ValitseKentta();

    }


    /// <summary>
    /// Pelaajan törmäys
    /// </summary>
    /// <param name="tormaaja">pelaaja</param>
    /// <param name="kohde">vihollinen</param>
    private void PelaajaTormasi(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Otit osuman");
            
    }


    /// <summary>
    /// Näyttää parhaat pisteet.
    /// </summary>
    private void NaytaPisteet()
    {
        HighScoreWindow pisteet = new HighScoreWindow("Parhaat pisteet", DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml"));
        pisteet.List.ScoreFormat = "{0:0.00}";
        Add(pisteet);
        pisteet.Closed += delegate (Window ikkuna)
        {
           AloitaPeli();
        };
    }

}