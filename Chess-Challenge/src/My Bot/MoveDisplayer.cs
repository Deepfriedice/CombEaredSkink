using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChessMove = ChessChallenge.Chess.Move;


class MoveDisplayer
{
    private Dictionary<int,List<Move>> movesByScore;
    private static FieldInfo moveField = (FieldInfo) typeof(Move).GetField("move", BindingFlags.NonPublic | BindingFlags.Instance)!;

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

    public static string MoveName(Move move)
    {
        ChessMove internalMove = (ChessMove) MoveDisplayer.moveField.GetValue(move)!;
        return ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(internalMove);
    }

    public void Print()
    {
        int[] scores = movesByScore.Keys.ToArray();
        Array.Sort(scores);
        Array.Reverse(scores);
        foreach (int score in scores)
        {
            Console.Write("{0,5} - ", score);
            foreach (Move move in movesByScore[score])
            {
                Console.Write("{0} ", MoveName(move));
            }
            Console.WriteLine();
        }
    }
}
