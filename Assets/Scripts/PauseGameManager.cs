using UnityEngine;

public class PauseGameManager : MonoBehaviour
{
    private bool isPaused = false;
    private float previousTimeScale;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // �C�ӂ̃L�[��ݒ�
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            previousTimeScale = Time.timeScale;//�i�s���Ԃ�ۑ�
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = previousTimeScale;
        }
    }
}
