using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using MelonLoader;
using Synth.Utils;
using UnityEngine.Events;
using Newtonsoft.Json;
using Photon.Pun;
using System.Threading.Tasks;

namespace MPScoreUploader
{
    public class MPScoreUploader : MelonMod 
    {
        public static MPScoreUploader Instance;

        // Set to true when scores have been submitted at the end of a song and after all players have finished.
        // This resets back to false on starting a new song in multiplayer.
        private bool scoreSubmitted = true;

        private static HttpClient sharedClient;

        private static GameControlManager gameControlManager;

        private MelonPreferences_Category prefs;
        private MelonPreferences_Entry<string> baseUrl;


        public override void OnApplicationStart()
        {
            prefs = MelonPreferences.CreateCategory("MPScoreUploader");

            baseUrl = prefs.CreateEntry<string>("baseUrl", "https://steglasaurous.com:3000");
            baseUrl.Comment = "The URL to submit scores to.";

            // FIXME: Eventually add an API Key or something that would not just let anyone submit whatever they want. 

            Instance = this;
            RuntimePatch.PatchAll();

            sharedClient = new HttpClient();
            sharedClient.BaseAddress = new Uri(baseUrl.Value);

            // Do anything?  Sanity check that we can submit?  Open a browser to login as needed?
            MelonLogger.Msg("MPScoreUploader started. Submitting scores to " + baseUrl.Value);
        }

        public void GameManagerInit()
        {
            if (gameControlManager == GameControlManager.s_instance) return;
            gameControlManager = GameControlManager.s_instance;
            try
            {
                StageEvents stageEvents = new StageEvents();
                stageEvents.OnSongStart = new UnityEvent();
                stageEvents.OnSongStart.AddListener(OnSongStart);
                stageEvents.OnCompleteSpecial = new UnityEvent();
                stageEvents.OnEnterSpecial = new UnityEvent();
                stageEvents.OnFailSpecial = new UnityEvent();
                stageEvents.OnNoteFail = new UnityEvent();
                stageEvents.OnNoteHit = new UnityEvent();
                stageEvents.OnSongEnd = new UnityEvent();
                
                GameControlManager.UpdateStageEventList(stageEvents);
            }
            catch (Exception e)
            {
                LoggerInstance.Msg(e.Message);
            }
        }

        public override void OnUpdate()
        {
            if (Game_InfoProvider.s_instance != null && Game_InfoProvider.s_instance.SessionPlayers != null)
            {
                bool someoneIsPlaying = false;
                
                foreach (Synth.Multiplayer.SynthSessionPlayer synthSessionPlayer in Game_InfoProvider.s_instance.SessionPlayers)
                {
                    if (synthSessionPlayer != null)
                    {
                        if (synthSessionPlayer.InGame)
                        {
                            someoneIsPlaying = true;
                        }
                    }
                }

                if (!someoneIsPlaying && !this.scoreSubmitted)
                {
                    // Setting this right away in case submitting fails - prevents an endless loop of errors
                    this.scoreSubmitted = true;
                    var mapInfo = new
                    {
                        title = GameControlManager.s_instance.InfoProvider.TrackName,
                        artist = GameControlManager.s_instance.InfoProvider.Author,
                        mapper = GameControlManager.s_instance.InfoProvider.Beatmapper,
                        difficulty = GameControlManager.s_instance.InfoProvider.CurrentDifficulty.ToString(),
                        totalNotes = GameControlManager.s_instance.InfoProvider.NotesTotal,
                        totalSpecials = GameControlManager.s_instance.InfoProvider.SpecialTotals
                    };

                    List<Object> scores = new List<Object>();

                    // FIXME: Continue here - once API is running, setup HTTP request and object to match. 
                    foreach (Synth.Multiplayer.SynthSessionPlayer synthSessionPlayer in Game_InfoProvider.s_instance.SessionPlayers)
                    {
                        if (synthSessionPlayer != null)
                        {
                            scores.Add(new
                            {
                                playerName = synthSessionPlayer.DisplayName,
                                score = synthSessionPlayer.SessionStats.Score,
                                perfectHits = synthSessionPlayer.SessionStats.PerfectHits,
                                goodHits = synthSessionPlayer.SessionStats.GoodHits,
                                poorHits = synthSessionPlayer.SessionStats.PoorHits,
                                longestStreak = synthSessionPlayer.SessionStats.LongestStreaak,
                                maxMultiplier = synthSessionPlayer.SessionStats.MaxMultiplier,
                                specialsHit = synthSessionPlayer.SessionStats.SpecialsComplete,
                            });
                        }
                    }

                    LoggerInstance.Msg("All player info collected - converting final object to JSON");

                    string jsonContent = JsonConvert.SerializeObject(new 
                        {
                            submitterName = PhotonNetwork.LocalPlayer.NickName,
                            roomName = PhotonNetwork.CurrentRoom.Name,
                            map = mapInfo,
                            scores = scores
                        }
                    );

                    LoggerInstance.Msg(jsonContent);

                    StringContent stringContent = new StringContent(
                        jsonContent,
                        Encoding.UTF8,
                        "application/json"
                    );
                    LoggerInstance.Msg("Converted to JSON - sending to API");
                    _ = this.SubmitScore(stringContent);
                }
            }
        }

        private async Task SubmitScore(StringContent content)
        {
            try
            {
                HttpResponseMessage response = await sharedClient.PostAsync("/score-submission", content);
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    MelonLogger.Msg("Error: Submitting scores to API failed: " + response.StatusCode + " " + response.ReasonPhrase);
                } else
                {
                    MelonLogger.Msg("Scores submitted successfully.");
                }
            }
            catch (HttpRequestException e)
            {
                MelonLogger.Msg("Error: Submitting scores to API threw an exception: " + e.Message);
            }
        }


        // FIXME: Replace this with scene change detection to determine when a song starts, so we don't bork the events list when using it with 

        private void OnSongStart()
        {
            // Can I detect whether this is a multiplayer thing so I don't needlessly check and submit scores?
            this.scoreSubmitted = false;
            MelonLoader.MelonLogger.Msg("OnSongStart called");
        }
    }
}
