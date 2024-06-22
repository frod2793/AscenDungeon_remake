using UnityEngine;

public class RecognitionModule : MonoBehaviour
{
    public bool IsLookAtLeft => _IsLookAtLeft;
    private Player _Player;

    [SerializeField] private bool _IsLookAtLeft;

    public void PlayerEnter(Player player)
    {
        _Player = player;
    }
    public void PlayerExit()
    {
        _Player = null;
    }
    public void SetLookingLeft(bool lookAtLeft)
    {
        _IsLookAtLeft = lookAtLeft;

        if (_IsLookAtLeft)
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        else
            transform.rotation = Quaternion.Euler(Vector3.up * 180);
    }
    public bool IsLookAtPlayer()
    {
        if (_Player != null)
        {
            return (_Player.transform.position.x < transform.position.x &&  _IsLookAtLeft ||
                    _Player.transform.position.x > transform.position.x && !_IsLookAtLeft);
        }
        return false;
    }
}
