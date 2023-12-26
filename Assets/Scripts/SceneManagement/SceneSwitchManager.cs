using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class SceneSwitchManager : SingletonMono<SceneSwitchManager>
{
    private void Start()
    {
        //EventCenterManager.Instance.AddEventListener<string>(GameEvent.SwitchToAnotherScene,SwitchToScene);
    }

    public void SwitchToScene(string sceneName)
    {
        //StopAllCoroutines();
        LoadSceneAsync(
            sceneName,
            loading: (progress) =>
            {
                // 在加载过程中的回调函数，你可以在这里更新加载进度条等UI
                Debug.Log("Loading progress: " + progress);
            },
            completed: (asyncOperation) =>
            {
                // 在加载完成后的回调函数，你可以在这里执行其他操作
                
            },
            setActiveAfterCompleted: true
            );
    }
    
    /// <summary>
    /// 异步加载场景  根据名字来切换场景 (From CSDN zaizai1007)
    /// </summary>
    private void LoadSceneAsync(
        string sceneName, 
        UnityAction<float> loading = null,
        UnityAction<AsyncOperation> completed = null, 
        bool setActiveAfterCompleted = true, 
        LoadSceneMode mode = LoadSceneMode.Single
        )
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, loading, completed, setActiveAfterCompleted, mode));
    }

    private IEnumerator LoadSceneCoroutine(
        string sceneName,
        UnityAction<float> loading = null,
        UnityAction<AsyncOperation> completed = null,
        bool setActiveAfterCompleted = true,
        LoadSceneMode mode = LoadSceneMode.Single
    )
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);

        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
        {
            loading?.Invoke(asyncOperation.progress);
            yield return null;
        }

        // 加载进度到达0.9后，我们人为将其设置为1，以方便外部进度条的显示
        loading?.Invoke(1);

        // 加载完成后的逻辑
        completed?.Invoke(asyncOperation);

        // 等待场景完全加载
        yield return asyncOperation;

        // 如果设置为在加载完成后激活场景，则将场景设置为活跃状态
        if (setActiveAfterCompleted)
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);

            if (loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
            }
            else
            {
                Debug.LogError("Scene is not loaded and therefore cannot be set active: " + sceneName);
            }
        }

        // 允许场景激活
        asyncOperation.allowSceneActivation = true;
        
    }
}