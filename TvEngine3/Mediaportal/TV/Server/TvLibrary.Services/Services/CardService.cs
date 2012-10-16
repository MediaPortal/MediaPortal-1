using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class CardService : ICardService
  {    

    public IList<Card> ListAllCards()
    {
      var listAllCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards();
      return listAllCards;
    }

    public IList<Card> ListAllCards(CardIncludeRelationEnum includeRelations)
    {
      var listAllCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(includeRelations);
      return listAllCards;
    }

    public IList<Card> SaveCards(IEnumerable<Card> cards)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveCards(cards);
    }

    public Card GetCardByDevicePath(string cardDevice)
    {
      var cardByDevicePath = TVDatabase.TVBusinessLayer.CardManagement.GetCardByDevicePath(cardDevice);
      return cardByDevicePath;
    }

    public Card GetCardByDevicePath(string cardDevice, CardIncludeRelationEnum includeRelations)
    {
      var cardByDevicePath = TVDatabase.TVBusinessLayer.CardManagement.GetCardByDevicePath(cardDevice, includeRelations);
      return cardByDevicePath;
    }

    public Card SaveCard(Card card)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveCard(card);
    }

    public void DeleteCard(int idCard)
    {
      TVDatabase.TVBusinessLayer.CardManagement.DeleteCard(idCard);
    }

    public DisEqcMotor SaveDisEqcMotor(DisEqcMotor motor)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveDisEqcMotor(motor);
    }

    public Card GetCard(int idCard)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.GetCard(idCard);
    }

    public Card GetCard(int idCard, CardIncludeRelationEnum includeRelations)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.GetCard(idCard, includeRelations);
    }

    public CardGroup SaveCardGroup(CardGroup @group)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveCardGroup(@group);
    }

    public void DeleteCardGroup(int idCardGroup)
    {
      TVDatabase.TVBusinessLayer.CardManagement.DeleteCardGroup(idCardGroup);
    }

    public IList<CardGroup> ListAllCardGroups()
    {
      return TVDatabase.TVBusinessLayer.CardManagement.ListAllCardGroups();
    }

    public IList<SoftwareEncoder> ListAllSofwareEncodersVideo()
    {
      return TVDatabase.TVBusinessLayer.CardManagement.ListAllSofwareEncodersVideo();
    }
    public IList<SoftwareEncoder> ListAllSofwareEncodersAudio()
    {
      return TVDatabase.TVBusinessLayer.CardManagement.ListAllSofwareEncodersAudio();
    }

    public IList<Satellite> ListAllSatellites()
    {
      return TVDatabase.TVBusinessLayer.CardManagement.ListAllSatellites();
    }

    public Satellite SaveSatellite(Satellite satellite)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveSatellite(satellite);
    }

    public SoftwareEncoder SaveSoftwareEncoder(SoftwareEncoder encoder)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveSoftwareEncoder(encoder);
    }

    public void DeleteGroupMap(int idMap)
    {
      TVDatabase.TVBusinessLayer.CardManagement.DeleteGroupMap(idMap);
    }

    public CardGroupMap SaveCardGroupMap(CardGroupMap map)
    {
      return TVDatabase.TVBusinessLayer.CardManagement.SaveCardGroupMap(map);
    }

    
    public LnbType GetLnbType(int idLnbType)
    {
      return LnbTypeManagement.GetLnbType(idLnbType);
    }

    public IList<LnbType> ListAllLnbTypes()
    {
      return LnbTypeManagement.ListAllLnbTypes();
    }
  }
}
