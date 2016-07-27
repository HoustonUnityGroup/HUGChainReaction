using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private ScreenManager m_ScreenManager = null;         // Link to main menu 

    public Ball ball;                       // Prefab for balls
    public Ball spark;                      // Prefab for spark
    public Transform ballHolder;            // Placeholder for all balls

    public int totBalls = 5;                // Total number of balls in scene
    public int neededReactions = 1;         // Reactions need to win level
    private List<Ball> balls;               // Refrence list of all balls in scene

    private bool droppedSpark = false;      // Have we started playing by dropping the spark
    private bool gameOver = false;          // Game is over
    private bool didWin = false;            // Did you win the game
    private bool inGame = false;            // Is the game currently being played

    public bool IsGameOver { get { return gameOver; } } // is the game over
    public bool DidPlayerWin { get { return didWin; } } // did we win

    public int Score { get; set; } // current score property, auto-backed, will not be saved or modifyable in the editor

    public int ChainMultiplier = 2; // chain reaction multiplier for increasing score
    private int totalReactions = 0; // total number of reactions that have occurred

	// transforms to use as the reference points for bonds
	public Transform LeftBound = null;
	public Transform RightBound = null;
	public Transform UpBound = null;
	public Transform DownBound = null;

	private Rect m_boundsRect = new Rect();
    private int level = 1;                  // Just a way to manage the number of balls on screen and reactions needed (Should probably create a level manager to control this!)

    // callback function called when a ball reacts and updates the score.
    public void OnBallScored(Ball ball)
    {
        int points = ball.ScoreValue * ball.ScoreMultiplyer;
        ball.SetPoints(points);
        Score = Score + points;
		totalReactions += 1;
    }

    // Use this for initialization
    void Start ()
    {
        m_ScreenManager.ShowTitle();
        // hook up our spark's event to the scoring function
        spark.OnBallReacted.AddListener(OnBallScored);
    }

    public void StartGame(int level)
    {
        m_ScreenManager.ShowGame();
        Score = 0;

        // disable the sparks collision until we drop it
        spark.GetComponent<Collider2D>().enabled = false;

        // constrains the bounds for the ball placement
        // the margin is hardcoded to the current half-thickness of the walls
        // this can be calculated dyanmically, but I wanted to keep the example
        // simple
        var x_min = LeftBound.transform.position.x + 0.5f;
        var x_max = RightBound.transform.position.x - 0.5f;

        var y_max = UpBound.transform.position.y - 0.5f;
        var y_min = DownBound.transform.position.y + 0.5f;

        m_boundsRect = new Rect(x_min, y_min, x_max - x_min, y_max - y_min);

        // Create balls for level
        balls = new List<Ball>(totBalls * level);
        for (int i = 0; i < totBalls * level; i++)
        {
            Ball b = Instantiate(ball, Vector3.zero, Quaternion.identity) as Ball;
            balls.Add(b.GetComponent<Ball>());
            balls[i].transform.SetParent(ballHolder);
            var random_vect = new Vector2(Random.Range(x_min, x_max), Random.Range(y_min, y_max));
            balls[i].transform.position = random_vect;
            balls[i].GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

            balls[i].Push();

            // hook up an spark's event to the scoring function
            balls[i].OnBallReacted.AddListener(OnBallScored);
        }

        // Reset game state
        inGame = true;
        didWin = false;
        gameOver = false;
        ResetSpark();
    }

    void ResetSpark()
    {
        droppedSpark = false;
        spark.didReact = false;
        spark.didShrink = false;
        spark.gameObject.SetActive(true);
        spark.transform.localScale = Vector3.one;
        spark.gameObject.layer = LayerMask.NameToLayer("Reaction");
    }

    void ClearGame()
    {
        foreach (Ball b in balls)
        {
            if (b != null)
                Destroy(b.gameObject);
        }
    }

    // checks the x/y bounds for the point
    private bool IsInBounds(Vector3 _point)
	{
		return m_boundsRect.Contains (_point);
	}

	// gets the spark point in our bounds, currently everything is at 0 z-depth
	// this may change if you push it back
	private Vector3 GetSparkPoint()
	{
		var spark_drop_pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		spark_drop_pos.z = 0.0f;
		return spark_drop_pos;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (inGame)
        {
            // Move starter spark, or hide it if out of bounds
            if (!spark.didReact && !spark.didShrink)
            {
                var spark_pos = GetSparkPoint();
                if (IsInBounds(spark_pos))
                {
                    spark.gameObject.SetActive(true);
                    spark.transform.position = spark_pos;
                }
                else
                    spark.gameObject.SetActive(false);
            }

            // drop Starter spark
            if (spark.gameObject.activeSelf && !droppedSpark && Input.GetMouseButtonDown(0))
            {
                spark.gameObject.SetActive(true);
                spark.transform.position = GetSparkPoint();
                spark.GetComponent<Collider2D>().enabled = true;
                droppedSpark = true;
                spark.React();
            }

            // Check to see if there are any reactions going
            bool stillReacting = false;
            foreach (Ball b in balls)
            {
                // Are we still reacting
                if (b.didReact && !b.didShrink)
                    stillReacting = true;
            }

            // Don't forget to check your spark
            if (spark.didReact && !spark.didShrink)
                stillReacting = true;

            // if no more reactions game over
            if (droppedSpark && !stillReacting)
            {
                if (totalReactions - 1 >= neededReactions)
                {
                    didWin = true;
                }

                gameOver = true;
                m_ScreenManager.ShowResult();
                inGame = false;
                ClearGame();
            }
        }
    }
}
