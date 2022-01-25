using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

    public class GameplayManager : MonoBehaviour
    {
        public Turn turn = Turn.NONE;                           // Player 1 or Player 2 turn
        public float stonesSpeed = 0.01f;                       // How fast are the stones are placed

        [Header("Game Configuration")]
        [Tooltip("You can change this in the editor to simulate. Don't forget to change the PlayerDataManager too")]
        private GameConfiguration gameConfiguration;            // The Game Configuration
        public GameConfiguration.GameMode gameMode;             // The Game Configuration Mode

        public GameState state = GameState.INITIAL;             // State machine	
        private List<Pit> pits;                               // The 14 pits
        private Pit lastPit;                                  // The last pit where the stone landed on
        private float turnDelay;                                // How long the current player is delaying its turn
        private SungkaBoard board;

        public static GameplayManager instance;

        // Returns dynamically the player 1 data
        private PlayerData player1Data
        {
            get { return PlayerDataManager.instance.playerData; }
            set { PlayerDataManager.instance.playerData = value; }
        }

        // Returns dynamically the turn time
        private float turnTime
        {
            get { return gameConfiguration.turnTime; }
        }

        public enum Turn
        {
            NONE,
            PLAYER_1,
            PLAYER_2
        }

        // Turn State Machine
        public enum GameState
        {
            INITIAL,
            START,
            TURN_START,
            MOVE_STONES,
            CAPTURE_STONES,
            CHECK_END_CONDITION,
            END
        }

        public enum WinState
        {
            PLAYER_1_WIN,
            PLAYER_2_WIN,
            DRAW
        }

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            gameMode = player1Data.gameMode;

            Initialize();
            GameInterfaceManager.instance.Initialize();
            StartGame();
        }

        void Update()
        {
            switch (state)
            {
                case GameState.INITIAL:
                    break;
                case GameState.START:
                    state = GameState.TURN_START;
                    StartCoroutine("HandleTimerCR");
                    break;
                case GameState.TURN_START:
                    // Waits for OnPitClick
                    // If is against an AI, calculates the best movement and plays for this player
                    if (gameMode == GameConfiguration.GameMode.AGAINST_AI && turn == Turn.PLAYER_2)
                    {
                        PlayAI();
                    }
                    break;
                case GameState.MOVE_STONES:
                    // Waits for the coroutine MovingStonesCR
                    break;
                case GameState.CAPTURE_STONES:
                    // Waits for the player to click or the AI plays
                    if (gameMode == GameConfiguration.GameMode.AGAINST_AI && turn == Turn.PLAYER_2)
                    {
                        CaptureStonesAI();
                    }
                    break;
                case GameState.CHECK_END_CONDITION:
                    CheckEndCondition();
                    break;
            }
        }

        // Initializes the board
        public void Initialize()
        {
            gameConfiguration = ConfigurationManager.instance.GetGameConfigurationByType(gameMode);

            LoadBoard();

            GameInterfaceManager.instance.UpdateScore(0, 0);
            InitializeStones();
        }

        public void InitializeStones()
        {
            StartCoroutine(InitializeStonesCR());
        }

        public void StartGame()
        {
            state = GameState.START;
        }

        // Places the pieces with a small delay
        IEnumerator InitializeStonesCR()
        {
            // Starts populating the pits
            for (int i = 0; i < 14; i++)
            {
                Pit pit = GetPit(i);
                if (!pit.house)
                {
                    for (int s = 0; s < 4; s++)
                    {
                        CreateStone(pit.number);
                        yield return new WaitForSeconds(0.001f);
                    }
                    pit.stones = 4;
                }
            }
            turn = Turn.PLAYER_1;
        }

        // Highlight a pit selection
        public void HighlightPit(int pitNum, bool show)
        {
            Pit pit = GetPit(pitNum);
            pit.pitSelect.GetComponent<Renderer>().enabled = show;
        }

        // Loads the board
        public void LoadBoard()
        {
            // This is for testing purposes. If I find a board in the scene, ignore the player preferences
            var boardGo = Instantiate(gameConfiguration.boardPrefab);

            board = boardGo.GetComponent<SungkaBoard>();

            Pit[] pitsTmp = board.GetComponentsInChildren<Pit>();
            pits = new List<Pit>();

            foreach (Pit pit in pitsTmp)
            {
                pits.Add(pit);
            }

            // Sorts the pit position
            pits.OrderBy(pit => pit.number);
        }

        // Created a random stone object
        public void CreateStone(int pitNum)
        {
            var stonePrefab = gameConfiguration.GetRandomStone();
            GameObject stoneGo = GameObject.Instantiate(stonePrefab, Vector3.zero, Quaternion.identity) as GameObject;
            stoneGo.name = "Stone";

            Transform pitTra = GetPit(pitNum).transform;

            PlaceStone(pitTra, stoneGo);
        }

        // Places a stone randomly inside a pit
        private void PlaceStone(Transform pitTra, GameObject stone)
        {
            stone.transform.parent = pitTra;

            // Make Dynamic
            SphereCollider sc = pitTra.GetComponent<Collider>() as SphereCollider;
            float posX = UnityEngine.Random.Range(-sc.radius / 2, sc.radius / 2);
            float posY = sc.radius / 2;
            float posZ = UnityEngine.Random.Range(-sc.radius / 2, sc.radius / 2);

            stone.transform.localPosition = new Vector3(posX, posY, posZ);
            stone.transform.rotation = UnityEngine.Random.rotation;

            // Starts dropping fast
            stone.GetComponent<Rigidbody>().velocity = Vector3.down;
        }

        public void OnPitClick(int pitNum)
        {
            OnPitClick(pitNum, false);
        }

        // Action when the player clicks a pit
        public void OnPitClick(int pitNum, bool aiPlay)
        {
            if (state == GameState.TURN_START)
            {
                //checks if the player can select the pit
                if (CanSelectPit(pitNum, aiPlay))
                {
                    MoveStones(pitNum, aiPlay);
                }
                else
                {
                    // Shakes board
                    ShakeStones(pitNum);
                }
            }
            else if (state == GameState.CAPTURE_STONES)
            {
                if (GetOppositePitNum(lastPit.number) == pitNum)
                {
                    CaptureStones(pitNum, aiPlay);
                }
                else
                {
                    // Shakes board
                    ShakeStones(pitNum);
                }
            }
        }

        private void ShakeStones(int pitNum)
        {
            Pit pit = GetPit(pitNum);
            Stone[] stones = pit.transform.GetComponentsInChildren<Stone>();

            foreach (Stone stone in stones)
            {
                Vector3 force = UnityEngine.Random.insideUnitSphere * 100;

                stone.GetComponent<Rigidbody>().AddForce(force);
            }
        }

        // Moves the stones to the pit
        public void MoveStones(int pitNum, bool aiPlay)
        {
            state = GameState.MOVE_STONES;
            StartCoroutine(MovingStonesCR(pitNum, aiPlay));

            // Stops timer
            StopCoroutine("HandleTimerCR");
        }

        // Stones movement animation
        IEnumerator MovingStonesCR(int pitNum, bool aiPlay)
        {
            // Waits a while before playing
            if (aiPlay)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(1, 3));
            }

            Pit pit = GetPit(pitNum);
            int stonesPool = pit.stones;
            pit.stones = 0;
            Stone[] stones = pit.transform.GetComponentsInChildren<Stone>();

            // Place the stones in another place not to be shown
            foreach (Stone stone in stones)
            {
                stone.transform.position = Vector3.one * 150;

            }

            int nextPitNum = pit.number;

            while (stonesPool > 0)
            {
                GameObject stoneGo = stones[stonesPool - 1].gameObject;

                nextPitNum = GetNextPit(nextPitNum);

                Pit nextPit = GetPit(nextPitNum);

                // Skips the other player house pit
                if (nextPit.house && ((turn == Turn.PLAYER_1 && !nextPit.p1Owner) || (turn == Turn.PLAYER_2 && nextPit.p1Owner)))
                {
                    nextPitNum = GetNextPit(nextPitNum);
                    nextPit = GetPit(nextPitNum);
                }

                nextPit.stones++;
                stonesPool--;

                PlaceStone(nextPit.transform, stoneGo);

                yield return new WaitForSeconds(stonesSpeed);
            }

            // Disables highlight
            foreach (Stone stone in stones)
            {
                stone.Highlight(false, true);
            }

            lastPit = GetPit(nextPitNum);

            // If the final stone lands on an empty hole on your side, grabs all stones on the opposite site and places in your mancala
            if (lastPit.stones == 1 && !lastPit.house && ((turn == Turn.PLAYER_1 && lastPit.p1Owner) || (turn == Turn.PLAYER_2 && !lastPit.p1Owner)))
            {
                // Waits for the other player to grab the stones
                Pit oppositePit = GetPit(GetOppositePitNum(lastPit.number));
                stonesPool = oppositePit.stones;

                if (stonesPool > 0)
                {
                    Debug.Log("Got the other player stones");
                    if (aiPlay || turn == Turn.PLAYER_2)
                    {
                        if (aiPlay)
                        {
                            GameInterfaceManager.instance.ShowMessage("The other player got your stones");
                        }
                        else
                        {
                            GameInterfaceManager.instance.ShowMessage("You can get the other player stones");
                        }
                    }
                    else
                    {
                        GameInterfaceManager.instance.ShowMessage("You can get the other player stones");
                    }

                    state = GameState.CAPTURE_STONES;

                    // Highlight the stones of the pit
                    stones = oppositePit.transform.GetComponentsInChildren<Stone>();
                    foreach (Stone stone in stones)
                    {
                        stone.Highlight(true, true);
                    }

                    // Restart the timer
                    StartCoroutine("HandleTimerCR");
                    yield break;
                }
            }

            state = GameState.CHECK_END_CONDITION;
        }

        private void CaptureStonesAI()
        {
            OnPitClick(GetOppositePitNum(lastPit.number), true);

            state = GameState.MOVE_STONES;
        }

        // Grabs the other player stones and places in the house.
        public void CaptureStones(int pitNum, bool aiPlay)
        {
            StartCoroutine(CaptureStonesCR(pitNum, aiPlay));
        }

        private IEnumerator CaptureStonesCR(int pitNum, bool aiPlay)
        {
            // Delays just for the message to show
            if (aiPlay)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(1, 3));
            }

            Pit pit = GetPit(pitNum);
            int stonesPool = pit.stones;

            Stone[] stones = pit.transform.GetComponentsInChildren<Stone>();
            pit.stones = 0;

            Pit playerHole = turn == Turn.PLAYER_1 ? GetPit(7) : GetPit(0);

            // Place the stones in another place so its not shown
            // TODO - Make a cool animation
            foreach (Stone stone in stones)
            {
                stone.transform.position = Vector3.one * 100;
            }

            while (stonesPool > 0)
            {
                playerHole.stones++;

                PlaceStone(playerHole.transform, stones[stonesPool - 1].gameObject);

                stonesPool--;
                yield return new WaitForSeconds(stonesSpeed);
            }

            // Capture single Opposite Stone
            Pit oppositePit = GetPit(GetOppositePitNum(pitNum));
            stonesPool = oppositePit.stones;
            oppositePit.stones = 0;

            Stone[] oppositeStones = oppositePit.transform.GetComponentsInChildren<Stone>();

            while (stonesPool > 0)
            {
                playerHole.stones++;

                PlaceStone(playerHole.transform, oppositeStones[stonesPool - 1].gameObject);
                stonesPool--;
                yield return new WaitForSeconds(stonesSpeed);
            }

            foreach (Stone stone in stones)
            {
                stone.Highlight(false, true);
            }

            state = GameState.CHECK_END_CONDITION;
        }

        // Grabs the opposite pit
        private int GetOppositePitNum(int pitNum)
        {
            return 14 - pitNum;
        }

        // Grabs the other player pit on the other side
        private int GetOtherPlayerPitNum(int pitNum)
        {
            return pitNum + 7;
        }

        // Verifies if the player pits are all empty
        private void CheckEndCondition()
        {
            bool pitsP1Empty = true;
            bool pitsP2Empty = true;

            // Player 1 Pits
            for (int i = 1; i < 7; i++)
            {
                if (GetPit(i).stones > 0)
                {
                    pitsP1Empty = false;
                    break;
                }
            }

            // Player 2 Pits
            for (int i = 8; i < 14; i++)
            {
                if (GetPit(i).stones > 0)
                {
                    pitsP2Empty = false;
                    break;
                }
            }

            int stonesP1 = GetPit(7).stones;
            int stonesP2 = GetPit(0).stones;

            // Updates the score
            GameInterfaceManager.instance.UpdateScore(stonesP1, stonesP2);

            if (pitsP1Empty || pitsP2Empty)
            {
                WinState winState = WinState.DRAW;

                if (stonesP1 == stonesP2)
                {
                    winState = WinState.DRAW;
                }
                else
                {
                    bool winner = stonesP1 > stonesP2;
                    if (winner)
                    {
                        winState = WinState.PLAYER_1_WIN;
                    }
                    else
                    {
                        winState = WinState.PLAYER_2_WIN;
                    }
                }

                GameInterfaceManager.instance.ShowEndGamePanel(winState);
                StopCoroutine("HandleTimerCR");
                state = GameState.END;
            }
            else
            {
                // checks if landed on the player house and starts the next turn
                // Is the players final pit?
                if (lastPit != null && ((turn == Turn.PLAYER_1 && lastPit.number == 7) ||
                   (turn == Turn.PLAYER_2 && lastPit.number == 0)))
                {
                    if (gameMode == GameConfiguration.GameMode.OFFLINE || (gameMode != GameConfiguration.GameMode.OFFLINE && turn == Turn.PLAYER_1))
                    {
                        GameInterfaceManager.instance.ShowMessage("You can play again");
                    }
                }
                else
                {
                    turn = turn == Turn.PLAYER_1 ? Turn.PLAYER_2 : Turn.PLAYER_1;

                    GameInterfaceManager.instance.SetPlayer(turn);
                }
                state = GameState.TURN_START;
                // Starts the turn timer
                StartCoroutine("HandleTimerCR");
            }

            lastPit = null;
        }

        // Lowers the player timer
        private IEnumerator HandleTimerCR()
        {
            turnDelay = 1f;

            while (turnDelay > 0)
            {
                GameInterfaceManager.instance.UpdateTimer(turn, turnDelay);
                turnDelay -= (Time.deltaTime / turnTime * 0.8f);
                yield return null;
            }

            // If there were no legit play, the game ends and the current player loses
            bool winner = turn == Turn.PLAYER_2 ? true : false;

            WinState win = WinState.DRAW;

            if (winner)
            {
                win = WinState.PLAYER_1_WIN;
            }
            else
            {
                win = WinState.PLAYER_2_WIN;
            }

            GameInterfaceManager.instance.ShowEndGamePanel(win);
        }

        // AI player calculates the best possible movement and plays
        private void PlayAI()
        {
            // The best movements (when you can play again)
            List<int> bestMovements = new List<int>();
            // The good movements (when you can place stones to get the best movements)
            List<int> goodMovements = new List<int>();
            // The possible movements
            List<int> possibleMovements = new List<int>();

            // Cycle between the possible pits
            for (int i = 8; i < 14; i++)
            {
                if (GetPit(i).stones > 0)
                {
                    possibleMovements.Add(i);
                }
            }

            // Tries to pick the best movements from the possibilities
            foreach (int i in possibleMovements)
            {
                Pit pit = GetPit(i);

                // Should calculate if the last stone ends on the ending hole
                // 8 + 6 = 14, 9 + 5 = 14, 10 + 4 = 14 and so on...
                if (pit.stones + i == 14)
                {
                    bestMovements.Add(i);
                }

                // Another best movement is to capture the opposing player stones
                // Forecast the ending stone position
                int stonesPool = pit.stones;
                int endPitNum = pit.number;
                while (stonesPool > 0)
                {
                    endPitNum = GetNextPit(endPitNum);
                    stonesPool--;
                }
                Pit endPit = GetPit(endPitNum);
                if (endPit.stones == 0 && !endPit.p1Owner && !endPit.house)
                {
                    // Now I need to check if the opposing pit if there is at least one stone
                    Pit oppositePit = GetPit(GetOppositePitNum(endPitNum));
                    if (oppositePit.stones > 0)
                    {
                        bestMovements.Add(i);
                    }
                }
            }

            // Forecast the good movements
            foreach (int i in possibleMovements)
            {
                // Starts at position 9 since pos 8 is the minimum
                for (int j = 9; j < 14; j++)
                {
                    // Needs to place at least one piece
                    if (GetPit(j).stones + i == 13)
                    {
                        goodMovements.Add(i);
                        break;
                    }
                }
            }

            int clickPitNum = 8;
            // After calculating choses the best, good then possible moves.
            if (bestMovements.Count > 0)
            {
                clickPitNum = bestMovements[UnityEngine.Random.Range(0, bestMovements.Count)];
            }
            else if (goodMovements.Count > 0)
            {
                clickPitNum = goodMovements[UnityEngine.Random.Range(0, goodMovements.Count)];
            }
            else
            {
                clickPitNum = possibleMovements[UnityEngine.Random.Range(0, possibleMovements.Count)];
            }

            OnPitClick(clickPitNum, true);
        }

        // Grabs the next cycling pit.
        public int GetNextPit(int pitNum)
        {
            if (pitNum == 13)
                return 0;

            return pitNum + 1;
        }

        // Condition for selecting the pit. The player must be the owner and the pit must have at least one stone
        public bool CanSelectPit(int pitNum, bool aiPlay)
        {
            Pit pit = GetPit(pitNum);

            // Cannot select empty pits
            if (pit.stones == 0)
                return false;

            switch (gameMode)
            {
                case GameConfiguration.GameMode.OFFLINE:
                    // can only select my own pits
                    return (turn == Turn.PLAYER_1 && pit.p1Owner) || (turn == Turn.PLAYER_2 && !pit.p1Owner);
                case GameConfiguration.GameMode.AGAINST_AI:
                    // If the AI is playing, can select the pit otherwise can only move my own pieces
                    return (turn == Turn.PLAYER_1 && pit.p1Owner) || aiPlay;
            }


            return false;
        }

        // Restart the game
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Get pit by number
        private Pit GetPit(int pitNum)
        {
            return pits[pitNum];
        }

    }

