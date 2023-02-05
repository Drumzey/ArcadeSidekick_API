using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private IServiceProvider services;

        public HighScoreHandler()
        {
        }

        public HighScoreHandler(IServiceProvider services)
        {
            this.services = services;
        }

        public Scores GetAllByLocation(IGameDetailsRepository repo, string gameName, string location)
        {
            var settings = repo.Load(gameName, "Settings");
            var details = repo.Load(gameName, location);

            var scores = new Scores();
            scores.Setting = new Dictionary<string, Setting>();
            scores.SimpleScores = new Dictionary<string, List<SimpleScoreWithVerified>>();

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
                    scores.SimpleScores.Add(setting.SettingsId, new List<SimpleScoreWithVerified>());

                    foreach (ScoreDetails score in scoresForSetting)
                    {
                        scores.SimpleScores[setting.SettingsId]
                            .Add(new SimpleScoreWithVerified { UserName = score.UserName, Score = score.Score, LevelName = score.LevelName, Verified = score.Verified });
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
            scores.SimpleScores = new Dictionary<string, List<SimpleScoreWithVerified>>();
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
                    scores.SimpleScores.Add(newSettingId, new List<SimpleScoreWithVerified>());
                    
                    foreach (GameDetailsRecord detail in details)
                    {
                        //Get the scores via the initial settingId
                        var scoresToAdd = detail.Scores.Where(x => x.SettingsId.Equals(settingId));

                        foreach (ScoreDetails score in scoresToAdd)
                        {
                            //Write the score with the newId (if we have overwritten it)
                            scores.SimpleScores[newSettingId].Add(new SimpleScoreWithVerified
                            {
                                Score = score.Score,
                                UserName = score.UserName,
                                LevelName = score.LevelName,
                                Verified = score.Verified,
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
                    scores.SimpleScores.Add("Unknown", new List<SimpleScoreWithVerified>());
                }

                foreach (KeyValuePair<string, string> score in gamescore.DictionaryValue)
                {
                    //Need to get the pairs of simple scores from this repository
                    scores.SimpleScores["Unknown"].Add(new SimpleScoreWithVerified
                    {
                        UserName = score.Key,
                        Score = score.Value,
                        LevelName = string.Empty,
                        Verified = false,
                    });
                }
            }

            return scores;
        }

        public void Set(
            IGameDetailsRepository repo,
            ILocationRepository locationRepo,
            IUserRepository userRepo,
            IMiscRepository miscRepo,
            IObjectRepository objRepo,
            string body)
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

            // If we are posting a new best score then update the simple score for this game.
            this.UpdateSimpleScoreIfRequired(
                doubleScore,
                userRepo,
                objRepo,
                userName,
                gameName,
                levelName);

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

            UpdateLevelInformation(repo, gameName, levelName);
            UpdateLocationInformation(locationRepo, gameName, location);
            UpdateUsersGameDetails(repo, gameName, userName, detailsToAdd);
            SaveRecentActivity(miscRepo, userName, gameName, score);
        }

        private void SaveRecentActivity(
            IMiscRepository miscRepo,
            string userName,
            string gameName,
            string scoreString)
        {
            var recentActivity = miscRepo.Load("Activity", "All");

            if (recentActivity == null)
            {
                recentActivity = new Misc();
                recentActivity.Key = "Activity";
                recentActivity.SortKey = "All";
                recentActivity.List1 = new List<string>();
            }

            try
            {
                
                var score = string.Format("{0:N0}", Convert.ToDouble(scoreString));

                if(IsTimedGameOrLevel(gameName, string.Empty))
                {
                    TimeSpan t = TimeSpan.FromMilliseconds(double.Parse(score));
                    score = string.Format(
                        "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds);
                }

                var message = GetMessage(gameName, userName, score, DateTime.UtcNow.ToString("dd/MM/yyyy h:mm tt"));
                recentActivity.List1.Add(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error uploadingrecent activity for game " + gameName);
                Console.WriteLine(e.Message);
            }

            var newList = recentActivity.List1.Skip(Math.Max(0, recentActivity.List1.Count() - 100));
            recentActivity.List1 = newList.ToList();

            miscRepo.Save(recentActivity);
        }

        private string GetMessage(string gameKey, string username, string score, string date)
        {
            var name = GetGameName(gameKey);
            return $"{date}: {username} - {name} - {score}";
        }

        private string GetGameName(string gameKey)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var name = textInfo.ToTitleCase(gameKey.Replace("_", " "));

            if (name.Contains(" Xi ") || name.EndsWith(" Xi"))
            {
                name = name.Replace(" Xi ", " XI ");
                name = name.Replace(" Xi", " XI");
            }
            else if (name.Contains(" Viii ") || name.EndsWith(" Viii"))
            {
                name = name.Replace(" Viii ", " VIII ");
                name = name.Replace(" Viii", " VIII");
            }
            else if (name.Contains(" Vii ") || name.EndsWith(" Vii"))
            {
                name = name.Replace(" Vii ", " VII ");
                name = name.Replace(" Vii", " VII");
            }
            else if (name.Contains(" Vi ") || name.EndsWith(" Vi"))
            {
                name = name.Replace(" Vi ", " VI ");
                name = name.Replace(" Vi", " VI");
            }
            else if (name.Contains(" Iii ") || name.EndsWith(" Iii"))
            {
                name = name.Replace(" Iii ", " III ");
                name = name.Replace(" Iii", " III");
            }
            else if (name.Contains(" Ii ") || name.EndsWith(" Ii"))
            {
                name = name.Replace(" Ii ", " II ");
                name = name.Replace(" Ii", " II");
            }

            return name;
        }


        private void UpdateSimpleScoreIfRequired(
            double doubleScore,
            IUserRepository userRepo,
            IObjectRepository objRepo,
            string userName,
            string gameName,
            string levelName)
        {
            var user = userRepo.Load(userName);
            
            double existingScore = 0;
            if (user.Games.ContainsKey(gameName))
            {
                existingScore = double.Parse(user.Games[gameName]);
            }
            else
            {
                user.Games.Add(gameName, "0");
            }

            if (this.IsTimedGameOrLevel(gameName, levelName))
            {
                if (existingScore > doubleScore)
                {
                    user.Games[gameName] = doubleScore.ToString();
                    userRepo.Save(user);
                }
            }
            else
            {
                if (existingScore < doubleScore)
                {
                    user.Games[gameName] = doubleScore.ToString();
                    userRepo.Save(user);
                }
            }

            // What about the object leaderboard for this game?
            var leaderboard = objRepo.Load(gameName);
            if(leaderboard == null)
            {
                // If no leaderboard exists yet
                leaderboard = new ObjectInformation
                {
                    Key = gameName,
                    DictionaryValue = new Dictionary<string, string>
                    {
                        { userName, doubleScore.ToString() }
                    }
                };
            }
            else
            {
                // If the leaderboard does exist
                if (!leaderboard.DictionaryValue.ContainsKey(userName))
                {
                    // This user doesnt have a score for it
                    leaderboard.DictionaryValue.Add(userName, doubleScore.ToString());
                }
                else
                {
                    // User has a score already.
                    if (this.IsTimedGameOrLevel(gameName, levelName))
                    {
                        if (existingScore > doubleScore)
                        {
                            leaderboard.DictionaryValue[userName] = doubleScore.ToString();
                        }
                    }
                    else
                    {
                        if (existingScore < doubleScore)
                        {
                            leaderboard.DictionaryValue[userName] = doubleScore.ToString();
                        }
                    }
                }
            }

            objRepo.Save(leaderboard);
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

        public void DeleteAll(
            IGameDetailsRepository repo,
            ILocationRepository locationRepo,
            IUserRepository userRepo,
            string body)
        {
            //construct the settings from the data, find the score and delete it from the user record
            var data = JsonConvert.DeserializeObject<dynamic>(body);

            var gameName = (string)data.GameName;
            var userName = (string)data.UserName;

            var detailedUserRecord = repo.Load(gameName, userName);

            if (detailedUserRecord == null)
            {
                // This user does not have any detailed records for this game
                Console.WriteLine($"Cant find detailed game record for {gameName} for {userName}");
            }
            else
            {
                Console.WriteLine($"Found detailed game record for {gameName} for {userName}");
                // we need to remove all the detailed scores for this user
                repo.Delete(gameName, userName);

                // If the scores made were against any particular location we need to remove those too
                var locations = detailedUserRecord.Scores.Select(x => x.Location).Distinct();
                foreach (var location in locations)
                {
                    var currentLocationRecord = repo.Load(gameName, location);
                    var scoresFromLocation = detailedUserRecord.Scores.Where(x => x.Location == location);
                    foreach (var scoreFromLocation in scoresFromLocation)
                    {
                        currentLocationRecord.Scores.Remove(scoreFromLocation);
                    }

                    if (currentLocationRecord.Scores.Count == 0)
                    {
                        // We have no scores for this game in this location anymore
                        // so do we really have this game at the location?
                        repo.Delete(gameName, location);
                    }
                    else
                    {
                        repo.Save(currentLocationRecord);
                    }
                }

                // if the scores had settings that are now no longer used by other scores then we can delete the setting
                // WILL LEAVE RIGHT NOW
            }

            // we need to remove their rating in the game details for that game
            var gameRating = repo.Load(gameName, "Rating");
            if (gameRating != null)
            {
                Console.WriteLine("Removing game rating from details table");

                // Change the game detailed rating
                var userRating = gameRating.Ratings[userName];

                Console.WriteLine($"User rating {userRating}");

                var result = gameRating.Ratings.Remove(userName);

                Console.WriteLine($"Removed rating: {result}");

                Console.WriteLine($"Old ratings: {gameRating.NumberOfRatings}");
                gameRating.NumberOfRatings = gameRating.NumberOfRatings - 1;
                Console.WriteLine($"New ratings: {gameRating.NumberOfRatings}");

                Console.WriteLine($"Old total: {gameRating.Total}");
                gameRating.Total = gameRating.Total - userRating;
                Console.WriteLine($"New total: {gameRating.Total}");

                if (gameRating.NumberOfRatings != 0)
                {
                    Console.WriteLine($"Old average: {gameRating.Total}");
                    var average = (double)gameRating.Total / gameRating.NumberOfRatings;
                    gameRating.Average = Math.Round(average, 2);
                    Console.WriteLine($"New average: {gameRating.Total}");
                    repo.Save(gameRating);
                }
                else
                {
                    // If we have no ratings left then remove the record
                    Console.WriteLine($"Deleting record");
                    repo.Delete(gameName, "Rating");
                }

                // Update the Misc Table with the totals for ratings
                SaveRatingTotalInformation(userRating);

                // Update the overall ratings for the game
                SaveIntoMiscTable(gameName, gameRating.Average, gameRating.NumberOfRatings);
            }

            // we need to remove their simple score in their user record
            var userSimple = userRepo.Load(userName);
            userSimple.Games.Remove(gameName);

            // we need to adjust the number of games played
            userSimple.NumberOfGamesPlayed = userSimple.NumberOfGamesPlayed - 1;

            // we need to adjust the number of ratings given
            userSimple.NumberOfRatingsGiven = userSimple.NumberOfRatingsGiven - 1;

            // we need to remove their rating in the user record
            userSimple.Ratings.Remove(gameName);
            userRepo.Save(userSimple);
        }

        public void Delete(
            IGameDetailsRepository repo,
            ILocationRepository locationRepo, 
            IUserRepository userRepo,
            string body)
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
                Console.WriteLine($"No Setting for {gameName}");
                throw new Exception("No settings");
            }
            if (userRecord == null)
            {
                Console.WriteLine($"No Setting for {userName}");
                throw new Exception("No user record");
            }
            if (currentLocationRecord == null)
            {
                Console.WriteLine($"No Location for {location}");
                throw new Exception("No location");
            }

            var existingSettingIndex = settingsRecord.Settings.IndexOf(submissionSetting);

            if (existingSettingIndex == -1)
            {
                Console.WriteLine($"No Setting index");
                throw new Exception("No existing settings index");
            }

            var existingSetting = settingsRecord.Settings[existingSettingIndex];

            if (existingSetting == null)
            {
                Console.WriteLine($"No existing setting");
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

            var removed = userRecord.Scores.Remove(scoreToRemove);
            Console.WriteLine($"score removed {removed}");
            repo.Save(userRecord);
            Console.WriteLine($"User record saved");

            //we have removed the score from our repo, need to remove it from the location it was
            //set at. We only store one score per person at a venue
            if (currentLocationRecord.Scores.Remove(scoreToRemove))
            {
                Console.WriteLine($"removing from location record");

                // We have removed the score and we want to find the next highest one if there is one
                // Find the next highest score in the same location with same settingsId
                var sameLocation = userRecord.Scores.Where(x => x.Location.Equals(location));
                Console.WriteLine($"Next highest score {sameLocation}");
                ScoreDetails nextBestScore;

                if (sameLocation.Any())
                {
                    Console.WriteLine($"We have a next highest score");
                    if (timedGames.Contains(gameName))
                    {
                        Console.WriteLine($"Timed game");
                        nextBestScore = sameLocation.OrderBy(x => double.Parse(x.Score)).FirstOrDefault();
                    }
                    else
                    {
                        Console.WriteLine($"Normal game");
                        nextBestScore = sameLocation.OrderByDescending(x => double.Parse(x.Score)).FirstOrDefault();
                    }

                    if (nextBestScore != null)
                    {
                        currentLocationRecord.Scores.Add(nextBestScore);
                    }
                }
            }

            Console.WriteLine($"Saving location record");
            repo.Save(currentLocationRecord);

            // If the score we removed was our top simple score then we need to find our next highest score
            // in all our detailed scores and assign it to our user record
            var user = userRepo.Load(userName);
            if (user.Games[gameName] == scoreToRemove.Score)
            {
                Console.WriteLine("Removing Top Score");
                var nextBestScore = userRecord.Scores.OrderByDescending(x => double.Parse(x.Score)).FirstOrDefault();
                if (nextBestScore != null)
                {
                    Console.WriteLine("Next Best Score: " + nextBestScore.Score);
                    user.Games[gameName] = nextBestScore.Score;
                }
                else
                {
                    Console.WriteLine($"No next best score");
                    user.Games[gameName] = "0";
                }
                userRepo.Save(user);
            }

            if (userRecord.Scores.Count() == 0)
            {
                //We have no scores for this game for this user now, so lets remove it
                Console.WriteLine($"Removing detailed game record for user as has no scores");
                repo.Delete(gameName, userName);
            }

            if (currentLocationRecord.Scores.Count() == 0)
            {
                //We have no scores left... so we have no indication that this game is actaully
                //present at this location.
                //We can delete the record at this location
                //And we can delete the gamename from the location table..
                Console.WriteLine($"Location has no scores for this game so can be removed");
                repo.Delete(gameName, location);

                if (location != "Home Arcade")
                {
                    Console.WriteLine($"No scores for this game for non home location, can remove game from location as no indication game actually exists for location");
                    var locationrecord = locationRepo.Load(location);
                    locationrecord.GamesAvailable.Remove(gameName);
                    locationRepo.Save(locationrecord);
                }
            }
        }

        private void SaveRatingTotalInformation(int ratingRemoved)
        {
            var miscRepository = (IMiscRepository)services.GetService(typeof(IMiscRepository));
            var totalNumber = miscRepository.Load("Ratings", "TotalNumber");
            var total = miscRepository.Load("Ratings", "Total");
            var averageRating = miscRepository.Load("Ratings", "Average");

            var totalNumberInt = int.Parse(totalNumber.Value);
            totalNumber.Value = (totalNumberInt - 1).ToString();

            var totalInt = int.Parse(total.Value);
            total.Value = (totalInt - ratingRemoved).ToString();

            averageRating.Value = Math.Round(double.Parse(total.Value) / double.Parse(totalNumber.Value), 2).ToString();

            miscRepository.Save(totalNumber);
            miscRepository.Save(total);
            miscRepository.Save(averageRating);
        }

        private string ConvertName(string name)
        {
            name = name.ToLower();
            name = name.Replace(" ", "_");
            return name;
        }

        private void SaveIntoMiscTable(string gameName, double average, int numberOfRatings)
        {
            var pinballGames = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Games", "Pinball");
            var pinballNames = pinballGames.List1.ConvertAll(g => ConvertName(g));
            pinballNames.AddRange(pinballGames.List2.ConvertAll(g => ConvertName(g)));

            var pinballRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Pinball");
            var arcadeRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Arcade Games");
            
            if (pinballNames.Contains(gameName))
            {
                if (pinballRatings.Dictionary.ContainsKey(gameName))
                {
                    pinballRatings.Dictionary[gameName] = average.ToString() + "," + numberOfRatings.ToString();
                    ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(pinballRatings);
                }
            }
            else
            {
                if (arcadeRatings.Dictionary.ContainsKey(gameName))
                {
                    arcadeRatings.Dictionary[gameName] = average.ToString() + "," + numberOfRatings.ToString();
                    ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(arcadeRatings);
                }
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

        private bool IsTimedGameOrLevel(
            string gameName,
            string levelName)
        {
            // If we are a timed game then return true
            if (timedGames.Contains(gameName))
            {
                return true;
            }

            // If we happen to be a timed level then also return true
            if (timedLevels.ContainsKey(gameName) && timedLevels[gameName].Contains(levelName))
            {
                return true;
            }

            return false;
        }
    }
}
