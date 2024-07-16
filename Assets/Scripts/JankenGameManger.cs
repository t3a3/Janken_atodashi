using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace KazukiJanken
{
    public class JankenGameManger : MonoBehaviour
    {

        //ゲーム中かどうかの判定
        [SerializeField]
        bool gaming = false;

        //ゲームクリアした時に表示させるオブジェクト
        [SerializeField]
        GameObject gameClearObj;

        //-----時間関係
        [Header("----------------------")]
        [Header("時間")]
        [Header("----------------------")]
        [Header("制限時間")]
        [SerializeField]
        float timeLimit = 30f;

        [Header("進行時間")]
        [SerializeField]
        float progressTime = 0f;//進行している時間
        [SerializeField]
        Text timeLeftText;//残り時間を表示させるテキスト

        //-----得点関係
        [Header("----------------------")]
        [Header("得点")]
        [Header("----------------------")]
        [SerializeField]
        int totalScore;//合計得点
        const int MIN_SCORE = 0;
        int answerStreak = 0;//連続正解数
        int highScore;//最高得点
        [SerializeField]
        Text[] totalScoreText;//合計得点のテキスト
        [SerializeField]
        Text highScoreText;//最高得点のテキスト

        [Header("----------------------")]
        [Header("判定")]
        [Header("----------------------")]
        [SerializeField]
        CanvasGroup judgeCanvasGroup; //判定のキャンバスグループ

        [SerializeField]
        Image judgeImage;

        /// <summary>
        /// 0が正解、１が不正解
        /// </summary>
        [SerializeField]
        Sprite[] judgeSprite;

        /// <summary>
        /// 0が正解、１が不正解
        /// </summary>
        [SerializeField]
        AudioClip[] judgeAudioClips;

        //じゃんけんの手
        public enum HandType
        {
            [InspectorName("グー")]
            Rock,
            [InspectorName("チョキ")]
            Scissors,
            [InspectorName("パー")]
            Paper
        }

        //-----
        [Header("----------------------")]
        [Header("プレイヤー")]
        [Header("----------------------")]
        public HandType playerHand;

        [SerializeField]
        bool playerSelected = false;
        //-----
        [Header("----------------------")]
        [Header("敵")]
        [Header("----------------------")]
        public HandType enemyHand;

        //下記２つをまとめたキャンバスグループ
        [SerializeField]
        CanvasGroup enemyHandCanvasGroup;

        //じゃんけんのテキスト
        [SerializeField]
        Image enemyHandImage;

        /// <summary>
        /// 0がグー、1がチョキ、2がパー
        /// </summary>
        [SerializeField]
        Sprite[] enemyhandSprite;

        //じゃんけんの指示
        public enum InstructionsType
        {
            [InspectorName("勝ち")]
            Win,
            [InspectorName("負け")]
            Lose,
            [InspectorName("引き分け")]
            Draw
        }
        public InstructionsType instructions;

        [SerializeField]
        Text instructText;//指示を表示させるテキスト

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
            timeLeftText.text = $"残り時間:{limittime.ToString()}秒";
            if (0 > limittime)
            {
                GameClear();
                timeLeftText.text = $"残り時間:0秒";
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
            totalScoreText[0].text = $"得点：0";
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = highScore.ToString();
            //-----
            instructText.text = "";
            //-----
            answerStreak = 0;
            //-----
            progressTime = timeLimit;
            timeLeftText.text = $"残り時間:{timeLimit}秒";
        }

        void JankenJudge()
        {
            // 判定ロジック
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
            totalScoreText[0].text = $"得点：{totalScore.ToString()}";
        }

        void GameClear()
        {
            StopGame();
            gaming = false;
            gameClearObj.SetActive(true);
            totalScoreText[1].text = $"得点：{totalScore.ToString()}";
            if (totalScore > highScore)
            {
                highScore = totalScore;
                PlayerPrefs.SetInt("HighScore", totalScore);
            }
            highScoreText.text = $"最高得点：{highScore.ToString()}";
        }

        //-----------
        //プレイヤー関連の関数
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
            //Debug.Log($"プレイヤーの手 : <size=28> {playerHand} </size>");
        }

        //-----------
        //敵関連の関数
        //-----------
        void EnemySelectHand()
        {
            instructText.text = "";
            enemyHandCanvasGroup.alpha = 0;
            enemyHand = (HandType)Random.Range(0, 3);
            instructions = (InstructionsType)Random.Range(0, 3);
            //Debug.Log($"相手の手 : <size=28> {enemyHand} </size>");
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
                    instructText.text = "勝って";
                    break;
                case InstructionsType.Lose:
                    instructText.text = "負けて";
                    break;
                case InstructionsType.Draw:
                    instructText.text = "引き分けて";
                    break;
            }
            enemyHandCanvasGroup.alpha = 1;
        }
    }
}

