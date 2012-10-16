using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface ICardService
  {
    IList<Card> ListAllCards();
    IList<Card> ListAllCards(CardIncludeRelationEnum includeRelations);
    IList<Card> SaveCards(IEnumerable<Card> cards);
    Card GetCardByDevicePath(string cardDevice);
    Card GetCardByDevicePath(string cardDevice, CardIncludeRelationEnum includeRelations);
    Card SaveCard(Card card);
    void DeleteCard(int idCard);
    DisEqcMotor SaveDisEqcMotor(DisEqcMotor motor);
    Card GetCard(int idCard);
    Card GetCard(int idCard, CardIncludeRelationEnum includeRelations);
    CardGroup SaveCardGroup(CardGroup @group);
    void DeleteCardGroup(int idCardGroup);
    IList<CardGroup> ListAllCardGroups();
    IList<SoftwareEncoder> ListAllSofwareEncodersVideo();
    IList<SoftwareEncoder> ListAllSofwareEncodersAudio();
    IList<Satellite> ListAllSatellites();
    Satellite SaveSatellite(Satellite satellite);
    SoftwareEncoder SaveSoftwareEncoder(SoftwareEncoder encoder);
    void DeleteGroupMap(int idMap);
    CardGroupMap SaveCardGroupMap(CardGroupMap map);
    LnbType GetLnbType(int idLnbType);
    IList<LnbType> ListAllLnbTypes();
  }
}
