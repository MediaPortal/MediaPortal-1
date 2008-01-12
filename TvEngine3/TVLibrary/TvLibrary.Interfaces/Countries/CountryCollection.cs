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
using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
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
      new Country(93,	"Afghanistan",	        "AF",AnalogVideoStandard.PAL_B),
      new Country(355,	"Albania",	            "AL",AnalogVideoStandard.PAL_B),
      new Country(213,	"Algeria",	            "DZ",AnalogVideoStandard.PAL_B),
      new Country(684,	"American Samoa",	    "AS",AnalogVideoStandard.NTSC_M),
      new Country(376,	"Andorra",	            "AD",AnalogVideoStandard.NTSC_M),
      new Country(244,	"Angola",	            "AO",AnalogVideoStandard.SECAM_K),
      new Country(1,	  "Anguilla",	            "AI",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Antigua And Barbuda",	"AG",AnalogVideoStandard.NTSC_M),
      new Country(54,	"Argentina",	        "AR",AnalogVideoStandard.PAL_N),
      new Country(374,	"Armenia",	            "AM",AnalogVideoStandard.SECAM_D),
      new Country(297,	"Aruba",	            "AW",AnalogVideoStandard.NTSC_M),
      new Country(61,	"Australia",	        "AU",AnalogVideoStandard.PAL_B),
      new Country(43,	"Austria",	            "AT",AnalogVideoStandard.SECAM_K),
      new Country(994,	"Azerbaijan",	        "AZ",AnalogVideoStandard.SECAM_D),
      new Country(1,	  "Bahamas",	            "BS",AnalogVideoStandard.NTSC_M),
      new Country(973,	"Bahrain",	            "BH",AnalogVideoStandard.PAL_B),
      new Country(880,	"Bangladesh",	        "BD",AnalogVideoStandard.PAL_B),
      new Country(1,	  "Barbados",	            "BB",AnalogVideoStandard.NTSC_M),
      new Country(375,	"Belarus",	            "BY",AnalogVideoStandard.SECAM_D),
      new Country(32,	"Belgium",	            "BE",AnalogVideoStandard.PAL_B),
      new Country(501,	"Belize",	            "BZ",AnalogVideoStandard.NTSC_M),
      new Country(229,	"Benin",	            "BJ",AnalogVideoStandard.SECAM_K),
      new Country(1,	  "Bermuda",	            "BM",AnalogVideoStandard.NTSC_M),
      new Country(975,	"Bhutan",	            "BT",AnalogVideoStandard.NTSC_M),
      new Country(591,	"Bolivia",	            "BO",AnalogVideoStandard.PAL_N),
      new Country(387,	"Bosnia And Herzegovina",	"BA",AnalogVideoStandard.PAL_B),
      new Country(267,	"Botswana",	            "BW",AnalogVideoStandard.SECAM_K),
      new Country(55,	"Brazil",	            "BR",AnalogVideoStandard.PAL_B),
      new Country(673,	"Brunei Darussalam",	"BN",AnalogVideoStandard.PAL_B),
      new Country(359,	"Bulgaria",	            "BG",AnalogVideoStandard.SECAM_D),
      new Country(226,	"Burkina Faso",	        "BF",AnalogVideoStandard.SECAM_K),
      new Country(257,	"Burundi",	            "BI",AnalogVideoStandard.SECAM_K),
      new Country(855,	"Cambodia",	            "KH",AnalogVideoStandard.PAL_B),
      new Country(237,	"Cameroon",	            "CM",AnalogVideoStandard.PAL_B),
      new Country(2,	  "Canada",	            "CA",AnalogVideoStandard.NTSC_M),
      new Country(238,	"Cape Verde",	        "CV",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Cayman Islands",	    "KY",AnalogVideoStandard.NTSC_M),
      new Country(236,	"Central African Republic",	"CF",AnalogVideoStandard.PAL_B),
      new Country(235,	"Chad",	                "TD",AnalogVideoStandard.PAL_B),
      new Country(56,	"Chile",	            "CL",AnalogVideoStandard.NTSC_M),
      new Country(86,	"China",	            "CN",AnalogVideoStandard.PAL_D),
      new Country(672,	"Christmas Island",	    "CX",AnalogVideoStandard.NTSC_M),
      new Country(61,	"Cocos (Keeling) Islands",	"CC",AnalogVideoStandard.PAL_B),
      new Country(57,	"Colombia",	            "CO",AnalogVideoStandard.NTSC_M),
      new Country(269,	"Comoros",	            "KM",AnalogVideoStandard.SECAM_K),
      new Country(242,	"Congo",	            "CG",AnalogVideoStandard.SECAM_D),
      new Country(243,	"Congo, The Democratic Republic of The",	"CD",AnalogVideoStandard.SECAM_K),
      new Country(682,	"Cook Islands",	        "CK",AnalogVideoStandard.PAL_B),
      new Country(506,	"Costa Rica",	        "CR",AnalogVideoStandard.NTSC_M),
      new Country(225,	"Côte D'Ivoire",	    "CI",AnalogVideoStandard.SECAM_K),
      new Country(385,	"Croatia",	            "HR",AnalogVideoStandard.PAL_B),
      new Country(53,	"Cuba",	                "CU",AnalogVideoStandard.NTSC_M),
      new Country(357,	"Cyprus",	            "CY",AnalogVideoStandard.PAL_B),
      new Country(420,	"Czech Republic",	    "CZ",AnalogVideoStandard.PAL_D),
      new Country(45,	"Denmark",	            "DK",AnalogVideoStandard.PAL_B),
      new Country(253,	"Djibouti",	            "DJ",AnalogVideoStandard.SECAM_K),
      new Country(1,	  "Dominica",	            "DM",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Dominican Republic",	"DO",AnalogVideoStandard.NTSC_M),
      new Country(593,	"Ecuador",	            "EC",AnalogVideoStandard.NTSC_M),
      new Country(20,	"Egypt",	            "EG",AnalogVideoStandard.SECAM_B),
      new Country(503,	"El Salvador",	        "SV",AnalogVideoStandard.NTSC_M),
      new Country(240,	"Equatorial Guinea",	"GQ",AnalogVideoStandard.SECAM_B),
      new Country(291,	"Eritrea",	            "ER",AnalogVideoStandard.NTSC_M),
      new Country(372,	"Estonia",	            "EE",AnalogVideoStandard.PAL_B),
      new Country(251,	"Ethiopia",	            "ET",AnalogVideoStandard.PAL_B),
      new Country(500,	"Falkland Islands (Malvinas)",	"FK",AnalogVideoStandard.PAL_I),
      new Country(298,	"Faroe Islands",	    "FO",AnalogVideoStandard.PAL_B),
      new Country(679,	"Fiji",	                "FJ",AnalogVideoStandard.NTSC_M),
      new Country(358,	"Finland",	            "FI",AnalogVideoStandard.PAL_B),
      new Country(33,	"France",	            "FR",AnalogVideoStandard.SECAM_L),
      new Country(594,	"French Guiana",	    "GF",AnalogVideoStandard.SECAM_K),
      new Country(689,	"French Polynesia",	    "PF",AnalogVideoStandard.SECAM_K),
      new Country(241,	"Gabon",	            "GA",AnalogVideoStandard.SECAM_K),
      new Country(220,	"Gambia",	            "GM",AnalogVideoStandard.SECAM_K),
      new Country(995,	"Georgia",	            "GE",AnalogVideoStandard.SECAM_D),
      new Country(49,	"Germany",	            "DE",AnalogVideoStandard.PAL_B),
      new Country(233,	"Ghana",	            "GH",AnalogVideoStandard.PAL_B),
      new Country(350,	"Gibraltar",	        "GI",AnalogVideoStandard.PAL_B),
      new Country(30,	"Greece",	            "GR",AnalogVideoStandard.SECAM_B),
      new Country(299,	"Greenland",	        "GL",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Grenada",	            "GD",AnalogVideoStandard.NTSC_M),
      new Country(590,	"Guadeloupe",	        "GP",AnalogVideoStandard.SECAM_K),
      new Country(671,	"Guam",	                "GU",AnalogVideoStandard.NTSC_M),
      new Country(502,	"Guatemala",	        "GT",AnalogVideoStandard.NTSC_M),
      new Country(224,	"Guinea",	            "GN",AnalogVideoStandard.SECAM_K),
      new Country(245,	"Guinea-Bissau",	    "GW",AnalogVideoStandard.NTSC_M),
      new Country(592,	"Guyana",	            "GY",AnalogVideoStandard.SECAM_K),
      new Country(509,	"Haiti",	            "HT",AnalogVideoStandard.NTSC_M),
      new Country(504,	"Honduras",	            "HN",AnalogVideoStandard.NTSC_M),
      new Country(852,	"Hong Kong, SAR",	    "HK",AnalogVideoStandard.PAL_I),
      new Country(36,	"Hungary",	            "HU",AnalogVideoStandard.SECAM_D),
      new Country(354,	"Iceland",	            "IS",AnalogVideoStandard.PAL_B),
      new Country(91,	"India",	            "IN",AnalogVideoStandard.PAL_B),
      new Country(62,	"Indonesia",	        "ID",AnalogVideoStandard.PAL_B),
      new Country(98,	"Iran, Islamic Republic Of",	"IR",AnalogVideoStandard.SECAM_B),
      new Country(964,	"Iraq",	                "IQ",AnalogVideoStandard.SECAM_B),
      new Country(353,	"Ireland",	            "IE",AnalogVideoStandard.PAL_I),
      new Country(972,	"Israel",	            "IL",AnalogVideoStandard.PAL_B),
      new Country(39,	"Italy",	            "IT",AnalogVideoStandard.PAL_B),
      new Country(1,	  "Jamaica",	            "JM",AnalogVideoStandard.NTSC_M),
      new Country(81,	"Japan",	            "JP",AnalogVideoStandard.NTSC_M_J),
      new Country(962,	"Jordan",	            "JO",AnalogVideoStandard.PAL_B),
      new Country(7,	  "Kazakhstan",	        "KZ",AnalogVideoStandard.SECAM_D),
      new Country(254,	"Kenya",	            "KE",AnalogVideoStandard.PAL_B),
      new Country(686,	"Kiribati",	            "KI",AnalogVideoStandard.PAL_B),
      new Country(82,	"Korea",	            "KR",AnalogVideoStandard.NTSC_M),
      new Country(850,	"Korea, Democratic People's Republic of",	"KP",AnalogVideoStandard.SECAM_D),
      new Country(965,	"Kuwait",	            "KW",AnalogVideoStandard.PAL_B),
      new Country(7,	  "Kyrgyzstan",	        "KG",AnalogVideoStandard.SECAM_D),
      new Country(856,	"Lao, People's Democratic Republic",	"LA",AnalogVideoStandard.PAL_B),
      new Country(371,	"Latvia",	            "LV",AnalogVideoStandard.SECAM_D),
      new Country(961,	"Lebanon",	            "LB",AnalogVideoStandard.SECAM_B),
      new Country(266,	"Lesotho",	            "LS",AnalogVideoStandard.PAL_I),
      new Country(231,	"Liberia",	            "LR",AnalogVideoStandard.PAL_B),
      new Country(218,	"Libyan Arab Jamahiriya",	"LY",AnalogVideoStandard.SECAM_B),
      new Country(41,	"Liechtenstein",	    "LI",AnalogVideoStandard.PAL_B),
      new Country(370,	"Lithuania",	        "LT",AnalogVideoStandard.PAL_B),
      new Country(352,	"Luxembourg",	        "LU",AnalogVideoStandard.PAL_B),
      new Country(853,	"Macao, SAR",	        "MO",AnalogVideoStandard.PAL_I),
      new Country(389,	"Macedonia, The Former Yugoslav Republic of",	"MK",AnalogVideoStandard.PAL_B),
      new Country(261,	"Madagascar",	        "MG",AnalogVideoStandard.SECAM_K),
      new Country(265,	"Malawi",	            "MW",AnalogVideoStandard.NTSC_M),
      new Country(60,	"Malaysia",	            "MY",AnalogVideoStandard.PAL_B),
      new Country(960,	"Maldives",	            "MV",AnalogVideoStandard.PAL_B),
      new Country(223,	"Mali",	                "ML",AnalogVideoStandard.PAL_B),
      new Country(356,	"Malta",	            "MT",AnalogVideoStandard.PAL_B),
      new Country(692,	"Marshall Islands",	    "MH",AnalogVideoStandard.NTSC_M),
      new Country(596,	"Martinique",	        "MQ",AnalogVideoStandard.SECAM_K),
      new Country(222,	"Mauritania",	        "MR",AnalogVideoStandard.SECAM_B),
      new Country(230,	"Mauritius",	        "MU",AnalogVideoStandard.SECAM_B),
      new Country(269,	"Mayotte",	            "YT",AnalogVideoStandard.SECAM_K),
      new Country(52,	"Mexico",	            "MX",AnalogVideoStandard.NTSC_M),
      new Country(691,	"Micronesia, Federated States of",	"FM",AnalogVideoStandard.NTSC_M),
      new Country(373,	"Moldova, Republic of",	"MD",AnalogVideoStandard.SECAM_D),
      new Country(377,	"Monaco",	            "MC",AnalogVideoStandard.SECAM_G),
      new Country(976,	"Mongolia",	            "MN",AnalogVideoStandard.SECAM_D),
      new Country(1,	  "Montserrat",	        "MS",AnalogVideoStandard.NTSC_M),
      new Country(212,	"Morocco",	            "MA",AnalogVideoStandard.SECAM_B),
      new Country(258,	"Mozambique",	        "MZ",AnalogVideoStandard.PAL_B),
      new Country(95,	"Myanmar",	            "MM",AnalogVideoStandard.NTSC_M),
      new Country(264,	"Namibia",	            "NA",AnalogVideoStandard.PAL_I),
      new Country(674,	"Nauru",	            "NR",AnalogVideoStandard.NTSC_M),
      new Country(977,	"Nepal",	            "NP",AnalogVideoStandard.PAL_B),
      new Country(31,	"The Netherlands",	        "NL",AnalogVideoStandard.PAL_B),
      new Country(599,	"Netherlands Antilles",	"AN",AnalogVideoStandard.NTSC_M),
      new Country(687,	"New Caledonia",	    "NC",AnalogVideoStandard.SECAM_K),
      new Country(64,	"New Zealand",	        "NZ",AnalogVideoStandard.PAL_B),
      new Country(505,	"Nicaragua",	        "NI",AnalogVideoStandard.NTSC_M),
      new Country(227,	"Niger",	            "NE",AnalogVideoStandard.SECAM_K),
      new Country(234,	"Nigeria",	            "NG",AnalogVideoStandard.PAL_B),
      new Country(683,	"Niue",	                "NU",AnalogVideoStandard.NTSC_M),
      new Country(672,	"Norfolk Island",	    "NF",AnalogVideoStandard.NTSC_M),
      new Country(47,	"Norway",	            "NO",AnalogVideoStandard.PAL_B),
      new Country(968,	"Oman",	                "OM",AnalogVideoStandard.PAL_B),
      new Country(92,	"Pakistan",	            "PK",AnalogVideoStandard.PAL_B),
      new Country(680,	"Palau",	            "PW",AnalogVideoStandard.NTSC_M),
      new Country(507,	"Panama",	            "PA",AnalogVideoStandard.NTSC_M),
      new Country(675,	"Papua New Guinea",	    "PG",AnalogVideoStandard.PAL_B),
      new Country(595,	"Paraguay",	            "PY",AnalogVideoStandard.PAL_N),
      new Country(51,	"Peru",	                "PE",AnalogVideoStandard.NTSC_M),
      new Country(63,	"Philippines",	        "PH",AnalogVideoStandard.NTSC_M),
      new Country(48,	"Poland",	            "PL",AnalogVideoStandard.PAL_B),
      new Country(351,	"Portugal",	            "PT",AnalogVideoStandard.PAL_B),
      new Country(1,	  "Puerto Rico",	        "PR",AnalogVideoStandard.NTSC_M),
      new Country(974,	"Qatar",	            "QA",AnalogVideoStandard.PAL_B),
      new Country(262,	"R‚union",	            "RE",AnalogVideoStandard.SECAM_K),
      new Country(40,	"Romania",	            "RO",AnalogVideoStandard.PAL_D),
      new Country(7,	  "Russian Federation",	"RU",AnalogVideoStandard.SECAM_D),
      new Country(250,	"Rwanda",	            "RW",AnalogVideoStandard.PAL_B),
      new Country(290,	"Saint Helena",	        "SH",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Saint Kitts And Nevis",	"KN",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Saint Lucia",	        "LC",AnalogVideoStandard.NTSC_M),
      new Country(508,	"Saint Pierre And Miquelon",	"PM",AnalogVideoStandard.SECAM_K),
      new Country(1,	  "Saint Vincent And The Grenadines",	"VC",AnalogVideoStandard.NTSC_M),
      new Country(685,	"Samoa",	            "WS",AnalogVideoStandard.PAL_B),
      new Country(378,	"San Marino",	        "SM",AnalogVideoStandard.PAL_B),
      new Country(239,	"Sao Tome And Principe",	"ST",AnalogVideoStandard.PAL_B),
      new Country(966,	"Saudi Arabia",	        "SA",AnalogVideoStandard.SECAM_B),
      new Country(221,	"Senegal",	            "SN",AnalogVideoStandard.SECAM_K),
      new Country(381,	"Serbia And Montenegro",	"CS",AnalogVideoStandard.PAL_B),
      new Country(248,	"Seychelles",	        "SC",AnalogVideoStandard.PAL_B),
      new Country(232,	"Sierra Leone",	        "SL",AnalogVideoStandard.PAL_B),
      new Country(65,	"Singapore",	        "SG",AnalogVideoStandard.PAL_B),
      new Country(421,	"Slovakia",	            "SK",AnalogVideoStandard.PAL_B),
      new Country(386,	"Slovenia",	            "SI",AnalogVideoStandard.PAL_B),
      new Country(677,	"Solomon Islands",	    "SB",AnalogVideoStandard.NTSC_M),
      new Country(252,	"Somalia",	            "SO",AnalogVideoStandard.PAL_B),
      new Country(27,	"South Africa",	        "ZA",AnalogVideoStandard.PAL_I),
      new Country(34,	"Spain",	            "ES",AnalogVideoStandard.PAL_B),
      new Country(94,	"Sri Lanka",	        "LK",AnalogVideoStandard.PAL_B),
      new Country(249,	"Sudan",	            "SD",AnalogVideoStandard.PAL_B),
      new Country(597,	"Suriname",	            "SR",AnalogVideoStandard.NTSC_M),
      new Country(268,	"Swaziland",	        "SZ",AnalogVideoStandard.PAL_B),
      new Country(46,	"Sweden",	            "SE",AnalogVideoStandard.PAL_B),
      new Country(41,	"Switzerland",	        "CH",AnalogVideoStandard.PAL_B),
      new Country(963,	"Syrian Arab Republic",	"SY",AnalogVideoStandard.SECAM_B),
      new Country(886,	"Taiwan",	            "TW",AnalogVideoStandard.NTSC_M),
      new Country(7,	  "Tajikistan",	        "TJ",AnalogVideoStandard.SECAM_D),
      new Country(255,	"Tanzania, United Republic of",	"TZ",AnalogVideoStandard.PAL_B),
      new Country(66,	"Thailand",	            "TH",AnalogVideoStandard.PAL_B),
      new Country(228,	"Togo",	                "TG",AnalogVideoStandard.SECAM_K),
      new Country(690,	"Tokelau",	            "TK",AnalogVideoStandard.NTSC_M),
      new Country(676,	"Tonga",	            "TO",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Trinidad And Tobago",	"TT",AnalogVideoStandard.NTSC_M),
      new Country(216,	"Tunisia",	            "TN",AnalogVideoStandard.SECAM_B),
      new Country(90,	"Turkey",	            "TR",AnalogVideoStandard.PAL_B),
      new Country(7,	  "Turkmenistan",	        "TM",AnalogVideoStandard.SECAM_D),
      new Country(1,	  "Turks And Caicos Islands",	"TC",AnalogVideoStandard.NTSC_M),
      new Country(688,	"Tuvalu",	            "TV",AnalogVideoStandard.NTSC_M),
      new Country(256,	"Uganda",	            "UG",AnalogVideoStandard.PAL_B),
      new Country(380,	"Ukraine",	            "UA",AnalogVideoStandard.SECAM_D),
      new Country(971,	"United Arab Emirates",	"AE",AnalogVideoStandard.PAL_B),
      new Country(44,	"United Kingdom",	    "GB",AnalogVideoStandard.PAL_I),
      new Country(1,	  "United States",	    "US",AnalogVideoStandard.NTSC_M),
      new Country(598,	"Uruguay",	            "UY",AnalogVideoStandard.PAL_N),
      new Country(7,	  "Uzbekistan",	        "UZ",AnalogVideoStandard.SECAM_D),
      new Country(678,	"Vanuatu",	            "VU",AnalogVideoStandard.NTSC_M),
      new Country(39,	"Vatican City State (Holy See)",	"VA",AnalogVideoStandard.PAL_B),
      new Country(58,	"Venezuela",	        "VE",AnalogVideoStandard.NTSC_M),
      new Country(84,	"Viet Nam",	            "VN",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Virgin Islands, British",	"VG",AnalogVideoStandard.NTSC_M),
      new Country(1,	  "Virgin Islands, U.S.",	"VI",AnalogVideoStandard.NTSC_M),
      new Country(681,	"Wallis and Futuna",	"WF",AnalogVideoStandard.SECAM_K),
      new Country(967,	"Yemen",	            "YE",AnalogVideoStandard.PAL_B),
      new Country(260,	"Zambia",	            "ZM",AnalogVideoStandard.PAL_B),
      new Country(263,	"Zimbabwe",	            "ZW",AnalogVideoStandard.PAL_B),
    };
    #endregion

    public CountryCollection()
    {
      int index = 0;
      foreach (Country country in _countryList)
      {
        country.Index = index++;
      }
    }
    /// <summary>
    /// Returns a country specified by name
    /// </summary>
    /// <param name="countryName">name of country</param>
    /// <returns><see cref="T:TvLibrary.Country"/> object or null if country is not found</returns>
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
    /// <param name="countryId">id of country</param>
    /// <returns><see cref="T:TvLibrary.Country"/>  object or null if country is not found</returns>
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
    /// <param name="code">country code</param>
    /// <returns><see cref="T:TvLibrary.Country"/>  object or null if country is not found</returns>
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

