using UnityEngine;

namespace KazukiJanken
{
    public class QuitGames : MonoBehaviour
    {
        public void QuitGame()
        {
            // ゲームを終了する
            Application.Quit();

            // UnityEditor上で実行している場合、エディタを停止する（ビルド実行時には無視される）
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
