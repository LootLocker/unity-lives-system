using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class EnergySystemsManager : MonoBehaviour
{
    [Header("Info texts")]
    // Text component to show info
    public TextMeshProUGUI livesInfoText;
    // Text component to show info
    public TextMeshProUGUI energyInfoText;
    // Text component to show info
    public TextMeshProUGUI coinsInfoText;

    [Header("Current energies")]
    // Current lives
    public int lives = 0;
    // Current energy
    public int energy = 0;
    // Current coins
    public int coins = 0;

    [Header("Time left")]
    // The time in seconds of when we should get a new life
    public double timeToGetNewLife = 0;
    // The time in seconds of when we should get a new life
    public double timeToGetNewEnergy = 0;

    [Header("System variables")]
    // The game time in seconds
    public double gameTime = 0;

    // Is the current time fresh from the server or not?
    bool timeIsFresh = false;

    // Timer to calculate the different energies
    public float timer = 0;

    // Are we logged in?
    bool loggedIn = false;

    [Header("Time left")]
    // Variable to store the time left for a new life
    public int timeLeftLives = 0;
    // Variable to store the time left for a new life
    public int timeLeftEnergy = 0;

    [Header("Amounts text")]
    public TextMeshProUGUI livesAmountText;
    public TextMeshProUGUI energyAmountText;
    public TextMeshProUGUI coinAmountText;

    [Header("Waiting times")]
    // How long we want the user to wait before adding a new life
    public double timeToWaitForLife = 30;
    // How long we want the user to wait before adding more energy
    public double timeToWaitForEnergy = 10;

    [Header("Max amounts")]
    // The max amount of lives the player can have
    public int maxLivesAmount = 5;
    // The max amount of energy the player can have
    public int maxEnergyAmount = 100;

    [Header("Coins")]
    // The current availabel offline coins
    public double offlineCoins;

    [Header("Animations")]
    // Animation curve for newLifeAnimation
    public AnimationCurve interactionAnimationCurve;

    // Image for heart
    public Transform heartImageTransform;
    // Image for heart
    public Transform energyImageTransform;
    // Image for heart
    public Transform coinImageTransform;

    // Animating the heart
    private Coroutine animateHeartRoutine;
    // Animating the heart
    private Coroutine animateEnergyRoutine;
    // Animating the heart
    private Coroutine animateCoinRoutine;

    // Start is called before the first frame update
    void Start()
    {
        // Fetch saved times
        timeToGetNewLife = (double)PlayerPrefs.GetInt("timeToGetNewLife", 0);
        timeToGetNewEnergy = (double)PlayerPrefs.GetInt("timeToGetNewEnergy", 0);
        gameTime = (double)PlayerPrefs.GetInt("gameTime", 0);

        // If first time playing, give the player max amount of hearts
        lives = PlayerPrefs.GetInt("lives", maxLivesAmount);
        // If first time playing, give the player max amount of energy
        energy = PlayerPrefs.GetInt("energy", maxEnergyAmount);
        // If first time playing, give the 0 coins
        coins = PlayerPrefs.GetInt("coins", 0);

        // Offline coins
        offlineCoins = PlayerPrefs.GetInt("offlineCoins", 0);

        livesInfoText.text = "Connecting to server";
        energyInfoText.text = "Connecting to server";
        coinsInfoText.text = "Connecting to server";

        UpdateLives();
        UpdateEnergy();
        UpdateCoins();

        StartCoroutine(SetupRoutine());
    }

    [ContextMenu("ClearPlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Deleted all player prefs");
    }
    void AnimateHeart()
    {
        if (animateHeartRoutine != null)
        {
            StopCoroutine(animateHeartRoutine);
        }
        animateHeartRoutine = StartCoroutine(AnimateTransformRoutine(heartImageTransform));
    }

    void AnimateEnergy()
    {
        if (animateEnergyRoutine != null)
        {
            StopCoroutine(animateEnergyRoutine);
        }
        animateEnergyRoutine = StartCoroutine(AnimateTransformRoutine(energyImageTransform));
    }

    void AnimateCoin()
    {
        if (animateCoinRoutine != null)
        {
            StopCoroutine(animateCoinRoutine);
        }
        animateCoinRoutine = StartCoroutine(AnimateTransformRoutine(coinImageTransform));
    }

    IEnumerator AnimateTransformRoutine(Transform transformToAnimate)
    {
        float timer = 0f;
        float duration = 1f;
        transformToAnimate.localScale = Vector3.one;
        while (timer <= duration)
        {
            transformToAnimate.localScale = Vector3.one * interactionAnimationCurve.Evaluate(timer / duration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        transformToAnimate.localScale = Vector3.one;

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
        UpdateTime(false, false, true);
    }

    void CalculateLivesGottenOffline()
    {
        // Save coins, just for outputting to the log
        int debugLifeAmount = 0;
        // Loop through and check if we got any new coins
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

        Debug.Log("User had " + previousLives + " lives, user will get " + debugLifeAmount + " new lives");
    }

    void CalculateEnergyGottenOffline()
    {
        // Save coins, just for outputting to the log
        int debugEnergyAmount = 0;
        // Loop through and check if we got any new coins
        int previousEnergy = energy;
        for (int i = energy; i < maxEnergyAmount; i++)
        {
            // Have we passed the time?
            if (gameTime > timeToGetNewEnergy)
            {
                // Give the player a new life if they can add one
                if (energy < maxEnergyAmount)
                {
                    energy++;
                    // Save it to PlayerPrefs
                    PlayerPrefs.SetInt("energy", energy);
                }
                // Add more waiting time to the timer
                timeToGetNewEnergy += timeToWaitForEnergy;

                debugEnergyAmount++;
            }
        }

        Debug.Log("User had " + previousEnergy + " energy, user will get " + debugEnergyAmount + " new energy");
    }

    void CalculateCoinsGottenOffline(double timeAwayInSeconds)
    {
        int previousCoins = coins;
        // Give the player 1 coin per second away from the game
        double coinsToGet = timeAwayInSeconds * 1;

        offlineCoins += coinsToGet;
        PlayerPrefs.SetInt("offlineCoins", (int)offlineCoins);
        coinsInfoText.text = "Offline earnings:\n" + offlineCoins;
        Debug.Log("User had " + previousCoins + " coins, user can get " + coinsToGet + " new coins");
    }

    void UpdateTime(bool updateLifeTime = false, bool updateEnergyTime = false, bool offlineCoins = false)
    {
        livesInfoText.text = "Connecting to server";
        energyInfoText.text = "Connecting to server";
        coinsInfoText.text = "Connecting to server";
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

            bool firstTimePlaying = false;
            // First time playing, set the gameTime to the difference
            if (gameTime == 0)
            {
                // "Start time" for the player
                gameTime = diffInSeconds;
                // At what time we should get a new life
                timeToGetNewLife = gameTime + timeToWaitForLife;
                timeToGetNewEnergy = gameTime + timeToWaitForEnergy;

                // No offline earnings the first time we start the game
                firstTimePlaying = true;
            }
            else
            {
                // Add the missing time to the gameTimer
                gameTime += timeLeftOffline;
            }

            // Save to player prefs
            PlayerPrefs.SetInt("gameTime", (int)gameTime);
            PlayerPrefs.SetInt("timeToGetNewLife", (int)timeToGetNewLife);
            PlayerPrefs.SetInt("timeToGetNewEnergy", (int)timeToGetNewEnergy);

            // Calculate if we got any lives when the game was not running
            CalculateLivesGottenOffline();

            // Calculate if we got any energy when the game was not running
            CalculateEnergyGottenOffline();

            // Calculate if we got any coins when the game was not running
            if (firstTimePlaying)
            {
                // The first time we start the game, we should not get any coins
                coinsInfoText.text = "Offline earnings:\n" + 0;
            }
            else if(offlineCoins)
            {
                CalculateCoinsGottenOffline(timeLeftOffline);
            }
            else
            {
                // Not getting any coins, only offline, show nothing
                coinsInfoText.text = "Offline earnings:\n" + offlineCoins;
            }

            // Should we update the time for the lives
            if (updateLifeTime)
            {
                // At what time we should get a new life
                timeToGetNewLife = gameTime + timeToWaitForLife;
                PlayerPrefs.SetInt("timeToGetNewLife", (int)timeToGetNewLife);
            }
            if (updateEnergyTime)
            {
                // At what time we should get a new life
                timeToGetNewEnergy = gameTime + timeToWaitForEnergy;
                PlayerPrefs.SetInt("timeToGetNewEnergy", (int)timeToGetNewEnergy);
            }
            // Time is correct
            timeIsFresh = true;
            UpdateLives();
            UpdateEnergy();
            UpdateCoins();
        });

    }

    // Update is called once per frame
    void Update()
    {
        if(loggedIn == false)
        {
            return;
        }
        // Do we know that the time we're using is correct?
        if (timeIsFresh)
        {
            // We use unscaled delta time when we count,
            // otherwise the timer would freeze if we pause the game by setting Time.timeScale to 0
            timer += Time.unscaledDeltaTime;
            if (timer >= 1)
            {
                // Increase gametimer by one second
                gameTime++;

                // Save to playerprefs
                PlayerPrefs.SetInt("gameTime", (int)gameTime);

                // Should we get a new life?
                if (gameTime >= timeToGetNewLife)
                {
                    // Give the player a new life if they can add one
                    if (lives < maxLivesAmount)
                    {
                        AnimateHeart();
                        lives++;
                        // Save it to PlayerPrefs
                        PlayerPrefs.SetInt("lives", lives);

                        // Update life amount
                        livesAmountText.text = "Lives:\n" + lives.ToString() + "/" + maxLivesAmount.ToString();
                    }
                    // Add more waiting time
                    timeToGetNewLife = gameTime + timeToWaitForLife;
                }
                // Should we get a more energy?
                if (gameTime >= timeToGetNewEnergy)
                {
                    // Give the player a new life if they can add one
                    if (energy < maxEnergyAmount)
                    {
                        AnimateEnergy();
                        energy++;
                        // Save it to PlayerPrefs
                        PlayerPrefs.SetInt("energy", energy);

                        // Update life amount
                        energyAmountText.text = "Energy:\n" + energy.ToString() + "/" + maxEnergyAmount.ToString();
                    }
                    // Add more waiting time
                    timeToGetNewEnergy = gameTime + timeToWaitForEnergy;
                }


                // Recalculate the time left for lives
                timeLeftLives = (int)timeToGetNewLife - (int)gameTime;
                // No negative numbers, in case we missed updating
                if (timeLeftLives < 0)
                {
                    timeLeftLives = 0;
                }

                // Recalculate the time left for energy
                timeLeftEnergy = (int)timeToGetNewEnergy - (int)gameTime;
                // No negative numbers, in case we missed updating
                if (timeLeftEnergy < 0)
                {
                    timeLeftEnergy = 0;
                }

                // Show lives info text
                if (lives >= maxLivesAmount)
                {
                    livesInfoText.text = "Lives full";
                }
                else
                {
                    int timeInMinutesLives = (int)Mathf.Floor((int)timeLeftLives / 60);
                    int timeLeftSecondsLives = (int)timeLeftLives - (timeInMinutesLives * 60);
                    livesInfoText.text = "New life in:\n" + timeInMinutesLives.ToString("00") + ":" + timeLeftSecondsLives.ToString("00");
                }

                // Show energy info text
                if (energy >= maxEnergyAmount)
                {
                    energyInfoText.text = "Energy full";
                }
                else
                {
                    int timeInMinutesEnergy = (int)Mathf.Floor((int)timeLeftEnergy / 60);
                    int timeLeftSecondsEnergy = (int)timeLeftEnergy - (timeInMinutesEnergy * 60);
                    energyInfoText.text = "More energy in:\n" + timeInMinutesEnergy.ToString("00") + ":" + timeLeftSecondsEnergy.ToString("00");
                }

                // Reset timer
                timer = 0;
            }
        }
    }

    void UpdateLives()
    {
        livesAmountText.text = "Lives:\n"+lives.ToString()+"/"+maxLivesAmount.ToString();
    }
    void UpdateEnergy()
    {
        energyAmountText.text = "Energy:\n" + energy.ToString() + "/" + maxEnergyAmount.ToString();
    }
    void UpdateCoins()
    {
        coinAmountText.text = "Coins:\n" + coins.ToString();
    }

    public void RemoveLife()
    {
        // If we removed the maximum-limit heart
        // We should start to count against that number
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
            PlayerPrefs.SetInt("lives", lives);
        }
        UpdateLives();
    }

    public void RemoveEnergy()
    {
        // If we removed the maximum-limit heart
        // We should start to count against that number
        if (energy >= maxEnergyAmount)
        {
            // Stop counting in game
            timeIsFresh = false;

            // Update time against server and set a new timeToGetNewLife
            UpdateTime(false, true);
        }
        // Decrease hearts if we can
        if (energy > 9)
        {
            energy-=10;
            PlayerPrefs.SetInt("energy", energy);
            AnimateEnergy();
        }
        UpdateEnergy();
    }

    public void ClaimCoins()
    {
        coins += (int)offlineCoins;
        offlineCoins = 0;
        PlayerPrefs.SetInt("offlineCoins", 0);
        coinsInfoText.text = "Offline earnings:\n" + offlineCoins;
        AnimateCoin();
        UpdateCoins();
        PlayerPrefs.SetInt("coins", coins);
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
            livesInfoText.text = "Connecting to server";
            energyInfoText.text = "Connecting to server";
            coinsInfoText.text = "Connecting to server";

            // Update the time
            UpdateTime();
        }
    }
}
