namespace BF1ServerTools.RES.Img;

public static class KitImg
{
    public static Dictionary<string, string> KitDict = new();
    public static Dictionary<string, string> Kit2Dict = new();

    static KitImg()
    {
        InitDict();
    }

    private static void InitDict()
    {
        KitDict.Add("Scout.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Scout.png");
        KitDict.Add("Support.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Support.png");
        KitDict.Add("Assault.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Assault.png");
        KitDict.Add("Medic.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Medic.png");
        KitDict.Add("Cavalry.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Cavalry.png");
        KitDict.Add("Tanker.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Tanker.png");
        KitDict.Add("Pilot.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes\Pilot.png");

        Kit2Dict.Add("Scout.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Scout.png");
        Kit2Dict.Add("Support.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Support.png");
        Kit2Dict.Add("Assault.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Assault.png");
        Kit2Dict.Add("Medic.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Medic.png");
        Kit2Dict.Add("Cavalry.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Cavalry.png");
        Kit2Dict.Add("Tanker.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Tanker.png");
        Kit2Dict.Add("Pilot.png", @"\BF1ServerTools.RES;component\Assets\Images\Classes2\Pilot.png");
    }
}
