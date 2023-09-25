using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private readonly Random rng;
    private const int UpperBound = 200;

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
        board.ForceSkipTurn();
        int otherMoveCount = board.GetLegalMoves().Length;
        board.UndoSkipTurn();
        return ownMoveCount - otherMoveCount;
        // note that board.IsInCheck is forced to false
    }

    private int RecursiveBoardScore(Board board, int depth)
    {
        if (depth == 0)
        {
            return BoardScore(board);
        }

        int score = -UpperBound;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            // play a checkmate move immediately
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                return UpperBound;
            }

            // update the list of best moves
            int opponentScore = RecursiveBoardScore(board, depth-1);
            score = Math.Max(score, -opponentScore);
            board.UndoMove(move);
        }

        // play a random move from the best moves
        return score;
    }

    // Select a random move from a list.
    private Move RandomMove(IList<Move> moves)
    {
        return moves[rng.Next(moves.Count)];
    }

    public Move Think(Board board, Timer timer)
    {
        int bestScore = UpperBound;
        MoveDisplayer displayer = new MoveDisplayer();  //#DEBUG
        List<Move> bestMoves = new List<Move>();
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            // play a checkmate move immediately
            if (board.IsInCheckmate())
            {
                return move;  // don't need to undo the move
            }

            // update the list of best moves
            int score = RecursiveBoardScore(board, 2);
            displayer.Add(move, score);  //#DEBUG
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

        displayer.Print();  //#DEBUG

        // play a random move from the best moves
        return RandomMove(bestMoves);
    }
}