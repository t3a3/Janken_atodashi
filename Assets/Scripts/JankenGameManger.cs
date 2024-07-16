using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace KazukiJanken
{
    public class JankenGameManger : MonoBehaviour
    {

        //�Q�[�������ǂ����̔���
        [SerializeField]
        bool gaming = false;

        //�Q�[���N���A�������ɕ\��������I�u�W�F�N�g
        [SerializeField]
        GameObject gameClearObj;

        //-----���Ԋ֌W
        [Header("----------------------")]
        [Header("����")]
        [Header("----------------------")]
        [Header("��������")]
        [SerializeField]
        float timeLimit = 30f;

        [Header("�i�s����")]
        [SerializeField]
        float progressTime = 0f;//�i�s���Ă��鎞��
        [SerializeField]
        Text timeLeftText;//�c�莞�Ԃ�\��������e�L�X�g

        //-----���_�֌W
        [Header("----------------------")]
        [Header("���_")]
        [Header("----------------------")]
        [SerializeField]
        int totalScore;//���v���_
        const int MIN_SCORE = 0;
        int answerStreak = 0;//�A������
        int highScore;//�ō����_
        [SerializeField]
        Text[] totalScoreText;//���v���_�̃e�L�X�g
        [SerializeField]
        Text highScoreText;//�ō����_�̃e�L�X�g

        [Header("----------------------")]
        [Header("����")]
        [Header("----------------------")]
        [SerializeField]
        CanvasGroup judgeCanvasGroup; //����̃L�����o�X�O���[�v

        [SerializeField]
        Image judgeImage;

        /// <summary>
        /// 0�������A�P���s����
        /// </summary>
        [SerializeField]
        Sprite[] judgeSprite;

        /// <summary>
        /// 0�������A�P���s����
        /// </summary>
        [SerializeField]
        AudioClip[] judgeAudioClips;

        //����񂯂�̎�
        public enum HandType
        {
            [InspectorName("�O�[")]
            Rock,
            [InspectorName("�`���L")]
            Scissors,
            [InspectorName("�p�[")]
            Paper
        }

        //-----
        [Header("----------------------")]
        [Header("�v���C���[")]
        [Header("----------------------")]
        public HandType playerHand;

        [SerializeField]
        bool playerSelected = false;
        //-----
        [Header("----------------------")]
        [Header("�G")]
        [Header("----------------------")]
        public HandType enemyHand;

        //���L�Q���܂Ƃ߂��L�����o�X�O���[�v
        [SerializeField]
        CanvasGroup enemyHandCanvasGroup;

        //����񂯂�̃e�L�X�g
        [SerializeField]
        Image enemyHandImage;

        /// <summary>
        /// 0���O�[�A1���`���L�A2���p�[
        /// </summary>
        [SerializeField]
        Sprite[] enemyhandSprite;

        //����񂯂�̎w��
        public enum InstructionsType
        {
            [InspectorName("����")]
            Win,
            [InspectorName("����")]
            Lose,
            [InspectorName("��������")]
            Draw
        }
        public InstructionsType instructions;

        [SerializeField]
        Text instructText;//�w����\��������e�L�X�g

        UniTask _playGameTask;

        private CancellationTokenSource _cts;

        //-----------
        private void Awake()
        {
            InitializationGame();
        }

        private void Update()
        {
            if (gaming == false) return;
            progressTime -= Time.deltaTime;
            float limittime = Mathf.Floor(progressTime * 100) / 100;
            timeLeftText.text = $"�c�莞��:{limittime.ToString()}�b";
            if (0 > limittime)
            {
                GameClear();
                timeLeftText.text = $"�c�莞��:0�b";
            }
        }

        public void PlayGame()
        {
            _cts = new CancellationTokenSource();
            PlayGameAsync(_cts.Token).Forget();
        }

        public void StopGame()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTask PlayGameAsync(CancellationToken token)
        {
            await CountAsync(token);
            gaming = true;
            while (gaming == true || !token.IsCancellationRequested)
            {
                playerSelected = false;
                judgeCanvasGroup.alpha = 0;
                EnemySelectHand();
                await UniTask.WaitUntil(() => playerSelected == true, cancellationToken: token);
                JankenJudge();
                await UniTask.Delay(1000, cancellationToken: token);
            }
            gaming = false;
        }

        private async UniTask CountAsync(CancellationToken token)
        {
            instructText.text = "3";
            await UniTask.Delay(1000, cancellationToken: token);
            instructText.text = "2";
            await UniTask.Delay(1000, cancellationToken: token);
            instructText.text = "1";
            await UniTask.Delay(1000, cancellationToken: token);
            instructText.text = "-START-";
            await UniTask.Delay(1000, cancellationToken: token);
        }


        public void InitializationGame()
        {
            gaming = false;
            //-----
            enemyHandCanvasGroup.alpha = 0;
            judgeCanvasGroup.alpha = 0;
            //-----
            totalScore = 0;
            totalScoreText[0].text = $"���_�F0";
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = highScore.ToString();
            //-----
            instructText.text = "";
            //-----
            answerStreak = 0;
            //-----
            progressTime = timeLimit;
            timeLeftText.text = $"�c�莞��:{timeLimit}�b";
        }

        void JankenJudge()
        {
            // ���胍�W�b�N
            switch (instructions)
            {
                case InstructionsType.Win:
                    if ((playerHand == HandType.Rock && enemyHand == HandType.Scissors) ||
                     (playerHand == HandType.Paper && enemyHand == HandType.Rock) ||
                     (playerHand == HandType.Scissors && enemyHand == HandType.Paper))
                    {
                        Win();
                    }
                    else
                    {
                        Lose();
                    }
                    break;
                case InstructionsType.Lose:
                    if ((enemyHand == HandType.Rock && playerHand == HandType.Scissors) ||
                     (enemyHand == HandType.Paper && playerHand == HandType.Rock) ||
                     (enemyHand == HandType.Scissors && playerHand == HandType.Paper))
                    {
                        Win();

                    }
                    else
                    {
                        Lose();
                    }
                    break;
                case InstructionsType.Draw:
                    if (playerHand == enemyHand)
                    {
                        Win();

                    }
                    else
                    {
                        Lose();
                    }
                    break;
            }
            judgeCanvasGroup.alpha = 1;
        }

        void Win()
        {
            AudioManager.instance.PlayGamingSE(judgeAudioClips[0]);
            judgeImage.sprite = judgeSprite[0];
            AddScore(100);
            answerStreak++;
            //Debug.Log("<size=18> Win </size>");
        }
        void Lose()
        {
            AudioManager.instance.PlayGamingSE(judgeAudioClips[1]);
            judgeImage.sprite = judgeSprite[1];
            answerStreak = 0;
            AddScore(-50);
            //Debug.Log("<size=18> Lose </size>");
        }
        void AddScore(int score)
        {
            totalScore += score + (answerStreak * 10);
            if (totalScore < 0) totalScore = MIN_SCORE;
            totalScoreText[0].text = $"���_�F{totalScore.ToString()}";
        }

        void GameClear()
        {
            StopGame();
            gaming = false;
            gameClearObj.SetActive(true);
            totalScoreText[1].text = $"���_�F{totalScore.ToString()}";
            if (totalScore > highScore)
            {
                highScore = totalScore;
                PlayerPrefs.SetInt("HighScore", totalScore);
            }
            highScoreText.text = $"�ō����_�F{highScore.ToString()}";
        }

        //-----------
        //�v���C���[�֘A�̊֐�
        //-----------
        public void PlayerSelectHand(int num)
        {
            if (playerSelected) return;
            switch (num)
            {
                case 0:
                    playerHand = HandType.Rock;
                    break;
                case 1:
                    playerHand = HandType.Scissors;
                    break;
                case 2:
                    playerHand = HandType.Paper;
                    break;
            }
            playerSelected = true;
            //Debug.Log($"�v���C���[�̎� : <size=28> {playerHand} </size>");
        }

        //-----------
        //�G�֘A�̊֐�
        //-----------
        void EnemySelectHand()
        {
            instructText.text = "";
            enemyHandCanvasGroup.alpha = 0;
            enemyHand = (HandType)Random.Range(0, 3);
            instructions = (InstructionsType)Random.Range(0, 3);
            //Debug.Log($"����̎� : <size=28> {enemyHand} </size>");
            switch (enemyHand)
            {
                case HandType.Rock:
                    enemyHandImage.sprite = enemyhandSprite[0];
                    break;
                case HandType.Scissors:
                    enemyHandImage.sprite = enemyhandSprite[1];
                    break;
                case HandType.Paper:
                    enemyHandImage.sprite = enemyhandSprite[2];
                    break;
            }
            switch (instructions)
            {
                case InstructionsType.Win:
                    instructText.text = "������";
                    break;
                case InstructionsType.Lose:
                    instructText.text = "������";
                    break;
                case InstructionsType.Draw:
                    instructText.text = "����������";
                    break;
            }
            enemyHandCanvasGroup.alpha = 1;
        }
    }
}

