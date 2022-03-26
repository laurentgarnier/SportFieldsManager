# Sport fields booking manager

In my company, several of us practice a sport collective during lunch break. 
Oragnizing the sessions is always a problem because we do not have a tool to share information and sign up as a participant. Sometimes it is a poll, sometimes a long serie of email, and at the end, we never know if there are enough of us to play or if there are too many of us and in that situation, who are the people who won't be able to play?

I decided as a POC to propose a tool that will try to manage this.

## Features

The tool will manage booking for sports fields. 

The main flow is as follows
* An administrator will create a session by specifying :
    - A sport
    - Maximum player number
    - Minimum player number
    - A date and an hour
    - A duration
    - An expiration date (how many time before the expected date, the session will be canceled)
* After a player signing in, he will have the list of all sessions ordered by date
* He will signing up for the sessions he is interesting in
* Each time a new player sign up, a notification will be send to all the session' players.
* When the session reach the minimum player number, a notification will be send to all the players to confirm the session.
* If a session is under the minimum player number at the expiration date, a notification will be send to the registered players to inform them.
* If the registered players number exceed the maximum player number, each extra player receive a notification to inform him that he is on a wait list.
* If the registered players number is a multiple of the minimum player number, the software will automatically create new sessions.
* A player can cancel his registration, in this case a notification will be send to all other participants
