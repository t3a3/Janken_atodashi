using UnityEngine;

namespace KazukiJanken
{
    public class QuitGames : MonoBehaviour
    {
        public void QuitGame()
        {
            // �Q�[�����I������
            Application.Quit();

            // UnityEditor��Ŏ��s���Ă���ꍇ�A�G�f�B�^���~����i�r���h���s���ɂ͖��������j
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
