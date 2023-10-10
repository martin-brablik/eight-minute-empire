using System;

public class Card
{
	public Resource Resource { get; private set; }
	public short Price { get; set; }
	public Player Owner { get; set; }
	public short Amount { get; set; }
	public CardAction[] CardActions { get; set; }

	public Card(Resource resource, CardAction[] cardActions)
	{
		Resource = resource;
		CardActions = cardActions;
		Price = 0;
	}

	public static void Recruit(Player player, short amount, Land targetLand)
	{

	}

	public static void Move(Player player, Land sourceLand, Land targetLand, short amount, bool moveOversea)
	{

	}

	public static void BuildCity(Player player, Land targetLand)
	{

	}

	public static void Battle(Player sourcePlayer, Player targetPlayer, Land land, short amount)
	{

	}
}