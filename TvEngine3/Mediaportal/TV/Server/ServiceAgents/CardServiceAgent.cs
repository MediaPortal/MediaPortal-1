using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class CardServiceAgent : ServiceAgent<ICardService>, ICardService
  {
    public CardServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Card> ListAllCards()
    {
      return _channel.ListAllCards();
    }

    public Card GetCardByDevicePath(string cardDevice)
    {
      return _channel.GetCardByDevicePath(cardDevice);
    }

    public Card SaveCard(Card card)
    {
      card.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveCard(card);
    }

    public void DeleteCard(int idCard)
    {
      _channel.DeleteCard(idCard);
    }

    public DisEqcMotor SaveDisEqcMotor(DisEqcMotor motor)
    {
      motor.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveDisEqcMotor(motor);
    }

    public Card GetCard(int idCard)
    {
      return _channel.GetCard(idCard);
    }

    public CardGroup SaveCardGroup(CardGroup @group)
    {
      @group.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveCardGroup(@group);
    }

    public void DeleteCardGroup(int idCardGroup)
    {
      _channel.DeleteCardGroup(idCardGroup);
    }

    public IList<CardGroup> ListAllCardGroups()
    {
      return _channel.ListAllCardGroups();
    }

    public IList<SoftwareEncoder> ListAllSofwareEncodersVideo()
    {
      return _channel.ListAllSofwareEncodersVideo();
    }

    public IList<SoftwareEncoder> ListAllSofwareEncodersAudio()
    {
      return _channel.ListAllSofwareEncodersAudio();
    }

    public IList<Satellite> ListAllSatellites()
    {
      return _channel.ListAllSatellites();
    }

    public Satellite SaveSatellite(Satellite satellite)
    {
      satellite.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveSatellite(satellite);
    }

    public SoftwareEncoder SaveSoftwareEncoder(SoftwareEncoder encoder)
    {
      encoder.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveSoftwareEncoder(encoder);
    }

    public void DeleteGroupMap(int idMap)
    {
      _channel.DeleteGroupMap(idMap);
    }

    public CardGroupMap SaveCardGroupMap(CardGroupMap map)
    {
      map.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveCardGroupMap(map);
    }

    public IList<Card> ListAllCards(CardIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllCards(includeRelations);
    }

    public Card GetCard(int idCard, CardIncludeRelationEnum includeRelations)
    {
      return _channel.GetCard(idCard, includeRelations);
    }

    public Card GetCardByDevicePath(string cardDevice, CardIncludeRelationEnum includeRelations)
    {
      return _channel.GetCardByDevicePath(cardDevice, includeRelations);
    }

    public IList<Card> SaveCards(IEnumerable<Card> cards)
    {
      return _channel.SaveCards(cards);
    }
  }
}
