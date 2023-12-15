using UnityEngine;

/// <summary>
/// 单例mono-自动销毁
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : Component
{
    private static T m_instance;

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find(typeof(T).Name)?.GetComponent<T>();
            }

            if (m_instance != null) return m_instance;
            m_instance = new GameObject(typeof(T).Name).AddComponent<T>();
            DontDestroyOnLoad(m_instance);
            return m_instance;
        }
    }

   
}