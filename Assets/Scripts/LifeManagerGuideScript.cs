using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class LifeManagerGuideScript : MonoBehaviour
{
    // Current lives
    public int lives = 0;

    // The time in seconds of when we should get a new life
    public double timeToGetNewLife = 0;

    // The game time in seconds
    public double gameTime = 0;

    // Is the current time fresh from the server or not?
    bool timeIsFresh = false;

    // Timer to calculate the lives
    public float lifeTimer = 0;

    // Variable to store the time left for a new life
    public int timeLeft = 0;

    public TextMeshProUGUI livesAmountText;

    // How long we want the user to wait before adding a new life
    public double timeToWaitForLife = 30;

    // The max amount of lives the player can have
    public int maxLivesAmount = 5;

    // Are we logged in?
    bool loggedIn = false;

    // Animation curve for newLifeAnimation
    public AnimationCurve newLifeCurve;

    // Image for heart
    public Transform heartImageTransform;

    // Animating the heart
    private Coroutine animateHeartRoutine;

    // Start is called before the first frame update
    void Start()
    {
        // Fetch saved times
        timeToGetNewLife = (double)PlayerPrefs.GetInt("timeToGetNewLife", 0);
        gameTime = (double)PlayerPrefs.GetInt("gameTime", 0);

        // If first time playing, give the player max amount of hearts
        lives = PlayerPrefs.GetInt("hearts", maxLivesAmount);

        GetStartValues();
        UpdateLives();
        StartCoroutine(SetupRoutine());
    }
    void AnimateHeart()
    {
        if (animateHeartRoutine != null)
        {
            StopCoroutine(animateHeartRoutine);
        }
        StartCoroutine(AnimateHeartRoutine());
    }

    IEnumerator AnimateHeartRoutine()
    {
        float timer = 0f;
        float duration = 1f;
        heartImageTransform.localScale = Vector3.one;
        while (timer <= duration)
        {
            heartImageTransform.localScale = Vector3.one * newLifeCurve.Evaluate(timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        heartImageTransform.localScale = Vector3.one;

    }

    void GetStartValues()
    {
        // Fetch saved times when we start the game
        timeToGetNewLife = (double)PlayerPrefs.GetInt("timeToGetNewLife", 0);
        gameTime = (double)PlayerPrefs.GetInt("gameTime", 0);

        // If first time playing, give the player max amount of hearts
        lives = PlayerPrefs.GetInt("hearts", maxLivesAmount);
    }

    public IEnumerator SetupRoutine()
    {
        // Start session
        bool gotResponse = false;
        LootLockerSDKManager.StartGuestSession((response) => { gotResponse = true; });
        // Wait until we've gotten a response
        yield return new WaitWhile(() => gotResponse == false);

        // We are logged in
        loggedIn = true;

        // Update the time
        UpdateTime();
    }

    void CalculateLivesGottenOffline()
    {
        // Save coins, just for outputting to the log
        int debugLifeAmount = 0;
        // Loop through and check if we got any new lives
        int previousLives = lives;
        for (int i = lives; i < maxLivesAmount; i++)
        {
            // Have we passed the time?
            if (gameTime > timeToGetNewLife)
            {
                // Give the player a new life if they can add one
                if (lives < maxLivesAmount)
                {
                    lives++;
                    // Save it to PlayerPrefs
                    PlayerPrefs.SetInt("coins", lives);
                }
                // Add more waiting time to the timer
                timeToGetNewLife += timeToWaitForLife;
                
                debugLifeAmount++;
            }
        }

        Debug.Log("User had " + previousLives + " coins, user will get " + debugLifeAmount + " new coins");
    }

    void UpdateTime(bool updateLifeTime = false)
    {
        // Ping the server
        LootLockerSDKManager.Ping((response) =>
        {
            string currentServerTime = response.date;

            // Get the servers time
            System.DateTime serverDateTime = System.DateTime.Parse(currentServerTime);

            // New date, so we have something to check against
            System.DateTime earlyDateTime = new System.DateTime(2022, 3, 16, 14, 27, 0);

            // Output to log
            Debug.Log("Server:" + serverDateTime);
            Debug.Log("earlyTime:" + earlyDateTime);

            // Get amount of seconds passed sicne that date
            double diffInSeconds = (serverDateTime - earlyDateTime).TotalSeconds;

            // Check against the gametime
            double timeLeftOffline = (int)diffInSeconds - (int)gameTime;
            Debug.Log("User was off for " + timeLeftOffline + " seconds");

            // First time playing, set the gameTime to the difference
            if (gameTime == 0)
            {
                // "Start time" for the player
                gameTime = diffInSeconds;
                // At what time we should get a new life
                timeToGetNewLife = gameTime + timeToWaitForLife;
            }
            else
            {
                // Add the missing time to the gameTimer
                gameTime += timeLeftOffline;
            }

            // Save to player prefs
            PlayerPrefs.SetInt("gameTime", (int)gameTime);
            PlayerPrefs.SetInt("timeToGetNewLife", (int)timeToGetNewLife);

            // Calculate if we got any coins when the game was not running
            CalculateLivesGottenOffline();

            // Should we update the time for the lives
            if(updateLifeTime)
            {
                // At what time we should get a new life
                timeToGetNewLife = gameTime + timeToWaitForLife;
                PlayerPrefs.SetInt("timeToGetNewLife", (int)timeToGetNewLife);
            }
            // Time is correct
            timeIsFresh = true;
            UpdateLives();
        });

    }

    // Update is called once per frame
    void Update()
    {
        // If we have the max amount of hearts, we shouldn't do anything
        if (lives >= maxLivesAmount)
        {
            return;
        }
        // Do we know that the time we're using is correct?
        if (timeIsFresh)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= 1)
            {
                // Increase gametimer by one second
                gameTime++;

                // Save to playerprefs
                PlayerPrefs.SetInt("gameTime", (int)gameTime);
                PlayerPrefs.SetInt("timeToGetNewLife", (int)timeToGetNewLife);

                // Should we get a new life?
                if (gameTime >= timeToGetNewLife)
                {
                    // Give the player a new life if they can add one
                    if (lives < maxLivesAmount)
                    {
                        lives++;
                        // Save it to PlayerPrefs
                        PlayerPrefs.SetInt("coins", lives);

                        // Update life amount
                        livesAmountText.text = lives.ToString();
                    }
                    // Add more waiting time
                    timeToGetNewLife = gameTime + timeToWaitForLife;
                }

                // Recalculate the time left
                timeLeft = (int)timeToGetNewLife - (int)gameTime;
                // No negative numbers, in case we missed updating
                if (timeLeft < 0)
                {
                    timeLeft = 0;
                }
                // Format the text
                int timeInMinutes = (int)Mathf.Floor((int)timeLeft / 60);
                int timeLeftSeconds = (int)timeLeft - (timeInMinutes * 60);
                Debug.Log("New life in:\n" + timeInMinutes.ToString("00") + ":" + timeLeftSeconds.ToString("00"));

                // Reset timer
                lifeTimer = 0;
            }
        }
    }

    void UpdateLives()
    {
        livesAmountText.text = lives.ToString();
    }

    public void RemoveLife()
    {
        // If we removed the maximum-limit heart
        // We should start to count from that time
        if (lives >= maxLivesAmount)
        {
            // Stop counting in game
            timeIsFresh = false;

            // Update time against server and set a new timeToGetNewLife
            UpdateTime(true);
        }
        // Decrease hearts if we can
        if (lives > 0)
        {
            lives--;
            AnimateHeart();
        }
        UpdateLives();
    }
    void OnApplicationFocus(bool focus)
    {
        // We must be sure that a session has been started before we do this
        if (loggedIn == false)
        {
            return;
        }

        // If the application got focus
        if (focus == true)
        {
            timeIsFresh = false;
            Debug.Log("Connecting to server");

            // Update the time
            UpdateTime();
        }
    }
}
