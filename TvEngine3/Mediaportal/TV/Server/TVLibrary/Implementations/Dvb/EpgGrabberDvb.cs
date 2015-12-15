#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  /// <summary>
  /// An implementation of <see cref="IEpgGrabber"/> for electronic programme
  /// guide data formats used by DVB broadcasters.
  /// </summary>
  internal class EpgGrabberDvb : IEpgGrabber, ICallBackGrabber
  {
    #region constants

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_TYPES_DVB = new Dictionary<byte, string>(16)
    {
      { 0x01, "Movie/Drama" },
      { 0x02, "News/Current Affairs" },
      { 0x03, "Show/Game Show" },
      { 0x04, "Sports" },
      { 0x05, "Children/Youth" },
      { 0x06, "Music/Ballet/Dance" },
      { 0x07, "Arts/Culture" },
      { 0x08, "Social/Political/Economics" },
      { 0x09, "Education/Science/Factual" },
      { 0x0a, "Leisure/Hobbies" },
      //{ 0x0b, "Special Characteristic" },
      { 0x0f, "User Defined" }
    };

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_SUB_TYPES_DVB = new Dictionary<byte, string>(256)
    {
      { 0x11, "Detective/Thriller" },
      { 0x12, "Adventure/Western/War" },
      { 0x13, "Science Fiction/Fantasy/Horror" },
      { 0x14, "Comedy" },
      { 0x15, "Soap/Melodrama/Folkloric" },
      { 0x16, "Romance" },
      { 0x17, "Serious/Classical/Religious/Historical Movie/Drama" },
      { 0x18, "Adult Movie/Drama" },

      { 0x21, "News/Weather Report" },
      { 0x22, "News Magazine" },
      { 0x23, "Documentary" },
      { 0x24, "Discussion/Interview/Debate" },

      { 0x31, "Game Show/Quiz/Contest" },
      { 0x32, "Variety Show" },
      { 0x33, "Talk Show" },

      { 0x41, "Special Event" },
      { 0x42, "Sports Magazine" },
      { 0x43, "Football/Soccer" },
      { 0x44, "Tennis/Squash" },
      { 0x45, "Team Sport" },
      { 0x46, "Athletics" },
      { 0x47, "Motor Sport" },
      { 0x48, "Water Sport" },
      { 0x49, "Winter Sport" },
      { 0x4a, "Equestrian" },
      { 0x4b, "Martial Sport" },

      { 0x51, "Pre-School" },
      { 0x52, "Entertainment (6 To 14 Year Old)" },
      { 0x53, "Entertainment (10 To 16 Year Old)" },
      { 0x54, "Information/Education/School" },
      { 0x55, "Cartoon/Puppets" },

      { 0x61, "Rock/Pop" },
      { 0x62, "Serious/Classical Music" },
      { 0x63, "Folk/Traditional Music" },
      { 0x64, "Jazz" },
      { 0x65, "Musical/Opera" },
      { 0x66, "Ballet" },

      { 0x71, "Performing Arts" },
      { 0x72, "Fine Arts" },
      { 0x73, "Religion" },
      { 0x74, "Popular Culture/Traditional Arts" },
      { 0x75, "Literature" },
      { 0x76, "Film/Cinema" },
      { 0x77, "Experimental Film/Video" },
      { 0x78, "Broadcasting/Press" },
      { 0x79, "New Media" },
      { 0x7a, "Arts/Culture Magazine" },
      { 0x7b, "Fashion" },

      { 0x81, "Magazine/Report/Documentary" },
      { 0x82, "Economics/Social Advisory" },
      { 0x83, "Remarkable People" },

      { 0x91, "Nature/Animals/Environment" },
      { 0x92, "Technology/Natural Science" },
      { 0x93, "Medicine/Physiology/Psychology" },
      { 0x94, "Foreign Countries/Expeditions" },
      { 0x95, "Social/Spiritual Science" },
      { 0x96, "Further Education" },
      { 0x97, "Languages" },

      { 0xa1, "Tourism/Travel" },
      { 0xa2, "Handicraft" },
      { 0xa3, "Motoring" },
      { 0xa4, "Fitness & Health" },
      { 0xa5, "Cooking" },
      { 0xa6, "Advertisement/Shopping" },
      { 0xa7, "Gardening" }

      /*{ 0xb0, "Original Language" },
      { 0xb1, "Black & White" },
      { 0xb2, "Unpublished" },
      { 0xb3, "Live Broadcast" },
      { 0xb4, "Plano-Stereoscopic" },
      { 0xb5, "Local Or Regional" }*/
    };
          
    private static readonly IDictionary<byte, string> MAPPING_CONTENT_TYPES_DISH = new Dictionary<byte, string>(16)
    {
      { 1, "Movie" },
      { 2, "Sports" },
      { 3, "News/Business" },
      { 4, "Family/Children" },
      { 5, "Education" },
      { 6, "Series/Special" },
      { 7, "Music/Art" },
      { 8, "Religious" }
    };

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_SUB_TYPES_DISH = new Dictionary<byte, string>(256)
    {
      { 1, "Action" },
      { 2, "Adult" },
      { 3, "Adventure" },
      { 4, "Animals" },
      { 5, "Animated" },
      { 6, "Anime" },
      { 7, "Anthology" },
      { 8, "Art" },
      { 9, "Auto" },
      { 10, "Anthology" },
      { 11, "Ballet" },
      { 12, "Baseball" },
      { 13, "Basketball" },
      { 14, "Beach Soccer" },
      { 15, "Beach Volleyball" },
      { 16, "Biathlon" },
      { 17, "Biography" },
      { 18, "Boats" },
      { 19, "Boat Racing" },
      { 20, "Bowling" },
      { 21, "Boxing" },
      { 22, "Business - Financial" },

      { 26, "Children" },
      { 27, "Children - Special" },
      { 28, "Children - News" },
      { 29, "Children - Music" },

      { 31, "Collectibles" },
      { 32, "Comedy" },
      { 33, "Comedy Drama" },
      { 34, "Computers" },
      { 35, "Cooking" },
      { 36, "Crime" },
      { 37, "Crime Drama" },
      { 38, "Curling" },
      { 39, "Dance" },
      { 40, "Dark Comedy" },
      { 41, "Docudrama" },
      { 42, "Documentary" },
      { 43, "Drama" },
      { 44, "Educational" },
      { 45, "Erotic" },

      { 47, "Exercise" },

      { 49, "Fantasy" },
      { 50, "Fashion" },
      { 51, "Fencing" },
      { 52, "Fishing" },
      { 53, "Football" },
      { 54, "French" },
      { 55, "Fundraiser" },
      { 56, "Game Show" },
      { 57, "Golf" },
      { 58, "Gymnastics" },
      { 59, "Health" },
      { 60, "History" },
      { 61, "Historical Drama" },
      { 62, "Hockey" },
      { 63, "Holiday" },
      { 64, "Holiday - Children" },
      { 65, "Holiday - Children Special" },
      { 66, "Holiday - Music" },
      { 67, "Holiday - Music Special" },
      { 68, "Holiday - Special" },
      { 69, "Horror" },
      { 70, "Horse Racing" },
      { 71, "House & Garden" },

      { 73, "How To" },

      { 75, "Interview" },

      { 77, "Lacrossse" },

      { 79, "Martial Arts" },
      { 80, "Medical" },
      { 81, "Mini-Series" },
      { 82, "Motor Sport" },
      { 83, "Motorcycle" },
      { 84, "Music" },
      { 85, "Music Special" },
      { 86, "Music Talk" },
      { 87, "Musical" },
      { 88, "Musical Comedy" },

      { 90, "Mystery" },
      { 91, "Nature" },
      { 92, "News" },

      { 95, "Opera" },
      { 96, "Outdoors" },
      { 97, "Parade" },
      { 98, "Politics" },
      { 99, "Public Affairs" },

      { 102, "Reality" },
      { 103, "Religious" },
      { 104, "Rodeo" },
      { 105, "Romance" },
      { 106, "Romantic Comedy" },
      { 107, "Rugby" },
      { 108, "Running" },

      { 110, "Science" },
      { 111, "Science Fiction" },
      { 112, "Self Improvement" },
      { 113, "Shopping" },

      { 116, "Skiing" },

      { 119, "Soap" },

      { 123, "Soccer" },
      { 124, "Softball" },
      { 125, "Spanish" },
      { 126, "Special" },
      { 127, "Speedskating" },
      { 128, "Sports Event" },
      { 129, "Sports Non-event" },
      { 130, "Sports Discussion" },
      { 131, "Suspense" },

      { 133, "Swimming" },
      { 134, "Discussion" },
      { 135, "Tennis" },
      { 136, "Thriller" },
      { 137, "Track & Field" },
      { 138, "Travel" },
      { 139, "Variety" },
      { 140, "Volleyball" },
      { 141, "War" },
      { 142, "Watersports" },
      { 143, "Weather" },
      { 144, "Western" },

      { 146, "Wrestling" },
      { 147, "Yoga" },
      { 148, "Agriculture" },
      { 149, "Anime" },

      { 151, "Arm Wrestling" },
      { 152, "Arts & Crafts" },
      { 153, "Auction" },
      { 154, "Motor Racing" },
      { 155, "Air Racing" },
      { 156, "Badminton" },

      { 160, "Cycle Racing" },
      { 161, "Sailing" },
      { 162, "Bobsled" },
      { 163, "Body Building" },
      { 164, "Canoeing" },
      { 165, "Cheerleading" },
      { 166, "Community" },
      { 167, "Consumer" },

      { 170, "Debate" },
      { 171, "Diving" },
      { 172, "Dog Show" },
      { 173, "Drag Racing" },
      { 174, "Entertainment" },
      { 175, "Environment" },
      { 176, "Equestrian" },

      { 179, "Field Hockey" },
      { 180, "Figure Skating" },
      { 181, "Football" },
      { 182, "Gay/Lesbian" },
      { 183, "Handball" },
      { 184, "Home Improvement" },
      { 185, "Hunting" },
      { 186, "Hurling" },
      { 187, "Hydroplane Racing" },

      { 193, "Law" },

      { 195, "Motorcycle Racing" },

      { 197, "News Magazine" },

      { 199, "Paranormal" },
      { 200, "Parenting" },

      { 202, "Performing Arts" },
      { 203, "Play-Off" },
      { 204, "Politics" },
      { 205, "Polo" },
      { 206, "Pool" },
      { 207, "Pro Wrestling" },
      { 208, "Ringuette" },
      { 209, "Roller Derby" },
      { 210, "Rowing" },
      { 211, "Sailing" },
      { 212, "Shooting" },
      { 213, "Sitcom" },
      { 214, "Skateboarding" },
      { 215, "Skating" },
      { 216, "Skeleton" },
      { 217, "Snowboarding" },
      { 218, "Snowmobile" },

      { 221, "Standup" },
      { 222, "Sumo Wrestling" },
      { 223, "Surfing" },
      { 224, "Tennis" },
      { 225, "Triathlon" },
      { 226, "Water Polo" },
      { 227, "Water Skiing" },
      { 228, "Weightlifting" },
      { 229, "Yacht Racing" },
      { 230, "Card Games" },
      { 231, "Poker" },

      { 233, "Musical" },
      { 234, "Military" },
      { 235, "Technology" },
      { 236, "Mixed Martial Arts" },
      { 237, "Action Sports" }
    };

    // Incomplete due to limited sample content.
    private static readonly IDictionary<byte, string> MAPPING_CONTENT_TYPES_VIRGIN_MEDIA = new Dictionary<byte, string>(16)
    {
      { 0x00, "Children" },
      { 0x02, "Comedy/Drama" },
      { 0x03, "Entertainment/Reality" },
      { 0x04, "Movie" },
      { 0x05, "Lifestyle" },
      { 0x08, "Factual" },
      { 0x0c, "Sport" },
      { 0x0f, "Special Interest" }
    };

    // Incomplete due to limited sample content.
    private static readonly IDictionary<byte, string> MAPPING_CONTENT_SUB_TYPES_VIRGIN_MEDIA = new Dictionary<byte, string>(256)
    {
      { 0x01, "Sitcom" },
      { 0x02, "Adventure" },
      { 0x03, "Arts/Crafts/Educational" },

      { 0x24, "Science Fiction/Fantasy" },

      { 0x31, "Game Show/Quiz/Contest" },

      { 0x41, "Comedy" },
      { 0x42, "Drama/Thriller" },
      { 0x48, "Romance" },
      { 0x4a, "Horror" },

      { 0x52, "Cooking" },
      { 0x54, "Fashion" },
      { 0x55, "Fitness & Health" },

      { 0x83, "News/Weather" },
      { 0x85, "Business" },
      { 0x86, "Documentary/Discussion/Interview/Debate" },

      { 0xc1, "Team Sports" },
      { 0xc6, "Individual Sports" },
      { 0xc9, "Motor Sport" },

      { 0xf3, "Adult" }
    };

    #region OpenTV

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_AU = new Dictionary<byte, string>(8)
    {
      { 0x02, "Entertainment" },
      { 0x04, "Movie" },
      { 0x06, "Sport" },
      { 0x09, "News & Documentary" },
      { 0x0a, "Kids & Family" },
      { 0x0c, "Music & Radio" },
      { 0x0e, "Special Interest" },
      { 0x0f, "Adult" }
    };

    // Entries marked with a question mark are uncertain.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_AU = new Dictionary<ushort, string>(256)
    {
      { 0x0201, "War & Western" },
      { 0x0202, "Drama" },
      { 0x0203, "Comedy" },
      { 0x0204, "Reality" },
      { 0x0205, "Talk Show" },
      { 0x0206, "Sci-Fi & Fantasy" },
      { 0x0207, "Lifestyle & Documentary" },
      { 0x0208, "Light Entertainment" },
      // unknown
      { 0x020a, "Other" },

      { 0x0401, "Action & Adventure" },
      { 0x0402, "War, Western & History" },
      { 0x0403, "Drama/Romance" },                          // ?
      { 0x0404, "Horror / Sci-Fi & Fantasy" },              // ?
      { 0x0405, "Comedy / Musical & Dance" },               // ?
      { 0x0406, "Mystery & Crime / Thriller & Suspense" },  // ?
      { 0x0407, "Animation / Kids & Family" },              // ?
      { 0x0408, "Documentary/Other" },                      // ?

      { 0x0601, "Rugby League" },
      { 0x0602, "AFL" },
      { 0x0603, "Rugby Union" },
      { 0x0604, "Football & Soccer" },
      { 0x0605, "Cricket" },            // ?
      { 0x0606, "Baseball/Golf" },
      { 0x0607, "Court Sports" },
      { 0x0608, "Boxing & Wrestling" },
      { 0x0609, "Track & Pool" },
      { 0x060a, "Extreme Sports" },
      { 0x060b, "Racing" },
      { 0x060c, "Other" },

      { 0x0901, "Business & Finance" },
      { 0x0902, "Local" },
      { 0x0903, "International" },
      { 0x0904, "People & Culture" },
      { 0x0905, "History" },
      { 0x0906, "Natural World" },
      { 0x0907, "Travel & Adventure" },
      { 0x0908, "Other" },

      { 0x0a01, "Pre-school" },
      { 0x0a02, "Adventure & Action" },
      { 0x0a03, "Comedy" },
      { 0x0a04, "Animation & Cartoon" },
      { 0x0a05, "Educational" },
      // unknown
      { 0x0a07, "Game Show" },
      { 0x0a08, "Other" },

      { 0x0c01, "Pop" },                // ?
      // unknown
      { 0x0c03, "Blues & Jazz" },       // ?
      // unknown
      { 0x0c05, "Dance & Techno" },     // ?
      // unknown
      { 0x0c09, "Classical & Opera" },
      // unknown
      { 0x0c0b, "Country" },
      { 0x0c0c, "Live & Request" },     // ?
      { 0x0c0d, "Other" },              // ?

      { 0x0e01, "Religion" },
      { 0x0e02, "Foreign Language" },   // ?
      // unknown
      { 0x0e04, "Shopping" },
      { 0x0e05, "Help/Information" },   // ?

      { 0x0f02, "Adult" }               // ?
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_IT = new Dictionary<byte, string>(8)
    {
      { 0x01, "Intrattenimento" },
      { 0x02, "Sport" },
      { 0x03, "Film" },
      { 0x04, "Mondo e Tendenze" },
      { 0x05, "Informazione" },
      { 0x06, "Ragazzi e Musica" },
      { 0x07, "Altri Programmi" }
    };

    // Incomplete due to limited sample content.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_IT = new Dictionary<ushort, string>(256)
    {
      { 0x0100, "Intrattenimento" },
      { 0x0101, "Fiction" },
      { 0x0102, "Sit Com" },
      { 0x0103, "Show" },
      { 0x0104, "Telefilm" },
      { 0x0105, "Soap Opera" },
      { 0x0106, "Telenovela" },
      { 0x0107, "Fantascienza" },
      { 0x0108, "Animazione" },
      { 0x0109, "Giallo" },
      { 0x010a, "Drammatico" },
      { 0x010b, "Reality Show" },
      { 0x010c, "Miniserie" },
      { 0x010d, "Spettacolo" },
      { 0x010e, "Quiz" },
      { 0x010f, "Talk Show" },
      { 0x0110, "Varietà" },
      { 0x0111, "Festival" },
      { 0x0112, "Teatro" },
      { 0x0113, "Gioco" },

      { 0x0200, "Sport" },
      { 0x0201, "Calcio" },
      { 0x0202, "Tennis" },
      { 0x0203, "Motori" },
      { 0x0204, "Altri" },
      { 0x0205, "Baseball" },
      { 0x0206, "Ciclismo" },
      { 0x0207, "Rugby" },
      { 0x0208, "Basket" },
      { 0x0209, "Boxe" },
      { 0x020a, "Atletica" },
      { 0x020b, "Football USA" },
      { 0x020c, "Hockey" },
      { 0x020d, "Sci" },
      { 0x020e, "Equestri" },
      { 0x020f, "Golf" },
      { 0x0210, "Nuoto" },
      { 0x0211, "Wrestling" },
      // unknown
      { 0x0213, "Volley" },
      { 0x0214, "Poker" },
      { 0x0215, "Vela" },
      { 0x0216, "Sport Invernali" },

      { 0x0300, "Cinema" },
      { 0x0301, "Drammatico" },
      { 0x0302, "Commedia" },
      { 0x0303, "Romantico" },
      { 0x0304, "Azione" },
      { 0x0305, "Fantascienza" },
      { 0x0306, "Western" },
      { 0x0307, "Comico" },
      { 0x0308, "Fantastico" },
      { 0x0309, "Avventura" },
      { 0x030a, "Poliziesco" },
      { 0x030b, "Guerra" },
      { 0x030c, "Horror" },
      { 0x030d, "Animazione" },
      { 0x030e, "Thriller" },
      { 0x030f, "Musicale" },
      { 0x0310, "Corto" },
      { 0x0311, "Cortometraggio" },

      { 0x0400, "Mondi e Culture" },
      { 0x0401, "Natura" },
      { 0x0402, "Arte e Cultura" },
      { 0x0403, "Lifestyle" },
      { 0x0404, "Viaggi" },
      { 0x0405, "Documentario" },
      { 0x0406, "Società" },
      { 0x0407, "Scienza" },
      { 0x0408, "Storia" },
      { 0x0409, "Sport" },
      { 0x040a, "Pesca" },
      { 0x040b, "Popoli" },
      { 0x040c, "Cinema" },
      { 0x040d, "Musica" },
      { 0x040e, "Hobby" },
      { 0x040f, "Caccia" },
      { 0x0410, "Reportage" },
      { 0x0411, "Magazine" },
      { 0x0412, "Magazine Cultura" },
      { 0x0413, "Magazine Scienza" },
      { 0x0414, "Politica" },
      { 0x0415, "Magazine Cinema" },
      { 0x0416, "Magazine Sport" },
      { 0x0417, "Attualità" },
      { 0x0418, "Moda" },
      { 0x0419, "Economia" },
      { 0x041a, "Tecnologia" },
      { 0x041b, "Magazine Viaggi" },
      { 0x041c, "Magazine Natura" },
      { 0x041d, "Avventura" },
      { 0x041e, "Cucina" },
      { 0x041f, "Televendita" },

      { 0x0500, "News" },
      { 0x0501, "Notiziario" },
      { 0x0502, "Sport" },
      { 0x0503, "Economia" },
      // unknown
      { 0x0505, "Meteo" },

      { 0x0601, "Bambini" },
      { 0x0602, "Educational" },
      { 0x0603, "Cartoni Animati" },
      { 0x0604, "Musica" },
      { 0x0605, "Film Animazione" },
      { 0x0606, "Film" },
      { 0x0607, "Telefilm" },
      { 0x0608, "Magazine" },
      // unknown
      { 0x060a, "Documentario" },
      // unknown
      { 0x0612, "Jazz" },
      // unknown
      { 0x0614, "Danza" },
      { 0x0615, "Videoclip" },

      { 0x0700, "Altri Canali" },
      { 0x0701, "Educational" },
      { 0x0702, "Regionale" },
      { 0x0703, "Shopping" },
      { 0x0704, "Altri" },
      { 0x0705, "Inizio e Fine Trasmissioni" },
      { 0x0706, "Eventi Speciali" },
      { 0x0707, "Film per Adulti" }
    };

    // These are overrides for DVB content types.
    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_NZ = new Dictionary<byte, string>(8)
    {
      { 0x01, "Movie" },
      { 0x0f, "Adult" }
    };

    // These are overrides for DVB content types.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_NZ = new Dictionary<ushort, string>(256)
    {
      { 0x010e, "Western" },

      { 0x0304, "Reality" },
      { 0x0305, "Action" },
      { 0x0306, "Drama" },
      { 0x0307, "Comedy" },
      { 0x0308, "Documentary" },
      { 0x0309, "Soap" },
      { 0x030a, "Sci-Fi" },
      { 0x030b, "Crime" },
      { 0x030c, "Sport" },

      { 0x0402, "Golf" },
      { 0x0405, "Rugby" },
      { 0x040c, "Rugby League" },
      { 0x040d, "Cricket" },
      { 0x040e, "Cycling" },

      { 0x0507, "Sport" },
      { 0x0509, "Comedy" },
      // unknown
      { 0x050b, "Movie" },
      { 0x050c, "Game Show" },

      { 0x0609, "Country" },

      { 0x0408, "History" },
      { 0x0409, "Reality/Documentary" },
      { 0x040a, "Biography/Documentary" },
      { 0x040b, "Reality/Travel" },
      // unknown
      { 0x040d, "Human Science/Culture" },
      { 0x040e, "Crime/Investigation" },

      { 0x0a07, "Property" },
      { 0x0a0b, "Home Restoration/Make-Over" }
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_UK = new Dictionary<byte, string>(8)
    {
      { 0x01, "Specialist" },
      { 0x02, "Children" },
      { 0x03, "Entertainment" },
      { 0x04, "Music & Radio" },
      { 0x05, "News & Documentaries" },
      { 0x06, "Movies" },
      { 0x07, "Sports" }
    };

    // Entries marked with a question mark are uncertain.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_UK = new Dictionary<ushort, string>(256)
    {
      { 0x0101, "Adult" },
      { 0x0102, "Information" },
      { 0x0103, "Shopping" },
      { 0x0104, "Gaming" },

      { 0x0201, "Cartoons" },
      { 0x0202, "Comedy" },
      { 0x0203, "Drama" },
      { 0x0204, "Educational" },
      { 0x0205, "Under 5" },
      { 0x0206, "Factual" },
      { 0x0207, "Magazine" },           // ?
      { 0x0208, "Games Shows" },
      { 0x0209, "Games" },              // ?

      { 0x0301, "Action" },
      { 0x0302, "Comedy" },
      { 0x0303, "Detective" },
      { 0x0304, "Drama" },
      { 0x0305, "Game Shows" },
      { 0x0306, "Sci-Fi" },
      { 0x0307, "Soaps" },
      { 0x0308, "Animation" },
      { 0x0309, "Chat Show" },
      { 0x030a, "Cooking" },
      { 0x030b, "Factual" },
      { 0x030c, "Fashion" },
      { 0x030d, "Gardening" },
      { 0x030e, "Travel" },
      { 0x030f, "Technology" },
      { 0x0310, "Arts" },
      { 0x0311, "Lifestyle" },
      { 0x0312, "Home" },
      { 0x0313, "Magazine" },
      { 0x0314, "Medical" },
      { 0x0315, "Reviews" },
      { 0x0316, "Antiques" },
      { 0x0317, "Motors" },
      { 0x0318, "Art & Literature" },   // ?
      { 0x0319, "Ballet" },             // ?
      { 0x031a, "Opera" },

      { 0x0401, "Classical" },
      { 0x0402, "Folk & Country" },
      { 0x0403, "National Music" },
      { 0x0404, "Jazz" },               // ?
      { 0x0405, "Opera" },              // ?
      { 0x0406, "Rock & Pop" },
      { 0x0407, "Alternative Music" },
      { 0x0408, "Events" },
      { 0x0409, "Club & Dance" },
      { 0x040a, "Hip Hop" },
      { 0x040b, "Soul/R+B" },
      { 0x040c, "Dance" },
      { 0x040d, "Ballet" },             // ?
      // unknown
      { 0x040f, "Current Affairs" },    // ?
      { 0x0410, "Features" },
      { 0x0411, "Arts & Literature" },  // ?
      { 0x0412, "Factual" },
      // unknown
      { 0x0415, "Lifestyle" },          // ?
      { 0x0416, "News & Weather" },     // ?
      { 0x0417, "Easy Listening" },     // ?
      { 0x0418, "Discussion" },         // ?
      { 0x0419, "Entertainment" },
      { 0x041a, "Religious" },

      { 0x0501, "Business" },
      { 0x0502, "World Cultures" },
      { 0x0503, "Adventure" },
      { 0x0504, "Biography" },          // ?
      { 0x0505, "Educational" },
      { 0x0506, "Features" },
      { 0x0507, "Politics" },
      { 0x0508, "News" },
      { 0x0509, "Nature" },
      { 0x050a, "Religious" },
      { 0x050b, "Science" },
      { 0x050c, "Showbiz" },
      { 0x050d, "War Documentaries" },
      { 0x050e, "Historical" },
      { 0x050f, "Ancient" },            // ?
      { 0x0510, "Transport" },          // ?
      { 0x0511, "Docudrama" },          // ?
      { 0x0512, "World Affairs" },      // ?
      { 0x0513, "Features" },           // ?
      { 0x0514, "Showbiz" },            // ?
      { 0x0515, "Politics" },           // ?
      // unknown
      { 0x0517, "World Affairs" },      // ?

      { 0x0601, "Action" },
      { 0x0602, "Animation" },
      // unknown
      { 0x0604, "Comedy" },
      { 0x0605, "Family" },
      { 0x0606, "Drama" },
      // unknown
      { 0x0608, "Sci-Fi" },
      { 0x0609, "Thriller" },
      { 0x060a, "Horror" },
      { 0x060b, "Romance" },
      { 0x060c, "Musical" },
      { 0x060d, "Mystery" },
      { 0x060e, "Western" },
      { 0x060f, "Factual" },
      { 0x0610, "Fantasy" },
      { 0x0611, "Erotic" },
      { 0x0612, "Adventure" },
      { 0x0613, "War" },

      { 0x0701, "American Football" },  // ?
      { 0x0702, "Athletics" },
      { 0x0703, "Baseball" },
      { 0x0704, "Basketball" },
      { 0x0705, "Boxing" },
      { 0x0706, "Cricket" },
      { 0x0707, "Fishing" },
      { 0x0708, "Football" },
      { 0x0709, "Golf" },
      { 0x070a, "Ice Hockey" },
      { 0x070b, "Motor Sport" },
      { 0x070c, "Racing" },
      { 0x070d, "Rugby" },
      { 0x070e, "Equestrian" },
      { 0x070f, "Winter Sports" },
      { 0x0710, "Snooker/Pool" },
      { 0x0711, "Tennis" },
      { 0x0712, "Wrestling" },
      { 0x0713, "Darts" },              // ?
      { 0x0714, "Watersports" },        // ?
      { 0x0715, "Extreme" },
      { 0x0716, "Other" }               // ?
    };

    #endregion

    #endregion

    #region variables

    /// <summary>
    /// Indicator: is the grabber grabbing electronic programme guide data?
    /// </summary>
    private bool _isGrabbing = false;

    /// <summary>
    /// A delegate to notify about grab progress.
    /// </summary>
    private IEpgGrabberCallBack _callBack = null;

    #region DVB

    /// <summary>
    /// Indicator: should the grabber grab DVB EPG data?
    /// </summary>
    private bool _grabDvb = false;

    /// <summary>
    /// The DVB EPG grabber.
    /// </summary>
    private IGrabberEpgDvb _grabberDvb = null;

    /// <summary>
    /// Indicator: has the grabber seen DVB EPG data?
    /// </summary>
    private bool _isSeenDvb = false;

    /// <summary>
    /// Indicator: has the grabber received all DVB EPG data?
    /// </summary>
    private bool _isCompleteDvb = false;

    #endregion

    #region MediaHighway

    /// <summary>
    /// Indicator: should the grabber grab MediaHighway EPG data?
    /// </summary>
    private bool _grabMhw = false;

    /// <summary>
    /// The MediaHighway EPG grabber.
    /// </summary>
    private IGrabberEpgMhw _grabberMhw = null;

    /// <summary>
    /// Indicator: has the grabber seen MediaHighway EPG data?
    /// </summary>
    private bool _isSeenMhw = false;

    /// <summary>
    /// Indicator: has the grabber received all MediaHighway EPG data?
    /// </summary>
    private bool _isCompleteMhw = false;

    #endregion

    #region OpenTV

    /// <summary>
    /// Indicator: should the grabber grab OpenTV EPG data?
    /// </summary>
    private bool _grabOpenTv = false;

    /// <summary>
    /// The OpenTV EPG grabber.
    /// </summary>
    private IGrabberEpgOpenTv _grabberOpenTv = null;

    /// <summary>
    /// Indicator: has the grabber seen OpenTV EPG data?
    /// </summary>
    private bool _isSeenOpenTv = false;

    /// <summary>
    /// Indicator: has the grabber received all OpenTV EPG data?
    /// </summary>
    private bool _isCompleteOpenTv = false;

    #endregion

    /// <summary>
    /// The current transmitter tuning detail.
    /// </summary>
    private IChannel _currentTuningDetail = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgGrabberDvb"/> class.
    /// </summary>
    /// <param name="grabberDvb">The DVB EPG grabber, if available/supported.</param>
    /// <param name="grabberMhw">The MediaHighway EPG grabber, if available/supported.</param>
    /// <param name="grabberOpenTv">The OpenTV EPG grabber, if available/supported.</param>
    public EpgGrabberDvb(IGrabberEpgDvb grabberDvb, IGrabberEpgMhw grabberMhw, IGrabberEpgOpenTv grabberOpenTv)
    {
      _grabberDvb = grabberDvb;
      _grabberMhw = grabberMhw;
      _grabberOpenTv = grabberOpenTv;
      ReloadConfiguration();
    }

    /// <summary>
    /// Retrieve, translate and collate the raw EPG data into a standard format.
    /// </summary>
    private void CollectEpgData()
    {
      IDictionary<IChannel, IList<EpgProgram>> epgChannels = null;
      try
      {
        try
        {
          this.LogInfo("EPG DVB: collect data, DVB = {0} / {1}, MediaHighway = {2} / {3}, OpenTV = {4} / {5}",
                        _isSeenDvb, _isCompleteDvb, _isSeenMhw, _isCompleteMhw, _isSeenOpenTv, _isCompleteOpenTv);

          if (_isSeenMhw && _grabberMhw != null)
          {
            epgChannels = CollectMediaHighwayData();
          }
          else if (_isSeenOpenTv && _grabberOpenTv != null)
          {
            epgChannels = CollectOpenTvData();
          }
          else if (_isSeenDvb && _grabberDvb != null)
          {
            epgChannels = CollectEitData();
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "EPG DVB: failed to collect data");
        }

        if (_callBack != null)
        {
          _callBack.OnEpgReceived(_currentTuningDetail, epgChannels);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "EPG DVB: failed to handle data");
      }
      finally
      {
        _isGrabbing = false;
      }
    }

    private IDictionary<IChannel, IList<EpgProgram>> CollectMediaHighwayData()
    {
      uint eventCount = _grabberMhw.GetEventCount();
      this.LogDebug("EPG DVB: MediaHighway, initial event count = {0}", eventCount);
      IDictionary<ulong, List<EpgProgram>> channels = new Dictionary<ulong, List<EpgProgram>>(100);

      const ushort BUFFER_SIZE_SERVICE_NAME = 300;
      IntPtr bufferServiceName = Marshal.AllocCoTaskMem(BUFFER_SIZE_SERVICE_NAME);
      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_DESCRIPTION = 1000;
      IntPtr bufferDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_DESCRIPTION);
      const ushort BUFFER_SIZE_THEME_NAME = 50;
      IntPtr bufferThemeName = Marshal.AllocCoTaskMem(BUFFER_SIZE_THEME_NAME);
      const ushort BUFFER_SIZE_SUB_THEME_NAME = 50;
      IntPtr bufferSubThemeName = Marshal.AllocCoTaskMem(BUFFER_SIZE_SUB_THEME_NAME);
      try
      {
        ulong eventId;
        ushort originalNetworkId;
        ushort transportStreamId;
        ushort serviceId;
        ulong startDateTimeEpoch;
        ushort duration;
        uint payPerViewId;
        byte descriptionLineCount;
        for (uint i = 0; i < eventCount; i++)
        {
          ushort bufferSizeServiceName = BUFFER_SIZE_SERVICE_NAME;
          ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
          ushort bufferSizeDescription = BUFFER_SIZE_DESCRIPTION;
          ushort bufferSizeThemeName = BUFFER_SIZE_THEME_NAME;
          ushort bufferSizeSubThemeName = BUFFER_SIZE_SUB_THEME_NAME;
          bool result = _grabberMhw.GetEvent(i,
                                              out eventId,
                                              out originalNetworkId,
                                              out transportStreamId,
                                              out serviceId,
                                              bufferServiceName,
                                              ref bufferSizeServiceName,
                                              out startDateTimeEpoch,
                                              out duration,
                                              bufferTitle,
                                              ref bufferSizeTitle,
                                              out payPerViewId,
                                              bufferDescription,
                                              ref bufferSizeDescription,
                                              out descriptionLineCount,
                                              bufferThemeName,
                                              ref bufferSizeThemeName,
                                              bufferSubThemeName,
                                              ref bufferSizeSubThemeName);
          if (!result)
          {
            this.LogWarn("EPG DVB: failed to get MediaHighway event, event index = {0}, event count = {1}", i, eventCount);
            continue;
          }

          string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
          if (string.IsNullOrEmpty(title))
          {
            // Placeholder or dummy event => discard.
            continue;
          }

          DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          programStartTime.AddSeconds(startDateTimeEpoch);
          programStartTime = programStartTime.ToLocalTime();
          EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
          program.Titles.Add("und", title);

          string description = TidyString(DvbTextConverter.Convert(bufferDescription, bufferSizeDescription));
          for (byte j = 0; j < descriptionLineCount; j++)
          {
            bufferSizeDescription = BUFFER_SIZE_DESCRIPTION;
            if (!_grabberMhw.GetDescriptionLine(eventId, j, bufferDescription, ref bufferSizeDescription))
            {
              this.LogWarn("EPG DVB: failed to get MediaHighway description line, event index = {0}, event ID = {1}, line count = {2}, line index = {3}",
                            i, eventId, descriptionLineCount, j);
            }
            else
            {
              description = string.Format("{0} {1}", description, TidyString(DvbTextConverter.Convert(bufferDescription, bufferSizeDescription)));
            }
          }
          program.Descriptions.Add("und", description);

          string themeName = TidyString(DvbTextConverter.Convert(bufferThemeName, bufferSizeThemeName));
          if (!string.IsNullOrEmpty(themeName))
          {
            string subThemeName = TidyString(DvbTextConverter.Convert(bufferSubThemeName, bufferSizeSubThemeName));
            if (!string.IsNullOrEmpty(themeName))
            {
              themeName = string.Format("{0}: {1}", themeName, subThemeName);
            }
            program.Categories.Add(themeName);
          }

          ulong channelKey = ((ulong)originalNetworkId << 32) | ((ulong)transportStreamId << 16) | serviceId;
          List<EpgProgram> programs;
          if (!channels.TryGetValue(channelKey, out programs))
          {
            programs = new List<EpgProgram>(100);
            channels.Add(channelKey, programs);
          }
          programs.Add(program);
        }

        IDictionary<IChannel, IList<EpgProgram>> epgChannels = new Dictionary<IChannel, IList<EpgProgram>>(channels.Count);
        int validEventCount = 0;
        foreach (var channel in channels)
        {
          ChannelDvbBase dvbChannel = _currentTuningDetail.Clone() as ChannelDvbBase;
          dvbChannel.OriginalNetworkId = (int)(channel.Key >> 32);
          dvbChannel.TransportStreamId = (int)((channel.Key >> 16) & 0xffff);
          dvbChannel.ServiceId = (int)channel.Key & 0xffff;
          dvbChannel.OpenTvChannelId = 0;
          channel.Value.Sort();
          epgChannels.Add(dvbChannel, channel.Value);
          validEventCount += channel.Value.Count;
        }

        this.LogDebug("EPG DVB: MediaHighway, channel count = {0}, event count = {1}", channels.Count, validEventCount);
        return epgChannels;
      }
      finally
      {
        if (bufferServiceName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferServiceName);
        }
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferDescription);
        }
        if (bufferThemeName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferThemeName);
        }
        if (bufferSubThemeName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferSubThemeName);
        }
      }
    }

    private IDictionary<IChannel, IList<EpgProgram>> CollectOpenTvData()
    {
      uint eventCount = _grabberOpenTv.GetEventCount();
      this.LogDebug("EPG DVB: OpenTV, initial event count = {0}", eventCount);
      IDictionary<ushort, List<EpgProgram>> channels = new Dictionary<ushort, List<EpgProgram>>(100);

      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_SHORT_DESCRIPTION = 1000;
      IntPtr bufferShortDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_SHORT_DESCRIPTION);
      const ushort BUFFER_SIZE_EXTENDED_DESCRIPTION = 1000;
      IntPtr bufferExtendedDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_EXTENDED_DESCRIPTION);
      try
      {
        ushort channelId;
        ushort eventId;
        ulong startDateTimeEpoch;
        ushort duration;
        byte categoryId;
        byte subCategoryId;
        bool isHighDefinition;
        bool hasSubtitles;
        byte parentalRating;
        ushort seriesLinkId;
        for (uint i = 0; i < eventCount; i++)
        {
          ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
          ushort bufferSizeShortDescription = BUFFER_SIZE_SHORT_DESCRIPTION;
          ushort bufferSizeExtendedDescription = BUFFER_SIZE_EXTENDED_DESCRIPTION;
          bool result = _grabberOpenTv.GetEvent(i,
                                                out channelId,
                                                out eventId,
                                                out startDateTimeEpoch,
                                                out duration,
                                                bufferTitle,
                                                ref bufferSizeTitle,
                                                bufferShortDescription,
                                                ref bufferSizeShortDescription,
                                                bufferExtendedDescription,
                                                ref bufferSizeExtendedDescription,
                                                out categoryId,
                                                out subCategoryId,
                                                out isHighDefinition,
                                                out hasSubtitles,
                                                out parentalRating,
                                                out seriesLinkId);
          if (!result)
          {
            this.LogWarn("EPG DVB: failed to get OpenTV event, event index = {0}, event count = {1}", i, eventCount);
            continue;
          }

          string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
          if (string.IsNullOrEmpty(title))
          {
            // Placeholder or dummy event => discard.
            continue;
          }

          DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          programStartTime.AddSeconds(startDateTimeEpoch);
          programStartTime = programStartTime.ToLocalTime();
          EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
          string language = "eng";
          if (string.Equals(RegionInfo.CurrentRegion.EnglishName, "Italy"))
          {
            language = "ita";
          }

          program.Titles.Add(language, title);
          program.Categories.Add(GetOpenTvProgramCategoryDescription(categoryId, subCategoryId));
          program.IsHighDefinition = isHighDefinition;
          if (hasSubtitles)
          {
            // assumption: subtitles language matches the country
            program.SubtitlesLanguages.Add(language);
          }
          string classification = GetOpenTvParentalRatingDescription(parentalRating);
          if (!string.IsNullOrEmpty(classification))
          {
            program.Classifications.Add("OpenTV", classification);
          }
          if (seriesLinkId != 0 && seriesLinkId != 0xffff)
          {
            program.SeriesId = string.Format("OpenTV:{0}", seriesLinkId);
          }

          // When available, extended description seems to contain various event attributes.
          string description = TidyString(DvbTextConverter.Convert(bufferShortDescription, bufferSizeShortDescription));
          string extendedDescription = TidyString(DvbTextConverter.Convert(bufferExtendedDescription, bufferSizeExtendedDescription));
          if (string.IsNullOrEmpty(description) || string.Equals(title, description))
          {
            description = extendedDescription;
          }
          else if (!string.IsNullOrEmpty(extendedDescription) && !description.Contains(extendedDescription))
          {
            if (extendedDescription.Contains(description))
            {
              description = extendedDescription;
            }
            else
            {
              description += Environment.NewLine + extendedDescription;
            }
          }
          program.Descriptions.Add(language, description);

          List<EpgProgram> programs;
          if (!channels.TryGetValue(channelId, out programs))
          {
            programs = new List<EpgProgram>(100);
            channels.Add(channelId, programs);
          }
          programs.Add(program);
        }

        IDictionary<IChannel, IList<EpgProgram>> epgChannels = new Dictionary<IChannel, IList<EpgProgram>>(channels.Count);
        int validEventCount = 0;
        foreach (var channel in channels)
        {
          ChannelDvbBase dvbChannel = _currentTuningDetail.Clone() as ChannelDvbBase;
          dvbChannel.OpenTvChannelId = channel.Key;
          channel.Value.Sort();
          epgChannels.Add(dvbChannel, channel.Value);
          validEventCount += channel.Value.Count;
        }

        this.LogDebug("EPG DVB: OpenTV, channel count = {0}, event count = {1}", channels.Count, validEventCount);
        return epgChannels;
      }
      finally
      {
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferShortDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferShortDescription);
        }
        if (bufferExtendedDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferExtendedDescription);
        }
      }
    }

    private IDictionary<IChannel, IList<EpgProgram>> CollectEitData()
    {
      ushort serviceCount = _grabberDvb.GetServiceCount();
      this.LogDebug("EPG DVB: EIT, initial service count = {0}", serviceCount);
      IDictionary<IChannel, IList<EpgProgram>> channels = new Dictionary<IChannel, IList<EpgProgram>>(serviceCount);
      if (serviceCount == 0)
      {
        return channels;
      }

      const ushort BUFFER_SIZE_SERIES_ID = 300;
      IntPtr bufferSeriesId = Marshal.AllocCoTaskMem(BUFFER_SIZE_SERIES_ID);
      const ushort BUFFER_SIZE_EPISODE_ID = 300;
      IntPtr bufferEpisodeId = Marshal.AllocCoTaskMem(BUFFER_SIZE_EPISODE_ID);
      const byte ARRAY_SIZE_AUDIO_LANGUAGES = 20;
      const byte ARRAY_SIZE_SUBTITLES_LANGUAGES = 20;
      const byte ARRAY_SIZE_DVB_CONTENT_TYPE_IDS = 10;
      const byte ARRAY_SIZE_DVB_PARENTAL_RATINGS = 10;
      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_SHORT_DESCRIPTION = 1000;
      IntPtr bufferShortDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_SHORT_DESCRIPTION);
      const ushort BUFFER_SIZE_EXTENDED_DESCRIPTION = 1000;
      IntPtr bufferExtendedDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_EXTENDED_DESCRIPTION);
      try
      {
        ushort originalNetworkId;
        ushort transportStreamId;
        ushort serviceId;
        ushort eventCount;
        ulong eventId;
        ulong startDateTimeEpoch;
        ushort duration;
        RunningStatus runningStatus;
        bool freeCaMode;
        ushort referenceServiceId;
        ulong referenceEventId;
        bool isHighDefinition;
        bool isStandardDefinition;
        bool isThreeDimensional;
        bool isPreviouslyShown;
        Iso639Code[] audioLanguages = new Iso639Code[ARRAY_SIZE_AUDIO_LANGUAGES];
        Iso639Code[] subtitlesLanguages = new Iso639Code[ARRAY_SIZE_SUBTITLES_LANGUAGES];
        ushort[] dvbContentTypeIds = new ushort[ARRAY_SIZE_DVB_CONTENT_TYPE_IDS];
        Iso639Code[] dvbParentalRatingCountryCodes = new Iso639Code[ARRAY_SIZE_DVB_PARENTAL_RATINGS];
        byte[] dvbParentalRatings = new byte[ARRAY_SIZE_DVB_PARENTAL_RATINGS];
        byte starRating;
        byte mpaaClassification;
        ushort dishBevAdvisories;
        byte vchipRating;
        byte textCount;
        Iso639Code language;
        byte descriptionItemCount;
        int validEventCount = 0;
        for (ushort i = 0; i < serviceCount; i++)
        {
          if (!_grabberDvb.GetService(i, out originalNetworkId, out transportStreamId, out serviceId, out eventCount))
          {
            this.LogWarn("EPG DVB: failed to get EIT service, service index = {0}, service count = {1}", i, serviceCount);
            continue;
          }

          List<EpgProgram> programs = new List<EpgProgram>(eventCount);
          for (ushort j = 0; j < eventCount; j++)
          {
            ushort bufferSizeSeriesId = BUFFER_SIZE_SERIES_ID;
            ushort bufferSizeEpisodeId = BUFFER_SIZE_EPISODE_ID;
            byte countAudioLanguages = ARRAY_SIZE_AUDIO_LANGUAGES;
            byte countSubtitlesLanguages = ARRAY_SIZE_SUBTITLES_LANGUAGES;
            byte countDvbContentTypeIds = ARRAY_SIZE_DVB_CONTENT_TYPE_IDS;
            byte countDvbParentalRatings = ARRAY_SIZE_DVB_PARENTAL_RATINGS;
            bool result = _grabberDvb.GetEvent(i, j,
                                                out eventId,
                                                out startDateTimeEpoch,
                                                out duration,
                                                out runningStatus,
                                                out freeCaMode,
                                                out referenceServiceId,
                                                out referenceEventId,
                                                bufferSeriesId,
                                                ref bufferSizeSeriesId,
                                                bufferEpisodeId,
                                                ref bufferSizeEpisodeId,
                                                out isHighDefinition,
                                                out isStandardDefinition,
                                                out isThreeDimensional,
                                                out isPreviouslyShown,
                                                audioLanguages,
                                                ref countAudioLanguages,
                                                subtitlesLanguages,
                                                ref countSubtitlesLanguages,
                                                dvbContentTypeIds,
                                                ref countDvbContentTypeIds,
                                                dvbParentalRatingCountryCodes,
                                                dvbParentalRatings,
                                                ref countDvbParentalRatings,
                                                out starRating,
                                                out mpaaClassification,
                                                out dishBevAdvisories,
                                                out vchipRating,
                                                out textCount);
            if (!result)
            {
              this.LogWarn("EPG DVB: failed to get EIT event, service index = {0}, service count = {1}, event index = {2}, event count = {3}", i, serviceCount, j, eventCount);
              continue;
            }

            DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            programStartTime.AddSeconds(startDateTimeEpoch);
            programStartTime = programStartTime.ToLocalTime();
            EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));

            bool isPlaceholderOrDummyEvent = false;
            for (byte k = 0; k < textCount; k++)
            {
              ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
              ushort bufferSizeShortDescription = BUFFER_SIZE_SHORT_DESCRIPTION;
              ushort bufferSizeExtendedDescription = BUFFER_SIZE_EXTENDED_DESCRIPTION;
              result = _grabberDvb.GetEventText(i, j, k,
                                                out language,
                                                bufferTitle,
                                                ref bufferSizeTitle,
                                                bufferShortDescription,
                                                ref bufferSizeShortDescription,
                                                bufferExtendedDescription,
                                                ref bufferSizeExtendedDescription,
                                                out descriptionItemCount);
              if (!result)
              {
                this.LogWarn("EPG DVB: failed to get EIT event text, service index = {0}, service count = {1}, event index = {2}, event count = {3}, text index = {4}, text count = {5}",
                              i, serviceCount, j, eventCount, k, textCount);
                continue;
              }

              string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
              if (string.IsNullOrEmpty(title) || title.Equals("."))
              {
                isPlaceholderOrDummyEvent = true;
                break;
              }
              program.Titles.Add(language.Code, title);

              // When available, extended description seems to contain various event attributes.
              string description = TidyString(DvbTextConverter.Convert(bufferShortDescription, bufferSizeShortDescription));
              string extendedDescription = TidyString(DvbTextConverter.Convert(bufferExtendedDescription, bufferSizeExtendedDescription));
              if (string.IsNullOrEmpty(description) || string.Equals(title, description))
              {
                description = extendedDescription;
              }
              else if (!string.IsNullOrEmpty(extendedDescription) && !description.Contains(extendedDescription))
              {
                if (extendedDescription.Contains(description))
                {
                  description = extendedDescription;
                }
                else
                {
                  description += Environment.NewLine + extendedDescription;
                }
              }

              // Dish Network
              if (originalNetworkId >= 4097 && originalNetworkId <= 4107)
              {
                description = ParseDishDescription(description, program);
              }

              Dictionary<string, string> premiereItems = new Dictionary<string, string>(5);
              for (byte l = 0; l < descriptionItemCount; l++)
              {
                // Note: reusing buffers here.
                ushort bufferSizeDescription = BUFFER_SIZE_SHORT_DESCRIPTION;
                ushort bufferSizeText = BUFFER_SIZE_EXTENDED_DESCRIPTION;
                result = _grabberDvb.GetEventDescriptionItem(i, j, k, l, bufferShortDescription, ref bufferSizeDescription, bufferExtendedDescription, ref bufferSizeText);
                if (!result)
                {
                  this.LogWarn("EPG DVB: failed to get EIT event description item, service index = {0}, service count = {1}, event index = {2}, event count = {3}, text index = {4}, text count = {5}, item index = {6}, item count = {7}",
                                i, serviceCount, j, eventCount, k, textCount, l, descriptionItemCount);
                  continue;
                }

                string itemDescription = TidyString(DvbTextConverter.Convert(bufferShortDescription, bufferSizeDescription));
                string itemText = TidyString(DvbTextConverter.Convert(bufferExtendedDescription, bufferSizeText));
                if (itemDescription.StartsWith("Premiere order "))
                {
                  premiereItems.Add(itemDescription, itemText);
                }
                else
                {
                  HandleDescriptionItem(itemDescription, itemText, program, ref description);
                }
              }

              // Handle Premiere order items separately for display order consistency.
              if (premiereItems.Count > 0)
              {
                if (!string.IsNullOrEmpty(description))
                {
                  description += Environment.NewLine;
                }
                description += string.Format("Bestellnummer: {0}", premiereItems["Premiere order number"]);
                description += string.Format("{0}Preis: {1}", Environment.NewLine, premiereItems["Premiere order price"]);
                description += string.Format("{0}Telefonnummer: {1}", Environment.NewLine, premiereItems["Premiere order phone number"]);
                description += string.Format("{0}SMS: {1}", Environment.NewLine, premiereItems["Premiere order SMS number"]);
                description += string.Format("{0}URL: {1}", Environment.NewLine, premiereItems["Premiere order URL"]);
              }

              program.Descriptions.Add(language.Code, description);
            }

            if (isPlaceholderOrDummyEvent)
            {
              continue;
            }

            if (isHighDefinition)
            {
              program.IsHighDefinition = true;
            }
            else if (isStandardDefinition)
            {
              program.IsHighDefinition = false;
            }
            if (isThreeDimensional)
            {
              program.IsThreeDimensional = true;
            }
            if (isPreviouslyShown)
            {
              program.IsPreviouslyShown = true;
            }
            if (bufferSizeSeriesId > 0)
            {
              program.SeriesId = DvbTextConverter.Convert(bufferSeriesId, bufferSizeSeriesId);
            }
            if (bufferSizeEpisodeId > 0)
            {
              program.EpisodeId = DvbTextConverter.Convert(bufferEpisodeId, bufferSizeEpisodeId);
            }
            for (byte x = 0; x < countAudioLanguages; x++)
            {
              program.AudioLanguages.Add(audioLanguages[x].Code);
            }
            for (byte x = 0; x < countSubtitlesLanguages; x++)
            {
              program.SubtitlesLanguages.Add(subtitlesLanguages[x].Code);
            }
            for (byte x = 0; x < countDvbContentTypeIds; x++)
            {
              bool? tempIsLive;
              bool? tempIsThreeDimensional;
              program.Categories.Add(GetContentTypeDescription(dvbContentTypeIds[x], originalNetworkId, out tempIsLive, out tempIsThreeDimensional));
              if (tempIsLive.HasValue)
              {
                program.IsLive = tempIsLive;
              }
              if (tempIsThreeDimensional.HasValue)
              {
                program.IsThreeDimensional |= tempIsThreeDimensional;
              }
            }
            for (byte x = 0; x < countDvbParentalRatings; x++)
            {
              string parentalRating = GetParentalRatingDescription(dvbParentalRatings[x]);
              if (parentalRating != null)
              {
                program.Classifications.Add(dvbParentalRatingCountryCodes[x].Code, parentalRating);
              }
            }
            if (starRating > 0 && starRating < 8)
            {
              program.StarRating = (starRating + 1) / 2;    // 1 = 1 star, 2 = 1.5 stars etc.; max. value = 7
              program.StarRatingMaximum = 4;
            }
            string mpaaClassificationDescription = GetMpaaClassificationDescription(mpaaClassification);
            if (mpaaClassificationDescription != null)
            {
              program.Classifications.Add("MPAA", mpaaClassificationDescription);
            }
            program.Advisories = GetContentAdvisories(dishBevAdvisories);
            string vchipRatingDescription = GetVchipRatingDescription(vchipRating);
            if (vchipRatingDescription != null)
            {
              program.Classifications.Add("V-Chip", vchipRatingDescription);
            }

            programs.Add(program);
          }

          ChannelDvbBase dvbChannel = _currentTuningDetail.Clone() as ChannelDvbBase;
          dvbChannel.OriginalNetworkId = originalNetworkId;
          dvbChannel.TransportStreamId = transportStreamId;
          dvbChannel.ServiceId = serviceId;
          dvbChannel.OpenTvChannelId = 0;
          programs.Sort();
          channels.Add(dvbChannel, programs);
          validEventCount += programs.Count;
        }

        this.LogDebug("EPG DVB: EIT, channel count = {0}, event count = {1}", channels.Count, validEventCount);
        return channels;
      }
      finally
      {
        if (bufferSeriesId != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferSeriesId);
        }
        if (bufferEpisodeId != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferEpisodeId);
        }
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferShortDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferShortDescription);
        }
        if (bufferExtendedDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferExtendedDescription);
        }
      }
    }

    private static string TidyString(string s)
    {
      if (s == null)
      {
        return string.Empty;
      }
      return s.Trim();
    }

    private static void HandleDescriptionItem(string itemName, string itemText, EpgProgram program, ref string description)
    {
      // Fake items added by TsWriter.
      if (string.Equals(itemName, "Dish episode info"))
      {
        // Example: {0}{1|Don't Worry, Speed Racer}{31|Charlie Sheen}{32|Jon Cryer}{33|Angus T. Jones}{5|When Jake tells Charlie and Alan that he often overhears Judith and Herb (Ryan Stiles) making out, it triggers a repressed memory for Charlie.}{6|CC}{7|Stereo}
        foreach (string section in itemText.Split(new char[] { '}', '{' }, StringSplitOptions.RemoveEmptyEntries))
        {
          string[] parts = section.Split('|');
          int sectionType;
          if (int.TryParse(parts[0], out sectionType))
          {
            if (sectionType == 0)
            {
              // unknown meaning
            }
            else if (parts.Length > 1)
            {
              if (sectionType == 1)
              {
                program.EpisodeName = parts[1];
              }
              else if (sectionType >= 31 && sectionType <= 49)  // Note: only seen 31, 32 and 33. Assuming 34-49 are the same.
              {
                if (!program.Actors.Contains(parts[1]))
                {
                  program.Actors.Add(parts[1]);
                }
              }
              else if (sectionType == 5)
              {
                string shortDescription = parts[1];
                if (shortDescription.EndsWith(" (HD)"))
                {
                  program.IsHighDefinition = true;
                  shortDescription.Substring(0, shortDescription.Length - " (HD)".Length);
                }
                if (shortDescription.EndsWith(" New."))
                {
                  program.IsPreviouslyShown = false;
                  shortDescription.Substring(0, shortDescription.Length - " New.".Length);
                }
                if (!description.Contains(shortDescription))
                {
                  description = shortDescription + Environment.NewLine + description;
                }
              }
              else if (sectionType == 6)
              {
                if (parts[1].Equals("CC") && !program.SubtitlesLanguages.Contains("eng"))
                {
                  program.SubtitlesLanguages.Add("eng");
                }
              }
              else if (sectionType == 7)
              {
                // nothing valuable
              }
              else
              {
                Log.Warn("EPG DVB: failed to interpret Dish Network episode detail, section type = {0}, section content = {1}", sectionType, section);
              }
            }
            else
            {
              Log.Warn("EPG DVB: failed to interpret Dish Network episode detail, section type = {0}", sectionType);
            }
          }
          else
          {
            Log.Warn("EPG DVB: failed to interpret Dish Network episode detail, section = {0}", section);
          }
        }
        return;
      }

      if (
        string.Equals(itemName, "actors", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "int", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        foreach (string actor in itemText.Split(','))
        {
          string actorTemp = actor.Trim();
          if (!string.IsNullOrEmpty(actorTemp) && !program.Actors.Contains(actorTemp))
          {
            program.Actors.Add(actorTemp);
          }
        }
        return;
      }

      if (
        itemName.StartsWith("director", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "dir", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        foreach (string director in itemText.Split(','))
        {
          string directorTemp = director.Trim();
          if (!string.IsNullOrEmpty(directorTemp) && !program.Directors.Contains(directorTemp))
          {
            program.Directors.Add(directorTemp);
          }
        }
        return;
      }

      if (string.Equals(itemName, "gui", StringComparison.InvariantCultureIgnoreCase))
      {
        foreach (string writer in itemText.Split(','))
        {
          string writerTemp = writer.Trim();
          if (!string.IsNullOrEmpty(writerTemp) && !program.Writers.Contains(writerTemp))
          {
            program.Writers.Add(writerTemp);
          }
        }
        return;
      }

      if (
        string.Equals(itemName, "production year", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "year", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "año", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        ushort year;
        if (ushort.TryParse(itemText, out year))
        {
          program.ProductionYear = year;
        }
        else
        {
          Log.Warn("EPG DVB: failed to interpret production year description item, item name = {0}, item text = {1}", itemName, itemText);
        }
        return;
      }

      if (
        string.Equals(itemName, "country", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "nac", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        program.ProductionCountry = itemText;
        return;
      }

      if (string.Equals(itemName, "episode", StringComparison.InvariantCultureIgnoreCase))
      {
        program.EpisodeName = itemText;
        return;
      }

      if (string.Equals(itemName, "tep", StringComparison.InvariantCultureIgnoreCase))
      {
        string[] parts = itemText.Split(':');
        if (parts.Length == 2)
        {
          int seasonNumber;
          int episodeNumber;
          if (int.TryParse(parts[0], out seasonNumber) && int.TryParse(parts[1], out episodeNumber))
          {
            program.SeasonNumber = seasonNumber;
            program.EpisodeNumber = episodeNumber;
            return;
          }
        }
        Log.Warn("EPG DVB: failed to interpret season/episode number description item, item text = {0}", itemText);
        return;
      }

      if (string.Equals(itemName, "seriesid", StringComparison.InvariantCultureIgnoreCase))
      {
        program.SeriesId = itemText;
        return;
      }

      if (string.Equals(itemName, "seasonid", StringComparison.InvariantCultureIgnoreCase))
      {
        if (string.IsNullOrEmpty(program.SeriesId))   // Prefer series ID over season ID.
        {
          program.SeriesId = itemText;
        }
        return;
      }

      if (string.Equals(itemName, "episodeid", StringComparison.InvariantCultureIgnoreCase))
      {
        program.EpisodeId = itemText;
        return;
      }

      Log.Warn("EPG DVB: failed to interpret description item, item name = {0}, item text = {1}", itemName, itemText);
    }

    private static string GetContentTypeDescription(ushort contentTypeId, ushort originalNetworkId, out bool? isLive, out bool? isThreeDimensional)
    {
      isLive = null;
      isThreeDimensional = null;
      byte level1Id = (byte)(contentTypeId >> 12);
      if (level1Id == 0xf)  // user defined
      {
        // Echostar Communications (Dish, Bell ExpressVu) - refer to http://www.dvbservices.com/identifiers/original_network_id&tab=table
        if (
          (originalNetworkId >= 0x1001 && originalNetworkId <= 0x100b) ||
          (originalNetworkId >= 0x1700 && originalNetworkId <= 0x1713)
        )
        {
          return GetDishBevContentTypeDescription(contentTypeId);
        }
      }
      else if (contentTypeId >> 8 == 0)
      {
        // Virgin Media, UK DVB-C
        if (originalNetworkId == 0xf020)
        {
          return GetVirginMediaContentTypeDescription(contentTypeId);
        }
      }

      byte level2Id = (byte)((contentTypeId >> 8) & 0x0f);

      // special characteristics
      if (level1Id == 0xb)
      {
        if (level2Id == 3)
        {
          isLive = true;
        }
        else if (level2Id == 4)
        {
          isThreeDimensional = true;
        }
        return null;
      }

      string level1Text;
      if (!MAPPING_CONTENT_TYPES_DVB.TryGetValue(level1Id, out level1Text))
      {
        level1Text = string.Format("DVB Reserved {0}", level1Id);
      }

      string level2Text;
      if (!MAPPING_CONTENT_SUB_TYPES_DVB.TryGetValue((byte)((level1Id << 4) | level2Id), out level2Text))
      {
        if (level2Id == 0 && level1Id < 11)
        {
          level2Text = "General";
        }
        else if (level2Id == 0xf)
        {
          level2Text = "DVB User Defined";
        }
        else
        {
          level2Text = string.Format("DVB Reserved {0}", level2Id);
        }
      }

      return string.Format("{0}: {1}", level1Text, level2Text);
    }

    private static string GetDishBevContentTypeDescription(ushort contentTypeId)
    {
      byte level1Id = (byte)((contentTypeId >> 8) & 0x0f);
      byte level2Id = (byte)(contentTypeId & 0xff);

      string level1Text;
      if (!MAPPING_CONTENT_TYPES_DISH.TryGetValue(level1Id, out level1Text))
      {
        level1Text = null;
      }

      string level2Text;
      if (!MAPPING_CONTENT_SUB_TYPES_DISH.TryGetValue(level2Id, out level2Text))
      {
        level2Text = null;
      }

      if (level1Text == null && level2Text == null)
      {
        return string.Format("Dish/BEV Content Type {0}-{1}", level1Id, level2Id);
      }
      else if (level1Text != null && level2Text != null)
      {
        return string.Format("{0}: {1}", level1Text, level2Text);
      }
      return level1Text ?? level2Text;
    }

    private static string GetVirginMediaContentTypeDescription(ushort contentTypeId)
    {
      byte level1Id = (byte)((contentTypeId >> 4) & 0xf);
      byte level2Id = (byte)(contentTypeId & 0xf);

      string level1Text;
      if (!MAPPING_CONTENT_TYPES_VIRGIN_MEDIA.TryGetValue(level1Id, out level1Text))
      {
        return string.Format("Virgin Media Content Type {0}-{1}", level1Id, level2Id);
      }

      string level2Text;
      if (!MAPPING_CONTENT_SUB_TYPES_VIRGIN_MEDIA.TryGetValue((byte)((level1Id << 4) | level2Id), out level2Text))
      {
        if (level2Id != 0 && level2Id != 0xf)
        {
          return level1Text;
        }
        level2Text = "General";
      }

      return string.Format("{0}: {1}", level1Text, level2Text);
    }

    private static string GetOpenTvProgramCategoryDescription(byte categoryId, byte subCategoryId)
    {
      IDictionary<byte, string> categoryNames = null;
      IDictionary<ushort, string> subCategoryNames = null;
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      if (countryName != null)
      {
        if (countryName.Equals("Australia"))
        {
          categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_AU;
          subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_AU;
        }
        else if (countryName.Equals("Italy"))
        {
          categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_IT;
          subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_IT;
        }
        else if (countryName.Equals("New Zealand"))
        {
          // Based on DVB content types, with some overrides and extensions.
          categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_NZ;
          subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_NZ;
        }
        else if (countryName.Equals("United Kindom"))
        {
          categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_UK;
          subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_UK;
        }
      }

      string categoryName;
      if (
        categoryNames == null ||
        (
          !categoryNames.TryGetValue(categoryId, out categoryName) &&
          (
            !countryName.Equals("New Zealand") ||
            !MAPPING_CONTENT_TYPES_DVB.TryGetValue(categoryId, out categoryName)
          )
        )
      )
      {
        return string.Format("OpenTV Program Category {0}-{1}", categoryId, subCategoryId);
      }

      string subCategoryName;
      if (!subCategoryNames.TryGetValue((ushort)((categoryId << 8) | subCategoryId), out subCategoryName))
      {
        if (!countryName.Equals("New Zealand"))
        {
          return categoryName;
        }

        if (!MAPPING_CONTENT_SUB_TYPES_DVB.TryGetValue((byte)((categoryId << 4) | subCategoryId), out subCategoryName))
        {
          if (subCategoryId != 0 && subCategoryId != 0xf)
          {
            return categoryName;
          }
          subCategoryName = "General";
        }
      }

      return string.Format("{0}: {1}", categoryName, subCategoryName);
    }

    private static string GetOpenTvParentalRatingDescription(byte rating)
    {
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      if (countryName != null)
      {
        if (countryName.Equals("Australia"))
        {
          switch (rating)
          {
            case 0:
              return "NC";      // not classified (exempt)
            case 2:
              return "P";       // suitable for pre-school children
            case 4:
              return "C";       // suitable for children
            case 6:
              return "G";
            case 8:
              return "PG";
            case 10:
              return "M";
            case 12:
              return "MA15+";   // formerly MA
            case 14:
              return "AV15+";   // formerly AV (suitable for adult viewers only - violent content); may no longer be used, or merged with MA15+... or may be R18+ if 15 is X18+
            case 15:
              return "R18+";    // formerly R; may be X18+
          }
        }
        else if (countryName.Equals("Italy"))
        {
          switch (rating)
          {
            case 1:
              return "PT";      // per tutti ("for all", green icon)
            case 2:
              return "BA";      // bambini accompagnati ("accompanied children", yellow icon)
            case 3:
              return "VM 12";   // vietato ai minori di 12 anni ("no one under 12 years", orange icon)
            case 4:
              return "VM 14";   // vietato ai minori di 14 anni ("no one under 14 years", pink icon)
            case 5:
              return "VM 16";   // guess; not actually seen
            case 6:
              return "VM 18";   // vietato ai minori di 18 anni ("no one under 18 years", red icon)
          }
        }
        else if (countryName.Equals("New Zealand"))
        {
          switch (rating)
          {
            case 0:
              return "NC";      // not classified (exempt)
            case 1:
            case 2:
              return "G";
            case 4:
              return "PG";
            case 6:
              return "M";
            case 8:
              return "R16";
            case 10:
            case 12:
            case 13:
            case 14:
              return "R18";
          }
        }
        else if (countryName.Equals("United Kindom"))
        {
          switch (rating)
          {
            case 1:
              return "U";       // universal
            case 2:
              return "PG";
            case 3:
              return "12";
            case 4:
              return "15";
            case 5:
              return "18";
          }
        }
      }
      return string.Empty;
    }

    private static string GetParentalRatingDescription(byte rating)
    {
      if (rating == 0 || rating > 0x0f)
      {
        // Undefined or broadcaster-specific.
        return null;
      }
      return string.Format("Min. age {0}", rating + 3);
    }

    private static string GetMpaaClassificationDescription(byte classification)
    {
      // Note: there is some uncertainty about values 5, 6 and 7. Our old
      // code differs with MythTV, and this code differs from the ATSC RRT.
      switch (classification)
      {
        case 1:
          return "G";       // general
        case 2:
          return "PG";      // parental guidance
        case 3:
          return "PG-13";   // parental guidance under 13
        case 4:
          return "R";       // restricted
        case 5:
          return "NC-17";   // nobody 17 and under
        case 6:
          return "NR";      // not rated
      }
      return null;
    }

    private static ContentAdvisory GetContentAdvisories(ushort advisories)
    {
      ContentAdvisory advisoryFlags = ContentAdvisory.None;
      if ((advisories & 0x01) != 0)
      {
        advisoryFlags |= ContentAdvisory.SexualSituations;
      }
      if ((advisories & 0x02) != 0)
      {
        advisoryFlags |= ContentAdvisory.CourseOrCrudeLanguage;
      }
      if ((advisories & 0x04) != 0)
      {
        advisoryFlags |= ContentAdvisory.MildSensuality;
      }
      if ((advisories & 0x08) != 0)
      {
        advisoryFlags |= ContentAdvisory.FantasyViolence;
      }
      if ((advisories & 0x10) != 0)
      {
        advisoryFlags |= ContentAdvisory.Violence;
      }
      if ((advisories & 0x20) != 0)
      {
        advisoryFlags |= ContentAdvisory.MildPeril;
      }
      if ((advisories & 0x40) != 0)
      {
        advisoryFlags |= ContentAdvisory.Nudity;
      }
      if ((advisories & 0x8000) != 0)
      {
        advisoryFlags |= ContentAdvisory.SuggestiveDialogue;
      }
      return advisoryFlags;
    }

    private static string GetVchipRatingDescription(byte rating)
    {
      switch (rating)
      {
        case 1:
          return "TV-Y";    // all children
        case 2:
          return "TV-Y7";   // children 7 and older
        case 3:
          return "TV-G";    // general audience
        case 4:
          return "TV-PG";   // parental guidance
        case 5:
          return "TV-14";   // adults 14 and older
        case 6:
          return "TV-MA";   // mature audience
      }
      return null;
    }

    private static string ParseDishDescription(string description, EpgProgram program)
    {
      // (<episode name>\r)? <category name>. (<CSV actors>.  \(<year>\))? <description>
      // <description> may end with zero or more of:
      // - New.
      // - Season Premiere.
      // - Series Premiere.
      // - Premiere.
      // - Season Finale.
      // - Series Finale.
      // - Finale.
      // - (HD)
      // - (DD)
      // - (SAP)
      // - (<PPV ID>) eg. (CE79D)
      // - (CC)
      // - (Stereo)
      int index = description.IndexOf((char)0xd);
      if (index > 0)
      {
        program.EpisodeName = description.Substring(0, index);
        description = description.Substring(index);
      }
      description = description.Trim();

      foreach (string contentType in MAPPING_CONTENT_TYPES_DISH.Values)
      {
        if (description.StartsWith(contentType + ". "))
        {
          description = description.Substring(contentType.Length + 2);
          break;
        }
      }

      Match m = Regex.Match(description, @"(  \(([12][0-9]{3})\) )");
      if (m.Success)
      {
        program.ProductionYear = ushort.Parse(m.Groups[2].Captures[0].Value);
        index = description.IndexOf(m.Groups[1].Captures[0].Value);
        if (index > 0)
        {
          string people = description.Substring(0, index - 1);
          foreach (string person in people.Split(new string[] { ", " }, StringSplitOptions.None))
          {
            if (person.StartsWith("Voice of: "))
            {
              program.Actors.Add(person.Substring("Voice of: ".Length));
              continue;
            }
            program.Actors.Add(person);
          }
        }
        description = description.Substring(index + 8);
      }

      index = description.IndexOf(" (HD)");
      if (index >= 0)
      {
        program.IsHighDefinition = true;
        description = description.Remove(index, " (HD)".Length);
      }

      index = description.IndexOf(" (CC)");
      if (index >= 0)
      {
        // assumption: language is English
        if (!program.SubtitlesLanguages.Contains("eng"))
        {
          program.SubtitlesLanguages.Add("eng");
        }
        description = description.Remove(index, " (CC)".Length);
      }

      index = description.IndexOf(" New.");
      if (index >= 0)
      {
        program.IsPreviouslyShown = false;
        description = description.Remove(index, " New.".Length);
      }

      if (description.Contains(" Series Premiere."))
      {
        program.SeasonNumber = 1;
        program.EpisodeNumber = 1;
      }
      else if (description.Contains(" Season Premiere."))
      {
        program.EpisodeNumber = 1;
      }

      return description;
    }

    #region ICallBackGrabber members

    /// <summary>
    /// This function is invoked when the first section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was received.</param>
    public void OnTableSeen(ushort pid, byte tableId)
    {
      if (pid == 0x12)
      {
        _isSeenDvb = true;
        _isCompleteDvb = false;
      }
      else if (pid == 0xd2)
      {
        _isSeenMhw = true;
        _isCompleteMhw = false;
      }
      if (pid == 0x30)
      {
        _isSeenOpenTv = true;
        _isCompleteOpenTv = false;
      }
    }

    /// <summary>
    /// This function is invoked after the last section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was completed.</param>
    public void OnTableComplete(ushort pid, byte tableId)
    {
      if (pid == 0x12)
      {
        _isCompleteDvb = true;
      }
      else if (pid == 0xd2)
      {
        _isCompleteMhw = true;
      }
      if (pid == 0x30)
      {
        _isCompleteOpenTv = true;
      }

      if (
        (!_isSeenDvb || _isCompleteDvb) &&
        (!_isSeenMhw || _isCompleteMhw) &&
        (!_isSeenOpenTv || _isCompleteOpenTv)
      )
      {
        this.LogDebug("EPG DVB: EPG complete");
        // Use a thread to collect the data. If we collect from this thread it
        // can cause stuttering and deadlocks.
        Thread collector = new Thread(CollectEpgData);
        collector.IsBackground = true;
        collector.Priority = ThreadPriority.Lowest;
        collector.Name = "EPG collector";
        collector.Start();
      }
    }

    /// <summary>
    /// This function is invoked after any section from a table changes.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that changed.</param>
    public void OnTableChange(ushort pid, byte tableId)
    {
      OnTableSeen(pid, tableId);
    }

    #endregion

    #region IEpgGrabber members

    /// <summary>
    /// Reload the controller's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("EPG DVB: reload configuration");

      bool defaultGrabBellExpressVu = false;
      bool defaultGrabDishNetwork = false;
      bool defaultGrabFreesat = false;
      bool defaultGrabMhw1 = false;
      bool defaultGrabMhw2 = false;
      bool defaultGrabMultiChoice = false;
      bool defaultGrabOpenTv = false;
      bool defaultGrabPremiere = false;
      bool defaultGrabViasatSweden = false;
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      if (countryName != null)
      {
        if (countryName.Equals("Australia"))
        {
          defaultGrabOpenTv = true;   // Foxtel
        }
        else if (countryName.Equals("Canada"))
        {
          defaultGrabBellExpressVu = true;
        }
        else if (countryName.Equals("France"))
        {
          defaultGrabMhw1 = true;   // Canalsat
        }
        else if (countryName.Equals("Germany"))
        {
          defaultGrabPremiere = true;
        }
        else if (countryName.Equals("Italy"))
        {
          defaultGrabOpenTv = true;   // Sky
        }
        else if (countryName.Equals("Netherlands, The"))
        {
          defaultGrabMhw1 = true;   // Canal Digitaal
        }
        else if (countryName.Equals("New Zealand"))
        {
          defaultGrabOpenTv = true;   // Sky
        }
        else if (countryName.Equals("Poland"))
        {
          defaultGrabMhw1 = true;   // Cyfra+
        }
        else if (countryName.Equals("Spain"))
        {
          defaultGrabMhw2 = true;   // Canal+/Digital+
        }
        else if (countryName.Equals("South Africa"))
        {
          defaultGrabMultiChoice = true;
        }
        else if (countryName.Equals("Sweden"))
        {
          defaultGrabViasatSweden = true;
        }
        else if (countryName.Equals("United Kingdom"))
        {
          defaultGrabFreesat = true;
          defaultGrabOpenTv = true;   // Sky
        }
        else if (countryName.Equals("United States"))
        {
          defaultGrabDishNetwork = true;
        }
      }

      bool grabBellExpressVu = SettingsManagement.GetValue("grabEpgBellExpressVu", defaultGrabBellExpressVu);
      bool grabDishNetwork = SettingsManagement.GetValue("grabEpgDishNetwork", defaultGrabDishNetwork);
      bool grabDvb = SettingsManagement.GetValue("grabEpgDvb", true);
      bool grabFreesat = SettingsManagement.GetValue("grabEpgFreesat", defaultGrabFreesat);
      bool grabMhw1 = SettingsManagement.GetValue("grabEpgMhw1", defaultGrabMhw1);
      bool grabMhw2 = SettingsManagement.GetValue("grabEpgMhw2", defaultGrabMhw2);
      bool grabMultiChoice = SettingsManagement.GetValue("grabEpgMultiChoice", defaultGrabMultiChoice);
      bool grabOpenTv = SettingsManagement.GetValue("grabEpgOpenTv", defaultGrabOpenTv);
      bool grabPremiere = SettingsManagement.GetValue("grabEpgPremiere", defaultGrabPremiere);
      bool grabViasatSweden = SettingsManagement.GetValue("grabEpgViasatSweden", defaultGrabViasatSweden);
      // TODO
    }

    /// <summary>
    /// Start grabbing electronic programme guide data.
    /// </summary>
    /// <param name="tuningDetail">The current transmitter tuning details.</param>
    /// <param name="callBack">The delegate to notify when grabbing is complete or canceled.</param>
    public void GrabEpg(IChannel tuningDetail, IEpgGrabberCallBack callBack)
    {
      this.LogDebug("EPG DVB: grab EPG");
      _currentTuningDetail = tuningDetail;
      _callBack = callBack;
      /*_grabber.Reset();
      _grabber.SetCallBack(this);
      _grabber.GrabEPG();
      _grabber.GrabMhw();*/
      _isGrabbing = true;
    }

    /// <summary>
    /// Get the grabber's current status.
    /// </summary>
    /// <value><c>true</c> if the grabber is grabbing, otherwise <c>false</c></value>
    public bool IsGrabbing
    {
      get
      {
        return _isGrabbing;
      }
    }

    /// <summary>
    /// Abort grabbing electronic programme guide data.
    /// </summary>
    public void AbortGrabbing()
    {
      this.LogDebug("EPG DVB: abort grabbing");
      //_grabber.AbortGrabbing();
      if (_callBack != null)
      {
        _callBack.OnEpgCancelled();
      }
      _isGrabbing = false;
    }

    #endregion
  }
}