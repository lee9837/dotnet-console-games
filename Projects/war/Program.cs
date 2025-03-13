using System;
using System.Collections.Generic;
using System.Globalization;

List<Card> deck;
List<Card> discardPile;
List<Card> dealerHand;

int playerWins = 0;
int dealerWins = 0;
int draws = 0;
bool gameRunning = true; // Flag variable to control whether the game continues to run

// Initialize the game
InitializeDeck();
Shuffle(deck);

dealerHand = new List<Card>();

// Play rounds until the game ends
while (gameRunning)
{
    PlayRound();
}

// Display final results
Console.Clear();
Console.WriteLine("Game Over!");
Console.WriteLine($"Player Wins: {playerWins}");
Console.WriteLine($"Dealer Wins: {dealerWins}");
Console.WriteLine($"Draws: {draws}");

void InitializeDeck()
{
    deck = new List<Card>();
    discardPile = new List<Card>();

    foreach (Suit suit in Enum.GetValues(typeof(Suit)))
    {
        foreach (Value value in Enum.GetValues(typeof(Value)))
        {
            deck.Add(new Card { Suit = suit, Value = value });
        }
    }
}

void Shuffle(List<Card> cards)
{
    for (int i = 0; i < cards.Count; i++)
    {
        int swap = Random.Shared.Next(cards.Count);
        (cards[i], cards[swap]) = (cards[swap], cards[i]);
    }
}

void ShuffleDiscardIntoDeck()
{
    deck.AddRange(discardPile);
    discardPile.Clear();
    Shuffle(deck);
}

Card DrawCard()
{
    if (deck.Count <= 0)
    {
        while (true) // Continue looping until the player enters a valid option
        {
            Console.WriteLine("The deck is empty!");
            Console.WriteLine("1. Shuffle discard pile into deck and continue");
            Console.WriteLine("2. End the game");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                ShuffleDiscardIntoDeck();
                Console.WriteLine("Deck has been shuffled. Continuing the game...");
                break; // Exit the loop and continue the game
            }
            else if (choice == "2")
            {
                Console.WriteLine("Ending the game...");
                gameRunning = false; // Set the flag variable to false and exit the main loop
                return null; // Return null to indicate there are no cards to draw
            }
            else
            {
                Console.WriteLine("Invalid option! Please choose 1 or 2.");
            }
        }
    }
    Card card = deck[^1];
    deck.RemoveAt(deck.Count - 1);
    return card;
}

void PlayRound()
{
    if (!gameRunning) return; // If the game is over, exit directly

    Console.Clear();

    // Show dealer Card
    Console.WriteLine("Dealer's Card:");
    var dealerCard = DrawCard();
    if (dealerCard == null) return; // If there are no cards to draw, exit
    RenderCard(dealerCard);
    dealerHand.Add(dealerCard);

    Console.WriteLine("Press Enter to draw player's card...");
    Console.ReadLine();

    // Show player Card
    Console.Clear();
    Console.WriteLine("Dealer's Card:");
    RenderCard(dealerCard);

    Console.WriteLine("\nPlayer's Card:");
    var playerCard = DrawCard();
    if (playerCard == null) return; // If there are no cards to draw, exit
    RenderCard(playerCard);

    Console.WriteLine("Press Enter to see the result...");
    Console.ReadLine();

    // Win or loss
    int playerValue = playerCard.GetValues();
    int dealerValue = dealerCard.GetValues();

    Console.Clear();
    Console.WriteLine("Dealer's Card:");
    RenderCard(dealerCard);

    Console.WriteLine("\nPlayer's Card:");
    RenderCard(playerCard);

    if (playerValue > dealerValue)
    {
        Console.WriteLine("\nPlayer Wins!");
        playerWins++;
    }
    else if (playerValue < dealerValue)
    {
        Console.WriteLine("\nDealer Wins!");
        dealerWins++;
    }
    else
    {
        Console.WriteLine("\nDraw!");
        draws++;
    }

    Console.WriteLine($"\nPlayer Wins: {playerWins}");
    Console.WriteLine($"Dealer Wins: {dealerWins}");
    Console.WriteLine($"Draws: {draws}");

    discardPile.Add(playerCard);
    discardPile.Add(dealerCard);

    Console.WriteLine("\nPress Enter to start the next round...");
    Console.ReadLine();
}

void RenderCard(Card card)
{
    string[] renderedCard = card.Render();
    foreach (var line in renderedCard)
    {
        Console.WriteLine(line);
    }
}

class Card
{
    public Suit Suit;
    public Value Value;

    public int GetValues()
    {
        return (int)Value;
    }

    public const int RenderHeight = 7;

    public string[] Render()
    {
        char suit = Suit.ToString()[0];
        string value = Value switch
        {
            Value.Ace => "A",
            Value.Ten => "10",
            Value.Jack => "J",
            Value.Queen => "Q",
            Value.King => "K",
            _ => ((int)Value).ToString(CultureInfo.InvariantCulture),
        };
        string card = $"{value}{suit}";
        string a = card.Length < 3 ? $"{card} " : card;
        string b = card.Length < 3 ? $" {card}" : card;
        return
        [
            $"┌───────┐",
            $"│{a}    │",
            $"│       │",
            $"│       │",
            $"│       │",
            $"│    {b}│",
            $"└───────┘",
        ];
    }
}

enum Suit
{
    Hearts,
    Clubs,
    Spades,
    Diamonds,
}

enum Value
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
}