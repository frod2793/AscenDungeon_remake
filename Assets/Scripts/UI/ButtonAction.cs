using UnityEngine.SceneManagement;
using UnityEngine;

public class ButtonAction : MonoBehaviour
{
    [SerializeField]
    private GameObject SwitchingObject;

    public void ActiveSwitching()
    {
        SwitchingObject.SetActive(!SwitchingObject.activeSelf);
    }

    public void PauseSwitching()
    {
        if (Time.timeScale > 0f)
        {
            Castle.Instance.PauseEnable();

            Time.timeScale = 0f;
        }
        else
        {
            Castle.Instance.PauseDisable();

            Time.timeScale = 1f;
        }
    }
    public void ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        #region Release Pause

        Castle.Instance.PauseDisable();

        Time.timeScale = 1f;

        #endregion
    }
}
