using ChessChallenge.API;
using System.Collections.Generic;
using System;

public class MyBot : IChessBot
{
    private readonly Random rng;

    public MyBot()
    {
        rng = new();
    }

    private Move RandomMove(IList<Move> moves)
    {
        return moves[rng.Next(moves.Count)];
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        return RandomMove(moves);
    }
}