using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.GameDetails.Handlers
{
    public class HighScoreHandler
    {
        private List<string> timedGames = new List<string>
        {
            "quick_and_crash",
            "neo_drift_out_new_technology",
            "cadash",
            "samurai_shodown_iv",
            "samurai_shodown_v",
            "samurai_shodown_v_special",
            "art_of_fighting_3",
            "outrunners",
            "tekken_3",
            "tekken_tag_tournament",
            "soul_calibur",
        };

        private Dictionary<string, List<string>> timedLevels = new Dictionary<string, List<string>>
        {
            { "track_and_field", new List<string> { "100M DASH", "110M HURDLES" } },
        };

        public Scores GetAllByLocation(IGameDetailsRepository repo, string gameName, string location)
        {
            var settings = repo.Load(gameName, "Settings");
            var details = repo.Load(gameName, location);

            var scores = new Scores();
            scores.Setting = new Dictionary<string, Setting>();
            scores.SimpleScores = new Dictionary<string, List<SimpleScore>>();

            if (settings == null || details == null)
            {
                return scores;
            }

            foreach (Setting setting in settings.Settings)
            {
                var scoresForSetting = details.Scores.Where(x => x.SettingsId.Equals(setting.SettingsId));

                if (scoresForSetting.Count() != 0)
                {
                    scores.Setting.Add(setting.SettingsId, setting);
                    scores.SimpleScores.Add(setting.SettingsId, new List<SimpleScore>());

                    foreach (ScoreDetails score in scoresForSetting)
                    {
                        scores.SimpleScores[setting.SettingsId]
                            .Add(new SimpleScore { UserName = score.UserName, Score = score.Score, LevelName = score.LevelName });
                    }
                }
            }

            return scores;
        }

        public Scores GetAll(IGameDetailsRepository repo, IObjectRepository objectRepo, string gameName)
        {
            var details = repo.QueryByGameName(gameName);

            var locationDetails = new List<GameDetailsRecord>();

            foreach (GameDetailsRecord rec in details)
            {
                if(rec.DataType == "Location")
                {
                    locationDetails.Add(rec);
                }
            }

            return GetAll(repo, objectRepo, locationDetails, gameName);
        }

        public Scores GetAll(IGameDetailsRepository repo, IObjectRepository objectRepo, List<GameDetailsRecord> details, string gameName)
        {
            var settings = repo.Load(gameName, "Settings");
            var scores = new Scores();
            scores.SimpleScores = new Dictionary<string, List<SimpleScore>>();
            scores.Setting = new Dictionary<string, Setting>();

            if (settings != null)
            {
                foreach (Setting setting in settings.Settings)
                {
                    var settingId = setting.SettingsId;
                    var newSettingId = setting.SettingsId;

                    if (setting.IsBlank)
                    {
                        newSettingId = "Unknown";
                    }

                    scores.Setting.Add(newSettingId, setting);
                    scores.SimpleScores.Add(newSettingId, new List<SimpleScore>());
                    
                    foreach (GameDetailsRecord detail in details)
                    {
                        //Get the scores via the initial settingId
                        var scoresToAdd = detail.Scores.Where(x => x.SettingsId.Equals(settingId));

                        foreach (ScoreDetails score in scoresToAdd)
                        {
                            //Write the score with the newId (if we have overwritten it)
                            scores.SimpleScores[newSettingId].Add(new SimpleScore
                            {
                                Score = score.Score,
                                UserName = score.UserName,
                                LevelName = score.LevelName,
                            });
                        }
                    }
                }
            }

            var gamescore = objectRepo.Load(gameName);
            if (gamescore != null)
            {
                if (!scores.SimpleScores.ContainsKey("Unknown"))
                {
                    scores.SimpleScores.Add("Unknown", new List<SimpleScore>());
                }

                foreach (KeyValuePair<string, string> score in gamescore.DictionaryValue)
                {
                    //Need to get the pairs of simple scores from this repository
                    scores.SimpleScores["Unknown"].Add(new SimpleScore
                    {
                        UserName = score.Key,
                        Score = score.Value,
                        LevelName = string.Empty,
                    });
                }
            }

            return scores;
        }

        public void Set(IGameDetailsRepository repo, ILocationRepository locationRepo, string body)
        {
            //Take the body and construct a new Game Details Object
            var data = JsonConvert.DeserializeObject<dynamic>(body);

            var gameName = (string) data.GameName;
            var userName = (string) data.UserName;
            var score = (string) data.Score;
            var date = (string) data.Date;
            var levelName = (string) data.LevelName;
            var eventName = (string) data.EventName;

            if (!double.TryParse(score, out double doubleScore))
            {
                //We cant pass the score as a double so throw an exception
                throw new Exception("Score not in correct format");
            }

            var location = (string) data.Location;
            var currentLocationRecord = repo.Load(gameName, location);

            if (currentLocationRecord == null)
            {
                currentLocationRecord = new GameDetailsRecord();
                currentLocationRecord.Game = gameName;
                currentLocationRecord.SortKey = location;
                currentLocationRecord.DataType = "Location";
                currentLocationRecord.Scores = new List<ScoreDetails>();
            }
                        
            var difficulty = (string) data.Difficulty;
            var lives = (string) data.Lives;
            var extraLivesAt = (string) data.ExtraLivesAt;
            var mameOrPCB = (string) data.MameOrPCB;
            var credits = (string)data.Credits;

            //Do we have an existing Setting that matches the current 
            var settingsRecord = repo.Load(gameName, "Settings");
            var submissionSetting = new Setting
            {
                Difficulty = difficulty,
                Lives = lives,
                ExtraLivesAt = extraLivesAt,
                Credits = credits,
                MameOrPCB = mameOrPCB,
            };

            if (settingsRecord == null)
            {
                settingsRecord = new GameDetailsRecord();
                settingsRecord.Game = gameName;
                settingsRecord.SortKey = "Settings";
                settingsRecord.DataType = "Settings";
                settingsRecord.Settings = new List<Setting>();
            }

            ScoreDetails detailsToAdd = new ScoreDetails
            {
                Date = date,
                LevelName = levelName,
                EventName = eventName,
                Score = score,
                UserName = userName,
                Verified = false,
                Image = false,
                Location = location,
            };

            if (settingsRecord.Settings.Contains(submissionSetting))
            {
                //This setting already exists
                var index = settingsRecord.Settings.IndexOf(submissionSetting);
                var existingSetting = settingsRecord.Settings[index];

                var existingUserScore = currentLocationRecord.Scores
                        .Where(x => AreEqual(x.SettingsId, existingSetting.SettingsId))
                        .Where(x => AreEqual(x.UserName, userName))
                        .Where(x => AreEqual(x.LevelName, levelName))
                        .FirstOrDefault();

                detailsToAdd.SettingsId = existingSetting.SettingsId;

                if (existingUserScore == null)
                {
                    //Create new record for user                 
                    currentLocationRecord.Scores.Add(detailsToAdd);
                }
                else
                {
                    //In timed games the lower the score the better.
                    //Only want to store the top score for each person in the locations record
                    if (timedGames.Contains(gameName) || 
                        (timedLevels.ContainsKey(gameName) && timedLevels[gameName].Contains(levelName)))
                    {
                        if (double.Parse(score) < double.Parse(existingUserScore.Score))
                        {
                            // Is the new score better than the last score?
                            // Update existing record for user
                            existingUserScore.Date = date;
                            existingUserScore.Score = score;
                            existingUserScore.EventName = eventName;
                            existingUserScore.LevelName = levelName;
                            existingUserScore.Verified = false;
                            existingUserScore.Image = false;
                        }
                    }
                    else
                    {
                        if (double.Parse(score) > double.Parse(existingUserScore.Score))
                        {
                            // Is the new score better than the last score?
                            //Update existing record for user
                            existingUserScore.Date = date;
                            existingUserScore.Score = score;
                            existingUserScore.EventName = eventName;
                            existingUserScore.LevelName = levelName;
                            existingUserScore.Verified = false;
                            existingUserScore.Image = false;
                        }
                    }
                }
            }
            else
            {
                //This is a brand new setting for this game
                //It needs to be added to the existing settings record
                //and added to the current gameDetails for this location

                //Set a new id to be the count of the records currently
                submissionSetting.SettingsId = settingsRecord.Settings.Count().ToString();
                settingsRecord.Settings.Add(submissionSetting);

                //Becuase this was a new setting, we are good to add a new score immediately
                detailsToAdd.SettingsId = submissionSetting.SettingsId;
                currentLocationRecord.Scores.Add(detailsToAdd);
            }

            //Save the locations record
            repo.Save(currentLocationRecord);
            //Save the settings record although only need to save if we have updated the settings.......
            repo.Save(settingsRecord);

            Console.WriteLine("updating level information");
            UpdateLevelInformation(repo, gameName, levelName);
            Console.WriteLine("updating location information");
            UpdateLocationInformation(locationRepo, gameName, location);
            Console.WriteLine("updating user information");
            UpdateUsersGameDetails(repo, gameName, userName, detailsToAdd);
        }

        private void UpdateLevelInformation(IGameDetailsRepository repo, string gameName, string levelName)
        {
            Console.WriteLine($"level name {levelName}");
            levelName = levelName.ToUpper();
            var levels = repo.Load(gameName, "Levels");
            if (levels == null)
            {
                Console.WriteLine($"levels are null");
                levels = new GameDetailsRecord();
                levels.Game = gameName;
                levels.SortKey = "Levels";
                levels.Levels = new List<string>();
                levels.Levels.Add(levelName);
                Console.WriteLine($"Added level");
            }
            else
            {
                if (!levels.Levels.Contains(levelName))
                {
                    levels.Levels.Add(levelName);
                }
            }

            repo.Save(levels);
        }

        public void Delete(IGameDetailsRepository repo, ILocationRepository locationRepo, string body)
        {
            //construct the settings from the data, find the score and delete it from the user record
            var data = JsonConvert.DeserializeObject<dynamic>(body);

            var gameName = (string)data.GameName;
            var userName = (string)data.UserName;
            var score = (string)data.Score;
            var date = (string)data.Date;
            var location = (string)data.Location;
            var eventName = (string)data.EventName;
            var levelName = (string)data.LevelName;

            var submissionSetting = GetSettingFromBody(data);

            //Do we have an existing Setting that matches the current 
            var settingsRecord = repo.Load(gameName, "Settings");
            var userRecord = repo.Load(gameName, userName);
            var currentLocationRecord = repo.Load(gameName, location);

            if(settingsRecord == null)
            {
                throw new Exception("No settings");
            }
            if (userRecord == null)
            {
                throw new Exception("No user record");
            }
            if (currentLocationRecord == null)
            {
                throw new Exception("No location");
            }

            var existingSettingIndex = settingsRecord.Settings.IndexOf(submissionSetting);

            if (existingSettingIndex == -1)
            {
                throw new Exception("No existing settings index");
            }

            var existingSetting = settingsRecord.Settings[existingSettingIndex];

            if (existingSetting == null)
            {
                throw new Exception("No existing settings");
            }

            var scoreToRemove = new ScoreDetails
            {
                Score = score,
                Date = date,
                SettingsId = existingSetting.SettingsId,
                Location = location,
                EventName = eventName,
                LevelName = levelName,
            };

            userRecord.Scores.Remove(scoreToRemove);

            //we have removed the score from our repo, need to remove it from the location it was
            //set at.
            currentLocationRecord.Scores.Remove(scoreToRemove);

            //We now need to add the next best score to the currentLocation
            //Find the next highest score in the same location with same settingsId
            var sameLocation = userRecord.Scores.Where(x => x.Location.Equals(location));
            ScoreDetails nextBestScore;

            if (timedGames.Contains(gameName))
            {
                nextBestScore = sameLocation.OrderBy(x => double.Parse(x.Score)).FirstOrDefault();
            }
            else
            {
                nextBestScore = sameLocation.OrderByDescending(x => double.Parse(x.Score)).FirstOrDefault();
            }

            if (nextBestScore != null)
            {
                currentLocationRecord.Scores.Add(nextBestScore);
            }

            repo.Save(userRecord);
            repo.Save(currentLocationRecord);

            if (userRecord.Scores.Count() == 0)
            {
                //We have no scores for this game for this user now, so lets remove it
                repo.Delete(gameName, userName);
            }

            if (currentLocationRecord.Scores.Count() == 0)
            {
                //We have no scores left... so we have no indication that this game is actaully
                //present at this location.
                //We can delete the record at this location
                //And we can delete the gamename from the location table..
                repo.Delete(gameName, location);

                var locationrecord = locationRepo.Load(location);
                locationrecord.GamesAvailable.Remove(gameName);
                locationRepo.Save(locationrecord);
            }
        }

        private Setting GetSettingFromBody(dynamic data)
        {
            var difficulty = (string)data.Difficulty;
            var lives = (string)data.Lives;
            var extraLivesAt = (string)data.ExtraLivesAt;
            var mameOrPCB = (string)data.MameOrPCB;
            var credits = (string)data.Credits;

            var submissionSetting = new Setting
            {
                Difficulty = difficulty,
                Lives = lives,
                ExtraLivesAt = extraLivesAt,
                Credits = credits,
                MameOrPCB = mameOrPCB,
            };

            return submissionSetting;
        }

        private void UpdateLocationInformation(ILocationRepository repo, string gameName, string location)
        {
            //Update the location games
            var record = repo.Load(location);
            if (record == null)
            {
                // Shouldnt get here as the locations are controlled by the database
            }
            else
            {
                if (!record.GamesAvailable.Contains(gameName))
                {
                    record.GamesAvailable.Add(gameName);
                    repo.Save(record);
                }
            }
        }

        private void UpdateUsersGameDetails(IGameDetailsRepository repo, string gameName, string userName, ScoreDetails score)
        {
            var currentLocationRecord = repo.Load(gameName, userName);

            if (currentLocationRecord == null)
            {
                //If we are a new game then create the record
                currentLocationRecord = new GameDetailsRecord();
                currentLocationRecord.Game = gameName;
                currentLocationRecord.SortKey = userName;
                currentLocationRecord.DataType = "User";
                currentLocationRecord.Scores = new List<ScoreDetails>();
            }

            currentLocationRecord.Scores.Add(score);
            repo.Save(currentLocationRecord);
        }

        private static bool AreEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b);
            }
            else
            {
                return string.Equals(a, b);
            }
        }
    }
}
