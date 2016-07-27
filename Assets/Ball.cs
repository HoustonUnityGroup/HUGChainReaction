using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class BallScoreEvent : UnityEvent<Ball> { }

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField]
    private GameObject m_shrankParticleSystemContainer = null;
    [SerializeField]
    private TextMesh m_pointsContainer = null;

    [SerializeField, Range(0.1f, 10.0f)]
    private float m_particleSystemCleanupDelay = 3.0f;

	private GameObject m_activeParticleSystemContainer = null;

    private Rigidbody2D myBody;         // Physics for your ball
    public float minSpeed = 5.0f;       // minimum speed ball can travel
    public float maxSpeed = 10.0f;      // maximum speed ball can travel
    public bool didReact;               // did your ball react
    private float reactionTime;         // timer when Reaction happened
    public bool didShrink;              // did the reaction end

    [Range(0.1f, 5.0f)]
    public float shrinkDelay = 5.0f; // delay to wait on for the ball to shrink

    // event to notify our game manager that this ball has reacted
	[SerializeField] private BallScoreEvent m_onBallReacted = new BallScoreEvent();

	// property to encapsulate the react event so it can't accidently be nulled out
	// from outside this class
	public BallScoreEvent OnBallReacted { get { return m_onBallReacted; } }

    [Range(100, 1000)]
    public int ScoreValue = 500;        // base score value of the ball
    public int ScoreMultiplyer = 1;     // multiplyer for chain reactions
	
	// Update is called once per frame
	void Update ()
    {
        // Do we need to shrink now?
        if (didReact && !didShrink)
        {
            // If 5 seconds have passed start to shrink
			if (Time.timeSinceLevelLoad - reactionTime > shrinkDelay)
                Shrink();
        }
    }

    // Start moving the balls 
    public void Push()
    {
        if (myBody == null)
            myBody = GetComponent<Rigidbody2D>();
        myBody.AddForce(new Vector2(Random.Range(minSpeed, maxSpeed), Random.Range(minSpeed, maxSpeed)));
    }

    // Make the balls React
    public void React()
    {
    	if(didReact) return;

        transform.localScale = new Vector3(5, 5, 5);
        didReact = true;
		reactionTime = Time.timeSinceLevelLoad;

		if(myBody == null)
			myBody = GetComponent<Rigidbody2D>();

		// stop the dynamic physics
		myBody.Sleep();
		myBody.isKinematic = true;

		// call our ball scoring event
		m_onBallReacted.Invoke(this);
    }

    // Make the ball shrink
    public void Shrink()
    {
    	if(didShrink) return;

    	transform.localScale = Vector3.zero;
        gameObject.layer = LayerMask.NameToLayer("Ball");

        // spawn our particle system container, and set a coroutine to clean it up
        if (m_shrankParticleSystemContainer != null && !m_activeParticleSystemContainer)
        {
            m_activeParticleSystemContainer = Instantiate(m_shrankParticleSystemContainer);
            
			m_activeParticleSystemContainer.transform.SetParent(transform.parent, false);

            // move to our ball's location
			m_activeParticleSystemContainer.transform.position = transform.position;

            StartCoroutine(WaitForParticleToDie());
        }
            

        didShrink = true;
    }

    private IEnumerator WaitForParticleToDie()
    {
		// wait for the delay to finish
        yield return new WaitForSeconds(m_particleSystemCleanupDelay);

		// since this is a game object, we can safely destroy it and all of it's children
        Destroy(m_activeParticleSystemContainer);
    }

    // Did I hit something?
    void OnCollisionEnter2D(Collision2D coll)
    {
        // Did I hit a Ball? - both should have the same tag
        if (CompareTag(coll.gameObject.tag))
        {
        	var other_ball = coll.gameObject.GetComponent<Ball>();
            // Has it reacted
            if (other_ball.didReact && !other_ball.didShrink)
            {
            	var other_rigidbody = coll.gameObject.GetComponent<Rigidbody2D>();
                // stop things from moving
				other_rigidbody.velocity = Vector3.zero;

                if (myBody == null)
                    myBody = GetComponent<Rigidbody2D>();

                // stop and react
                myBody.velocity = Vector3.zero;
				other_rigidbody.velocity = Vector3.zero;
                ScoreMultiplyer = other_ball.ScoreMultiplyer + 1;
                other_ball.React();
                React();
                gameObject.layer = LayerMask.NameToLayer("Reaction");
            }
        }
    }

    public void SetPoints(int value)
    {
        m_pointsContainer.text = value.ToString();
    }
}
