# Simple lives System
![](https://github.com/LootLocker/unity-lives-system/blob/main/livesGif.gif)
This is an implementation of a lives system that uses a server to check the time against. Meaning that you can not cheat it easily by changing the time on your device.

How to use
This implementation is for Unity. Follow this guide [link] to setup an account and install the LootLocker SDK and to see a breakdown of the steps in the script.

How does it work?
It fetches the time from a server every time the game is started or if it loses focus and gets the focus back again. It never uses the time on the device to check how much time has passed. This means that even if you change your system clock, the game will only give you rewards for as much real time as you were away.

The energy and lives are synced every time that the app regains focus, the offline earnings is only synced when the game starts, so you have to actually turn off and restart the game to get offline earnings for the coins. 

Let us know if you utilize this little system that we made in your game.

Join our Discord channel if you want to show off what you just made or need help with the implementation.
