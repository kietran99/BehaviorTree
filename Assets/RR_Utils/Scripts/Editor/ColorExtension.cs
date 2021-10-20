namespace RR.Utils
{
    public static class ColorExtension
    {
        public static UnityEngine.Color Create(float v) => new UnityEngine.Color(v / 255f, v / 255f, v / 255f);
    }
}