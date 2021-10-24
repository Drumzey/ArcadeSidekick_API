using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arcade.GameDetails.Models;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

namespace Arcade.GameDetails.Handlers
{
    public class UploadAllHandler
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

        public UploadAllHandler()
        {
        }

        public UploadAllHandler(IServiceProvider services)
        {
            this.services = services;
        }

        public void UploadData(string body)
        {
            Console.WriteLine($"Request Body {body}");

            //Take the body and construct a new Game Details Object
            var data = JsonConvert.DeserializeObject<UploadDetailsModel>(body);
            var userName = data.UserName;
            var simpleScores = data.SimpleScores;
            var detailedScores = data.DetailedScores;
            var ratings = data.Ratings;

            var userRepo = (IUserRepository)this.services.GetService(typeof(IUserRepository));

            var user = userRepo.Load(userName);
            this.SetSimpleScores(simpleScores, user);

            // Ratings

            this.SetRatings(ratings, user);
            user.NumberOfScoresUploaded = user.Games.Count; // Use the number of non zero scores
            user.NumberOfRatingsGiven = user.Ratings.Count; // USe the number of non zero ratings
            user.NumberOfGamesPlayed = simpleScores.Count; // use the number of entries in this collection
            userRepo.Save(user);

            var ratingsUpdate = this.SetRatingsInGameDetailsTable(userName, ratings);
            this.SaveRatingTotalInformation(ratingsUpdate.ratingDifferences, ratingsUpdate.newRatingsAdded);
            this.SaveRatingIntoMiscTable(ratingsUpdate.ratingDifferences, ratingsUpdate.newRatingsAdded);

            // Details
            Console.WriteLine("Adding Detailed Games");
            this.SaveDetailedGames(userName, detailedScores);
        }

        private void SaveDetailedGames(string userName, Dictionary<string, List<DetailedGame>> detailedScores)
        {
            var recordsToSave = new List<GameDetailsRecord>();

            // Each key in the dictionary represents a game
            // and each item in the value represent one score of that game.
            // Each score could have a new setting or a preexisting one, so for each one we may need to overwrite the setting
            var gameDetailsRepo = (IGameDetailsRepository)this.services.GetService(typeof(IGameDetailsRepository));
            gameDetailsRepo.SetupTable();

            foreach(var game in detailedScores)
            {
                Console.WriteLine($"Adding Detailed Game: {game.Key}");
                var scoreRecord = gameDetailsRepo.Load(game.Key, userName);
                // On an upload we should be all brand new but we might allow upload of data for existing users who havent
                // restore account on new phones etc
                if (scoreRecord == null)
                {
                    Console.WriteLine($"No Score record for game: {game.Key}");
                    scoreRecord = new GameDetailsRecord();
                    scoreRecord.Game = game.Key;
                    scoreRecord.SortKey = userName;
                    scoreRecord.DataType = "User";
                    scoreRecord.Scores = new List<ScoreDetails>();
                }

                // For each score we need to figure out if it has an available setting or not.
                var settingRecord = gameDetailsRepo.Load(game.Key, "Settings");
                // No one has ever posted to this game before so we have no problems
                if (settingRecord == null)
                {
                    Console.WriteLine($"No Setting record for game: {game.Key}");
                    settingRecord = new GameDetailsRecord();
                    settingRecord.Game = game.Key;
                    settingRecord.SortKey = "Settings";
                    settingRecord.DataType = "Settings";
                    settingRecord.Settings = new List<Setting>();
                }

                // Grab each unique setting for the plays of this game.
                var playSettings = new List<Setting>();
                // At the same time grab each location for the game too
                var locations = new List<string>();
                // At the same time grab each level in the game
                var levelNames = new List<string>();

                foreach (var play in game.Value)
                {
                    Console.WriteLine($"Processing play: {game.Key}");
                    var submissionSetting = new Setting
                    {
                        Difficulty = play.Difficulty,
                        Lives = play.Lives,
                        ExtraLivesAt = play.ExtraLivesAt,
                        Credits = play.Credits,
                        MameOrPCB = play.MameOrPCB,
                    };

                    if (!playSettings.Contains(submissionSetting))
                    {
                        playSettings.Add(submissionSetting);
                    }

                    if (!locations.Contains(play.Location))
                    {
                        locations.Add(play.Location);
                    }

                    if (!levelNames.Contains(play.LevelName))
                    {
                        levelNames.Add(play.LevelName);
                    }
                }

                // Do these settings exist in the online record for this game?
                foreach(var setting in playSettings)
                {
                    Console.WriteLine($"Checking For Setting {setting}");

                    // We have not seen this setting before
                    if (!settingRecord.Settings.Contains(setting))
                    {
                        Console.WriteLine($"new game setting for: {game.Key} {setting.ToString()}");
                        // So assign the id to be the next one
                        setting.SettingsId = settingRecord.Settings.Count().ToString();
                        settingRecord.Settings.Add(setting);
                    }
                }

                // Now we have all the settings available we can go through each detailed score and create the submission
                foreach (var play in game.Value)
                {
                    var submissionSetting = new Setting
                    {
                        Difficulty = play.Difficulty,
                        Lives = play.Lives,
                        ExtraLivesAt = play.ExtraLivesAt,
                        Credits = play.Credits,
                        MameOrPCB = play.MameOrPCB,
                    };

                    Console.WriteLine($"Play with score {play.Score} has setting {submissionSetting.ToString()}");

                    var playSettingIndex = settingRecord.Settings.IndexOf(submissionSetting);
                    var playSetting = settingRecord.Settings[playSettingIndex];

                    Console.WriteLine($"Play with score {play.Score} is mapping to {playSetting.ToString()}");

                    var newPlayRecord = new ScoreDetails
                    {
                        Date = play.Date,
                        EventName = play.EventName,
                        LevelName = play.LevelName,
                        Location = play.Location,
                        Score = play.Score,
                        UserName = userName,
                        Verified = false,
                        Image = false,
                        SettingsId = playSetting.SettingsId,
                    };

                    Console.WriteLine($"Adding play record for: {game.Key} and setting {playSetting.SettingsId}");
                    scoreRecord.Scores.Add(newPlayRecord);
                }

                // We now need to find the top score for each of the games at each location to add to the location record...
                var locationsForGame = game.Value.Select(x => x.Location).Distinct();

                Console.WriteLine($"Locations where {game.Key} was played {string.Join(',', locationsForGame)}");

                // We have a list of locations where these scores were made
                // we need to grab the location/game records and add the highest score to them
                var locationRecords = new List<GameDetailsRecord>();

                foreach(var location in locationsForGame)
                {
                    Console.WriteLine($"{game.Key} played at location {location}");

                    var locationRecord = gameDetailsRepo.Load(game.Key, location);
                    if (locationRecord == null)
                    {
                        Console.WriteLine($"no location record: {game.Key} {location}");
                        locationRecord = new GameDetailsRecord()
                        {
                            Game = game.Key,
                            SortKey = location,
                            DataType = "Location",
                            Scores = new List<ScoreDetails>(),
                        };
                    }

                    // Get all the scores at that location
                    var scoresAtLocation = scoreRecord.Scores.Where(x => x.Location == location);
                    // Get all the settings ids for all these scores
                    var settingIds = scoreRecord.Scores.Select(x => x.SettingsId).Distinct();

                    Console.WriteLine($"Settings for location {location} where {game.Key} was played {string.Join(',', settingIds)}");

                    // For each setting id find all scores for that game
                    foreach (string setting in settingIds)
                    {
                        Console.WriteLine($"processing setting {setting} for location {location} - {game.Key}");
                        // All the scores we are submitting
                        var scoresForSetting = scoresAtLocation.Where(x => x.SettingsId == setting);

                        // Get all the different levels that are being submitted
                        var levelsForScores = scoresForSetting.Select(x => x.LevelName).Distinct();

                        foreach (var levelName in levelsForScores)
                        {
                            Console.WriteLine($"processing level {levelName} - {setting} - {game.Key}");
                            // Scores for that settings and level
                            var sameSettingsAndLevelScores = scoresForSetting.Where(x => x.LevelName == levelName);

                            // Scores that already exist for our user for this setting and level name
                            var existingUserScore = locationRecord.Scores
                            .Where(x => AreEqual(x.SettingsId, setting))
                            .Where(x => AreEqual(x.UserName, userName))
                            .Where(x => AreEqual(x.LevelName, levelName))
                            .FirstOrDefault();

                            ScoreDetails bestRecord;
                            bool timed = false;

                            // Find the best score from all those being submitted
                            if (timedGames.Contains(game.Key) ||
                            (timedLevels.ContainsKey(game.Key) && timedLevels[game.Key].Contains(levelName)))
                            {
                                // What is the lowest score
                                bestRecord = sameSettingsAndLevelScores.OrderBy(s => int.Parse(s.Score)).First();
                                timed = true;
                            }
                            else
                            {
                                // what is the highest score
                                bestRecord = sameSettingsAndLevelScores.OrderByDescending(s => int.Parse(s.Score)).First();
                            }

                            if (existingUserScore == null)
                            {
                                Console.WriteLine($"Adding score to location {location} - {bestRecord.Score}");
                                locationRecord.Scores.Add(bestRecord);
                            }
                            else
                            {
                                Console.WriteLine($"We have an existing score for this game: {game.Key}");
                                //We have an existing score
                                if (timed)
                                {
                                    if (double.Parse(bestRecord.Score) < double.Parse(existingUserScore.Score))
                                    {
                                        existingUserScore.Date = bestRecord.Date;
                                        existingUserScore.Score = bestRecord.Score;
                                        existingUserScore.EventName = bestRecord.EventName;
                                        existingUserScore.LevelName = levelName;
                                        existingUserScore.Verified = false;
                                        existingUserScore.Image = false;
                                    }
                                }
                                else
                                {
                                    if (double.Parse(bestRecord.Score) > double.Parse(existingUserScore.Score))
                                    {
                                        existingUserScore.Date = bestRecord.Date;
                                        existingUserScore.Score = bestRecord.Score;
                                        existingUserScore.EventName = bestRecord.EventName;
                                        existingUserScore.LevelName = levelName;
                                        existingUserScore.Verified = false;
                                        existingUserScore.Image = false;
                                    }
                                }
                            }
                        }
                    };

                    Console.WriteLine($"Adding location record");
                    locationRecords.Add(locationRecord);
                }

                // Create/Update level names record
                var levels = gameDetailsRepo.Load(game.Key, "Levels");
                if (levels == null)
                {
                    Console.WriteLine($"levels are null");
                    levels = new GameDetailsRecord();
                    levels.Game = game.Key;
                    levels.SortKey = "Levels";
                    levels.Levels = new List<string>();
                    levels.Levels.AddRange(levelNames);
                    Console.WriteLine($"Added levels ");
                }
                else
                {
                    foreach (string levelName in levelNames)
                    {
                        if (!levels.Levels.Contains(levelName))
                        {
                            levels.Levels.Add(levelName);
                        }
                    }
                }

                Console.WriteLine($"Adding records to save locations {locationRecords.Count}: {game.Key}");
                recordsToSave.AddRange(locationRecords);
                Console.WriteLine($"Adding records to save settings: {game.Key}");
                recordsToSave.Add(settingRecord);
                Console.WriteLine($"Adding records to save user score: {game.Key}");
                recordsToSave.Add(scoreRecord);
                Console.WriteLine($"Adding Level record to save user score: {game.Key}");
                recordsToSave.Add(levels);
            }

            Console.WriteLine($"Writing batch: {recordsToSave.Count}");
            gameDetailsRepo.SaveBatch(recordsToSave);
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

        private void SetSimpleScores(Dictionary<string, string> simpleScores, UserInformation user)
        {
            if (user.Games == null)
            {
                user.Games = new Dictionary<string, string>();
            }

            foreach (var simpleScore in simpleScores)
            {
                var gameName = simpleScore.Key;
                var score = simpleScore.Value;
                if (score != "0")
                {
                    Console.WriteLine($"Adding Simple Score {gameName}:{score}");

                    if (user.Games.ContainsKey(gameName))
                    {
                        if (timedGames.Contains(gameName) ||
                            (timedLevels.ContainsKey(gameName) && timedLevels[gameName].Contains(gameName)))
                        {
                            if (double.Parse(score) < double.Parse(user.Games[gameName]))
                            {
                                user.Games[gameName] = score;
                            }
                        }
                        else
                        {
                            if (double.Parse(score) > double.Parse(user.Games[gameName]))
                            {
                                user.Games[gameName] = score;
                            }
                        }
                    }
                    else
                    {
                        user.Games.Add(gameName, score);
                    }
                }
            }
        }

        private void SetRatings(Dictionary<string, int> ratings, UserInformation user)
        {
            if (user.Ratings == null)
            {
                user.Ratings = new Dictionary<string, int>();
            }

            foreach (var rating in ratings)
            {
                var gameName = rating.Key;
                var value = rating.Value;
                if (value != 0)
                {
                    Console.WriteLine($"Adding Rating {gameName}:{value}");
                    if (user.Ratings.ContainsKey(gameName))
                    {
                        user.Ratings[gameName] = value;
                    }
                    else
                    {
                        user.Ratings.Add(gameName, value);
                    }
                }
            }
        }

        private (Dictionary<string, int> ratingDifferences, List<string> newRatingsAdded) SetRatingsInGameDetailsTable(string userName, Dictionary<string, int> ratings)
        {
            var ratingsDifferences = new Dictionary<string, int>();
            var newRatingsAdded = new List<string>();

            var gameDetailsRepository = (IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository));

            var ratingsToUpsert = new List<GameDetailsRecord>();

            foreach (var input in ratings)
            {
                var gameName = input.Key;
                var rating = input.Value;

                // If the rating is 0 then we forget about it and leave the old rating where it was (if there was one)
                if (rating == 0)
                {
                    continue;
                }

                var ratingFromGameDetailsTable = ((IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository)))
                    .Load(gameName, "Rating");

                if (ratingFromGameDetailsTable == null)
                {
                    Console.WriteLine($"Adding Game Details Rating {gameName}:{rating}");

                    ratingFromGameDetailsTable = new GameDetailsRecord();
                    ratingFromGameDetailsTable.Game = gameName;
                    ratingFromGameDetailsTable.SortKey = "Rating";
                    ratingFromGameDetailsTable.DataType = "Rating";
                    ratingFromGameDetailsTable.CreatedAt = DateTime.Now;

                    ratingFromGameDetailsTable.NumberOfRatings = 1;
                    ratingFromGameDetailsTable.Average = rating;
                    ratingFromGameDetailsTable.Total = rating;
                    ratingFromGameDetailsTable.CreatedAt = DateTime.Now;
                    ratingFromGameDetailsTable.Ratings = new Dictionary<string, int>
                    {
                        { userName, rating },
                    };

                    ratingsDifferences.Add(gameName, rating);
                    newRatingsAdded.Add(gameName);
                }
                else
                {
                    Console.WriteLine($"Appending to Game Details Rating {gameName}:{rating}");
                    Console.WriteLine($"Previous number of ratings {ratingFromGameDetailsTable.NumberOfRatings}");
                    Console.WriteLine($"Previous total of ratings {ratingFromGameDetailsTable.Total}");
                    Console.WriteLine($"Previous average of ratings {ratingFromGameDetailsTable.Average}");

                    if (!ratingFromGameDetailsTable.Ratings.ContainsKey(userName))
                    {
                        ratingFromGameDetailsTable.Ratings.Add(userName, rating);
                        ratingFromGameDetailsTable.NumberOfRatings++;
                        ratingFromGameDetailsTable.Total += rating;
                        ratingsDifferences.Add(gameName, rating);
                        newRatingsAdded.Add(gameName);
                    }
                    else
                    {
                        var oldRating = ratingFromGameDetailsTable.Ratings[userName];
                        var difference = rating - oldRating; // e.g. 7 - 5
                        ratingFromGameDetailsTable.Ratings[userName] = rating;
                        ratingFromGameDetailsTable.Total += difference;
                        ratingsDifferences.Add(gameName, difference);
                    }

                    var average = (double)ratingFromGameDetailsTable.Total / ratingFromGameDetailsTable.NumberOfRatings;

                    ratingFromGameDetailsTable.Average = Math.Round(average, 2);

                    Console.WriteLine($"New number of ratings {ratingFromGameDetailsTable.NumberOfRatings}");
                    Console.WriteLine($"New total of ratings {ratingFromGameDetailsTable.Total}");
                    Console.WriteLine($"New average of ratings {ratingFromGameDetailsTable.Average}");
                }

                ratingsToUpsert.Add(ratingFromGameDetailsTable);
            }

            ((IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository))).SaveBatch(ratingsToUpsert);

            return (ratingsDifferences, newRatingsAdded);
        }

        private void SaveRatingTotalInformation(Dictionary<string, int> ratingsDifference, List<string> newRatingsAdded)
        {
            var totalNumberOfUserRatings = newRatingsAdded.Count;
            var totalAddedToUserRatings = ratingsDifference.Sum(x => x.Value);

            var miscRepository = (IMiscRepository)services.GetService(typeof(IMiscRepository));
            var totalNumber = miscRepository.Load("Ratings", "TotalNumber");
            var total = miscRepository.Load("Ratings", "Total");
            var averageRating = miscRepository.Load("Ratings", "Average");

            Console.WriteLine($"Previous total of all ratings added together {totalNumber.Value}");
            Console.WriteLine($"Previous number of ratings submitted {total.Value}");
            Console.WriteLine($"Previous average of all ratings {averageRating.Value}");

            var totalNumberInt = int.Parse(totalNumber.Value);
            totalNumber.Value = (totalNumberInt + totalNumberOfUserRatings).ToString();

            var totalInt = int.Parse(total.Value);
            total.Value = (totalInt + totalAddedToUserRatings).ToString();

            averageRating.Value = Math.Round(double.Parse(total.Value) / double.Parse(totalNumber.Value), 2).ToString();

            miscRepository.Save(totalNumber);
            miscRepository.Save(total);
            miscRepository.Save(averageRating);

            Console.WriteLine($"New total of all ratings added together {totalNumber.Value}");
            Console.WriteLine($"New number of ratings submitted {total.Value}");
            Console.WriteLine($"New average of all ratings {averageRating.Value}");
        }

        private void SaveRatingIntoMiscTable(Dictionary<string, int> ratings, List<string> newGamesAdded)
        {
            var pinballGames = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Games", "Pinball");
            var pinballNames = pinballGames.List1.ConvertAll(g => ConvertName(g));
            pinballNames.AddRange(pinballGames.List2.ConvertAll(g => ConvertName(g)));

            var pinballRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Pinball");
            var arcadeRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Arcade Games");

            foreach (var rating in ratings)
            {
                // If the rating difference is 0 then the rating stayed the same so we dont need to do anythign with it
                // or the rating given was 0 in which case we forget about it.
                if (rating.Value == 0)
                {
                    continue;
                }

                if (pinballNames.Contains(rating.Key))
                {
                    if (pinballRatings.Dictionary == null)
                    {
                        pinballRatings.Dictionary = new Dictionary<string, string>();
                    }

                    if (pinballRatings.Dictionary.ContainsKey(rating.Key))
                    {
                        var oldRating = pinballRatings.Dictionary[rating.Key];
                        var average = double.Parse(oldRating.Split(',')[0]);
                        var numberOfRatings = double.Parse(oldRating.Split(',')[1]);
                        var total = average * numberOfRatings;

                        // Add the difference in the array for the gamekey
                        var newTotal = total + rating.Value;
                        // If our new games collection has the game name in then we need to add to the value
                        // otherwise we leave it the same
                        var newNumberOfRatings = newGamesAdded.Contains(rating.Key) ? numberOfRatings + 1 : numberOfRatings;
                        var newAverage = Math.Round(newTotal / newNumberOfRatings, 2).ToString();

                        pinballRatings.Dictionary[rating.Key] = newAverage + "," + newNumberOfRatings.ToString();
                    }
                    else
                    {
                        pinballRatings.Dictionary.Add(rating.Key, rating.Value.ToString() + ",1");
                    }
                }
                else
                {
                    if (arcadeRatings.Dictionary == null)
                    {
                        arcadeRatings.Dictionary = new Dictionary<string, string>();
                    }

                    if (arcadeRatings.Dictionary.ContainsKey(rating.Key))
                    {
                        var oldRating = arcadeRatings.Dictionary[rating.Key];
                        var average = double.Parse(oldRating.Split(',')[0]);
                        var numberOfRatings = double.Parse(oldRating.Split(',')[1]);
                        var total = average * numberOfRatings;

                        Console.WriteLine($"Amending old rating to misc table {rating.Key}, average {average}, number {numberOfRatings}, total {total}");

                        // Add the difference in the array for the gamekey
                        var newTotal = total + rating.Value;
                        // If our new games collection has the game name in then we need to add to the value
                        // otherwise we leave it the same
                        var newNumberOfRatings = newGamesAdded.Contains(rating.Key) ? numberOfRatings + 1 : numberOfRatings;
                        var newAverage = Math.Round(newTotal / newNumberOfRatings, 2).ToString();

                        arcadeRatings.Dictionary[rating.Key] = newAverage + "," + newNumberOfRatings.ToString();

                        Console.WriteLine($"New rating to misc table {rating.Key}, average {newAverage}, number {newNumberOfRatings}, total {newTotal}");
                    }
                    else
                    {
                        Console.WriteLine($"Adding new rating to misc table {rating.Key}, {rating.Value}");
                        arcadeRatings.Dictionary.Add(rating.Key, rating.Value.ToString() + ",1");
                    }
                }
            }

            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(pinballRatings);
            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(arcadeRatings);
        }

        private string ConvertName(string name)
        {
            name = name.ToLower();
            name = name.Replace(" ", "_");
            return name;
        }
    }
}
