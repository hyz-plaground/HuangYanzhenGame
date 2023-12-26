using UnityEngine;

public abstract class RegionalTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
        {
            DoPlayerEnterAction();
        }
        else
        {
            DoObjectEnterAction(other);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PlayerProperties.Instance.PLAYER_TAG))
        {
            DoPlayerExitAction();
        }
        else
        {
            DoObjectExitAction(other);
        }
    }

    protected abstract void DoPlayerEnterAction();
    protected abstract void DoPlayerExitAction();
    protected abstract void DoObjectEnterAction(Collider2D other);
    protected abstract void DoObjectExitAction(Collider2D other);
}