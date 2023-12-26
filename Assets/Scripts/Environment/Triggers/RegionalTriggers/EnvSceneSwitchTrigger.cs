using UnityEngine;

public class EnvSceneSwitchTrigger : RegionalTrigger
{
    [SerializeField] 
    public string targetSceneName = "";
    protected override void DoPlayerEnterAction()
    {
        if (targetSceneName == "")
            return;
        SceneSwitchManager.Instance.SwitchToScene(targetSceneName);
    }
    
    protected override void DoPlayerExitAction()
    {
        
    }

    protected override void DoObjectEnterAction(Collider2D other)
    {
        
    }

    protected override void DoObjectExitAction(Collider2D other)
    {
       
    }
}