using UnityEngine;

/// <summary>
/// 单例mono不销毁
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoDontDestroy<T> : MonoBehaviour where T : SingletonMonoDontDestroy<T>
{
    private static T m_instance;
    public static T Instance {  get { return m_instance; } }
    
    protected virtual void Awake()
    {
        if(m_instance != null)
        {
            Destroy(gameObject);
        } else
        {
            m_instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if(m_instance == this)
        {
            m_instance = null;
        }
    }
}