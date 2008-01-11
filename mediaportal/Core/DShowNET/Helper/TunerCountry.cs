#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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

namespace DShowNET
{
  /// <summary>
  /// Summary description for TunerCountry.
  /// </summary>
  public class TunerCountry
  {
    public int Id;
    public string Country;
    public string CountryCode;

    public TunerCountry(int id, string country, string code)
    {
      this.Id = id;
      this.Country = country;
      this.CountryCode = code;
    }

    public override string ToString()
    {
      return Country;
    }
  }

  public class TunerCountries
  {
    public static TunerCountry GetTunerCountry(string countryName)
    {
      foreach (TunerCountry country in Countries)
      {
        if (country.Country == countryName)
        {
          return country;
        }
      }
      return null;
    }
    public static TunerCountry GetTunerCountryFromID(int countryId)
    {
      foreach (TunerCountry country in Countries)
      {
        if (country.Id == countryId)
        {
          return country;
        }
      }
      return null;
    }
    public static TunerCountry[] Countries = new TunerCountry[]
    {
      new TunerCountry(93,	"Afghanistan",	        "AF"),
      new TunerCountry(355,	"Albania",	            "AL"),
      new TunerCountry(213,	"Algeria",	            "DZ"),
      new TunerCountry(684,	"American Samoa",	    "AS"),
      new TunerCountry(376,	"Andorra",	            "AD"),
      new TunerCountry(244,	"Angola",	            "AO"),
      new TunerCountry(1,	  "Anguilla",	            "AI"),
      new TunerCountry(1,	  "Antigua And Barbuda",	"AG"),
      new TunerCountry(54,	"Argentina",	        "AR"),
      new TunerCountry(374,	"Armenia",	            "AM"),
      new TunerCountry(297,	"Aruba",	            "AW"),
      new TunerCountry(61,	"Australia",	        "AU"),
      new TunerCountry(43,	"Austria",	            "AT"),
      new TunerCountry(994,	"Azerbaijan",	        "AZ"),
      new TunerCountry(1,	  "Bahamas",	            "BS"),
      new TunerCountry(973,	"Bahrain",	            "BH"),
      new TunerCountry(880,	"Bangladesh",	        "BD"),
      new TunerCountry(1,	  "Barbados",	            "BB"),
      new TunerCountry(375,	"Belarus",	            "BY"),
      new TunerCountry(32,	"Belgium",	            "BE"),
      new TunerCountry(501,	"Belize",	            "BZ"),
      new TunerCountry(229,	"Benin",	            "BJ"),
      new TunerCountry(1,	  "Bermuda",	            "BM"),
      new TunerCountry(975,	"Bhutan",	            "BT"),
      new TunerCountry(591,	"Bolivia",	            "BO"),
      new TunerCountry(387,	"Bosnia And Herzegovina",	"BA"),
      new TunerCountry(267,	"Botswana",	            "BW"),
      new TunerCountry(55,	"Brazil",	            "BR"),
      new TunerCountry(673,	"Brunei Darussalam",	"BN"),
      new TunerCountry(359,	"Bulgaria",	            "BG"),
      new TunerCountry(226,	"Burkina Faso",	        "BF"),
      new TunerCountry(257,	"Burundi",	            "BI"),
      new TunerCountry(855,	"Cambodia",	            "KH"),
      new TunerCountry(237,	"Cameroon",	            "CM"),
      new TunerCountry(1,	  "Canada",	            "CA"),
      new TunerCountry(238,	"Cape Verde",	        "CV"),
      new TunerCountry(1,	  "Cayman Islands",	    "KY"),
      new TunerCountry(236,	"Central African Republic",	"CF"),
      new TunerCountry(235,	"Chad",	                "TD"),
      new TunerCountry(56,	"Chile",	            "CL"),
      new TunerCountry(86,	"China",	            "CN"),
      new TunerCountry(672,	"Christmas Island",	    "CX"),
      new TunerCountry(61,	"Cocos (Keeling) Islands",	"CC"),
      new TunerCountry(57,	"Colombia",	            "CO"),
      new TunerCountry(269,	"Comoros",	            "KM"),
      new TunerCountry(242,	"Congo",	            "CG"),
      new TunerCountry(243,	"Congo, The Democratic Republic of The",	"CD"),
      new TunerCountry(682,	"Cook Islands",	        "CK"),
      new TunerCountry(506,	"Costa Rica",	        "CR"),
      new TunerCountry(225,	"Côte D'Ivoire",	    "CI"),
      new TunerCountry(385,	"Croatia",	            "HR"),
      new TunerCountry(53,	"Cuba",	                "CU"),
      new TunerCountry(357,	"Cyprus",	            "CY"),
      new TunerCountry(420,	"Czech Republic",	    "CZ"),
      new TunerCountry(45,	"Denmark",	            "DK"),
      new TunerCountry(253,	"Djibouti",	            "DJ"),
      new TunerCountry(1,	  "Dominica",	            "DM"),
      new TunerCountry(1,	  "Dominican Republic",	"DO"),
      new TunerCountry(593,	"Ecuador",	            "EC"),
      new TunerCountry(20,	"Egypt",	            "EG"),
      new TunerCountry(503,	"El Salvador",	        "SV"),
      new TunerCountry(240,	"Equatorial Guinea",	"GQ"),
      new TunerCountry(291,	"Eritrea",	            "ER"),
      new TunerCountry(372,	"Estonia",	            "EE"),
      new TunerCountry(251,	"Ethiopia",	            "ET"),
      new TunerCountry(500,	"Falkland Islands (Malvinas)",	"FK"),
      new TunerCountry(298,	"Faroe Islands",	    "FO"),
      new TunerCountry(679,	"Fiji",	                "FJ"),
      new TunerCountry(358,	"Finland",	            "FI"),
      new TunerCountry(33,	"France",	            "FR"),
      new TunerCountry(594,	"French Guiana",	    "GF"),
      new TunerCountry(689,	"French Polynesia",	    "PF"),
      new TunerCountry(241,	"Gabon",	            "GA"),
      new TunerCountry(220,	"Gambia",	            "GM"),
      new TunerCountry(995,	"Georgia",	            "GE"),
      new TunerCountry(49,	"Germany",	            "DE"),
      new TunerCountry(233,	"Ghana",	            "GH"),
      new TunerCountry(350,	"Gibraltar",	        "GI"),
      new TunerCountry(30,	"Greece",	            "GR"),
      new TunerCountry(299,	"Greenland",	        "GL"),
      new TunerCountry(1,	  "Grenada",	            "GD"),
      new TunerCountry(590,	"Guadeloupe",	        "GP"),
      new TunerCountry(671,	"Guam",	                "GU"),
      new TunerCountry(502,	"Guatemala",	        "GT"),
      new TunerCountry(224,	"Guinea",	            "GN"),
      new TunerCountry(245,	"Guinea-Bissau",	    "GW"),
      new TunerCountry(592,	"Guyana",	            "GY"),
      new TunerCountry(509,	"Haiti",	            "HT"),
      new TunerCountry(504,	"Honduras",	            "HN"),
      new TunerCountry(852,	"Hong Kong, SAR",	    "HK"),
      new TunerCountry(36,	"Hungary",	            "HU"),
      new TunerCountry(354,	"Iceland",	            "IS"),
      new TunerCountry(91,	"India",	            "IN"),
      new TunerCountry(62,	"Indonesia",	        "ID"),
      new TunerCountry(98,	"Iran, Islamic Republic Of",	"IR"),
      new TunerCountry(964,	"Iraq",	                "IQ"),
      new TunerCountry(353,	"Ireland",	            "IE"),
      new TunerCountry(972,	"Israel",	            "IL"),
      new TunerCountry(39,	"Italy",	            "IT"),
      new TunerCountry(1,	  "Jamaica",	            "JM"),
      new TunerCountry(81,	"Japan",	            "JP"),
      new TunerCountry(962,	"Jordan",	            "JO"),
      new TunerCountry(7,	  "Kazakhstan",	        "KZ"),
      new TunerCountry(254,	"Kenya",	            "KE"),
      new TunerCountry(686,	"Kiribati",	            "KI"),
      new TunerCountry(82,	"Korea",	            "KR"),
      new TunerCountry(850,	"Korea, Democratic People's Republic of",	"KP"),
      new TunerCountry(965,	"Kuwait",	            "KW "),
      new TunerCountry(7,	  "Kyrgyzstan",	        "KG"),
      new TunerCountry(856,	"Lao, People's Democratic Republic",	"LA"),
      new TunerCountry(371,	"Latvia",	            "LV"),
      new TunerCountry(961,	"Lebanon",	            "LB"),
      new TunerCountry(266,	"Lesotho",	            "LS"),
      new TunerCountry(231,	"Liberia",	            "LR"),
      new TunerCountry(218,	"Libyan Arab Jamahiriya",	"LY"),
      new TunerCountry(41,	"Liechtenstein",	    "LI"),
      new TunerCountry(370,	"Lithuania",	        "LT"),
      new TunerCountry(352,	"Luxembourg",	        "LU"),
      new TunerCountry(853,	"Macao, SAR",	        "MO"),
      new TunerCountry(389,	"Macedonia, The Former Yugoslav Republic of",	"MK"),
      new TunerCountry(261,	"Madagascar",	        "MG"),
      new TunerCountry(265,	"Malawi",	            "MW"),
      new TunerCountry(60,	"Malaysia",	            "MY"),
      new TunerCountry(960,	"Maldives",	            "MV"),
      new TunerCountry(223,	"Mali",	                "ML"),
      new TunerCountry(356,	"Malta",	            "MT"),
      new TunerCountry(692,	"Marshall Islands",	    "MH"),
      new TunerCountry(596,	"Martinique",	        "MQ"),
      new TunerCountry(222,	"Mauritania",	        "MR"),
      new TunerCountry(230,	"Mauritius",	        "MU"),
      new TunerCountry(269,	"Mayotte",	            "YT"),
      new TunerCountry(52,	"Mexico",	            "MX"),
      new TunerCountry(691,	"Micronesia, Federated States of",	"FM"),
      new TunerCountry(373,	"Moldova, Republic of",	"MD"),
      new TunerCountry(377,	"Monaco",	            "MC"),
      new TunerCountry(976,	"Mongolia",	            "MN"),
      new TunerCountry(1,	  "Montserrat",	        "MS"),
      new TunerCountry(212,	"Morocco",	            "MA"),
      new TunerCountry(258,	"Mozambique",	        "MZ"),
      new TunerCountry(95,	"Myanmar",	            "MM"),
      new TunerCountry(264,	"Namibia",	            "NA"),
      new TunerCountry(674,	"Nauru",	            "NR"),
      new TunerCountry(977,	"Nepal",	            "NP"),
      new TunerCountry(31,	"The Netherlands",	        "NL"),
      new TunerCountry(599,	"Netherlands Antilles",	"AN"),
      new TunerCountry(687,	"New Caledonia",	    "NC"),
      new TunerCountry(64,	"New Zealand",	        "NZ"),
      new TunerCountry(505,	"Nicaragua",	        "NI"),
      new TunerCountry(227,	"Niger",	            "NE"),
      new TunerCountry(234,	"Nigeria",	            "NG"),
      new TunerCountry(683,	"Niue",	                "NU"),
      new TunerCountry(672,	"Norfolk Island",	    "NF"),
      new TunerCountry(47,	"Norway",	            "NO"),
      new TunerCountry(968,	"Oman",	                "OM"),
      new TunerCountry(92,	"Pakistan",	            "PK"),
      new TunerCountry(680,	"Palau",	            "PW"),
      new TunerCountry(507,	"Panama",	            "PA"),
      new TunerCountry(675,	"Papua New Guinea",	    "PG"),
      new TunerCountry(595,	"Paraguay",	            "PY"),
      new TunerCountry(51,	"Peru",	                "PE"),
      new TunerCountry(63,	"Philippines",	        "PH"),
      new TunerCountry(48,	"Poland",	            "PL"),
      new TunerCountry(351,	"Portugal",	            "PT"),
      new TunerCountry(1,	  "Puerto Rico",	        "PR"),
      new TunerCountry(974,	"Qatar",	            "QA"),
      new TunerCountry(262,	"R‚union",	            "RE"),
      new TunerCountry(40,	"Romania",	            "RO"),
      new TunerCountry(7,	  "Russian Federation",	"RU"),
      new TunerCountry(250,	"Rwanda",	            "RW"),
      new TunerCountry(290,	"Saint Helena",	        "SH"),
      new TunerCountry(1,	  "Saint Kitts And Nevis",	"KN"),
      new TunerCountry(1,	  "Saint Lucia",	        "LC"),
      new TunerCountry(508,	"Saint Pierre And Miquelon",	"PM"),
      new TunerCountry(1,	  "Saint Vincent And The Grenadines",	"VC"),
      new TunerCountry(685,	"Samoa",	            "WS"),
      new TunerCountry(378,	"San Marino",	        "SM"),
      new TunerCountry(239,	"Sao Tome And Principe",	"ST"),
      new TunerCountry(966,	"Saudi Arabia",	        "SA"),
      new TunerCountry(221,	"Senegal",	            "SN"),
      new TunerCountry(381,	"Serbia And Montenegro",	"CS"),
      new TunerCountry(248,	"Seychelles",	        "SC"),
      new TunerCountry(232,	"Sierra Leone",	        "SL"),
      new TunerCountry(65,	"Singapore",	        "SG"),
      new TunerCountry(421,	"Slovakia",	            "SK"),
      new TunerCountry(386,	"Slovenia",	            "SI"),
      new TunerCountry(677,	"Solomon Islands",	    "SB"),
      new TunerCountry(252,	"Somalia",	            "SO"),
      new TunerCountry(27,	"South Africa",	        "ZA"),
      new TunerCountry(34,	"Spain",	            "ES"),
      new TunerCountry(94,	"Sri Lanka",	        "LK"),
      new TunerCountry(249,	"Sudan",	            "SD"),
      new TunerCountry(597,	"Suriname",	            "SR"),
      new TunerCountry(268,	"Swaziland",	        "SZ"),
      new TunerCountry(46,	"Sweden",	            "SE"),
      new TunerCountry(41,	"Switzerland",	        "CH"),
      new TunerCountry(963,	"Syrian Arab Republic",	"SY"),
      new TunerCountry(886,	"Taiwan",	            "TW"),
      new TunerCountry(7,	  "Tajikistan",	        "TJ"),
      new TunerCountry(255,	"Tanzania, United Republic of",	"TZ"),
      new TunerCountry(66,	"Thailand",	            "TH"),
      new TunerCountry(228,	"Togo",	                "TG"),
      new TunerCountry(690,	"Tokelau",	            "TK"),
      new TunerCountry(676,	"Tonga",	            "TO"),
      new TunerCountry(1,	  "Trinidad And Tobago",	"TT"),
      new TunerCountry(216,	"Tunisia",	            "TN"),
      new TunerCountry(90,	"Turkey",	            "TR"),
      new TunerCountry(7,	  "Turkmenistan",	        "TM"),
      new TunerCountry(1,	  "Turks And Caicos Islands",	"TC"),
      new TunerCountry(688,	"Tuvalu",	            "TV"),
      new TunerCountry(256,	"Uganda",	            "UG"),
      new TunerCountry(380,	"Ukraine",	            "UA"),
      new TunerCountry(971,	"United Arab Emirates",	"AE"),
      new TunerCountry(44,	"United Kingdom",	    "GB"),
      new TunerCountry(1,	  "United States",	    "US"),
      new TunerCountry(598,	"Uruguay",	            "UY"),
      new TunerCountry(7,	  "Uzbekistan",	        "UZ"),
      new TunerCountry(678,	"Vanuatu",	            "VU"),
      new TunerCountry(39,	"Vatican City State (Holy See)",	"VA"),
      new TunerCountry(58,	"Venezuela",	        "VE"),
      new TunerCountry(84,	"Viet Nam",	            "VN"),
      new TunerCountry(1,	  "Virgin Islands, British",	"VG"),
      new TunerCountry(1,	  "Virgin Islands, U.S.",	"VI"),
      new TunerCountry(681,	"Wallis and Futuna",	"WF"),
      new TunerCountry(967,	"Yemen",	            "YE"),
      new TunerCountry(260,	"Zambia",	            "ZM"),
      new TunerCountry(263,	"Zimbabwe",	            "ZW")
    };
  }
}
