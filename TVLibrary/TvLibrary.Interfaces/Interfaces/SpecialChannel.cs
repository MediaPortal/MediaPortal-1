using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces.Interfaces
{
  /// <summary>
  /// Represents special tv channels
  /// </summary>
  public class SpecialChannel
  {
    #region variables
    public string Name;
    public long Frequency;
    public int Number;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialChannel"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="frequency">The frequency.</param>
    /// <param name="number">The number.</param>
    public SpecialChannel(string name, long frequency, int number)
    {
      Name = name;
      Frequency = frequency;
      Number = number;
    }
    #endregion

    /// <summary>
    /// All special channels
    /// </summary>
    public static SpecialChannel[] SpecialChannels = 
		{
			new SpecialChannel("K2",48250000L,2),
			new SpecialChannel("K3",55250000L,3),
			new SpecialChannel("K4",62250000L,4),
			new SpecialChannel("S1",105250000L,1),
			new SpecialChannel("S2",112250000L,2),
			new SpecialChannel("S3",119250000L,3),
			new SpecialChannel("S4",126250000L,4),
			new SpecialChannel("S5",133250000L,5),
			new SpecialChannel("S6",140250000L,6),
			new SpecialChannel("S7",147250000L,7),
			new SpecialChannel("S8",154250000L,8),
			new SpecialChannel("S9",161250000L,9),
			new SpecialChannel("S10",168250000L,10),
			new SpecialChannel("K5",175250000L,5),
			new SpecialChannel("K6",182250000L,6),
			new SpecialChannel("K7",189250000L,7),
			new SpecialChannel("K8",196250000L,8),
			new SpecialChannel("K9",203250000L,9),
			new SpecialChannel("K10",210250000L,10),
			new SpecialChannel("K11",217250000L,11),
			new SpecialChannel("K12",224250000L,12),
			new SpecialChannel("S11",231250000L,11),
			new SpecialChannel("S12",238250000L,12),
			new SpecialChannel("S13",245250000L,13),
			new SpecialChannel("S14",252250000L,14),
			new SpecialChannel("S15",259250000L,15),
			new SpecialChannel("S16",266250000L,16),
			new SpecialChannel("S17",273250000L,17),
			new SpecialChannel("S18",280250000L,18),
			new SpecialChannel("S19",287250000L,19),
			new SpecialChannel("S20",294250000L,20),
			new SpecialChannel("S21",303250000L,21),
			new SpecialChannel("S22",311250000L,22),
			new SpecialChannel("S23",319250000L,23),
			new SpecialChannel("S24",327250000L,24),
			new SpecialChannel("S25",335250000L,25),
			new SpecialChannel("S26",343250000L,26),
			new SpecialChannel("S27",351250000L,27),
			new SpecialChannel("S28",359250000L,28),
			new SpecialChannel("S29",367250000L,29),
			new SpecialChannel("S30",375250000L,30),
			new SpecialChannel("S31",383250000L,31),
			new SpecialChannel("S32",391250000L,32),
			new SpecialChannel("S33",399250000L,33),
			new SpecialChannel("S34",407250000L,34),
			new SpecialChannel("S35",415250000L,35),
			new SpecialChannel("S36",423250000L,36),
			new SpecialChannel("S37",431250000L,37),
			new SpecialChannel("S38",439250000L,38),
			new SpecialChannel("S39",447250000L,39),
			new SpecialChannel("S40",455250000L,40),
			new SpecialChannel("S41",463250000L,41),
			new SpecialChannel("K21",471250000L,21),
			new SpecialChannel("K22",479250000L,22),
			new SpecialChannel("K23",487250000L,23),
			new SpecialChannel("K24",495250000L,24),
			new SpecialChannel("K25",503250000L,25),
			new SpecialChannel("K26",511250000L,26),
			new SpecialChannel("K27",519250000L,27),
			new SpecialChannel("K28",527250000L,28),
			new SpecialChannel("K29",535250000L,29),
			new SpecialChannel("K30",543250000L,30),
			new SpecialChannel("K31",551250000L,31),
			new SpecialChannel("K32",559250000L,32),
			new SpecialChannel("K33",567250000L,33),
			new SpecialChannel("K34",575250000L,34),
			new SpecialChannel("K35",583250000L,35),
			new SpecialChannel("K36",591250000L,36),
			new SpecialChannel("K37",599250000L,37),
			new SpecialChannel("K38",607250000L,38),
			new SpecialChannel("K39",615250000L,39),
			new SpecialChannel("K40",623250000L,40),
			new SpecialChannel("K41",631250000L,41),
			new SpecialChannel("K42",639250000L,42),
			new SpecialChannel("K43",647250000L,43),
			new SpecialChannel("K44",655250000L,44),
			new SpecialChannel("K45",663250000L,45),
			new SpecialChannel("K46",671250000L,46),
			new SpecialChannel("K47",679250000L,47),
			new SpecialChannel("K48",687250000L,48),
			new SpecialChannel("K49",695250000L,49),
			new SpecialChannel("K50",703250000L,50),
			new SpecialChannel("K51",711250000L,51),
			new SpecialChannel("K52",719250000L,52),
			new SpecialChannel("K53",727250000L,53),
			new SpecialChannel("K54",735250000L,54),
			new SpecialChannel("K55",743250000L,55),
			new SpecialChannel("K56",751250000L,56),
			new SpecialChannel("K57",759250000L,57),
			new SpecialChannel("K58",767250000L,58),
			new SpecialChannel("K59",775250000L,59),
			new SpecialChannel("K60",783250000L,60),
			new SpecialChannel("K61",791250000L,61),
			new SpecialChannel("K62",799250000L,62),
			new SpecialChannel("K63",807250000L,63),
			new SpecialChannel("K64",815250000L,64),
			new SpecialChannel("K65",823250000L,65),
			new SpecialChannel("K66",831250000L,66),
			new SpecialChannel("K67",839250000L,67),
			new SpecialChannel("K68",847250000L,68),
			new SpecialChannel("K69",855250000L,69),
		};
  }

}
