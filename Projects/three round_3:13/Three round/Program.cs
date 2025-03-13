using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

      List<Card> deck; // The main deck of cards
      List<Card> discardPile; // Discarded cards
      List<Card> playerHand; // Player's hand of cards
      List<Card> dealerHand; // Dealer's hand of cards

      int playerWins = 0; // Tracks player's total wins
      int dealerWins = 0; // Tracks dealer's total wins
      int draws = 0; // Tracks total draws
      bool gameRunning = true; // Controls whether the game continues


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
    

    // Initialize the deck with 52 cards + 2 Jokers
      void InitializeDeck()
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
      void Shuffle(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int swap = Random.Shared.Next(cards.Count);
            (cards[i], cards[swap]) = (cards[swap], cards[i]);
        }
    }

    // Shuffle the discard pile back into the deck
      void ShuffleDiscardIntoDeck()
    {
        deck.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(deck);
    }

    // Draw a card from the deck
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

    // Play a round of the game
      void PlayRound()
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

        // Check for Crazy Twos and ask the player if they want to activate them
        bool crazyTwosActivated = ActivateCrazyTwos(playerHand, dealerHand);

        // If Crazy Twos was activated, clear the console and re-render hands
        if (crazyTwosActivated)
        {
            Console.Clear();
            Console.WriteLine("Player's Hand After Crazy Twos:");
            RenderHandHorizontal(playerHand);

            Console.WriteLine("\nDealer's Hand After Crazy Twos:");
            RenderHandHorizontal(dealerHand);
        }

        // Player assigns cards to rounds
        var playerRounds = AssignCardsToRounds(playerHand, "Player");

        // If Diamond 2 was activated, dealer's rounds are assigned randomly
        var dealerRounds = HasDiamondTwo(playerHand) ? RandomAssignCardsToRounds(dealerHand) : AssignCardsToRounds(dealerHand, "Dealer");

        // Play Round 1: Single Card Comparison
        Console.WriteLine("\nRound 1: Single Card Comparison");
        int playerScore1 = GetCardValue(playerRounds[0][0], isRound1: true); // Get value of the single card
        int dealerScore1 = GetCardValue(dealerRounds[0][0], isRound1: true);
        CompareScoresround1(playerScore1, dealerScore1, 1);

        // Display Round 1 results
        Console.WriteLine("\nRound 1 Results:");
        Console.WriteLine($"Player's Card: {playerRounds[0][0].Render()[1]}");
        Console.WriteLine($"Dealer's Card: {dealerRounds[0][0].Render()[1]}");

        // Display current scores
        DisplayCurrentScores();

        // Play Round 2: Two-Card Comparison (10.5 Points Rule)
        Console.WriteLine("\nRound 2: Two-Card Comparison");
        float playerScore2 = CalculateTwoCardScore(playerRounds[1]);
        float dealerScore2 = CalculateTwoCardScore(dealerRounds[1]);
        CompareScoresround2(playerScore2, dealerScore2, 2);

        // Display Round 2 results
        Console.WriteLine("\nRound 2 Results:");
        Console.WriteLine($"Player's Cards: {playerRounds[1][0].Render()[1]}, {playerRounds[1][1].Render()[1]}");
        Console.WriteLine($"Dealer's Cards: {dealerRounds[1][0].Render()[1]}, {dealerRounds[1][1].Render()[1]}");

        // Display current scores
        DisplayCurrentScores();

        // Play Round 3: Three-Card Comparison (Bluffing Rules)
        Console.WriteLine("\nRound 3: Three-Card Comparison");
        var playerRanking = EvaluateHandRanking(playerRounds[2]);
        var dealerRanking = EvaluateHandRanking(dealerRounds[2]);
        CompareScoresround3(playerRanking, dealerRanking, 3);

        // Display Round 3 results
        Console.WriteLine("\nRound 3 Results:");
        Console.WriteLine($"Player's Cards: {playerRounds[2][0].Render()[1]}, {playerRounds[2][1].Render()[1]}, {playerRounds[2][2].Render()[1]}");
        Console.WriteLine($"Player's Hand Ranking: {playerRanking.ranking}");
        Console.WriteLine($"Dealer's Cards: {dealerRounds[2][0].Render()[1]}, {dealerRounds[2][1].Render()[1]}, {dealerRounds[2][2].Render()[1]}");
        Console.WriteLine($"Dealer's Hand Ranking: {dealerRanking.ranking}");

        // Display current scores
        DisplayCurrentScores();

        // Add used cards to the discard pile
        discardPile.AddRange(playerHand);
        discardPile.AddRange(dealerHand);

        Console.WriteLine("\nPress Enter to start the next round...");
        Console.ReadLine();
    }

    // Assign cards to rounds (1 card for Round 1, 2 cards for Round 2, 3 cards for Round 3)
      List<List<Card>> AssignCardsToRounds(List<Card> hand, string playerName)
    {
        var rounds = new List<List<Card>>();
        var selectedIndices = new HashSet<int>();

        Console.WriteLine($"\n{playerName}, assign your cards to rounds:");

        // Round 1: 1 card
        Console.WriteLine("Round 1: Select 1 card (enter index 1-6):");
        int index1 = GetValidCardIndex(selectedIndices);
        selectedIndices.Add(index1);
        rounds.Add(new List<Card> { hand[index1 - 1] });

        // Round 2: 2 cards
        Console.WriteLine("Round 2: Select 2 cards (enter indices 1-6, separated by spaces):");
        var indices2 = GetValidCardIndices(selectedIndices, 2);
        selectedIndices.UnionWith(indices2);
        rounds.Add(new List<Card> { hand[indices2[0] - 1], hand[indices2[1] - 1] });

        // Round 3: 3 cards
        Console.WriteLine("Round 3: Select 3 cards (enter indices 1-6, separated by spaces):");
        var indices3 = GetValidCardIndices(selectedIndices, 3);
        selectedIndices.UnionWith(indices3);
        rounds.Add(new List<Card> { hand[indices3[0] - 1], hand[indices3[1] - 1], hand[indices3[2] - 1] });

        return rounds;
    }

    // Randomly assign cards to rounds (1 card for Round 1, 2 cards for Round 2, 3 cards for Round 3)
      List<List<Card>> RandomAssignCardsToRounds(List<Card> hand)
    {
        var rounds = new List<List<Card>>();

        // Shuffle the hand
        Shuffle(hand);

        // Assign cards to rounds
        rounds.Add(new List<Card> { hand[0] }); // Round 1: 1 card
        rounds.Add(new List<Card> { hand[1], hand[2] }); // Round 2: 2 cards
        rounds.Add(new List<Card> { hand[3], hand[4], hand[5] }); // Round 3: 3 cards

        return rounds;
    }

    // Get a valid card index from the player
      int GetValidCardIndex(HashSet<int> selectedIndices)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= 6 && !selectedIndices.Contains(index))
            {
                return index;
            }
            Console.WriteLine("Invalid index! Please enter a number between 1 and 6 that hasn't been selected yet.");
        }
    }

    // Get valid card indices from the player
      List<int> GetValidCardIndices(HashSet<int> selectedIndices, int count)
    {
        while (true)
        {
            var input = Console.ReadLine().Split(' ');
            if (input.Length == count && input.All(s => int.TryParse(s, out int index) && index >= 1 && index <= 6 && !selectedIndices.Contains(index)))
            {
                return input.Select(int.Parse).ToList();
            }
            Console.WriteLine($"Invalid indices! Please enter {count} unique numbers between 1 and 6 that haven't been selected yet.");
        }
    }

    // Compare scores for a round and update win/draw counts
      void CompareScoresround1(int playerScore, int dealerScore, int roundMultiplier)
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

      void CompareScoresround2(float playerScore, float dealerScore, int roundMultiplier)
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

    // Compare scores for Round 3 based on hand rankings
      void CompareScoresround3((HandRanking ranking, int maxCard) playerRanking, (HandRanking ranking, int maxCard) dealerRanking, int roundMultiplier)
    {
        if (playerRanking.ranking > dealerRanking.ranking)
        {
            Console.WriteLine("Player Wins this round!");
            playerWins += roundMultiplier;
        }
        else if (playerRanking.ranking < dealerRanking.ranking)
        {
            Console.WriteLine("Dealer Wins this round!");
            dealerWins += roundMultiplier;
        }
        else
        {
            if (playerRanking.maxCard > dealerRanking.maxCard)
            {
                Console.WriteLine("Player Wins this round!");
                playerWins += roundMultiplier;
            }
            else if (playerRanking.maxCard < dealerRanking.maxCard)
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
    }

    // Calculate the score for two-card comparison (10.5 Points Rule)
      float CalculateTwoCardScore(List<Card> cards)
    {
        float total = 0;
        foreach (var card in cards)
        {
            float value = GetCardValue(card, isRound1: false);
            if (value > 10) value = 0.5f; // J, Q, K count as 0.5
            total += value;
        }
        return total > 10.5f ? 0 : total; // Bust if over 10.5
    }

    // Evaluate the ranking of a three-card hand
      (HandRanking ranking, int maxCard) EvaluateHandRanking(List<Card> cards)
    {
        var values = cards.Select(c => GetCardValue(c, isRound1: false)).OrderBy(v => v).ToList();
        var suits = cards.Select(c => c.Suit).ToList();

        // Check for Straight Flush
        if (values[0] + 1 == values[1] && values[1] + 1 == values[2] && suits[0] == suits[1] && suits[1] == suits[2])
        {
            return (HandRanking.StraightFlush, values[2]);
        }

        // Check for Three of a Kind
        if (values[0] == values[1] && values[1] == values[2])
        {
            return (HandRanking.ThreeOfAKind, values[2]);
        }

        // Check for Flush
        if (suits[0] == suits[1] && suits[1] == suits[2])
        {
            return (HandRanking.Flush, values[2]);
        }

        // Check for Straight
        if (values[0] + 1 == values[1] && values[1] + 1 == values[2])
        {
            return (HandRanking.Straight, values[2]);
        }

        // Check for Pair
        if (values[0] == values[1] || values[1] == values[2])
        {
            return (HandRanking.Pair, values[2]);
        }

        // High Card
        return (HandRanking.HighCard, values[2]);
    }

    // Get the value of a single card
      int GetCardValue(Card card, bool isRound1)
    {
        if (card.Value == Value.Joker)
        {
            return isRound1 ? 15 : 0; // Joker is the highest in Round 1, otherwise 0
        }
        return (int)card.Value;
    }

    // Check if the player has a Diamond 2 in their hand
      bool HasDiamondTwo(List<Card> hand)
    {
        return hand.Any(card => card.Suit == Suit.Diamonds && card.Value == Value.Two);
    }

    // Activate Crazy Twos effects
      bool ActivateCrazyTwos(List<Card> playerHand, List<Card> dealerHand)
    {
        bool activated = false; // Track if any Crazy Twos effect was activated

        // Create a copy of playerHand to avoid modifying the collection during enumeration
        var playerHandCopy = new List<Card>(playerHand);

        foreach (var card in playerHandCopy)
        {
            if (card.GetValues() == 2) // Check if the card is a Two
            {
                Console.WriteLine($"You have a {card.Render()[1]} (Two).");
                Console.WriteLine("Crazy Twos Effects:");
                Console.WriteLine("1. Hearts: Victory points doubled for this round!");
                Console.WriteLine("2. Clubs: Draw 6 new cards!");
                Console.WriteLine("3. Spades: Swap hands with the dealer!");
                Console.WriteLine("4. Diamonds: Randomize dealer's assigned cards!");
                Console.WriteLine("Do you want to activate its Crazy Twos effect? (yes/no)");
                string choice = Console.ReadLine().ToLower();

                if (choice == "yes")
                {
                    activated = true; // Mark that Crazy Twos was activated
                    switch (card.GetSuit())
                    {
                        case 1:
                            Console.WriteLine("Player activates Red Heart 2: Victory points doubled for this round!");
                            playerWins *= 2;
                            break;
                        case 2:
                            Console.WriteLine("Player activates Club 2: Drawing 6 new cards!");
                            playerHand.Clear(); // Clear the original hand
                            for (int i = 0; i < 6; i++) playerHand.Add(DrawCard()); // Draw new cards
                            break;
                        case 3:
                            Console.WriteLine("Player activates Spade 2: Swapping hands with the dealer!");
                            (playerHand, dealerHand) = (dealerHand, playerHand); // Swap hands
                            break;
                        case 4:
                            Console.WriteLine("Player activates Diamond 2: Randomizing dealer's assigned cards!");
                            break;
                    }
                }
            }
        }

        return activated; // Return whether Crazy Twos was activated
    }

    // Render a hand of cards horizontally
      void RenderHandHorizontal(List<Card> hand)
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

    // Display current scores
      void DisplayCurrentScores()
    {
        Console.WriteLine("\nCurrent Scores:");
        Console.WriteLine($"Player Wins: {playerWins}");
        Console.WriteLine($"Dealer Wins: {dealerWins}");
        Console.WriteLine($"Draws: {draws}");
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
        string suit = Suit switch
        {
            Suit.Clubs => "♣",
            Suit.Diamonds => "♦",
            Suit.Hearts => "♥",
            Suit.Spades => "♠",
            Suit.Joker => "⚗",
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
    Spades = 3,
    Diamonds = 4,
    Joker = 5 // Special suit for Jokers
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

// Hand ranking enum for Round 3
enum HandRanking
{
    HighCard = 0,
    Pair = 1,
    Straight = 2,
    Flush = 3,
    ThreeOfAKind = 4,
    StraightFlush = 5
}