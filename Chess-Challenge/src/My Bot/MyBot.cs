using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private readonly Random rng;

    public MyBot()
    {
        rng = new();
    }

    // Try to evaluate how good a position is.
    // Higher number -> better for the current player.
    private int BoardScore(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        int ownMoveCount = moves.Length;
        int otherMoveCount;

        if (board.TrySkipTurn())
        {
            otherMoveCount = board.GetLegalMoves().Length;
            board.UndoSkipTurn();
        }
        else
        {
            Move checkMove = RandomMove(moves);
            board.MakeMove(checkMove);
            otherMoveCount = board.GetLegalMoves().Length;
            board.UndoMove(checkMove);
        }

        return ownMoveCount - otherMoveCount;
    }

    // Select a random move from a list.
    private Move RandomMove(IList<Move> moves)
    {
        return moves[rng.Next(moves.Count)];
    }

    public Move Think(Board board, Timer timer)
    {
        int bestScore = 1000;
        MoveDisplayer displayer = new MoveDisplayer();
        List<Move> bestMoves = new List<Move>();
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            // play a checkmate move immediately
            if (board.IsInCheckmate())
            {
                return move;
            }

            // update the list of best moves
            int score = BoardScore(board);
            displayer.Add(move, score);
            if (score == bestScore)
            {
                bestMoves.Add(move);
            }
            else if (score < bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
                bestMoves.Add(move);
            }
            board.UndoMove(move);
        }

        displayer.Print();

        // play a random move from the best moves
        return RandomMove(bestMoves);
    }
}