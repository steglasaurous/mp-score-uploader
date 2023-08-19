# Synth Multiplayer Score Uploader

This mod uploads scores from multiplayer rooms to a server.  Useful for tracking tournaments and scores outside of the official leaderboards. This couples with the [synth-tourney server](https://github.com/steglasaurous/synth-tourney) to manage tournaments.



## Quick Start

- Requires [MelonLoader](https://melonwiki.xyz/#/) v0.5.3 or later.  Make sure that is installed prior to installing this mod. 



1. Download the zip file from the latest release [here](https://github.com/steglasaurous/mp-score-uploader/releases)
2. Unzip the contents into the Mods folder in your Synth Riders installation.  



## How this works

The following is dev stuff.  Proceed forward if you want to understand the inner workings of this mod.  Otherwise, no need to continue. ;) 

The mod will monitor when it's in a multiplayer room. When all players have finished playing the song, it will send an HTTP POST to a server as defined in MelonPreferences.cfg, with the path `/score-submission` appended.  This is meant to be used with the [synth-tourney server](https://github.com/steglasaurous/synth-tourney).



```typescript
// POST /score-submission
{
    // The current player's name that is submitting the score
    "submitterName": "steglasaurous",
	
	// The name of the multiplayer room they're in.
    "roomName": "testroom1",
    
	// Details about the map being played.
    "map": {
        "title": "RandoSong3",
        "artist": "SomeGuy",
        "mapper": "SomeMapper",
        "difficulty": "Master",
        "totalNotes": 526,
        "totalSpecials": 6
    },
    // All players' score information - essentially what you would see on the score summary scene when finishing a song.
    "scores": [
        {
            "playerName": "steglasaurous",
            "score": 7182193,
            "perfectHits": 100,
            "goodHits": 50,
            "poorHits": 25,
            "longestStreak": 150,
            "maxMultiplier": 6,
            "specialsHit": 5
        },
        {
            "playerName": "anotherPlayer",
            "score": 8182193,
            "perfectHits": 200,
            "goodHits": 150,
            "poorHits": 125,
            "longestStreak": 350,
            "maxMultiplier": 6,
            "specialsHit": 6
        }
    ]
}
```

