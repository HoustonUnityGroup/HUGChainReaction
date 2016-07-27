using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UI : MonoBehaviour 
{
	[SerializeField]
    private GameManager m_gameManager = null;

    [SerializeField]
    Text m_score = null;
    [SerializeField]
    Text m_status = null;
    [SerializeField]
    Text m_resultScore = null;
    [SerializeField]
    Text m_resultStatus = null;


    private void Update()
	{
		// we ensure that each reference is properly set
		// otherwise, we report an error and throw an exception
		Assert.IsNotNull(m_gameManager);
		Assert.IsNotNull(m_score);
		Assert.IsNotNull(m_status);

		// format the text to fill in the value with our score number
		m_score.text = string.Format("Score: {0:N0}", m_gameManager.Score);

		// change the status text based on the game's current state.
		m_status.text = !m_gameManager.IsGameOver ? 
			"Reactions Needed: " + m_gameManager.neededReactions : m_gameManager.DidPlayerWin ? 
			"You Won!" : " You Lost...";

        m_resultScore.text = string.Format("{0:N0}", m_gameManager.Score);
        m_resultStatus.text = m_status.text;

    }
}
