#region Copyright (C) 2005-2007 Team MediaPortal
/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
 * 
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion
using System;
using System.Collections.Generic;

namespace ProcessPlugins.EpgGrabber
{
  public static class GenreMap
  {
    private static Dictionary<string, string> _genreMap = new Dictionary<string, string>(204);

    /// <summary>
    /// Initializes the <see cref="T:GenreMap"/> class.
    /// </summary>
    static GenreMap()
    {
      _genreMap.Add("Adventure", "Action/Adventure");
      _genreMap.Add("Action", "Action/Adventure");
      _genreMap.Add("Adults Only", "Adults Only");
      _genreMap.Add("Animated", "Animation");
      _genreMap.Add("Anime", "Animation");
      _genreMap.Add("Arts/crafts", "Arts");
      _genreMap.Add("Ballet", "Arts");
      _genreMap.Add("Performing arts", "Arts");
      _genreMap.Add("Theater", "Arts");
      _genreMap.Add("Dance", "Arts");
      _genreMap.Add("Art", "Arts");
      _genreMap.Add("Opera", "Arts");
      _genreMap.Add("Children", "Children");
      _genreMap.Add("Children-music", "Children");
      _genreMap.Add("Children-special", "Children");
      _genreMap.Add("Children-talk", "Children");
      _genreMap.Add("Comedy-drama", "Comedy");
      _genreMap.Add("Comedy", "Comedy");
      _genreMap.Add("Community", "Community");
      _genreMap.Add("Cooking", "Cooking");
      _genreMap.Add("Interview", "Documentary");
      _genreMap.Add("Docudrama", "Documentary");
      _genreMap.Add("Documentary", "Documentary");
      _genreMap.Add("Drama", "Drama");
      _genreMap.Add("Suspense", "Drama");
      _genreMap.Add("Crime drama", "Drama");
      _genreMap.Add("Mystery", "Drama");
      _genreMap.Add("Crime", "Drama");
      _genreMap.Add("Anthology", "Educational");
      _genreMap.Add("Consumer", "Educational");
      _genreMap.Add("Biography", "Educational");
      _genreMap.Add("Debate", "Educational");
      _genreMap.Add("Environment", "Educational");
      _genreMap.Add("Educational", "Educational");
      _genreMap.Add("Entertainment", "Entertainment");
      _genreMap.Add("Fashion", "Special Interest");
      _genreMap.Add("Game show", "Game Show");
      _genreMap.Add("Historical drama", "History");
      _genreMap.Add("History", "History");
      _genreMap.Add("Holiday special", "Holiday");
      _genreMap.Add("Holiday music special", "Holiday");
      _genreMap.Add("Holiday-children", "Holiday");
      _genreMap.Add("Holiday music", "Holiday");
      _genreMap.Add("Holiday-children special", "Holiday");
      _genreMap.Add("Holiday", "Holiday");
      _genreMap.Add("Horror", "Horror");
      _genreMap.Add("Miniseries", "Miniseries");
      _genreMap.Add("Parade", "Misc");
      _genreMap.Add("Awards", "Misc");
      _genreMap.Add("Extreme", "Misc");
      _genreMap.Add("Auction", "Misc");
      _genreMap.Add("Collectibles", "Misc");
      _genreMap.Add("Agriculture", "Nature");
      _genreMap.Add("Nature", "Nature");
      _genreMap.Add("Animals", "Nature");
      _genreMap.Add("Bus./financial", "News");
      _genreMap.Add("News", "News");
      _genreMap.Add("Newsmagazine", "News");
      _genreMap.Add("Public affairs", "News");
      _genreMap.Add("Talk", "News");
      _genreMap.Add("Politics", "News");
      _genreMap.Add("Reality", "Reality");
      _genreMap.Add("Religious", "Religious");
      _genreMap.Add("Romance", "Romance");
      _genreMap.Add("Romance-comedy", "Romance");
      _genreMap.Add("Computers", "Science");
      _genreMap.Add("Fantasy", "Science Fiction");
      _genreMap.Add("Science fiction", "Science Fiction");
      _genreMap.Add("Sitcom", "Sitcom");
      _genreMap.Add("Soap special", "Soaps");
      _genreMap.Add("Soap talk", "Soaps");
      _genreMap.Add("Soap", "Soaps");
      _genreMap.Add("Special", "Special");
      _genreMap.Add("House/garden", "Special Interest");
      _genreMap.Add("Parenting", "Special Interest");
      _genreMap.Add("Home improvement", "Special Interest");
      _genreMap.Add("Shopping", "Special Interest");
      _genreMap.Add("Health", "Special Interest");
      _genreMap.Add("Horse", "Special Interest");
      _genreMap.Add("Medical", "Special Interest");
      _genreMap.Add("Variety", "Special Interest");
      _genreMap.Add("Travel", "Special Interest");
      _genreMap.Add("War", "Special Interest");
      _genreMap.Add("Gay/lesbian", "Special Interest");
      _genreMap.Add("Fundraiser", "Special Interest");
      _genreMap.Add("Skeleton", "Special Interest");
      _genreMap.Add("Aviation", "Special Interest");
      _genreMap.Add("Paranormal", "Special Interest");
      _genreMap.Add("How-to", "Special Interest");
      _genreMap.Add("Self improvement", "Special Interest");
      _genreMap.Add("Law", "Special Interest");
      _genreMap.Add("Indoor soccer", "Sports");
      _genreMap.Add("Bicycle racing", "Sports");
      _genreMap.Add("Curling", "Sports");
      _genreMap.Add("Intl hockey", "Sports");
      _genreMap.Add("Intl soccer", "Sports");
      _genreMap.Add("Mountain biking", "Sports");
      _genreMap.Add("Motorcycle", "Sports");
      _genreMap.Add("Billiards", "Sports");
      _genreMap.Add("Darts", "Sports");
      _genreMap.Add("Lacrosse", "Sports");
      _genreMap.Add("Luge", "Sports");
      _genreMap.Add("Martial arts", "Sports");
      _genreMap.Add("Diving", "Sports");
      _genreMap.Add("Motorsports", "Sports");
      _genreMap.Add("Kayaking", "Sports");
      _genreMap.Add("Auto", "Sports");
      _genreMap.Add("Boat", "Sports");
      _genreMap.Add("Motorcycle racing", "Sports");
      _genreMap.Add("Intl basketball", "Sports");
      _genreMap.Add("Dog racing", "Sports");
      _genreMap.Add("Boat racing", "Sports");
      _genreMap.Add("Hydroplane racing", "Sports");
      _genreMap.Add("Dog sled", "Sports");
      _genreMap.Add("Aerobics", "Sports");
      _genreMap.Add("Auto racing", "Sports");
      _genreMap.Add("Bobsled", "Sports");
      _genreMap.Add("Olympics", "Sports");
      _genreMap.Add("Bodybuilding", "Sports");
      _genreMap.Add("Outdoors", "Sports");
      _genreMap.Add("Bowling", "Sports");
      _genreMap.Add("Equestrian", "Sports");
      _genreMap.Add("Playoff sports", "Sports");
      _genreMap.Add("Boxing", "Sports");
      _genreMap.Add("Exercise", "Sports");
      _genreMap.Add("Pool", "Sports");
      _genreMap.Add("Pro wrestling", "Sports");
      _genreMap.Add("Table tennis", "Sports");
      _genreMap.Add("Racquet", "Sports");
      _genreMap.Add("Rodeo", "Sports");
      _genreMap.Add("Sumo wrestling", "Sports");
      _genreMap.Add("Roller derby", "Sports");
      _genreMap.Add("Badminton", "Sports");
      _genreMap.Add("Canoe", "Sports");
      _genreMap.Add("Sports non-event", "Sports");
      _genreMap.Add("Field hockey", "Sports");
      _genreMap.Add("Rowing", "Sports");
      _genreMap.Add("Rugby", "Sports");
      _genreMap.Add("Softball", "Sports");
      _genreMap.Add("Running", "Sports");
      _genreMap.Add("Fishing", "Sports");
      _genreMap.Add("Sailing", "Sports");
      _genreMap.Add("Football", "Sports");
      _genreMap.Add("Shooting", "Sports");
      _genreMap.Add("Skateboarding", "Sports");
      _genreMap.Add("Gaelic football", "Sports");
      _genreMap.Add("Skating", "Sports");
      _genreMap.Add("Skiing", "Sports");
      _genreMap.Add("Snooker", "Sports");
      _genreMap.Add("Baseball", "Sports");
      _genreMap.Add("Snowboarding", "Sports");
      _genreMap.Add("Snowmobile", "Sports");
      _genreMap.Add("Archery", "Sports");
      _genreMap.Add("Golf", "Sports");
      _genreMap.Add("Gymnastics", "Sports");
      _genreMap.Add("Soccer", "Sports");
      _genreMap.Add("Cheerleading", "Sports");
      _genreMap.Add("Handball", "Sports");
      _genreMap.Add("Arm wrestling", "Sports");
      _genreMap.Add("Basketball", "Sports");
      _genreMap.Add("Figure skating", "Sports");
      _genreMap.Add("Speed skating", "Sports");
      _genreMap.Add("Sports event", "Sports");
      _genreMap.Add("Sports talk", "Sports");
      _genreMap.Add("Squash", "Sports");
      _genreMap.Add("Hockey", "Sports");
      _genreMap.Add("Beach soccer", "Sports");
      _genreMap.Add("Surfing", "Sports");
      _genreMap.Add("Fencing", "Sports");
      _genreMap.Add("Swimming", "Sports");
      _genreMap.Add("Tennis", "Sports");
      _genreMap.Add("Track/field", "Sports");
      _genreMap.Add("Bullfighting", "Sports");
      _genreMap.Add("Beach volleyball", "Sports");
      _genreMap.Add("Triathlon", "Sports");
      _genreMap.Add("Volleyball", "Sports");
      _genreMap.Add("Polo", "Sports");
      _genreMap.Add("Watersports", "Sports");
      _genreMap.Add("Water polo", "Sports");
      _genreMap.Add("Water skiiing", "Sports");
      _genreMap.Add("Biathlon", "Sports");
      _genreMap.Add("Weightlifting", "Sports");
      _genreMap.Add("Wrestling", "Sports");
      _genreMap.Add("Cricket", "Sports");
      _genreMap.Add("Drag racing", "Sports");
      _genreMap.Add("Yacht racing", "Sports");
      _genreMap.Add("Hunting", "Sports");
      _genreMap.Add("Bicycle", "Sports");
      _genreMap.Add("Hurling", "Sports");
      _genreMap.Add("Weather", "Weather");
      _genreMap.Add("Western", "Western");
      _genreMap.Add("Ringuette", "International");
      _genreMap.Add("Pelota vasca", "International");
      _genreMap.Add("Music talk", "Music");
      _genreMap.Add("Music special", "Music");
      _genreMap.Add("Music", "Music");
      _genreMap.Add("Musical", "Musical");
      _genreMap.Add("Musical comedy", "Musical");
      _genreMap.Add("Science", "Science");
      _genreMap.Add("Event", "Special");
      _genreMap.Add("French", "International");
      _genreMap.Add("Standup", "Comedy");
      _genreMap.Add("Spanish", "Internaltional");
      _genreMap.Add("Dog show", "Special Interest");
    }

    /// <summary>
    /// Gets the simple genre for specified genre.
    /// </summary>
    /// <param name="genre">The genre to find.</param>
    /// <returns>The simple genre if found otherwise the genre parameter is returned</returns>
    public static string GetSimpleGenre(string genre)
    {
      string str;
      return (_genreMap.TryGetValue(genre, out str) ? str : genre);
    }

  }
}