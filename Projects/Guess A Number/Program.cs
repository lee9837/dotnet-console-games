// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Security.Cryptography;


string i;
int input = 0;
int mode;
int range = 100; // Default guessing range
int maxGuesses = 5; // Default guessing number
bool modeSelected = false;
Console.WriteLine("Let's Guess a Number");
//determine the mode of game iteration break when mode selected
while (!modeSelected)
{
    // display game mode on console
    Console.WriteLine("Choose your game mode:");
    Console.WriteLine("1. Range 1-100 (5 guesses)");
    Console.WriteLine("2. Range 1-150 (7 guesses)");
    Console.WriteLine("3. Range 1-300 (9 guesses)");

    string modeInput = Console.ReadLine() ?? "";

    if (int.TryParse(modeInput, out mode))
    {
        if (mode == 1)
        {
            range = 100;
            maxGuesses = 5;
            modeSelected = true;
        }
        else if (mode == 2)
        {
            range = 150;
            maxGuesses = 7;
            modeSelected = true;
        }
        else if (mode == 3)
        {
            range = 300;
            maxGuesses = 9;
            modeSelected = true;
        }
        else
        {
            Console.WriteLine("Invalid mode. Please choose 1, 2, or 3.");
        }
    }
    else
    {
        Console.WriteLine("Invalid input. Please enter a number (1, 2, or 3).");
    }
}

/* Random creator */
Random rand = new Random();
int guess = rand.Next(0, range + 1);
int remainingGuesses = maxGuesses;
Console.WriteLine($"Game starts! Guess the number between 0 and {range}. You have {maxGuesses} guesses.");

while (remainingGuesses > 0)
{
    Console.WriteLine($"You have {remainingGuesses} guesses remaining. Guess a Number (0-{range}):");
    i = Console.ReadLine() ?? "";

    if (int.TryParse(i, out input))
    {
        if (input == guess)
        {
            Console.WriteLine("You win!");
            break;
        }
        else
        {
            int difference = Math.Abs(guess - input);

            if (difference <= 5)
            {
                if (input < guess)
                {
                    Console.WriteLine("A bit low!");
                }
                else
                {
                    Console.WriteLine("A bit high!");
                }
            }
            else if (difference <= 20)
            {
                if (input < guess)
                {
                    Console.WriteLine("Too low!");
                }
                else
                {
                    Console.WriteLine("Too high!");
                }
            }
            else
            {
                if (input < guess)
                {
                    Console.WriteLine("Way too low!");
                }
                else
                {
                    Console.WriteLine("Way too high!");
                }
            }

            remainingGuesses--;
        }
    }
    else
    {
        Console.WriteLine("Incorrect input! Please enter a number!");
    }

    if (remainingGuesses == 0)
    {
        Console.WriteLine($"You lose! The correct number was {guess}");
    }
}
Console.WriteLine("Any key to exit");
Console.ReadKey(true);