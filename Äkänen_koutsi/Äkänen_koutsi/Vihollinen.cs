using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;


/// @author Chuchat Phatchanon
/// @version 2020
/// <summary>
/// Vihollisen elämäpiste
/// </summary>


public class Vihollinen : PhysicsObject
{
    public IntMeter HP;


    /// <summary>
    /// Viholisen elämäpiste
    /// </summary>
    /// <param name="leveys">vihollisen leveys</param>
    /// <param name="korkeus">vihollisen korkeus</param>
    /// <param name="elamapisteet">vihollisen elämäpiste</param>
    public Vihollinen(double leveys, double korkeus, int elamapisteet)
   : base(leveys, korkeus)
    {
        HP = new IntMeter(elamapisteet, 0, elamapisteet);
        HP.LowerLimit += delegate () { this.Destroy(); };
    }
}

