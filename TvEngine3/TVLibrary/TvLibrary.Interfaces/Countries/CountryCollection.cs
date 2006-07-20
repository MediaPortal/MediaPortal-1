using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary
{
  /// <summary>
  /// class which holds a collection of all countries
  /// </summary>
  [Serializable]
  public class CountryCollection
  {
    #region countries
    Country[] _countryList = new Country[]
    {
      new Country(93,	"Afghanistan",	        "AF"),
      new Country(355,	"Albania",	            "AL"),
      new Country(213,	"Algeria",	            "DZ"),
      new Country(684,	"American Samoa",	    "AS"),
      new Country(376,	"Andorra",	            "AD"),
      new Country(244,	"Angola",	            "AO"),
      new Country(1,	  "Anguilla",	            "AI"),
      new Country(1,	  "Antigua And Barbuda",	"AG"),
      new Country(54,	"Argentina",	        "AR"),
      new Country(374,	"Armenia",	            "AM"),
      new Country(297,	"Aruba",	            "AW"),
      new Country(61,	"Australia",	        "AU"),
      new Country(43,	"Austria",	            "AT"),
      new Country(994,	"Azerbaijan",	        "AZ"),
      new Country(1,	  "Bahamas",	            "BS"),
      new Country(973,	"Bahrain",	            "BH"),
      new Country(880,	"Bangladesh",	        "BD"),
      new Country(1,	  "Barbados",	            "BB"),
      new Country(375,	"Belarus",	            "BY"),
      new Country(32,	"Belgium",	            "BE"),
      new Country(501,	"Belize",	            "BZ"),
      new Country(229,	"Benin",	            "BJ"),
      new Country(1,	  "Bermuda",	            "BM"),
      new Country(975,	"Bhutan",	            "BT"),
      new Country(591,	"Bolivia",	            "BO"),
      new Country(387,	"Bosnia And Herzegovina",	"BA"),
      new Country(267,	"Botswana",	            "BW"),
      new Country(55,	"Brazil",	            "BR"),
      new Country(673,	"Brunei Darussalam",	"BN"),
      new Country(359,	"Bulgaria",	            "BG"),
      new Country(226,	"Burkina Faso",	        "BF"),
      new Country(257,	"Burundi",	            "BI"),
      new Country(855,	"Cambodia",	            "KH"),
      new Country(237,	"Cameroon",	            "CM"),
      new Country(1,	  "Canada",	            "CA"),
      new Country(238,	"Cape Verde",	        "CV"),
      new Country(1,	  "Cayman Islands",	    "KY"),
      new Country(236,	"Central African Republic",	"CF"),
      new Country(235,	"Chad",	                "TD"),
      new Country(56,	"Chile",	            "CL"),
      new Country(86,	"China",	            "CN"),
      new Country(672,	"Christmas Island",	    "CX"),
      new Country(61,	"Cocos (Keeling) Islands",	"CC"),
      new Country(57,	"Colombia",	            "CO"),
      new Country(269,	"Comoros",	            "KM"),
      new Country(242,	"Congo",	            "CG"),
      new Country(243,	"Congo, The Democratic Republic of The",	"CD"),
      new Country(682,	"Cook Islands",	        "CK"),
      new Country(506,	"Costa Rica",	        "CR"),
      new Country(225,	"Côte D'Ivoire",	    "CI"),
      new Country(385,	"Croatia",	            "HR"),
      new Country(53,	"Cuba",	                "CU"),
      new Country(357,	"Cyprus",	            "CY"),
      new Country(420,	"Czech Republic",	    "CZ"),
      new Country(45,	"Denmark",	            "DK"),
      new Country(253,	"Djibouti",	            "DJ"),
      new Country(1,	  "Dominica",	            "DM"),
      new Country(1,	  "Dominican Republic",	"DO"),
      new Country(593,	"Ecuador",	            "EC"),
      new Country(20,	"Egypt",	            "EG"),
      new Country(503,	"El Salvador",	        "SV"),
      new Country(240,	"Equatorial Guinea",	"GQ"),
      new Country(291,	"Eritrea",	            "ER"),
      new Country(372,	"Estonia",	            "EE"),
      new Country(251,	"Ethiopia",	            "ET"),
      new Country(500,	"Falkland Islands (Malvinas)",	"FK"),
      new Country(298,	"Faroe Islands",	    "FO"),
      new Country(679,	"Fiji",	                "FJ"),
      new Country(358,	"Finland",	            "FI"),
      new Country(33,	"France",	            "FR"),
      new Country(594,	"French Guiana",	    "GF"),
      new Country(689,	"French Polynesia",	    "PF"),
      new Country(241,	"Gabon",	            "GA"),
      new Country(220,	"Gambia",	            "GM"),
      new Country(995,	"Georgia",	            "GE"),
      new Country(49,	"Germany",	            "DE"),
      new Country(233,	"Ghana",	            "GH"),
      new Country(350,	"Gibraltar",	        "GI"),
      new Country(30,	"Greece",	            "GR"),
      new Country(299,	"Greenland",	        "GL"),
      new Country(1,	  "Grenada",	            "GD"),
      new Country(590,	"Guadeloupe",	        "GP"),
      new Country(671,	"Guam",	                "GU"),
      new Country(502,	"Guatemala",	        "GT"),
      new Country(224,	"Guinea",	            "GN"),
      new Country(245,	"Guinea-Bissau",	    "GW"),
      new Country(592,	"Guyana",	            "GY"),
      new Country(509,	"Haiti",	            "HT"),
      new Country(504,	"Honduras",	            "HN"),
      new Country(852,	"Hong Kong, SAR",	    "HK"),
      new Country(36,	"Hungary",	            "HU"),
      new Country(354,	"Iceland",	            "IS"),
      new Country(91,	"India",	            "IN"),
      new Country(62,	"Indonesia",	        "ID"),
      new Country(98,	"Iran, Islamic Republic Of",	"IR"),
      new Country(964,	"Iraq",	                "IQ"),
      new Country(353,	"Ireland",	            "IE"),
      new Country(972,	"Israel",	            "IL"),
      new Country(39,	"Italy",	            "IT"),
      new Country(1,	  "Jamaica",	            "JM"),
      new Country(81,	"Japan",	            "JP"),
      new Country(962,	"Jordan",	            "JO"),
      new Country(7,	  "Kazakhstan",	        "KZ"),
      new Country(254,	"Kenya",	            "KE"),
      new Country(686,	"Kiribati",	            "KI"),
      new Country(82,	"Korea",	            "KR"),
      new Country(850,	"Korea, Democratic People's Republic of",	"KP"),
      new Country(965,	"Kuwait",	            "KW "),
      new Country(7,	  "Kyrgyzstan",	        "KG"),
      new Country(856,	"Lao, People's Democratic Republic",	"LA"),
      new Country(371,	"Latvia",	            "LV"),
      new Country(961,	"Lebanon",	            "LB"),
      new Country(266,	"Lesotho",	            "LS"),
      new Country(231,	"Liberia",	            "LR"),
      new Country(218,	"Libyan Arab Jamahiriya",	"LY"),
      new Country(41,	"Liechtenstein",	    "LI"),
      new Country(370,	"Lithuania",	        "LT"),
      new Country(352,	"Luxembourg",	        "LU"),
      new Country(853,	"Macao, SAR",	        "MO"),
      new Country(389,	"Macedonia, The Former Yugoslav Republic of",	"MK"),
      new Country(261,	"Madagascar",	        "MG"),
      new Country(265,	"Malawi",	            "MW"),
      new Country(60,	"Malaysia",	            "MY"),
      new Country(960,	"Maldives",	            "MV"),
      new Country(223,	"Mali",	                "ML"),
      new Country(356,	"Malta",	            "MT"),
      new Country(692,	"Marshall Islands",	    "MH"),
      new Country(596,	"Martinique",	        "MQ"),
      new Country(222,	"Mauritania",	        "MR"),
      new Country(230,	"Mauritius",	        "MU"),
      new Country(269,	"Mayotte",	            "YT"),
      new Country(52,	"Mexico",	            "MX"),
      new Country(691,	"Micronesia, Federated States of",	"FM"),
      new Country(373,	"Moldova, Republic of",	"MD"),
      new Country(377,	"Monaco",	            "MC"),
      new Country(976,	"Mongolia",	            "MN"),
      new Country(1,	  "Montserrat",	        "MS"),
      new Country(212,	"Morocco",	            "MA"),
      new Country(258,	"Mozambique",	        "MZ"),
      new Country(95,	"Myanmar",	            "MM"),
      new Country(264,	"Namibia",	            "NA"),
      new Country(674,	"Nauru",	            "NR"),
      new Country(977,	"Nepal",	            "NP"),
      new Country(31,	"The Netherlands",	        "NL"),
      new Country(599,	"Netherlands Antilles",	"AN"),
      new Country(687,	"New Caledonia",	    "NC"),
      new Country(64,	"New Zealand",	        "NZ"),
      new Country(505,	"Nicaragua",	        "NI"),
      new Country(227,	"Niger",	            "NE"),
      new Country(234,	"Nigeria",	            "NG"),
      new Country(683,	"Niue",	                "NU"),
      new Country(672,	"Norfolk Island",	    "NF"),
      new Country(47,	"Norway",	            "NO"),
      new Country(968,	"Oman",	                "OM"),
      new Country(92,	"Pakistan",	            "PK"),
      new Country(680,	"Palau",	            "PW"),
      new Country(507,	"Panama",	            "PA"),
      new Country(675,	"Papua New Guinea",	    "PG"),
      new Country(595,	"Paraguay",	            "PY"),
      new Country(51,	"Peru",	                "PE"),
      new Country(63,	"Philippines",	        "PH"),
      new Country(48,	"Poland",	            "PL"),
      new Country(351,	"Portugal",	            "PT"),
      new Country(1,	  "Puerto Rico",	        "PR"),
      new Country(974,	"Qatar",	            "QA"),
      new Country(262,	"R‚union",	            "RE"),
      new Country(40,	"Romania",	            "RO"),
      new Country(7,	  "Russian Federation",	"RU"),
      new Country(250,	"Rwanda",	            "RW"),
      new Country(290,	"Saint Helena",	        "SH"),
      new Country(1,	  "Saint Kitts And Nevis",	"KN"),
      new Country(1,	  "Saint Lucia",	        "LC"),
      new Country(508,	"Saint Pierre And Miquelon",	"PM"),
      new Country(1,	  "Saint Vincent And The Grenadines",	"VC"),
      new Country(685,	"Samoa",	            "WS"),
      new Country(378,	"San Marino",	        "SM"),
      new Country(239,	"Sao Tome And Principe",	"ST"),
      new Country(966,	"Saudi Arabia",	        "SA"),
      new Country(221,	"Senegal",	            "SN"),
      new Country(381,	"Serbia And Montenegro",	"CS"),
      new Country(248,	"Seychelles",	        "SC"),
      new Country(232,	"Sierra Leone",	        "SL"),
      new Country(65,	"Singapore",	        "SG"),
      new Country(421,	"Slovakia",	            "SK"),
      new Country(386,	"Slovenia",	            "SI"),
      new Country(677,	"Solomon Islands",	    "SB"),
      new Country(252,	"Somalia",	            "SO"),
      new Country(27,	"South Africa",	        "ZA"),
      new Country(34,	"Spain",	            "ES"),
      new Country(94,	"Sri Lanka",	        "LK"),
      new Country(249,	"Sudan",	            "SD"),
      new Country(597,	"Suriname",	            "SR"),
      new Country(268,	"Swaziland",	        "SZ"),
      new Country(46,	"Sweden",	            "SE"),
      new Country(41,	"Switzerland",	        "CH"),
      new Country(963,	"Syrian Arab Republic",	"SY"),
      new Country(886,	"Taiwan",	            "TW"),
      new Country(7,	  "Tajikistan",	        "TJ"),
      new Country(255,	"Tanzania, United Republic of",	"TZ"),
      new Country(66,	"Thailand",	            "TH"),
      new Country(228,	"Togo",	                "TG"),
      new Country(690,	"Tokelau",	            "TK"),
      new Country(676,	"Tonga",	            "TO"),
      new Country(1,	  "Trinidad And Tobago",	"TT"),
      new Country(216,	"Tunisia",	            "TN"),
      new Country(90,	"Turkey",	            "TR"),
      new Country(7,	  "Turkmenistan",	        "TM"),
      new Country(1,	  "Turks And Caicos Islands",	"TC"),
      new Country(688,	"Tuvalu",	            "TV"),
      new Country(256,	"Uganda",	            "UG"),
      new Country(380,	"Ukraine",	            "UA"),
      new Country(971,	"United Arab Emirates",	"AE"),
      new Country(44,	"United Kingdom",	    "GB"),
      new Country(1,	  "United States",	    "US"),
      new Country(598,	"Uruguay",	            "UY"),
      new Country(7,	  "Uzbekistan",	        "UZ"),
      new Country(678,	"Vanuatu",	            "VU"),
      new Country(39,	"Vatican City State (Holy See)",	"VA"),
      new Country(58,	"Venezuela",	        "VE"),
      new Country(84,	"Viet Nam",	            "VN"),
      new Country(1,	  "Virgin Islands, British",	"VG"),
      new Country(1,	  "Virgin Islands, U.S.",	"VI"),
      new Country(681,	"Wallis and Futuna",	"WF"),
      new Country(967,	"Yemen",	            "YE"),
      new Country(260,	"Zambia",	            "ZM"),
      new Country(263,	"Zimbabwe",	            "ZW")
    };
#endregion

    /// <summary>
    /// Returns a country specified by name
    /// </summary>
    /// <param name="countryName">name of country</param>
    /// <returns>Country object or null if country is not found</returns>
    public Country GetTunerCountry(string countryName)
    {
      foreach (Country country in _countryList)
      {
        if (country.Name == countryName)
        {
          return country;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns a country specified by id
    /// </summary>
    /// <param name="countryName">id of country</param>
    /// <returns>Country object or null if country is not found</returns>
    public Country GetTunerCountryFromID(int countryId)
    {
      foreach (Country country in _countryList)
      {
        if (country.Id == countryId)
        {
          return country;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns a country specified by id
    /// </summary>
    /// <param name="countryName">country code</param>
    /// <returns>Country object or null if country is not found</returns>
    public Country GetTunerCountryFromCode(string code)
    {
      foreach (Country country in _countryList)
      {
        if (country.Code == code)
        {
          return country;
        }
      }
      return null;
    }


    /// <summary>
    /// Returns an array of all countries
    /// </summary>
    public Country[] Countries
    {
      get
      {
        return _countryList;
      }
    }

  }
}

