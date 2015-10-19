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
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.Common.Types.Country
{
  /// <summary>
  /// A collection of all countries.
  /// </summary>
  [Serializable]
  public class CountryCollection
  {
    #region countries

    private readonly Country[] _countries = new Country[]
    {
      new Country(93, "Afghanistan", "AF", AnalogVideoStandard.PalB),
      new Country(355, "Albania", "AL", AnalogVideoStandard.PalB),
      new Country(213, "Algeria", "DZ", AnalogVideoStandard.PalB),
      new Country(684, "American Samoa", "AS", AnalogVideoStandard.NtscM),
      new Country(376, "Andorra", "AD", AnalogVideoStandard.NtscM),
      new Country(244, "Angola", "AO", AnalogVideoStandard.SecamK),
      new Country(1, "Anguilla", "AI", AnalogVideoStandard.NtscM),
      new Country(1, "Antigua And Barbuda", "AG", AnalogVideoStandard.NtscM),
      new Country(54, "Argentina", "AR", AnalogVideoStandard.PalN),
      new Country(374, "Armenia", "AM", AnalogVideoStandard.SecamD),
      new Country(297, "Aruba", "AW", AnalogVideoStandard.NtscM),
      new Country(61, "Australia", "AU", AnalogVideoStandard.PalB),
      new Country(43, "Austria", "AT", AnalogVideoStandard.SecamK),
      new Country(994, "Azerbaijan", "AZ", AnalogVideoStandard.SecamD),
      new Country(1, "Bahamas", "BS", AnalogVideoStandard.NtscM),
      new Country(973, "Bahrain", "BH", AnalogVideoStandard.PalB),
      new Country(880, "Bangladesh", "BD", AnalogVideoStandard.PalB),
      new Country(1, "Barbados", "BB", AnalogVideoStandard.NtscM),
      new Country(375, "Belarus", "BY", AnalogVideoStandard.SecamD),
      new Country(32, "Belgium", "BE", AnalogVideoStandard.PalB),
      new Country(501, "Belize", "BZ", AnalogVideoStandard.NtscM),
      new Country(229, "Benin", "BJ", AnalogVideoStandard.SecamK),
      new Country(1, "Bermuda", "BM", AnalogVideoStandard.NtscM),
      new Country(975, "Bhutan", "BT", AnalogVideoStandard.NtscM),
      new Country(591, "Bolivia", "BO", AnalogVideoStandard.PalN),
      new Country(387, "Bosnia And Herzegovina", "BA", AnalogVideoStandard.PalB),
      new Country(267, "Botswana", "BW", AnalogVideoStandard.SecamK),
      new Country(55, "Brazil", "BR", AnalogVideoStandard.PalB),
      new Country(673, "Brunei Darussalam", "BN", AnalogVideoStandard.PalB),
      new Country(359, "Bulgaria", "BG", AnalogVideoStandard.SecamD),
      new Country(226, "Burkina Faso", "BF", AnalogVideoStandard.SecamK),
      new Country(257, "Burundi", "BI", AnalogVideoStandard.SecamK),
      new Country(855, "Cambodia", "KH", AnalogVideoStandard.PalB),
      new Country(237, "Cameroon", "CM", AnalogVideoStandard.PalB),
      new Country(2, "Canada", "CA", AnalogVideoStandard.NtscM),
      new Country(238, "Cape Verde", "CV", AnalogVideoStandard.NtscM),
      new Country(1, "Cayman Islands", "KY", AnalogVideoStandard.NtscM),
      new Country(236, "Central African Republic", "CF", AnalogVideoStandard.PalB),
      new Country(235, "Chad", "TD", AnalogVideoStandard.PalB),
      new Country(56, "Chile", "CL", AnalogVideoStandard.NtscM),
      new Country(86, "China", "CN", AnalogVideoStandard.PalD),
      new Country(672, "Christmas Island", "CX", AnalogVideoStandard.NtscM),
      new Country(61, "Cocos (Keeling) Islands", "CC", AnalogVideoStandard.PalB),
      new Country(57, "Colombia", "CO", AnalogVideoStandard.NtscM),
      new Country(269, "Comoros", "KM", AnalogVideoStandard.SecamK),
      new Country(242, "Congo", "CG", AnalogVideoStandard.SecamD),
      new Country(243, "Congo, The Democratic Republic of The", "CD", AnalogVideoStandard.SecamK),
      new Country(682, "Cook Islands", "CK", AnalogVideoStandard.PalB),
      new Country(506, "Costa Rica", "CR", AnalogVideoStandard.NtscM),
      new Country(225, "Côte D'Ivoire", "CI", AnalogVideoStandard.SecamK),
      new Country(385, "Croatia", "HR", AnalogVideoStandard.PalB),
      new Country(53, "Cuba", "CU", AnalogVideoStandard.NtscM),
      new Country(357, "Cyprus", "CY", AnalogVideoStandard.PalB),
      new Country(420, "Czech Republic", "CZ", AnalogVideoStandard.PalD),
      new Country(45, "Denmark", "DK", AnalogVideoStandard.PalB),
      new Country(253, "Djibouti", "DJ", AnalogVideoStandard.SecamK),
      new Country(1, "Dominica", "DM", AnalogVideoStandard.NtscM),
      new Country(1, "Dominican Republic", "DO", AnalogVideoStandard.NtscM),
      new Country(593, "Ecuador", "EC", AnalogVideoStandard.NtscM),
      new Country(20, "Egypt", "EG", AnalogVideoStandard.SecamB),
      new Country(503, "El Salvador", "SV", AnalogVideoStandard.NtscM),
      new Country(240, "Equatorial Guinea", "GQ", AnalogVideoStandard.SecamB),
      new Country(291, "Eritrea", "ER", AnalogVideoStandard.NtscM),
      new Country(372, "Estonia", "EE", AnalogVideoStandard.PalB),
      new Country(251, "Ethiopia", "ET", AnalogVideoStandard.PalB),
      new Country(500, "Falkland Islands (Malvinas)", "FK", AnalogVideoStandard.PalI),
      new Country(298, "Faroe Islands", "FO", AnalogVideoStandard.PalB),
      new Country(679, "Fiji", "FJ", AnalogVideoStandard.NtscM),
      new Country(358, "Finland", "FI", AnalogVideoStandard.PalB),
      new Country(33, "France", "FR", AnalogVideoStandard.SecamL),
      new Country(594, "French Guiana", "GF", AnalogVideoStandard.SecamK),
      new Country(689, "French Polynesia", "PF", AnalogVideoStandard.SecamK),
      new Country(241, "Gabon", "GA", AnalogVideoStandard.SecamK),
      new Country(220, "Gambia", "GM", AnalogVideoStandard.SecamK),
      new Country(995, "Georgia", "GE", AnalogVideoStandard.SecamD),
      new Country(49, "Germany", "DE", AnalogVideoStandard.PalB),
      new Country(233, "Ghana", "GH", AnalogVideoStandard.PalB),
      new Country(350, "Gibraltar", "GI", AnalogVideoStandard.PalB),
      new Country(30, "Greece", "GR", AnalogVideoStandard.SecamB),
      new Country(299, "Greenland", "GL", AnalogVideoStandard.NtscM),
      new Country(1, "Grenada", "GD", AnalogVideoStandard.NtscM),
      new Country(590, "Guadeloupe", "GP", AnalogVideoStandard.SecamK),
      new Country(671, "Guam", "GU", AnalogVideoStandard.NtscM),
      new Country(502, "Guatemala", "GT", AnalogVideoStandard.NtscM),
      new Country(224, "Guinea", "GN", AnalogVideoStandard.SecamK),
      new Country(245, "Guinea-Bissau", "GW", AnalogVideoStandard.NtscM),
      new Country(592, "Guyana", "GY", AnalogVideoStandard.SecamK),
      new Country(509, "Haiti", "HT", AnalogVideoStandard.NtscM),
      new Country(504, "Honduras", "HN", AnalogVideoStandard.NtscM),
      new Country(852, "Hong Kong, SAR", "HK", AnalogVideoStandard.PalI),
      new Country(36, "Hungary", "HU", AnalogVideoStandard.SecamD),
      new Country(354, "Iceland", "IS", AnalogVideoStandard.PalB),
      new Country(91, "India", "IN", AnalogVideoStandard.PalB),
      new Country(62, "Indonesia", "ID", AnalogVideoStandard.PalB),
      new Country(98, "Iran, Islamic Republic Of", "IR", AnalogVideoStandard.SecamB),
      new Country(964, "Iraq", "IQ", AnalogVideoStandard.SecamB),
      new Country(353, "Ireland", "IE", AnalogVideoStandard.PalI),
      new Country(972, "Israel", "IL", AnalogVideoStandard.PalB),
      new Country(39, "Italy", "IT", AnalogVideoStandard.PalB),
      new Country(1, "Jamaica", "JM", AnalogVideoStandard.NtscM),
      new Country(81, "Japan", "JP", AnalogVideoStandard.NtscMj),
      new Country(962, "Jordan", "JO", AnalogVideoStandard.PalB),
      new Country(7, "Kazakhstan", "KZ", AnalogVideoStandard.SecamD),
      new Country(254, "Kenya", "KE", AnalogVideoStandard.PalB),
      new Country(686, "Kiribati", "KI", AnalogVideoStandard.PalB),
      new Country(82, "Korea", "KR", AnalogVideoStandard.NtscM),
      new Country(850, "Korea, Democratic People's Republic of", "KP", AnalogVideoStandard.SecamD),
      new Country(965, "Kuwait", "KW", AnalogVideoStandard.PalB),
      new Country(7, "Kyrgyzstan", "KG", AnalogVideoStandard.SecamD),
      new Country(856, "Lao, People's Democratic Republic", "LA", AnalogVideoStandard.PalB),
      new Country(371, "Latvia", "LV", AnalogVideoStandard.SecamD),
      new Country(961, "Lebanon", "LB", AnalogVideoStandard.SecamB),
      new Country(266, "Lesotho", "LS", AnalogVideoStandard.PalI),
      new Country(231, "Liberia", "LR", AnalogVideoStandard.PalB),
      new Country(218, "Libyan Arab Jamahiriya", "LY", AnalogVideoStandard.SecamB),
      new Country(41, "Liechtenstein", "LI", AnalogVideoStandard.PalB),
      new Country(370, "Lithuania", "LT", AnalogVideoStandard.PalB),
      new Country(352, "Luxembourg", "LU", AnalogVideoStandard.PalB),
      new Country(853, "Macao, SAR", "MO", AnalogVideoStandard.PalI),
      new Country(389, "Macedonia, The Former Yugoslav Republic of", "MK", AnalogVideoStandard.PalB),
      new Country(261, "Madagascar", "MG", AnalogVideoStandard.SecamK),
      new Country(265, "Malawi", "MW", AnalogVideoStandard.NtscM),
      new Country(60, "Malaysia", "MY", AnalogVideoStandard.PalB),
      new Country(960, "Maldives", "MV", AnalogVideoStandard.PalB),
      new Country(223, "Mali", "ML", AnalogVideoStandard.PalB),
      new Country(356, "Malta", "MT", AnalogVideoStandard.PalB),
      new Country(692, "Marshall Islands", "MH", AnalogVideoStandard.NtscM),
      new Country(596, "Martinique", "MQ", AnalogVideoStandard.SecamK),
      new Country(222, "Mauritania", "MR", AnalogVideoStandard.SecamB),
      new Country(230, "Mauritius", "MU", AnalogVideoStandard.SecamB),
      new Country(269, "Mayotte", "YT", AnalogVideoStandard.SecamK),
      new Country(52, "Mexico", "MX", AnalogVideoStandard.NtscM),
      new Country(691, "Micronesia, Federated States of", "FM", AnalogVideoStandard.NtscM),
      new Country(373, "Moldova, Republic of", "MD", AnalogVideoStandard.SecamD),
      new Country(377, "Monaco", "MC", AnalogVideoStandard.SecamG),
      new Country(976, "Mongolia", "MN", AnalogVideoStandard.SecamD),
      new Country(1, "Montserrat", "MS", AnalogVideoStandard.NtscM),
      new Country(212, "Morocco", "MA", AnalogVideoStandard.SecamB),
      new Country(258, "Mozambique", "MZ", AnalogVideoStandard.PalB),
      new Country(95, "Myanmar", "MM", AnalogVideoStandard.NtscM),
      new Country(264, "Namibia", "NA", AnalogVideoStandard.PalI),
      new Country(674, "Nauru", "NR", AnalogVideoStandard.NtscM),
      new Country(977, "Nepal", "NP", AnalogVideoStandard.PalB),
      new Country(31, "The Netherlands", "NL", AnalogVideoStandard.PalB),
      new Country(599, "Netherlands Antilles", "AN", AnalogVideoStandard.NtscM),
      new Country(687, "New Caledonia", "NC", AnalogVideoStandard.SecamK),
      new Country(64, "New Zealand", "NZ", AnalogVideoStandard.PalB),
      new Country(505, "Nicaragua", "NI", AnalogVideoStandard.NtscM),
      new Country(227, "Niger", "NE", AnalogVideoStandard.SecamK),
      new Country(234, "Nigeria", "NG", AnalogVideoStandard.PalB),
      new Country(683, "Niue", "NU", AnalogVideoStandard.NtscM),
      new Country(672, "Norfolk Island", "NF", AnalogVideoStandard.NtscM),
      new Country(47, "Norway", "NO", AnalogVideoStandard.PalB),
      new Country(968, "Oman", "OM", AnalogVideoStandard.PalB),
      new Country(92, "Pakistan", "PK", AnalogVideoStandard.PalB),
      new Country(680, "Palau", "PW", AnalogVideoStandard.NtscM),
      new Country(507, "Panama", "PA", AnalogVideoStandard.NtscM),
      new Country(675, "Papua New Guinea", "PG", AnalogVideoStandard.PalB),
      new Country(595, "Paraguay", "PY", AnalogVideoStandard.PalN),
      new Country(51, "Peru", "PE", AnalogVideoStandard.NtscM),
      new Country(63, "Philippines", "PH", AnalogVideoStandard.NtscM),
      new Country(48, "Poland", "PL", AnalogVideoStandard.PalB),
      new Country(351, "Portugal", "PT", AnalogVideoStandard.PalB),
      new Country(1, "Puerto Rico", "PR", AnalogVideoStandard.NtscM),
      new Country(974, "Qatar", "QA", AnalogVideoStandard.PalB),
      new Country(262, "R‚union", "RE", AnalogVideoStandard.SecamK),
      new Country(40, "Romania", "RO", AnalogVideoStandard.PalD),
      new Country(7, "Russian Federation", "RU", AnalogVideoStandard.SecamD),
      new Country(250, "Rwanda", "RW", AnalogVideoStandard.PalB),
      new Country(290, "Saint Helena", "SH", AnalogVideoStandard.NtscM),
      new Country(1, "Saint Kitts And Nevis", "KN", AnalogVideoStandard.NtscM),
      new Country(1, "Saint Lucia", "LC", AnalogVideoStandard.NtscM),
      new Country(508, "Saint Pierre And Miquelon", "PM", AnalogVideoStandard.SecamK),
      new Country(1, "Saint Vincent And The Grenadines", "VC", AnalogVideoStandard.NtscM),
      new Country(685, "Samoa", "WS", AnalogVideoStandard.PalB),
      new Country(378, "San Marino", "SM", AnalogVideoStandard.PalB),
      new Country(239, "Sao Tome And Principe", "ST", AnalogVideoStandard.PalB),
      new Country(966, "Saudi Arabia", "SA", AnalogVideoStandard.SecamB),
      new Country(221, "Senegal", "SN", AnalogVideoStandard.SecamK),
      new Country(381, "Serbia And Montenegro", "CS", AnalogVideoStandard.PalB),
      new Country(248, "Seychelles", "SC", AnalogVideoStandard.PalB),
      new Country(232, "Sierra Leone", "SL", AnalogVideoStandard.PalB),
      new Country(65, "Singapore", "SG", AnalogVideoStandard.PalB),
      new Country(421, "Slovakia", "SK", AnalogVideoStandard.PalB),
      new Country(386, "Slovenia", "SI", AnalogVideoStandard.PalB),
      new Country(677, "Solomon Islands", "SB", AnalogVideoStandard.NtscM),
      new Country(252, "Somalia", "SO", AnalogVideoStandard.PalB),
      new Country(27, "South Africa", "ZA", AnalogVideoStandard.PalI),
      new Country(34, "Spain", "ES", AnalogVideoStandard.PalB),
      new Country(94, "Sri Lanka", "LK", AnalogVideoStandard.PalB),
      new Country(249, "Sudan", "SD", AnalogVideoStandard.PalB),
      new Country(597, "Suriname", "SR", AnalogVideoStandard.NtscM),
      new Country(268, "Swaziland", "SZ", AnalogVideoStandard.PalB),
      new Country(46, "Sweden", "SE", AnalogVideoStandard.PalB),
      new Country(41, "Switzerland", "CH", AnalogVideoStandard.PalB),
      new Country(963, "Syrian Arab Republic", "SY", AnalogVideoStandard.SecamB),
      new Country(886, "Taiwan", "TW", AnalogVideoStandard.NtscM),
      new Country(7, "Tajikistan", "TJ", AnalogVideoStandard.SecamD),
      new Country(255, "Tanzania, United Republic of", "TZ", AnalogVideoStandard.PalB),
      new Country(66, "Thailand", "TH", AnalogVideoStandard.PalB),
      new Country(228, "Togo", "TG", AnalogVideoStandard.SecamK),
      new Country(690, "Tokelau", "TK", AnalogVideoStandard.NtscM),
      new Country(676, "Tonga", "TO", AnalogVideoStandard.NtscM),
      new Country(1, "Trinidad And Tobago", "TT", AnalogVideoStandard.NtscM),
      new Country(216, "Tunisia", "TN", AnalogVideoStandard.SecamB),
      new Country(90, "Turkey", "TR", AnalogVideoStandard.PalB),
      new Country(7, "Turkmenistan", "TM", AnalogVideoStandard.SecamD),
      new Country(1, "Turks And Caicos Islands", "TC", AnalogVideoStandard.NtscM),
      new Country(688, "Tuvalu", "TV", AnalogVideoStandard.NtscM),
      new Country(256, "Uganda", "UG", AnalogVideoStandard.PalB),
      new Country(380, "Ukraine", "UA", AnalogVideoStandard.SecamD),
      new Country(971, "United Arab Emirates", "AE", AnalogVideoStandard.PalB),
      new Country(44, "United Kingdom", "GB", AnalogVideoStandard.PalI),
      new Country(1, "United States", "US", AnalogVideoStandard.NtscM),
      new Country(598, "Uruguay", "UY", AnalogVideoStandard.PalN),
      new Country(7, "Uzbekistan", "UZ", AnalogVideoStandard.SecamD),
      new Country(678, "Vanuatu", "VU", AnalogVideoStandard.NtscM),
      new Country(39, "Vatican City State (Holy See)", "VA", AnalogVideoStandard.PalB),
      new Country(58, "Venezuela", "VE", AnalogVideoStandard.NtscM),
      new Country(84, "Viet Nam", "VN", AnalogVideoStandard.NtscM),
      new Country(1, "Virgin Islands, British", "VG", AnalogVideoStandard.NtscM),
      new Country(1, "Virgin Islands, U.S.", "VI", AnalogVideoStandard.NtscM),
      new Country(681, "Wallis and Futuna", "WF", AnalogVideoStandard.SecamK),
      new Country(967, "Yemen", "YE", AnalogVideoStandard.PalB),
      new Country(260, "Zambia", "ZM", AnalogVideoStandard.PalB),
      new Country(263, "Zimbabwe", "ZW", AnalogVideoStandard.PalB)
    };

    #endregion

    private static CountryCollection _countryCollection = new CountryCollection();

    /// <summary>
    /// Default constructor
    /// </summary>
    private CountryCollection()
    {
      int index = 0;
      foreach (Country country in _countries)
      {
        country.Id = index++;
      }
    }

    /// <summary>
    /// Get the single country collection instance.
    /// </summary>
    public static CountryCollection Instance
    {
      get
      {
        return _countryCollection;
      }
    }

    /// <summary>
    /// Get the country associated with a given name.
    /// </summary>
    /// <param name="name">The name of the country.</param>
    /// <returns>a <see cref="T:Mediaportal.TV.Server.TVLibrary.Interfaces.Countries.Country"/> object, or null if a country with the given name is not found</returns>
    public Country GetCountryByName(string name)
    {
      foreach (Country country in _countries)
      {
        if (country.Name.Equals(name))
        {
          return country;
        }
      }
      return null;
    }

    /// <summary>
    /// Get the country associated with a given ITU code.
    /// </summary>
    /// <param name="ituCode">The country's ITU E.123/164 code.</param>
    /// <returns>a <see cref="T:Mediaportal.TV.Server.TVLibrary.Interfaces.Countries.Country"/> object, or null if a country with the given ITU code is not found</returns>
    public Country GetCountryByItuCode(int ituCode)
    {
      foreach (Country country in _countries)
      {
        if (country.ItuCode == ituCode)
        {
          return country;
        }
      }
      return null;
    }

    /// <summary>
    /// Get the country associated with a given ISO code.
    /// </summary>
    /// <param name="isoCode">The country's ISO code.</param>
    /// <returns>a <see cref="T:Mediaportal.TV.Server.TVLibrary.Interfaces.Countries.Country"/> object, or null if a country with the given ISO code is not found</returns>
    public Country GetCountryByIsoCode(string isoCode)
    {
      foreach (Country country in _countries)
      {
        if (country.IsoCode == isoCode)
        {
          return country;
        }
      }
      return null;
    }

    /// <summary>
    /// Get the country associated with a MediaPortal country identifier.
    /// </summary>
    /// <param name="id">The identifier that MediaPortal uses for the country.</param>
    /// <returns>a <see cref="T:Mediaportal.TV.Server.TVLibrary.Interfaces.Countries.Country"/> object, or null if a country with the given identifier is not found</returns>
    public Country GetCountryById(int id)
    {
      try
      {
        return _countries[id];
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    /// Get an array of all countries that MediaPortal knows about.
    /// </summary>
    public Country[] Countries
    {
      get
      {
        return _countries;
      }
    }
  }
}