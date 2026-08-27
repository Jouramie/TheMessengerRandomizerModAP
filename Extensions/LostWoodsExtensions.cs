namespace MessengerRando.Extensions;

public static class LostWoodsExtensions
{
    public static void SetAsUnsolved(this LostWoods self)
    {
        self.SetPrivateField("solved", false);
        self.InvokeMethod("SetRoom", self.initialRoom.gameObject);
        self.toSunkenShrineRoom.SetActive(false);
    }
}
