using System;
using System.Collections.Generic;
using System.Globalization;

class Program
{
    static List<Card> deck; // The main deck of cards
    static List<Card> discardPile; // Discarded cards
    static List<Card> playerHand; // Player's hand of cards
    static List<Card> dealerHand; // Dealer's hand of cards

    static int playerWins = 0; // Tracks player's total wins
    static int dealerWins = 0; // Tracks dealer's total wins
    static int draws = 0; // Tracks total draws
    static bool gameRunning = true; // Controls whether the game continues

    static void Main(string[] args)
    {
        // Initialize the game
        InitializeDeck();
        Shuffle(deck);

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
    }

    // Initialize the deck with 52 cards + 2 Jokers
    static void InitializeDeck()
    {
        deck = new List<Card>();
        discardPile = new List<Card>();

        // Add standard cards (excluding Joker suit)
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            if (suit == Suit.Joker) continue; // Skip Joker suit during standard card creation

            foreach (Value value in Enum.GetValues(typeof(Value)))
            {
                if (value == Value.Joker) continue; // Skip Joker value during standard card creation
                deck.Add(new Card { Suit = suit, Value = value });
            }
        }

        // Add 2 Jokers
        deck.Add(new Card { Suit = Suit.Joker, Value = Value.Joker });
        deck.Add(new Card { Suit = Suit.Joker, Value = Value.Joker });
    }

    // Shuffle the deck using the Fisher-Yates algorithm
    static void Shuffle(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int swap = Random.Shared.Next(cards.Count);
            (cards[i], cards[swap]) = (cards[swap], cards[i]);
        }
    }

    // Shuffle the discard pile back into the deck
    static void ShuffleDiscardIntoDeck()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(deck);
    }

    // Draw a card from the deck
    static Card DrawCard()
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

    // Play a round of the game
    static void PlayRound()
    {
        if (!gameRunning) return; // If the game is over, exit directly

        Console.Clear();

        // Deal 6 cards to each player
        playerHand = new List<Card>();
        dealerHand = new List<Card>();
        for (int i = 0; i < 6; i++)
        {
            playerHand.Add(DrawCard());
            dealerHand.Add(DrawCard());
        }

        // Show player's and dealer's hands
        Console.WriteLine("Player's Hand:");
        RenderHandHorizontal(playerHand);

        Console.WriteLine("\nDealer's Hand:");
        RenderHandHorizontal(dealerHand);
        // Check for Crazy Twos and activate their effects
        ActivateCrazyTwos(playerHand, dealerHand);
        
        // Player assigns cards to rounds
        var playerRounds = AssignCardsToRounds(playerHand, "Player");
        var dealerRounds = AssignCardsToRounds(dealerHand, "Dealer");



        // Play Round 1: Single Card Comparison
        Console.WriteLine("\nRound 1: Single Card Comparison");
        int playerScore1 = GetCardValue(playerRounds[0][0]); // Get value of the single card
        int dealerScore1 = GetCardValue(dealerRounds[0][0]);
        CompareScoresround1(playerScore1, dealerScore1, 1);

        // Play Round 2: Two-Card Comparison (10.5 Points Rule)
        Console.WriteLine("\nRound 2: Two-Card Comparison");
        float playerScore2 = CalculateTwoCardScore(playerRounds[1]);
        float dealerScore2 = CalculateTwoCardScore(dealerRounds[1]);
        CompareScoresround2(playerScore2, dealerScore2, 2);

        // Play Round 3: Three-Card Comparison (Bluffing Rules)
        Console.WriteLine("\nRound 3: Three-Card Comparison");
        int playerScore3 = CalculateThreeCardScore(playerRounds[2]);
        int dealerScore3 = CalculateThreeCardScore(dealerRounds[2]);
        CompareScoresround1(playerScore3, dealerScore3, 3);

        // Add used cards to the discard pile
        discardPile.AddRange(playerHand);
        discardPile.AddRange(dealerHand);

        Console.WriteLine("\nPress Enter to start the next round...");
        Console.ReadLine();
    }

    // Assign cards to rounds (1 card for Round 1, 2 cards for Round 2, 3 cards for Round 3)
    static List<List<Card>> AssignCardsToRounds(List<Card> hand, string playerName)
    {
        var rounds = new List<List<Card>>();
        Console.WriteLine($"\n{playerName}, assign your cards to rounds:");

        // Round 1: 1 card
        Console.WriteLine("Round 1: Select 1 card (enter index 0-5):");
        int index1 = int.Parse(Console.ReadLine());
        rounds.Add(new List<Card> { hand[index1] });

        // Round 2: 2 cards
        Console.WriteLine("Round 2: Select 2 cards (enter indices 0-5, separated by spaces):");
        var indices2 = Console.ReadLine().Split(' ');
        rounds.Add(new List<Card> { hand[int.Parse(indices2[0])], hand[int.Parse(indices2[1])] });

        // Round 3: 3 cards
        Console.WriteLine("Round 3: Select 3 cards (enter indices 0-5, separated by spaces):");
        var indices3 = Console.ReadLine().Split(' ');
        rounds.Add(new List<Card> { hand[int.Parse(indices3[0])], hand[int.Parse(indices3[1])], hand[int.Parse(indices3[2])] });

        return rounds;
    }

    // Compare scores for a round and update win/draw counts
    static void CompareScoresround1(int playerScore, int dealerScore, int roundMultiplier)
    {
        if (playerScore > dealerScore)
        {
            Console.WriteLine("Player Wins this round!");
            playerWins += roundMultiplier;
        }
        else if (playerScore < dealerScore)
        {
            Console.WriteLine("Dealer Wins this round!");
            dealerWins += roundMultiplier;
        }
        else
        {
            Console.WriteLine("Draw this round!");
            draws++;
        }
    }
    static void CompareScoresround2(float playerScore, float dealerScore, int roundMultiplier)
    {
        if (playerScore > dealerScore)
        {
            Console.WriteLine("Player Wins this round!");
            playerWins += roundMultiplier;
        }
        else if (playerScore < dealerScore)
        {
            Console.WriteLine("Dealer Wins this round!");
            dealerWins += roundMultiplier;
        }
        else
        {
            Console.WriteLine("Draw this round!");
            draws++;
        }
    }
    // Calculate the score for two-card comparison (10.5 Points Rule)
    static float CalculateTwoCardScore(List<Card> cards)
    {
        int total = 0;
        foreach (var card in cards)
        {
            float Value = GetCardValue(card);
            if (Value >10){
                Value = 0.5f;
            }
            total += GetCardValue(card);
        }
        return total > 10.5 ? 0 : total; // Bust if over 10
    }

    // Calculate the score for three-card comparison (Bluffing Rules)
    static int CalculateThreeCardScore(List<Card> cards)
    {
        int total = 0;
        foreach (var card in cards)
        {
            total += GetCardValue(card);
        }
        return total;
    }

    // Get the value of a single card
    static int GetCardValue(Card card)
    {
        return card.GetValues();
    }

    // Activate Crazy Twos effects
    static void ActivateCrazyTwos(List<Card> playerHand, List<Card> dealerHand)
    {
        foreach (var card in playerHand)
        {
            if (card.GetValues() == 2)
            {
                switch (card.GetSuit())
                {
                    case 1:
                        Console.WriteLine("Player activates Red Heart 2: Victory points doubled for all rounds!");
                        playerWins *= 2;
                        break;
                    case 2:
                        Console.WriteLine("Player activates Club 2: Drawing 6 new cards!");
                        playerHand.Clear();
                        for (int i = 0; i < 6; i++) playerHand.Add(DrawCard());
                        break;
                    case 3:
                        Console.WriteLine("Player activates Spade 2: Swapping hands with the dealer!");
                        (playerHand, dealerHand) = (dealerHand, playerHand);
                        break;
                    case 4:
                        Console.WriteLine("Player activates Diamond 2: Randomizing dealer's assigned cards!");
                        Shuffle(dealerHand);
                        break;
                }
            }
        }
    }

    // Render a hand of cards horizontally
    static void RenderHandHorizontal(List<Card> hand)
    {
        string[] renderedCards = new string[Card.RenderHeight];
        for (int i = 0; i < Card.RenderHeight; i++)
        {
            renderedCards[i] = "";
        }

        foreach (var card in hand)
        {
            string[] renderedCard = card.Render();
            for (int i = 0; i < Card.RenderHeight; i++)
            {
                renderedCards[i] += renderedCard[i] + " ";
            }
        }

        for (int i = 0; i < Card.RenderHeight; i++)
        {
            Console.WriteLine(renderedCards[i]);
        }
    }
}

// Card class representing a single card
class Card
{
    public Suit Suit;
    public Value Value;

    // Get the numeric value of the card
    public int GetValues()
    {
        return (int)Value;
    }
    public int GetSuit()
    {
        return (int)Suit;
    }
    // Render the card as ASCII art
    public string[] Render()
    {
        // char suit = Suit.ToString()[0];
        string suit = Suit switch
        {
            Suit.Clubs => "♣",
            Suit.Diamonds => "♢",
            Suit.Hearts => "♡",
            Suit.Spades => "♠",
            Suit.Joker => "♔",

        };
        string value = Value switch
        {
            Value.Ace => "A",
            Value.Ten => "10",
            Value.Jack => "J",
            Value.Queen => "Q",
            Value.King => "K",
            Value.Joker => "⚗",
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

    // Height of the rendered card
    public const int RenderHeight = 7;
}

// Suit enum representing card suits
enum Suit
{
    Hearts = 1,
    Clubs = 2,
    Spades= 3,
    Diamonds= 4,
    Joker= 5 // Special suit for Jokers
}

// Value enum representing card values
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
    Joker = 14 // Special value for Jokers
}