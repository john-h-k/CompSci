using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Playground
{

    internal class WarGame
    {
        private readonly Deck _deck1;
        private readonly Deck _deck2;

        private readonly List<Card> _inPlayCards = new List<Card>();

        public enum Player
        {
            Player1,
            Player2
        }


        public WarGame(Deck deck = null)
        {
            deck = deck ?? new Deck(true);
            _deck1 = new Deck(deck.DrawCards(26));
            _deck2 = new Deck(deck.DrawCards(26));
        }

        private (Card card1, Card card2) DrawPair()
        {
            return (_deck1.DrawCard(), _deck2.DrawCard());
        }

        public Player? RunGame()
        {
            int i = 0;
            while (ResolveWinner() is null && i < 1000 && _deck1.Count != _deck2.Count)
            {
                RunTurn();
                i++;
            }

            return ResolveWinner() ?? (_deck1.Count > _deck2.Count ? Player.Player1 : Player.Player2);
        }

        private Player? ResolveWinner()
        {
            if (_deck1.Count != 0 && _deck2.Count != 0) return null;
            return _deck1.Count == 0 ? Player.Player2 : Player.Player1;
        }

        private Player? EnsureSize()
        {
            if (ResolveWinner() != null)
                return WinPair(_inPlayCards[_inPlayCards.Count - 2], _inPlayCards[_inPlayCards.Count - 1]);

            return null;
        }

        private Player RunTurn()
        {
            while (true)
            {
                Player? fallbackWinner = EnsureSize();
                if (fallbackWinner != null) return fallbackWinner.Value;

                (Card card1, Card card2) = DrawPair();
                _inPlayCards.Add(card1, card2);

                if (card1.Value != card2.Value)
                {
                    return WinPair(card1, card2);
                }

                fallbackWinner = EnsureSize();
                if (fallbackWinner != null) return fallbackWinner.Value;

                (Card downCard1, Card downCard2) = DrawPair();
                _inPlayCards.Add(downCard1, downCard2);
            }
        }

        private Player WinPair(Card card1, Card card2)
        {
            bool didPlayer1Win = card1.Value > card2.Value;
            (didPlayer1Win ? _deck1 : _deck2).AddCardsToBase(_inPlayCards.ToArray());
            _inPlayCards.Clear();
            return didPlayer1Win ? Player.Player1 : Player.Player2;
        }
    }

    // TODO ace high support
    internal readonly struct Card
    {
        public enum CardTypes
        {
            Spades = 1,
            Clubs = 2,
            Diamonds = 3,
            Hearts = 4
        }
        
        public Card(CardTypes type, int value)
        {
            if (!Enum.IsDefined(typeof(CardTypes), type)) throw new ArgumentException("Invalid suit");
            if (value < 0 || value > 13) throw new ArgumentOutOfRangeException(nameof(value));
            Suit = type;
            Value = (byte)value;
        }

        public readonly CardTypes Suit;
        public readonly byte Value;
    }

    internal class Deck
    {
        public int Count => _cards.Count;

        public const int DeckSize = 52;

        private readonly Queue<Card> _cards;

        private static Card[] GetOrderedDeck()
        {
            var cards = new Card[DeckSize];
            int next = 0;
            for (int i = 0; i < 4; i++)
            {
                for (var j = 0; j < 13; j++)
                {
                    cards[next++] = new Card((Card.CardTypes)i + 1, j + 1);
                }
            }

            return cards;
        }

        public Deck(bool random = false)
        {
            if (random)
            {
                Card[] orderedCards = GetOrderedDeck();
                orderedCards.Shuffle();
                _cards = new Queue<Card>(orderedCards);
            }
            else
            {
                _cards = new Queue<Card>(GetOrderedDeck());
            }
        }

        public Deck(IEnumerable<Card> deck)
        {
            _cards = new Queue<Card>(deck);
        }
        
        public Card[] DrawCards(int count)
        {
            if (count < 1 || count > _cards.Count) throw new ArgumentOutOfRangeException(nameof(count));
            var cards = new Card[count];
            for (var i = 0; i < cards.Length; i++)
            {
                cards[i] = _cards.Dequeue();
            }

            return cards;
        }

        [DebuggerStepThrough]
        public Card DrawCard()
        {
            return _cards.Dequeue();
        }

        [DebuggerStepThrough]
        public Card PeekCard()
        {
            return _cards.Peek();
        }

        [DebuggerStepThrough]
        public void AddCardToBase(Card card)
        {
            _cards.Enqueue(card);
        }

        [DebuggerStepThrough]
        public void AddCardsToBase(params Card[] cards)
        {
            for (var i = 0; i < cards.Length; i++) _cards.Enqueue(cards[i]);
        }
    }

    internal static class DeckExtensions
    {
        public static void Shuffle<T>(this T[] array)
        {
            var rng = new Random();
            for (int i = array.Length; i > 1;)
            {
                int j = rng.Next(i--);
                T value = array[j];
                array[j] = array[i];
                array[i] = value;
            }
        }

        [DebuggerStepThrough]
        public static void Add<T>(this List<T> list, params T[] items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                list.Add(items[i]);
            }
        }
    }
}