using UnityEngine;

namespace CustomESP
{
    public class Loader
    {
        public static void Init()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<MyESP>();
            UnityEngine.Object.DontDestroyOnLoad(obj);
        }
    }
}
