namespace TheHunt.Player.Inventory;

public class LocalAmmoManager
{
    public static int? MagazineLimit { get; set; } = null;
    public static int GrabbedAmmo { get; set; }
    
    public static bool HasAmmo()
    {
        if (MagazineLimit == null)
            return true;
        
        return GrabbedAmmo < MagazineLimit;
    }
    
    public static void IncrementAmmo()
    {
        if (MagazineLimit == null)
            return;
        
        GrabbedAmmo++;
    }
}