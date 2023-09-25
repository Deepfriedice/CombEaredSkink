using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;


class MoveDisplayer
{
    private Dictionary<int,List<Move>> movesByScore;
    public MoveDisplayer()
    {
        movesByScore = new Dictionary<int,List<Move>>();
    }

    public void Add(Move move, int score)
    {

        if (!movesByScore.ContainsKey(score))
        {
            movesByScore[score] = new List<Move>();
        }
        movesByScore[score].Add(move);
    }

    public void Clear()
    {
        movesByScore.Clear();
    }

    public void Print()
    {
        int[] scores = movesByScore.Keys.ToArray();
        Array.Sort(scores);
        foreach (int score in scores)
        {
            Console.Write("{0,5} - ", score);
            foreach (Move move in movesByScore[score])
            {
                string moveName = move.ToString().Substring(7,4);
                Console.Write("{0} ", moveName);
            }
            Console.WriteLine();
        }
    }
}
