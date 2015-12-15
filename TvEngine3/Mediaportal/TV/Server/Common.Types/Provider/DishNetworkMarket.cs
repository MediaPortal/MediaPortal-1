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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mediaportal.TV.Server.Common.Types.Provider
{
  public sealed class DishNetworkMarket
  {
    private readonly int _id;
    private readonly string _stateAbbreviation;
    private readonly string _state;
    private readonly IList<string> _cities;

    private static readonly IDictionary<int, IDictionary<string, DishNetworkMarket>> _values = new Dictionary<int, IDictionary<string, DishNetworkMarket>>(300);
    private static readonly IList<DishNetworkMarket> _valueList = new List<DishNetworkMarket>(300);

    #region values

    public static readonly DishNetworkMarket Abilene = new DishNetworkMarket(3841, "TX", "Texas", "Abilene");
    public static readonly DishNetworkMarket AlbanyNewYork = new DishNetworkMarket(3842, "NY", "New York", "Albany");
    public static readonly DishNetworkMarket AlbanyGeorgia = new DishNetworkMarket(3843, "GA", "Georgia", "Albany");
    public static readonly DishNetworkMarket AlbuquerqueSantaFe = new DishNetworkMarket(3844, "NM", "New Mexico", new List<string> { "Albuquerque", "Santa Fe" });
    public static readonly DishNetworkMarket Alexandria = new DishNetworkMarket(3845, "LA", "Louisiana", "Alexandria");
    public static readonly DishNetworkMarket Alpena = new DishNetworkMarket(3846, "MI", "Michigan", "Alpena");
    public static readonly DishNetworkMarket Amarillo = new DishNetworkMarket(3847, "TX", "Texas", "Amarillo");
    public static readonly DishNetworkMarket Anchorage = new DishNetworkMarket(3848, "AK", "Alaska", "Anchorage");
    public static readonly DishNetworkMarket Atlanta = new DishNetworkMarket(3849, "GA", "Georgia", "Atlanta");
    public static readonly DishNetworkMarket Augusta = new DishNetworkMarket(3850, "GA", "Georgia", "Augusta");
    public static readonly DishNetworkMarket Austin = new DishNetworkMarket(3851, "TX", "Texas", "Austin");
    public static readonly DishNetworkMarket Bakersfield = new DishNetworkMarket(3852, "CA", "California", "Bakersfield");
    public static readonly DishNetworkMarket Baltimore = new DishNetworkMarket(3853, "MD", "Maryland", "Baltimore");
    public static readonly DishNetworkMarket Bangor = new DishNetworkMarket(3854, "ME", "Maine", "Bangor");
    public static readonly DishNetworkMarket BatonRouge = new DishNetworkMarket(3855, "LA", "Louisiana", "Baton Rouge");
    public static readonly DishNetworkMarket BeaumontPortArthur = new DishNetworkMarket(3856, "TX", "Texas", "Beaumont-Port Arthur");
    public static readonly DishNetworkMarket Bend = new DishNetworkMarket(3857, "OR", "Oregon", "Bend");
    public static readonly DishNetworkMarket Billings = new DishNetworkMarket(3858, "MT", "Montana", "Billings");
    public static readonly DishNetworkMarket BiloxiGulfport = new DishNetworkMarket(3859, "MS", "Mississippi", new List<string> { "Biloxi", "Gulfport" });
    public static readonly DishNetworkMarket Binghamton = new DishNetworkMarket(3860, "NY", "New York", "Binghamton");
    public static readonly DishNetworkMarket Birmingham = new DishNetworkMarket(3861, "AL", "Alabama", "Birmingham");
    public static readonly DishNetworkMarket BluefieldOakHill = new DishNetworkMarket(3862, "WV", "West Virginia", new List<string> { "Bluefield", "Oak Hill" });
    public static readonly DishNetworkMarket Boise = new DishNetworkMarket(3863, "ID", "Idaho", "Boise");
    public static readonly DishNetworkMarket Boston = new DishNetworkMarket(3864, "MA", "Massachusetts", "Boston");
    public static readonly DishNetworkMarket BowlingGreen = new DishNetworkMarket(3865, "KY", "Kentucky", "Bowling Green");
    public static readonly DishNetworkMarket Buffalo = new DishNetworkMarket(3866, "NY", "New York", "Buffalo");
    public static readonly DishNetworkMarket Burlington = new DishNetworkMarket(3867, "VT", "Vermont", "Burlington");
    public static readonly DishNetworkMarket Butte = new DishNetworkMarket(3868, "MT", "Montana", "Butte");
    public static readonly DishNetworkMarket Casper = new DishNetworkMarket(3869, "WY", "Wyoming", "Casper");
    public static readonly DishNetworkMarket CedarRapids = new DishNetworkMarket(3870, "IA", "Iowa", "Cedar Rapids");
    public static readonly DishNetworkMarket ChampaignSpringfield = new DishNetworkMarket(3871, "IL", "Illinois", new List<string> { "Champaign", "Springfield" });
    public static readonly DishNetworkMarket CharlestonWestVirginia = new DishNetworkMarket(3872, "WV", "West Virginia", "Charleston");
    public static readonly DishNetworkMarket CharlestonSouthCarolina = new DishNetworkMarket(3873, "SC", "South Carolina", "Charleston");
    public static readonly DishNetworkMarket Charlotte = new DishNetworkMarket(3874, "NC", "North Carolina", "Charlotte");
    public static readonly DishNetworkMarket Charlottesville = new DishNetworkMarket(3875, "VA", "Virginia", "Charlottesville");
    public static readonly DishNetworkMarket Chattanooga = new DishNetworkMarket(3876, "TN", "Tennessee", "Chattanooga");
    public static readonly DishNetworkMarket Cheyenne = new DishNetworkMarket(3877, "WY", "Wyoming", "Cheyenne");
    public static readonly DishNetworkMarket Chicago = new DishNetworkMarket(3878, "IL", "Illinois", "Chicago");
    public static readonly DishNetworkMarket ChicoRedding = new DishNetworkMarket(3879, "CA", "California", new List<string> { "Chico", "Redding" });
    public static readonly DishNetworkMarket Cincinnati = new DishNetworkMarket(3880, "OH", "Ohio", "Cincinnati");
    public static readonly DishNetworkMarket Clarksburg = new DishNetworkMarket(3881, "WV", "West Virginia", "Clarksburg");
    public static readonly DishNetworkMarket Cleveland = new DishNetworkMarket(3882, "OH", "Ohio", "Cleveland");
    public static readonly DishNetworkMarket ColoradoSprings = new DishNetworkMarket(3883, "CO", "Colorado", "Colorado Springs");
    public static readonly DishNetworkMarket Columbia = new DishNetworkMarket(3884, "SC", "South Carolina", "Columbia");
    public static readonly DishNetworkMarket ColumbiaJeffersonCity = new DishNetworkMarket(3885, "MO", "Missouri", new List<string> { "Columbia", "Jefferson City" });
    public static readonly DishNetworkMarket ColumbusGeorgia = new DishNetworkMarket(3886, "GA", "Georgia", "Columbus");
    public static readonly DishNetworkMarket ColumbusTupelo = new DishNetworkMarket(3887, "MS", "Mississippi", new List<string> { "Columbus", "Tupelo" });
    public static readonly DishNetworkMarket ColumbusOhio = new DishNetworkMarket(3888, "OH", "Ohio", "Columbus");
    public static readonly DishNetworkMarket CorpusChristi = new DishNetworkMarket(3889, "TX", "Texas", "Corpus Christi");
    public static readonly DishNetworkMarket Dallas = new DishNetworkMarket(3890, "TX", "Texas", "Dallas");
    // Davenport/Rock Island
    public static readonly DishNetworkMarket Dayton = new DishNetworkMarket(3892, "OH", "Ohio", "Dayton");
    public static readonly DishNetworkMarket Denver = new DishNetworkMarket(3893, "CO", "Colorado", "Denver");
    public static readonly DishNetworkMarket DesMoines = new DishNetworkMarket(3894, "IA", "Iowa", "Des Moines");
    public static readonly DishNetworkMarket Detroit = new DishNetworkMarket(3895, "MI", "Michigan", "Detroit");
    public static readonly DishNetworkMarket Dothan = new DishNetworkMarket(3896, "AL", "Alabama", "Dothan");
    public static readonly DishNetworkMarket Duluth = new DishNetworkMarket(3897, "MN", "Minnesota", "Duluth");
    public static readonly DishNetworkMarket ElPaso = new DishNetworkMarket(3898, "TX", "Texas", "El Paso");
    public static readonly DishNetworkMarket Elmira = new DishNetworkMarket(3899, "NY", "New York", "Elmira");
    public static readonly DishNetworkMarket Erie = new DishNetworkMarket(3900, "PA", "Pennsylvania", "Erie");
    public static readonly DishNetworkMarket Eugene = new DishNetworkMarket(3901, "OR", "Oregon", "Eugene");
    public static readonly DishNetworkMarket Eureka = new DishNetworkMarket(3902, "CA", "California", "Eureka");
    public static readonly DishNetworkMarket Evansville = new DishNetworkMarket(3903, "IN", "Indiana", "Evansville");
    public static readonly DishNetworkMarket Fairbanks = new DishNetworkMarket(3904, "AK", "Alaska", "Fairbanks");
    public static readonly DishNetworkMarket Fargo = new DishNetworkMarket(3905, "ND", "North Dakota", "Fargo");
    public static readonly DishNetworkMarket Flint = new DishNetworkMarket(3906, "MI", "Michigan", "Flint");
    public static readonly DishNetworkMarket Florence = new DishNetworkMarket(3907, "SC", "South Carolina", "Florence");
    public static readonly DishNetworkMarket Fresno = new DishNetworkMarket(3908, "CA", "California", "Fresno");
    public static readonly DishNetworkMarket FortMyers = new DishNetworkMarket(3909, "FL", "Florida", "Fort Myers");
    public static readonly DishNetworkMarket FortSmith = new DishNetworkMarket(3910, "AR", "Arkansas", "Fort Smith");
    public static readonly DishNetworkMarket FortWayne = new DishNetworkMarket(3911, "IN", "Indiana", "Fort Wayne");
    public static readonly DishNetworkMarket Gainesville = new DishNetworkMarket(3912, "FL", "Florida", "Gainesville");
    public static readonly DishNetworkMarket Glendive = new DishNetworkMarket(3913, "MT", "Montana", "Glendive");
    public static readonly DishNetworkMarket GrandJunction = new DishNetworkMarket(3914, "CO", "Colorado", "Grand Junction");
    public static readonly DishNetworkMarket GrandRapids = new DishNetworkMarket(3915, "MI", "Michigan", "Grand Rapids");
    public static readonly DishNetworkMarket GreatFalls = new DishNetworkMarket(3916, "MT", "Montana", "Great Falls");
    public static readonly DishNetworkMarket GreenBay = new DishNetworkMarket(3917, "WI", "Wisconsin", "Green Bay");
    public static readonly DishNetworkMarket GreensboroWinstonSalem = new DishNetworkMarket(3918, "NC", "North Carolina", new List<string> { "Greensboro", "Winston Salem" });
    public static readonly DishNetworkMarket GreenvilleNewBern = new DishNetworkMarket(3919, "NC", "North Carolina", new List<string> { "Greenville", "New Bern" });
    public static readonly DishNetworkMarket GreenvilleSpartanburg = new DishNetworkMarket(3920, "SC", "South Carolina", new List<string> { "Greenville", "Spartanburg" });
    public static readonly DishNetworkMarket Greenwood = new DishNetworkMarket(3921, "MS", "Mississippi", "Greenwood");
    public static readonly DishNetworkMarket HarlingenBrownsville = new DishNetworkMarket(3922, "TX", "Texas", new List<string> { "Harlingen", "Brownsville" });
    public static readonly DishNetworkMarket HarrisburgPennsylvania = new DishNetworkMarket(3923, "PA", "Pennsylvania", "Harrisburg");
    public static readonly DishNetworkMarket Harrisonburg = new DishNetworkMarket(3924, "VA", "Virginia", "Harrisonburg");
    public static readonly DishNetworkMarket HartfordNewHaven = new DishNetworkMarket(3925, "CT", "Connecticut", new List<string> { "Hartford", "New Haven" });
    public static readonly DishNetworkMarket HattiesburgLaurel = new DishNetworkMarket(3926, "MS", "Mississippi", new List<string> { "Hattiesburg", "Laurel" });
    public static readonly DishNetworkMarket Helena = new DishNetworkMarket(3927, "MT", "Montana", "Helena");
    public static readonly DishNetworkMarket Honolulu = new DishNetworkMarket(3928, "HI", "Hawaii", "Honolulu");
    public static readonly DishNetworkMarket Houston = new DishNetworkMarket(3929, "TX", "Texas", "Houston");
    public static readonly DishNetworkMarket Huntsville = new DishNetworkMarket(3930, "AL", "Alabama", "Huntsville");
    public static readonly DishNetworkMarket Falls = new DishNetworkMarket(3931, "ID", "Idaho", " Falls");
    public static readonly DishNetworkMarket Indianapolis = new DishNetworkMarket(3932, "IN", "Indiana", "Indianapolis");
    public static readonly DishNetworkMarket JacksonMississippi = new DishNetworkMarket(3933, "MS", "Mississippi", "Jackson");
    public static readonly DishNetworkMarket JacksonTennessee = new DishNetworkMarket(3934, "TN", "Tennessee", "Jackson");
    public static readonly DishNetworkMarket Jacksonville = new DishNetworkMarket(3935, "FL", "Florida", "Jacksonville");
    public static readonly DishNetworkMarket JohnstownAltoona = new DishNetworkMarket(3936, "PA", "Pennsylvania", new List<string> { "Johnstown", "Altoona" });
    public static readonly DishNetworkMarket Jonesboro = new DishNetworkMarket(3937, "AR", "Arkansas", "Jonesboro");
    public static readonly DishNetworkMarket Joplin = new DishNetworkMarket(3938, "MO", "Missouri", "Joplin");
    public static readonly DishNetworkMarket Juneau = new DishNetworkMarket(3939, "AK", "Alaska", "Juneau");
    public static readonly DishNetworkMarket KansasCity = new DishNetworkMarket(3940, "MO", "Missouri", " City");
    public static readonly DishNetworkMarket Knoxville = new DishNetworkMarket(3941, "TN", "Tennessee", "Knoxville");
    public static readonly DishNetworkMarket LaCrosse = new DishNetworkMarket(3942, "WI", "Wisconsin", "La Crosse");
    public static readonly DishNetworkMarket LafayetteIndiana = new DishNetworkMarket(3943, "IN", "Indiana", "Lafayette");
    public static readonly DishNetworkMarket LafayetteLouisiana = new DishNetworkMarket(3944, "LA", "Louisiana", "Lafayette");
    public static readonly DishNetworkMarket LakeCharles = new DishNetworkMarket(3945, "LA", "Louisiana", "Lake Charles");
    public static readonly DishNetworkMarket Lansing = new DishNetworkMarket(3946, "MI", "Michigan", "Lansing");
    public static readonly DishNetworkMarket Larado = new DishNetworkMarket(3947, "TX", "Texas", "Larado");
    public static readonly DishNetworkMarket LasVegas = new DishNetworkMarket(3948, "NV", "Nevada", "Las Vegas");
    public static readonly DishNetworkMarket Lexington = new DishNetworkMarket(3949, "KY", "Kentucky", "Lexington");
    public static readonly DishNetworkMarket Lima = new DishNetworkMarket(3950, "OH", "Ohio", "Lima");
    public static readonly DishNetworkMarket Lincoln = new DishNetworkMarket(3951, "NE", "Nebraska", "Lincoln");
    public static readonly DishNetworkMarket LittleRock = new DishNetworkMarket(3952, "AR", "Arkansas", "Little Rock");
    public static readonly DishNetworkMarket LosAngeles = new DishNetworkMarket(3953, "CA", "California", "Los Angeles");
    public static readonly DishNetworkMarket Lousiville = new DishNetworkMarket(3954, "KY", "Kentucky", "Lousiville");
    public static readonly DishNetworkMarket Lubbock = new DishNetworkMarket(3955, "TX", "Texas", "Lubbock");
    public static readonly DishNetworkMarket Macon = new DishNetworkMarket(3956, "GA", "Georgia", "Macon");
    public static readonly DishNetworkMarket Madison = new DishNetworkMarket(3957, "WI", "Wisconsin", "Madison");
    public static readonly DishNetworkMarket Mankato = new DishNetworkMarket(3958, "MN", "Minnesota", "Mankato");
    public static readonly DishNetworkMarket Marquette = new DishNetworkMarket(3959, "MI", "Michigan", "Marquette");
    public static readonly DishNetworkMarket Medford = new DishNetworkMarket(3960, "OR", "Oregon", "Medford");
    public static readonly DishNetworkMarket Memphis = new DishNetworkMarket(3961, "TN", "Tennessee", "Memphis");
    public static readonly DishNetworkMarket Meridian = new DishNetworkMarket(3962, "MS", "Mississippi", "Meridian");
    public static readonly DishNetworkMarket Miami = new DishNetworkMarket(3963, "FL", "Florida", "Miami");
    public static readonly DishNetworkMarket Milwaukee = new DishNetworkMarket(3964, "WI", "Wisconsin", "Milwaukee");
    public static readonly DishNetworkMarket MinneapolisStPaul = new DishNetworkMarket(3965, "MN", "Minnesota", new List<string> { "Minneapolis", "St Paul" });
    public static readonly DishNetworkMarket MinotBismarck = new DishNetworkMarket(3966, "ND", "North Dakota", new List<string> { "Minot", "Bismarck" });
    public static readonly DishNetworkMarket Missoula = new DishNetworkMarket(3967, "MT", "Montana", "Missoula");
    // Pensacola/Mobile
    public static readonly DishNetworkMarket Monroe = new DishNetworkMarket(3969, "LA", "Louisiana", "Monroe");
    public static readonly DishNetworkMarket Monterey = new DishNetworkMarket(3970, "CA", "California", "Monterey");
    public static readonly DishNetworkMarket Montgomery = new DishNetworkMarket(3971, "AL", "Alabama", "Montgomery");
    public static readonly DishNetworkMarket Nashville = new DishNetworkMarket(3972, "TN", "Tennessee", "Nashville");
    public static readonly DishNetworkMarket NewOrleans = new DishNetworkMarket(3973, "LA", "Louisiana", "New Orleans");
    public static readonly DishNetworkMarket NewYork = new DishNetworkMarket(3974, "NY", "New York", "New York");
    public static readonly DishNetworkMarket Norfolk = new DishNetworkMarket(3975, "VA", "Virginia", "Norfolk");
    public static readonly DishNetworkMarket NorthPlatte = new DishNetworkMarket(3976, "NE", "Nebraska", "North Platte");
    public static readonly DishNetworkMarket OdessaMidland = new DishNetworkMarket(3977, "TX", "Texas", new List<string> { "Odessa", "Midland" });
    public static readonly DishNetworkMarket OklahomaCity = new DishNetworkMarket(3978, "OK", "Oklahoma", "Oklahoma City");
    public static readonly DishNetworkMarket Omaha = new DishNetworkMarket(3979, "NE", "Nebraska", "Omaha");
    public static readonly DishNetworkMarket Orlando = new DishNetworkMarket(3980, "FL", "Florida", "Orlando");
    // Ottumwa/Kirksville
    // Harrisburg/Paducah
    public static readonly DishNetworkMarket PalmSprings = new DishNetworkMarket(3983, "CA", "California", "Palm Springs");
    public static readonly DishNetworkMarket PanamaCity = new DishNetworkMarket(3984, "FL", "Florida", "Panama City");
    public static readonly DishNetworkMarket Parkersburg = new DishNetworkMarket(3985, "WV", "West Virginia", "Parkersburg");
    public static readonly DishNetworkMarket Peoria = new DishNetworkMarket(3986, "IL", "Illinois", "Peoria");
    public static readonly DishNetworkMarket Philadelphia = new DishNetworkMarket(3987, "PA", "Pennsylvania", "Philadelphia");
    public static readonly DishNetworkMarket Phoenix = new DishNetworkMarket(3988, "AZ", "Arizona", "Phoenix");
    public static readonly DishNetworkMarket Pittsburgh = new DishNetworkMarket(3989, "PA", "Pennsylvania", "Pittsburgh");
    public static readonly DishNetworkMarket PortlandOregon = new DishNetworkMarket(3990, "OR", "Oregon", "Portland");
    public static readonly DishNetworkMarket PortlandMaine = new DishNetworkMarket(3991, "ME", "Maine", "Portland");
    public static readonly DishNetworkMarket PresqueIsle = new DishNetworkMarket(3992, "ME", "Maine", "Presque Isle");
    // New Bedford/Providence
    // Quincy/Hannibal
    public static readonly DishNetworkMarket RaleighDurham = new DishNetworkMarket(3995, "NC", "North Carolina", new List<string> { "Raleigh", "Durham" });
    public static readonly DishNetworkMarket RapidCity = new DishNetworkMarket(3996, "SD", "South Dakota", "Rapid City");
    public static readonly DishNetworkMarket Reno = new DishNetworkMarket(3997, "NV", "Nevada", "Reno");
    public static readonly DishNetworkMarket Richmond = new DishNetworkMarket(3998, "VA", "Virginia", "Richmond");
    public static readonly DishNetworkMarket RoanokeLynchburg = new DishNetworkMarket(3999, "VA", "Virginia", new List<string> { "Roanoke", "Lynchburg" });
    public static readonly DishNetworkMarket RochesterNewYork = new DishNetworkMarket(4000, "NY", "New York", "Rochester");
    // Mason City/Rochester
    public static readonly DishNetworkMarket Rockford = new DishNetworkMarket(4002, "IL", "Illinois", "Rockford");
    public static readonly DishNetworkMarket Sacramento = new DishNetworkMarket(4003, "CA", "California", "Sacramento");
    public static readonly DishNetworkMarket Salisbury = new DishNetworkMarket(4004, "MD", "Maryland", "Salisbury");
    public static readonly DishNetworkMarket SaltLakeCity = new DishNetworkMarket(4005, "UT", "Utah", "Salt Lake City");
    public static readonly DishNetworkMarket SanAngelo = new DishNetworkMarket(4006, "TX", "Texas", "San Angelo");
    public static readonly DishNetworkMarket SanAntonio = new DishNetworkMarket(4007, "TX", "Texas", "San Antonio");
    public static readonly DishNetworkMarket SanDiego = new DishNetworkMarket(4008, "CA", "California", "San Diego");
    public static readonly DishNetworkMarket SanFransisco = new DishNetworkMarket(4009, "CA", "California", "San Fransisco");
    public static readonly DishNetworkMarket SantaBarbara = new DishNetworkMarket(4010, "CA", "California", "Santa Barbara");
    public static readonly DishNetworkMarket Savannah = new DishNetworkMarket(4011, "GA", "Georgia", "Savannah");
    public static readonly DishNetworkMarket Seattle = new DishNetworkMarket(4012, "WA", "Washington", "Seattle");
    // Ada/Sherman
    public static readonly DishNetworkMarket Shreveport = new DishNetworkMarket(4014, "LA", "Louisiana", "Shreveport");
    public static readonly DishNetworkMarket SiouxCity = new DishNetworkMarket(4015, "IA", "Iowa", "Sioux City");
    public static readonly DishNetworkMarket SiouxFalls = new DishNetworkMarket(4016, "SD", "South Dakota", "Sioux Falls");
    public static readonly DishNetworkMarket SouthBend = new DishNetworkMarket(4017, "IN", "Indiana", "South Bend");
    public static readonly DishNetworkMarket Spokane = new DishNetworkMarket(4018, "WA", "Washington", "Spokane");
    public static readonly DishNetworkMarket SpringfieldMissouri = new DishNetworkMarket(4019, "MO", "Missouri", "Springfield");
    public static readonly DishNetworkMarket SpringfieldMassachusetts = new DishNetworkMarket(4020, "MA", "Massachusetts", "Springfield");
    public static readonly DishNetworkMarket StJoseph = new DishNetworkMarket(4021, "MO", "Missouri", "St Joseph");
    public static readonly DishNetworkMarket StLouis = new DishNetworkMarket(4022, "MO", "Missouri", "St Louis");
    public static readonly DishNetworkMarket Syracuse = new DishNetworkMarket(4023, "NY", "New York", "Syracuse");
    public static readonly DishNetworkMarket Tallahassee = new DishNetworkMarket(4024, "FL", "Florida", "Tallahassee");
    public static readonly DishNetworkMarket Tampa = new DishNetworkMarket(4025, "FL", "Florida", "Tampa");
    public static readonly DishNetworkMarket TerreHaute = new DishNetworkMarket(4026, "IN", "Indiana", "Terre Haute");
    public static readonly DishNetworkMarket Toledo = new DishNetworkMarket(4027, "OH", "Ohio", "Toledo");
    public static readonly DishNetworkMarket Topeka = new DishNetworkMarket(4028, "KS", "Kansas", "Topeka");
    public static readonly DishNetworkMarket TraverseCity = new DishNetworkMarket(4029, "MI", "Michigan", "Traverse City");
    // Tri-Cities
    public static readonly DishNetworkMarket Tucson = new DishNetworkMarket(4031, "AZ", "Arizona", "Tucson");
    public static readonly DishNetworkMarket Tulsa = new DishNetworkMarket(4032, "OK", "Oklahoma", "Tulsa");
    public static readonly DishNetworkMarket TwinFalls = new DishNetworkMarket(4033, "ID", "Idaho", "Twin Falls");
    public static readonly DishNetworkMarket Tyler = new DishNetworkMarket(4034, "TX", "Texas", "Tyler");
    public static readonly DishNetworkMarket Utica = new DishNetworkMarket(4035, "NY", "New York", "Utica");
    public static readonly DishNetworkMarket Victoria = new DishNetworkMarket(4036, "TX", "Texas", "Victoria");
    public static readonly DishNetworkMarket Waco = new DishNetworkMarket(4037, "TX", "Texas", "Waco");
    public static readonly DishNetworkMarket Washington = new DishNetworkMarket(4038, "DC", "District Of Columbia", "Washington");
    public static readonly DishNetworkMarket Watertown = new DishNetworkMarket(4039, "NY", "New York", "Watertown");
    public static readonly DishNetworkMarket Wausau = new DishNetworkMarket(4040, "WI", "Wisconsin", "Wausau");
    public static readonly DishNetworkMarket WestPalmBeach = new DishNetworkMarket(4041, "FL", "Florida", "West Palm Beach");
    // Steubenville/Wheeling
    // Lawton/Wichita Falls
    public static readonly DishNetworkMarket Wichita = new DishNetworkMarket(4044, "KS", "Kansas", "Wichita");
    public static readonly DishNetworkMarket WilkesBarreScranton = new DishNetworkMarket(4045, "PA", "Pennsylvania", new List<string> { "Wilkes-Barre", "Scranton" });
    public static readonly DishNetworkMarket Wilmington = new DishNetworkMarket(4046, "NC", "North Carolina", "Wilmington");
    public static readonly DishNetworkMarket Yakima = new DishNetworkMarket(4047, "WA", "Washington", "Yakima");
    public static readonly DishNetworkMarket Youngstown = new DishNetworkMarket(4048, "OH", "Ohio", "Youngstown");
    // Yuma/El Centro
    public static readonly DishNetworkMarket Zanesville = new DishNetworkMarket(4050, "OH", "Ohio", "Zanesville");
    public static readonly DishNetworkMarket SanJuan = new DishNetworkMarket(4051, "PR", "Puerto Rico", "San Juan");

    // multi-state markets, split for user clarity
    public static readonly DishNetworkMarket Davenport = new DishNetworkMarket(3891, "IA", "Iowa", "Davenport");
    public static readonly DishNetworkMarket RockIsland = new DishNetworkMarket(3891, "IL", "Illinois", "Rock Island");
    public static readonly DishNetworkMarket Pensacola = new DishNetworkMarket(3968, "FL", "Florida", "Pensacola");
    public static readonly DishNetworkMarket Mobile = new DishNetworkMarket(3968, "LA", "Louisiana", "Mobile");
    public static readonly DishNetworkMarket Ottumwa = new DishNetworkMarket(3981, "IA", "Iowa", "Ottumwa");
    public static readonly DishNetworkMarket Kirksville = new DishNetworkMarket(3981, "MO", "Missouri", "Kirksville");
    public static readonly DishNetworkMarket HarrisburgIllinois = new DishNetworkMarket(3982, "IL", "Illinois", "Harrisburg");
    public static readonly DishNetworkMarket Paducah = new DishNetworkMarket(3982, "KY", "Kentucky", "Paducah");
    public static readonly DishNetworkMarket NewBedford = new DishNetworkMarket(3993, "MA", "Massachussets", "New Bedford");
    public static readonly DishNetworkMarket Providence = new DishNetworkMarket(3993, "RI", "Rhode Island", "Providence");
    public static readonly DishNetworkMarket Quincy = new DishNetworkMarket(3994, "IL", "Illinois", "Quincy");
    public static readonly DishNetworkMarket Hannibal = new DishNetworkMarket(3994, "MO", "Missouri", "Hannibal");
    public static readonly DishNetworkMarket MasonCity = new DishNetworkMarket(4001, "IA", "Iowa", "Mason City");
    public static readonly DishNetworkMarket RochesterMinnesota = new DishNetworkMarket(4001, "MN", "Minnesota", "Rochester");
    public static readonly DishNetworkMarket Ada = new DishNetworkMarket(4013, "OK", "Oklahoma", "Ada");
    public static readonly DishNetworkMarket Sherman = new DishNetworkMarket(4013, "TX", "Texas", "Sherman");
    public static readonly DishNetworkMarket TriCitiesTennessee = new DishNetworkMarket(4030, "TN", "Tennessee", "Tri-Cities");
    public static readonly DishNetworkMarket TriCitiesVirginia = new DishNetworkMarket(4030, "VA", "Virginia", "Tri-Cities");
    public static readonly DishNetworkMarket Steubenville = new DishNetworkMarket(4042, "OH", "Ohio", "Steubenville");
    public static readonly DishNetworkMarket Wheeling = new DishNetworkMarket(4042, "WV", "West Virginia", "Wheeling");
    public static readonly DishNetworkMarket Lawton = new DishNetworkMarket(4043, "OK", "Oklahoma", "Lawton");
    public static readonly DishNetworkMarket WichitaFalls = new DishNetworkMarket(4043, "TX", "Texas", "Wichita Falls");
    public static readonly DishNetworkMarket Yuma = new DishNetworkMarket(4049, "AZ", "Arizona", "Yuma");
    public static readonly DishNetworkMarket ElCentro = new DishNetworkMarket(4049, "CA", "California", "El Centro");

    #endregion

    private DishNetworkMarket(int id, string stateAbbreviation, string state, string city)
    {
      _id = id;
      _stateAbbreviation = stateAbbreviation;
      _state = state;
      _cities = new List<string> { city };
      AddToValuesDictionary();
    }

    private DishNetworkMarket(int id, string stateAbbreviation, string state, IList<string> cities)
    {
      _id = id;
      _stateAbbreviation = stateAbbreviation;
      _state = state;
      _cities = cities;
      AddToValuesDictionary();
    }

    private void AddToValuesDictionary()
    {
      IDictionary<string, DishNetworkMarket> subMarkets;
      if (!_values.TryGetValue(_id, out subMarkets))
      {
        subMarkets = new Dictionary<string, DishNetworkMarket>(2);
        _values[_id] = subMarkets;
      }
      subMarkets.Add(_stateAbbreviation, this);
      _valueList.Add(this);
    }

    #region properties

    public int Id
    {
      get
      {
        return _id;
      }
    }

    public string StateAbbreviation
    {
      get
      {
        return _stateAbbreviation;
      }
    }

    public string State
    {
      get
      {
        return _state;
      }
    }

    public ReadOnlyCollection<string> Cities
    {
      get
      {
        return new ReadOnlyCollection<string>(_cities);
      }
    }

    #endregion

    #region object overrides

    public override string ToString()
    {
      return string.Format("{0} - {1}", _stateAbbreviation, string.Join(", ", _cities));
    }

    public override bool Equals(object obj)
    {
      DishNetworkMarket market = obj as DishNetworkMarket;
      if (market != null && this == market)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _id.GetHashCode() ^ _state.GetHashCode() ^ _cities.GetHashCode();
    }

    #endregion

    #region static members

    public static ICollection<DishNetworkMarket> Values
    {
      get
      {
        return _valueList;
      }
    }

    public static DishNetworkMarket GetValue(int id, string stateAbbreviation)
    {
      IDictionary<string, DishNetworkMarket> subMarkets;
      if (!_values.TryGetValue(id, out subMarkets))
      {
        return null;
      }
      DishNetworkMarket lastValue = null;
      foreach (var subMarket in subMarkets)
      {
        if (string.IsNullOrEmpty(stateAbbreviation) || string.Equals(stateAbbreviation, subMarket.Key))
        {
          return subMarket.Value;
        }
        lastValue = subMarket.Value;
      }
      return lastValue;
    }

    #endregion
  }
}