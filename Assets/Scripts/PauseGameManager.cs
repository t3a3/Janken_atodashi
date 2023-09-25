using UnityEngine;

public class PauseGameManager : MonoBehaviour
{
    private bool isPaused = false;
    private float previousTimeScale;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // 任意のキーを設定
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
            previousTimeScale = Time.timeScale;//進行時間を保存
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
